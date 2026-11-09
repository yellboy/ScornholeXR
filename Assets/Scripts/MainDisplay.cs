using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class MainDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _roundPointsText;
        [SerializeField] private TMP_Text _totalPointsText;
        [SerializeField] private TMP_Text _queryText;
        [SerializeField] private TMP_Text _answerText;

        [SerializeField] private GameObject _roundPanel;
        [SerializeField] private GameObject _roundFinishedPanel;

        private void Awake()
        {
            if (GetComponent<RuntimeDebugMenu>() == null)
            {
                gameObject.AddComponent<RuntimeDebugMenu>();
            }
        }

        public void ShowStatus(string status)
        {
            _roundPanel.SetActive(true);
            _roundFinishedPanel.SetActive(false);
            _queryText.text = status;
            _roundPointsText.text = string.Empty;
        }

        public void ShowGameState(GameSnapshot snapshot, int localPlayer)
        {
            ShowTask(snapshot.Task);
            _roundPointsText.text =
                $"Round {snapshot.RoundNumber}/5\n" +
                $"You are Player {localPlayer}\n" +
                $"Player {snapshot.ActivePlayer}'s turn\n" +
                $"Round score: P1 {snapshot.PlayerOneRoundPoints} - P2 {snapshot.PlayerTwoRoundPoints}";
            UpdateScoreboard(snapshot);
        }

        public void ShowRoundResults(GameSnapshot snapshot)
        {
            ShowAnswer(snapshot.Task);
            _roundPointsText.text =
                $"Round {snapshot.RoundNumber} complete\n" +
                $"Player 1: {snapshot.PlayerOneRoundPoints} points\n" +
                $"Player 2: {snapshot.PlayerTwoRoundPoints} points";
            UpdateScoreboard(snapshot);
        }

        public void ShowGameResults(GameSnapshot snapshot)
        {
            ShowRoundResults(snapshot);

            var result = snapshot.PlayerOneTotalPoints == snapshot.PlayerTwoTotalPoints
                ? "The game is a tie!"
                : $"Player {(snapshot.PlayerOneTotalPoints > snapshot.PlayerTwoTotalPoints ? 1 : 2)} wins!";

            _roundPointsText.text = $"Game complete\n{result}";
        }

        private void UpdateScoreboard(GameSnapshot snapshot)
        {
            _totalPointsText.text =
                $"Total: P1 {snapshot.PlayerOneTotalPoints} - P2 {snapshot.PlayerTwoTotalPoints}\n" +
                $"Throws: P1 {snapshot.PlayerOneThrows}/5 - P2 {snapshot.PlayerTwoThrows}/5";
        }

        public void ShowRoundPoints(int points)
        {
            _roundPanel.SetActive(false);
            _roundFinishedPanel.SetActive(true);
            _roundPointsText.text = $"Round Points: {points}";
        }

        public void ShowAnswer(TaskObject task)
        {
            _roundPanel.SetActive(false);
            _roundFinishedPanel.SetActive(true);

            var answerBuilder = new StringBuilder();
            for (var i = 0; i < task.Rank.Length; i++)
            {
                var optionRank = task.Rank[i];
                var points = 5 - i;
                var option = optionRank switch
                {
                    1 => task.Option1,
                    2 => task.Option2,
                    3 => task.Option3,
                    4 => task.Option4,
                    5 => task.Option5
                };

                answerBuilder.AppendLine($"{points} points - {option}");
            }

            _answerText.text = answerBuilder.ToString();
        }

        public void UpdateTotalPoints(int points)
        {
            _totalPointsText.text = $"Total Points: {points}";
        }

        public void ShowTask(TaskObject task)
        {
            _roundPanel.SetActive(true);
            _roundFinishedPanel.SetActive(false);

            _roundPointsText.text = "Round Points: 0";

            _queryText.text =
@$"{task.Question}
1. {task.Option1}
2. {task.Option2}
3. {task.Option3}
4. {task.Option4}
5. {task.Option5}";
        }
    }
}
