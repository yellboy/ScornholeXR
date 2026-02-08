using UnityEngine;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private TaskFetcher _taskFetcher;
        [SerializeField] private ScoreDisplay _scoreDisplay;
        [SerializeField] private CornbagsController _cornbagsController;

        private Round _currentRound;
        private int _totalPoints = 0;

        private void OnEnable()
        {
            _taskFetcher.TaskReceived += OnTaskReceived;
            _cornbagsController.Thrown += OnCornbagThrown;
        }

        private void OnDisable()
        {
            _taskFetcher.TaskReceived -= OnTaskReceived;
            _cornbagsController.Thrown -= OnCornbagThrown;
        }

        private void OnTaskReceived(TaskObject taskObject)
        {
            _currentRound = new Round(taskObject);
        }

        private void OnCornbagThrown(Cornbag cornbag)
        {
            if (cornbag.Result == null)
            {
                Debug.LogWarning("Cornbag thrown without result");
                return;
            }

            _currentRound.ThrowMade(cornbag.Index, cornbag.Result.Value);
            EndRoundIfNeeded();
        }

        private void EndRoundIfNeeded()
        {
            if (_currentRound.ThrowCount < 5)
            {
                return;
            }

            _totalPoints += _currentRound.Points;
            _scoreDisplay.UpdateRoundPoints((_currentRound.Points));
            _scoreDisplay.UpdateTotalPoints(_totalPoints);
            _cornbagsController.Reset();
            _taskFetcher.FetchTask();
        }
    }
}
