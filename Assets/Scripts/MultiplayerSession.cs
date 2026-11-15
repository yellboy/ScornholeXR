using Fusion;
using Fusion.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class MultiplayerSession : MonoBehaviour, INetworkRunnerCallbacks
    {
        private const string ThrowMessageType = "throw";
        private const string SnapshotMessageType = "snapshot";
        private const int MaximumThrowSendAttempts = 5;
        private const float ThrowRetryInterval = 0.75f;

        [SerializeField] private string _roomName = "scornhole-main-v2";

        private readonly List<PlayerRef> _players = new List<PlayerRef>(2);
        private NetworkRunner _runner;
        private PlayerRef _authorityPlayer = PlayerRef.None;
        private int _nextMessageId;
        private WireMessage _pendingThrow;
        private int _pendingThrowCount;
        private int _throwSendAttempts;
        private float _nextThrowRetryTime;
        private GameSnapshot _lastSnapshot;

        public event Action SessionReady;
        public event Action<int, int, ThrowResult> ThrowReceived;
        public event Action<GameSnapshot> SnapshotReceived;
        public event Action<string> StatusChanged;

        public NetworkRunner Runner => _runner;
        public bool IsAuthority => _runner != null && _runner.IsRunning && _runner.IsSharedModeMasterClient;
        public int LocalPlayerNumber { get; private set; }
        public bool IsReady { get; private set; }

        private async void Start()
        {
            StatusChanged?.Invoke("Connecting to multiplayer...");

            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(this);

            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = _roomName,
                PlayerCount = 2
            });

            if (!result.Ok)
            {
                StatusChanged?.Invoke($"Could not connect: {result.ShutdownReason}");
            }
        }

        private void Update()
        {
            if (_pendingThrow == null || Time.unscaledTime < _nextThrowRetryTime)
            {
                return;
            }

            if (_throwSendAttempts >= MaximumThrowSendAttempts)
            {
                Debug.LogError(
                    $"Player {LocalPlayerNumber}'s throw was not acknowledged after {_throwSendAttempts} attempts");
                _pendingThrow = null;
                return;
            }

            SendPendingThrow();
        }

        public void SubmitThrow(int option, ThrowResult result)
        {
            if (!IsReady || LocalPlayerNumber == 0)
            {
                Debug.LogWarning($"Throw ignored locally: ready={IsReady}, player={LocalPlayerNumber}");
                return;
            }

            Debug.Log($"Player {LocalPlayerNumber} submitted option {option} with result {result}");

            if (IsAuthority)
            {
                ThrowReceived?.Invoke(LocalPlayerNumber, option, result);
                return;
            }

            _pendingThrowCount = GetLocalThrowCount(_lastSnapshot);
            _pendingThrow = new WireMessage
            {
                Type = ThrowMessageType,
                SenderRef = _runner.LocalPlayer.RawEncoded,
                Option = option,
                Result = result
            };
            _throwSendAttempts = 0;
            SendPendingThrow();
        }

        public void BroadcastSnapshot(GameSnapshot snapshot)
        {
            if (!IsAuthority)
            {
                return;
            }

            SnapshotReceived?.Invoke(snapshot);

            foreach (var player in _runner.ActivePlayers)
            {
                if (player != _runner.LocalPlayer)
                {
                    Send(player, new WireMessage
                    {
                        Type = SnapshotMessageType,
                        SenderRef = _runner.LocalPlayer.RawEncoded,
                        Snapshot = snapshot
                    });
                }
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsSharedModeMasterClient)
            {
                return;
            }

            if (!_players.Contains(player) && _players.Count < 2)
            {
                _players.Add(player);
            }

            if (_players.Count < 2)
            {
                LocalPlayerNumber = 1;
                StatusChanged?.Invoke("Waiting for player 2...");
                return;
            }

            _authorityPlayer = runner.LocalPlayer;
            LocalPlayerNumber = GetPlayerNumber(runner.LocalPlayer);
            IsReady = true;
            SessionReady?.Invoke();
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            _players.Remove(player);
            IsReady = false;
            StatusChanged?.Invoke("The other player left. Waiting for player 2...");
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            var json = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
            var message = JsonConvert.DeserializeObject<WireMessage>(json);
            if (message == null)
            {
                return;
            }

            if (message.Type == ThrowMessageType && IsAuthority)
            {
                var sender = PlayerRef.FromEncoded(message.SenderRef);
                var playerNumber = GetPlayerNumber(sender);
                Debug.Log($"Authority received throw from player {playerNumber}: option {message.Option}, result {message.Result}");
                ThrowReceived?.Invoke(playerNumber, message.Option, message.Result);
                return;
            }

            if (message.Type == SnapshotMessageType && !IsAuthority && message.Snapshot != null)
            {
                ApplyPlayerAssignment(message.Snapshot);
                _authorityPlayer = PlayerRef.FromEncoded(message.Snapshot.AuthorityRef);
                IsReady = true;
                AcknowledgePendingThrow(message.Snapshot);
                _lastSnapshot = message.Snapshot;
                Debug.Log(
                    $"Player {LocalPlayerNumber} received snapshot: round {message.Snapshot.RoundNumber}, " +
                    $"active player {message.Snapshot.ActivePlayer}, authority {_authorityPlayer.RawEncoded}");
                SnapshotReceived?.Invoke(message.Snapshot);
            }
        }

        private void SendPendingThrow()
        {
            var target = ResolveAuthorityTarget();
            _throwSendAttempts++;
            _nextThrowRetryTime = Time.unscaledTime + ThrowRetryInterval;
            Debug.Log(
                $"Sending player {LocalPlayerNumber} throw to authority {target.RawEncoded} " +
                $"(attempt {_throwSendAttempts}/{MaximumThrowSendAttempts})");
            Send(target, _pendingThrow);
        }

        private PlayerRef ResolveAuthorityTarget()
        {
            if (_authorityPlayer.IsRealPlayer && _authorityPlayer != _runner.LocalPlayer)
            {
                return _authorityPlayer;
            }

            foreach (var player in _runner.ActivePlayers)
            {
                if (player != _runner.LocalPlayer)
                {
                    return player;
                }
            }

            return PlayerRef.None;
        }

        private void AcknowledgePendingThrow(GameSnapshot snapshot)
        {
            if (_pendingThrow == null || GetLocalThrowCount(snapshot) <= _pendingThrowCount)
            {
                return;
            }

            Debug.Log($"Player {LocalPlayerNumber}'s throw was acknowledged by the authority");
            _pendingThrow = null;
        }

        private int GetLocalThrowCount(GameSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 0;
            }

            return LocalPlayerNumber == 1 ? snapshot.PlayerOneThrows : snapshot.PlayerTwoThrows;
        }

        private void ApplyPlayerAssignment(GameSnapshot snapshot)
        {
            var localPlayer = _runner.LocalPlayer.RawEncoded;
            LocalPlayerNumber = snapshot.PlayerOneRef == localPlayer ? 1 :
                snapshot.PlayerTwoRef == localPlayer ? 2 : 0;
        }

        private int GetPlayerNumber(PlayerRef player)
        {
            var index = _players.IndexOf(player);
            return index >= 0 ? index + 1 : 0;
        }

        private void Send(PlayerRef player, WireMessage message)
        {
            if (!player.IsRealPlayer)
            {
                Debug.LogError(
                    $"Cannot send {message.Type}: target PlayerRef {player.RawEncoded} is not a real player");
                return;
            }

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            var key = ReliableKey.FromInts(
                _runner.LocalPlayer.RawEncoded,
                ++_nextMessageId,
                message.Type == ThrowMessageType ? 1 :
                    message.Type == SnapshotMessageType ? 2 : 3,
                0);
            _runner.SendReliableDataToPlayer(player, key, data);
        }

        public GameSnapshot CreateSnapshotWithPlayerRefs(GameSnapshot snapshot)
        {
            if (_players.Count == 2)
            {
                snapshot.AuthorityRef = _runner.LocalPlayer.RawEncoded;
                snapshot.PlayerOneRef = _players[0].RawEncoded;
                snapshot.PlayerTwoRef = _players[1].RawEncoded;
            }

            return snapshot;
        }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) => StatusChanged?.Invoke($"Connection failed: {reason}");
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) => StatusChanged?.Invoke($"Disconnected: {reason}");
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) => StatusChanged?.Invoke($"Session ended: {shutdownReason}");
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        [Serializable]
        private class WireMessage
        {
            public string Type;
            public int SenderRef;
            public int Option;
            public ThrowResult Result;
            public GameSnapshot Snapshot;
        }
    }

    [Serializable]
    public class GameSnapshot
    {
        public int AuthorityRef;
        public int PlayerOneRef;
        public int PlayerTwoRef;
        public int RoundNumber;
        public int ActivePlayer;
        public int PlayerOneThrows;
        public int PlayerTwoThrows;
        public int PlayerOneRoundPoints;
        public int PlayerTwoRoundPoints;
        public int PlayerOneTotalPoints;
        public int PlayerTwoTotalPoints;
        public bool RoundComplete;
        public bool GameComplete;
        public TaskObject Task;
    }
}