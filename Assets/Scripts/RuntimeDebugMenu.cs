using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    public class RuntimeDebugMenu : MonoBehaviour
    {
        private const int MaximumLogEntries = 500;

        private readonly ConcurrentQueue<LogEntry> _pendingEntries = new ConcurrentQueue<LogEntry>();
        private readonly List<LogEntry> _entries = new List<LogEntry>(MaximumLogEntries);

        private GameObject _panel;
        private TMP_Text _logText;
        private TMP_Text _toggleText;
        private ScrollRect _scrollRect;

        private void Awake()
        {
            BuildMenu();
        }

        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += OnLogReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= OnLogReceived;
        }

        private void Update()
        {
            var changed = false;
            while (_pendingEntries.TryDequeue(out var entry))
            {
                _entries.Add(entry);
                if (_entries.Count > MaximumLogEntries)
                {
                    _entries.RemoveAt(0);
                }

                changed = true;
            }

            if (!changed)
            {
                return;
            }

            RefreshText();
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        private void OnLogReceived(string condition, string stackTrace, LogType type)
        {
            _pendingEntries.Enqueue(new LogEntry
            {
                Time = DateTime.Now,
                Message = condition,
                StackTrace = stackTrace,
                Type = type
            });
        }

        private void BuildMenu()
        {
            var toggleButton = CreateButton("Debug Log Toggle", transform, "Hide logs", new Color(0.12f, 0.14f, 0.16f, 0.96f));
            var toggleRect = toggleButton.GetComponent<RectTransform>();
            SetTopRight(toggleRect, new Vector2(-20f, -20f), new Vector2(210f, 64f));
            _toggleText = toggleButton.GetComponentInChildren<TMP_Text>();
            toggleButton.onClick.AddListener(TogglePanel);

            _panel = CreateUiObject("Debug Log Panel", transform);
            var panelRect = _panel.GetComponent<RectTransform>();
            SetTopRight(panelRect, new Vector2(-20f, -94f), new Vector2(650f, 900f));
            var panelImage = _panel.AddComponent<Image>();
            panelImage.color = new Color(0.035f, 0.045f, 0.055f, 0.94f);

            var title = CreateText("Title", _panel.transform, "Runtime logs", 30f, TextAlignmentOptions.MidlineLeft);
            SetRect(title.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(20f, -68f), new Vector2(-180f, -8f));

            var clearButton = CreateButton("Clear Logs", _panel.transform, "Clear", new Color(0.25f, 0.28f, 0.31f, 1f));
            SetRect(clearButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), Vector2.one, new Vector2(-160f, -64f), new Vector2(-16f, -12f));
            clearButton.onClick.AddListener(ClearLogs);

            var viewport = CreateUiObject("Viewport", _panel.transform);
            var viewportRect = viewport.GetComponent<RectTransform>();
            SetRect(viewportRect, Vector2.zero, Vector2.one, new Vector2(16f, 16f), new Vector2(-16f, -78f));
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.025f);
            viewport.AddComponent<RectMask2D>();

            var content = CreateUiObject("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            _logText = content.AddComponent<TextMeshProUGUI>();
            _logText.fontSize = 23f;
            _logText.alignment = TextAlignmentOptions.TopLeft;
            _logText.textWrappingMode = TextWrappingModes.Normal;
            _logText.richText = true;
            _logText.raycastTarget = false;
            _logText.margin = new Vector4(12f, 10f, 12f, 10f);
            _logText.text = "Waiting for logs...";

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scrollRect = _panel.AddComponent<ScrollRect>();
            _scrollRect.content = contentRect;
            _scrollRect.viewport = viewportRect;
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.scrollSensitivity = 35f;
        }

        private void TogglePanel()
        {
            var show = !_panel.activeSelf;
            _panel.SetActive(show);
            _toggleText.text = show ? "Hide logs" : $"Logs ({_entries.Count})";
        }

        private void ClearLogs()
        {
            _entries.Clear();
            while (_pendingEntries.TryDequeue(out _)) { }
            _logText.text = "Logs cleared.";
            _toggleText.text = "Hide logs";
        }

        private void RefreshText()
        {
            var builder = new StringBuilder();
            foreach (var entry in _entries)
            {
                var color = GetColor(entry.Type);
                builder.Append("<color=").Append(color).Append('>')
                    .Append(entry.Time.ToString("HH:mm:ss.fff"))
                    .Append(" [").Append(entry.Type).Append("]</color> ")
                    .AppendLine(EscapeRichText(entry.Message));

                if ((entry.Type == LogType.Error || entry.Type == LogType.Exception || entry.Type == LogType.Assert) &&
                    !string.IsNullOrWhiteSpace(entry.StackTrace))
                {
                    builder.Append("<color=#AEB7C2>")
                        .Append(EscapeRichText(entry.StackTrace))
                        .AppendLine("</color>");
                }
            }

            _logText.text = builder.ToString();
            if (!_panel.activeSelf)
            {
                _toggleText.text = $"Logs ({_entries.Count})";
            }
        }

        private static string GetColor(LogType type)
        {
            return type switch
            {
                LogType.Warning => "#F4C95D",
                LogType.Error => "#FF6B6B",
                LogType.Exception => "#FF6B6B",
                LogType.Assert => "#FF8C69",
                _ => "#75D6A4"
            };
        }

        private static string EscapeRichText(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private GameObject CreateUiObject(string name, Transform parent)
        {
            var uiObject = new GameObject(name, typeof(RectTransform));
            uiObject.layer = gameObject.layer;
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private Button CreateButton(string name, Transform parent, string label, Color color)
        {
            var buttonObject = CreateUiObject(name, parent);
            var image = buttonObject.AddComponent<Image>();
            image.color = color;
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            var text = CreateText("Label", buttonObject.transform, label, 25f, TextAlignmentOptions.Center);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            var textObject = CreateUiObject(name, parent);
            var textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.alignment = alignment;
            textComponent.color = Color.white;
            textComponent.raycastTarget = false;
            return textComponent;
        }

        private static void SetTopRight(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private struct LogEntry
        {
            public DateTime Time;
            public string Message;
            public string StackTrace;
            public LogType Type;
        }
    }
}