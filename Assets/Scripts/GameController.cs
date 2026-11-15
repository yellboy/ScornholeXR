using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
    {
        private const int ThrowsPerPlayer = 5;
        private const int TotalRounds = 5;
        private const float RoundResultsDuration = 5f;

        [SerializeField] private TaskFetcher _taskFetcher;
        [SerializeField] private MainDisplay _mainDisplay;
        [SerializeField] private CornbagsController _cornbagsController;

        private MultiplayerSession _multiplayerSession;
        private Round _playerOneRound;
        private Round _playerTwoRound;
        private int _roundNumber;
        private int _appliedRoundNumber;
        private int _activePlayer;
        private int _playerOneTotalPoints;
        private int _playerTwoTotalPoints;

        private void Awake()
        {
            _multiplayerSession = GetComponent<MultiplayerSession>();
            if (_multiplayerSession == null)
            {
                _multiplayerSession = gameObject.AddComponent<MultiplayerSession>();
            }
        }

        private void OnEnable()
        {
            _taskFetcher.TaskReceived += OnTaskReceived;
            _cornbagsController.Thrown += OnCornbagThrown;
            _multiplayerSession.SessionReady += OnSessionReady;
            _multiplayerSession.ThrowReceived += OnNetworkThrowReceived;
            _multiplayerSession.SnapshotReceived += ApplySnapshot;
            _multiplayerSession.StatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            _taskFetcher.TaskReceived -= OnTaskReceived;
            _cornbagsController.Thrown -= OnCornbagThrown;
            _multiplayerSession.SessionReady -= OnSessionReady;
            _multiplayerSession.ThrowReceived -= OnNetworkThrowReceived;
            _multiplayerSession.SnapshotReceived -= ApplySnapshot;
            _multiplayerSession.StatusChanged -= OnStatusChanged;
        }

        private void Start()
        {
            _cornbagsController.SetThrowingEnabled(false);
            _mainDisplay.ShowStatus("Connecting to multiplayer...");
        }

        private void OnSessionReady()
        {
            if (!_multiplayerSession.IsAuthority || _roundNumber != 0)
            {
                return;
            }

            _roundNumber = 1;
            BeginRound();
        }

        private void OnTaskReceived(TaskObject taskObject)
        {
            if (!_multiplayerSession.IsAuthority)
            {
                return;
            }

            _playerOneRound = new Round(taskObject);
            _playerTwoRound = new Round(taskObject);
            _activePlayer = 1;
            BroadcastSnapshot();
        }

        private void OnCornbagThrown(Cornbag cornbag)
        {
            if (cornbag.Result == null)
            {
                Debug.LogWarning("Cornbag thrown without result");
                return;
            }

            _multiplayerSession.SubmitThrow(cornbag.Index, cornbag.Result.Value);
            _cornbagsController.SetThrowingEnabled(false);
        }

        private void OnNetworkThrowReceived(int player, int option, ThrowResult result)
        {
            if (!_multiplayerSession.IsAuthority)
            {
                return;
            }

            if (player != _activePlayer || _playerOneRound == null || _playerTwoRound == null)
            {
                Debug.LogWarning(
                    $"Throw rejected: player={player}, active={_activePlayer}, roundReady={_playerOneRound != null && _playerTwoRound != null}");
                return;
            }

            var playerRound = player == 1 ? _playerOneRound : _playerTwoRound;
            if (playerRound.ThrowCount >= ThrowsPerPlayer)
            {
                Debug.LogWarning($"Throw rejected: player {player} already made {ThrowsPerPlayer} throws");
                return;
            }

            playerRound.ThrowMade(option, result);

            if (_playerOneRound.ThrowCount >= ThrowsPerPlayer &&
                _playerTwoRound.ThrowCount >= ThrowsPerPlayer)
            {
                EndRound();
                return;
            }

            var otherPlayer = player == 1 ? 2 : 1;
            var otherRound = otherPlayer == 1 ? _playerOneRound : _playerTwoRound;
            _activePlayer = otherRound.ThrowCount < ThrowsPerPlayer ? otherPlayer : player;
            BroadcastSnapshot();
        }

        public void StartRound()
        {
            if (_multiplayerSession.IsAuthority && _multiplayerSession.IsReady && _roundNumber == 0)
            {
                _roundNumber = 1;
                BeginRound();
            }
        }

        private void BeginRound()
        {
            _playerOneRound = null;
            _playerTwoRound = null;
            _activePlayer = 0;
            _cornbagsController.Reset();
            _mainDisplay.ShowStatus($"Loading round {_roundNumber}...");
            _taskFetcher.FetchTask();
        }

        private void EndRound()
        {
            _playerOneTotalPoints += _playerOneRound.Points;
            _playerTwoTotalPoints += _playerTwoRound.Points;
            _activePlayer = 0;

            var gameComplete = _roundNumber >= TotalRounds;
            BroadcastSnapshot(true, gameComplete);

            if (!gameComplete)
            {
                StartCoroutine(StartNextRoundAfterDelay());
            }
        }

        private IEnumerator StartNextRoundAfterDelay()
        {
            yield return new WaitForSeconds(RoundResultsDuration);
            _roundNumber++;
            BeginRound();
        }

        private void BroadcastSnapshot(bool roundComplete = false, bool gameComplete = false)
        {
            var snapshot = new GameSnapshot
            {
                RoundNumber = _roundNumber,
                ActivePlayer = _activePlayer,
                PlayerOneThrows = _playerOneRound?.ThrowCount ?? 0,
                PlayerTwoThrows = _playerTwoRound?.ThrowCount ?? 0,
                PlayerOneRoundPoints = _playerOneRound?.Points ?? 0,
                PlayerTwoRoundPoints = _playerTwoRound?.Points ?? 0,
                PlayerOneTotalPoints = _playerOneTotalPoints,
                PlayerTwoTotalPoints = _playerTwoTotalPoints,
                RoundComplete = roundComplete,
                GameComplete = gameComplete,
                Task = _playerOneRound?.Task
            };

            _multiplayerSession.BroadcastSnapshot(
                _multiplayerSession.CreateSnapshotWithPlayerRefs(snapshot));
        }

        private void ApplySnapshot(GameSnapshot snapshot)
        {
            _cornbagsController.StartNetworkedSession(_multiplayerSession.Runner);

            if (snapshot.RoundComplete)
            {
                _cornbagsController.Reset();
            }
            else if (snapshot.RoundNumber != _appliedRoundNumber)
            {
                _appliedRoundNumber = snapshot.RoundNumber;
                _cornbagsController.Reset();
            }

            var canThrow = !snapshot.RoundComplete &&
                snapshot.ActivePlayer == _multiplayerSession.LocalPlayerNumber;
            _cornbagsController.SetThrowingEnabled(canThrow);

            if (snapshot.GameComplete)
            {
                _mainDisplay.ShowGameResults(snapshot);
            }
            else if (snapshot.RoundComplete)
            {
                _mainDisplay.ShowRoundResults(snapshot);
            }
            else if (snapshot.Task != null)
            {
                _mainDisplay.ShowGameState(snapshot, _multiplayerSession.LocalPlayerNumber);
            }
        }

        private void OnStatusChanged(string status)
        {
            _cornbagsController.SetThrowingEnabled(false);
            _mainDisplay.ShowStatus(status);
        }
    }
}
