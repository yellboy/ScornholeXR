using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class GameMenu : MonoBehaviour
    {
        private const int MinimumRounds = 1;
        private const int MaximumRounds = 10;
        private const int MinimumThrowsPerRound = 1;
        private const int MaximumThrowsPerRound = 10;

        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _joinGameButton;
        [SerializeField] private Button _roundsDecreaseButton;
        [SerializeField] private Button _roundsIncreaseButton;
        [SerializeField] private Button _throwsDecreaseButton;
        [SerializeField] private Button _throwsIncreaseButton;
        [SerializeField] private Button _confirmStartButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _roundsValueText;
        [SerializeField] private TMP_Text _throwsValueText;
        [SerializeField] private TMP_Text _messageText;

        private int _rounds = 5;
        private int _throwsPerRound = 5;

        public event Action<int, int> StartGameRequested;
        public event Action JoinGameRequested;

        private void Awake()
        {
            _startGameButton.onClick.AddListener(ShowSettings);
            _joinGameButton.onClick.AddListener(OnJoinClicked);
            _roundsDecreaseButton.onClick.AddListener(() => ChangeRounds(-1));
            _roundsIncreaseButton.onClick.AddListener(() => ChangeRounds(1));
            _throwsDecreaseButton.onClick.AddListener(() => ChangeThrowsPerRound(-1));
            _throwsIncreaseButton.onClick.AddListener(() => ChangeThrowsPerRound(1));
            _confirmStartButton.onClick.AddListener(OnConfirmStartClicked);
            _backButton.onClick.AddListener(ShowMain);

            RefreshValues();
            ShowMain();
        }

        public void Show(string message = "")
        {
            gameObject.SetActive(true);
            ShowMain();
            _messageText.text = message;
        }

        private void ShowMain()
        {
            _mainPanel.SetActive(true);
            _settingsPanel.SetActive(false);
        }

        private void ShowSettings()
        {
            _messageText.text = string.Empty;
            _mainPanel.SetActive(false);
            _settingsPanel.SetActive(true);
        }

        private void OnJoinClicked()
        {
            gameObject.SetActive(false);
            JoinGameRequested?.Invoke();
        }

        private void OnConfirmStartClicked()
        {
            gameObject.SetActive(false);
            StartGameRequested?.Invoke(_rounds, _throwsPerRound);
        }

        private void ChangeRounds(int delta)
        {
            _rounds = Mathf.Clamp(_rounds + delta, MinimumRounds, MaximumRounds);
            RefreshValues();
        }

        private void ChangeThrowsPerRound(int delta)
        {
            _throwsPerRound = Mathf.Clamp(_throwsPerRound + delta, MinimumThrowsPerRound, MaximumThrowsPerRound);
            RefreshValues();
        }

        private void RefreshValues()
        {
            _roundsValueText.text = _rounds.ToString();
            _throwsValueText.text = _throwsPerRound.ToString();
        }
    }
}
