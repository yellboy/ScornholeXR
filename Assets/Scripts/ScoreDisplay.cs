using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _roundPointsText;
        [SerializeField] private TMP_Text _totalPointsText;

        public void UpdateRoundPoints(int points)
        {
            _roundPointsText.text = $"Round Points: {points}";
        }

        public void UpdateTotalPoints(int points)
        {
            _totalPointsText.text = $"Total Points: {points}";
        }
    }
}
