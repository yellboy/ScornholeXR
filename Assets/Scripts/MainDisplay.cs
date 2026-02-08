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
