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

        [SerializeField] private string _roomName = "scornhole-main";

        private readonly List<PlayerRef> _players = new List<PlayerRef>(2);
        private NetworkRunner _runner;
        private PlayerRef _authorityPlayer = PlayerRef.None;

        public event Action SessionReady;
        public event Action<int, int, ThrowResult> ThrowReceived;
        public event Action<GameSnapshot> SnapshotReceived;
        public event Action<string> StatusChanged;

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

        public void SubmitThrow(int option, ThrowResult result)
        {
            if (!IsReady || LocalPlayerNumber == 0)
            {
                return;
            }

            if (IsAuthority)
            {
                ThrowReceived?.Invoke(LocalPlayerNumber, option, result);
                return;
            }

            Send(_authorityPlayer, new WireMessage
            {
                Type = ThrowMessageType,
                Option = option,
                Result = result
            });
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
                ThrowReceived?.Invoke(GetPlayerNumber(player), message.Option, message.Result);
                return;
            }

            if (message.Type == SnapshotMessageType && !IsAuthority && message.Snapshot != null)
            {
                _authorityPlayer = player;
                ApplyPlayerAssignment(message.Snapshot);
                IsReady = true;
                SnapshotReceived?.Invoke(message.Snapshot);
            }
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
                return;
            }

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _runner.SendReliableDataToPlayer(player, default, data);
        }

        public GameSnapshot CreateSnapshotWithPlayerRefs(GameSnapshot snapshot)
        {
            if (_players.Count == 2)
            {
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
            public int Option;
            public ThrowResult Result;
            public GameSnapshot Snapshot;
        }
    }

    [Serializable]
    public class GameSnapshot
    {
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