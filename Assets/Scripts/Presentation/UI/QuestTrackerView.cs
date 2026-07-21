using DemonKing.Domain.Quests;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// 選択されたQuestの表示Modelを常設Trackerへ反映します。
    /// 表示ポリシー、通知寿命、Quest進行ルールは担当しません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class QuestTrackerView : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.05f, 0.075f, 0.12f, 0.92f);
        private static readonly Color AccentColor = new(0.93f, 0.72f, 0.28f, 0.98f);
        private static readonly Color ActiveColor = new(0.98f, 0.82f, 0.42f, 1f);
        private static readonly Color ReadyColor = new(0.45f, 0.82f, 1f, 1f);
        private static readonly Color CompletedColor = new(0.52f, 0.92f, 0.62f, 1f);
        private static readonly Color TextColor = new(0.95f, 0.95f, 0.88f, 1f);

        private QuestProgressionService questService;
        private QuestNotificationView notificationView;
        private Font uiFont;
        private GameObject trackerPanel;
        private Text statusText;
        private Text titleText;
        private Text objectiveText;
        private string displayedQuestId = string.Empty;

        public bool IsVisible => trackerPanel != null && trackerPanel.activeSelf;
        public string DisplayedStatusText => statusText == null ? string.Empty : statusText.text;
        public string DisplayedQuestTitle => titleText == null ? string.Empty : titleText.text;
        public string DisplayedObjectiveText => objectiveText == null ? string.Empty : objectiveText.text;
        public string DisplayedQuestId => displayedQuestId;

        public void Initialize(
            Font font,
            QuestProgressionService service,
            QuestNotificationView notifications)
        {
            Unbind();
            uiFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            questService = service;
            notificationView = notifications;
            BuildHierarchy();

            if (questService != null)
            {
                questService.QuestAccepted += HandleQuestAccepted;
                questService.ProgressChanged += HandleProgressChanged;
                questService.QuestReadyToTurnIn += HandleQuestReadyToTurnIn;
                questService.QuestCompleted += HandleQuestCompleted;
                displayedQuestId = QuestTrackerSelector.SelectInitialQuestId(questService.States);
            }

            Refresh();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (questService != null)
            {
                questService.QuestAccepted -= HandleQuestAccepted;
                questService.ProgressChanged -= HandleProgressChanged;
                questService.QuestReadyToTurnIn -= HandleQuestReadyToTurnIn;
                questService.QuestCompleted -= HandleQuestCompleted;
            }

            questService = null;
            notificationView = null;
        }

        private void HandleQuestAccepted(QuestProgressState state)
        {
            displayedQuestId = state.QuestId;
            Refresh();
            notificationView?.Show(QuestNotificationFormatter.Accepted(
                GetDefinition(state.QuestId),
                state.QuestId));
        }

        private void HandleProgressChanged(QuestProgressUpdate update)
        {
            displayedQuestId = update.QuestId;
            Refresh();
            if (!update.QuestReadyToTurnIn)
            {
                notificationView?.Show(QuestNotificationFormatter.Progress(
                    GetDefinition(update.QuestId),
                    update));
            }
        }

        private void HandleQuestReadyToTurnIn(QuestProgressState state)
        {
            displayedQuestId = state.QuestId;
            Refresh();
            notificationView?.Show(QuestNotificationFormatter.ReadyToTurnIn(
                GetDefinition(state.QuestId),
                state.QuestId));
        }

        private void HandleQuestCompleted(QuestProgressState state)
        {
            displayedQuestId = state.QuestId;
            Refresh();
            notificationView?.Show(QuestNotificationFormatter.Completed(
                GetDefinition(state.QuestId),
                state.QuestId));
        }

        private QuestDefinition GetDefinition(string questId)
        {
            return questService != null &&
                   questService.TryGetDefinition(questId, out QuestDefinition definition)
                ? definition
                : null;
        }

        private void Refresh()
        {
            if (trackerPanel == null || statusText == null ||
                titleText == null || objectiveText == null)
            {
                return;
            }

            if (questService == null ||
                string.IsNullOrWhiteSpace(displayedQuestId) ||
                !questService.TryGetState(displayedQuestId, out QuestProgressState state) ||
                !questService.TryGetDefinition(displayedQuestId, out QuestDefinition definition) ||
                !QuestTrackerProjection.TryCreate(definition, state, out QuestTrackerDisplayModel model))
            {
                HideTracker();
                return;
            }

            trackerPanel.SetActive(true);
            statusText.text = model.StatusText;
            statusText.color = model.Status switch
            {
                QuestTrackerDisplayStatus.ReadyToTurnIn => ReadyColor,
                QuestTrackerDisplayStatus.Completed => CompletedColor,
                _ => ActiveColor,
            };
            titleText.text = model.Title;
            objectiveText.text = model.ObjectiveText;
        }

        private void HideTracker()
        {
            trackerPanel.SetActive(false);
            statusText.text = string.Empty;
            titleText.text = string.Empty;
            objectiveText.text = string.Empty;
        }

        private void BuildHierarchy()
        {
            if (trackerPanel != null)
            {
                Destroy(trackerPanel);
            }

            RectTransform panel = CreatePanel(
                "Quest Tracker",
                transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-22f, -22f),
                new Vector2(460f, 190f));
            trackerPanel = panel.gameObject;

            RectTransform accent = CreateRect("Quest Accent", panel);
            accent.anchorMin = new Vector2(0f, 0f);
            accent.anchorMax = new Vector2(0f, 1f);
            accent.pivot = new Vector2(0f, 0.5f);
            accent.sizeDelta = new Vector2(5f, 0f);
            Image accentImage = accent.gameObject.AddComponent<Image>();
            accentImage.color = AccentColor;
            accentImage.raycastTarget = false;

            statusText = CreateText(
                "Quest Status", panel, 15, ActiveColor, FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-18f, -8f), new Vector2(-36f, 26f));

            titleText = CreateText(
                "Quest Title", panel, 22, TextColor, FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(18f, -38f), new Vector2(-36f, 38f));

            objectiveText = CreateText(
                "Quest Objectives", panel, 17, TextColor, FontStyle.Normal,
                TextAnchor.UpperLeft,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                new Vector2(18f, -32f), new Vector2(-36f, -82f));
            objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
            objectiveText.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject child = new(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static RectTransform CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = PanelColor;
            image.raycastTarget = false;
            return rect;
        }

        private Text CreateText(
            string name,
            Transform parent,
            int fontSize,
            Color color,
            FontStyle fontStyle,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Text text = rect.gameObject.AddComponent<Text>();
            text.font = uiFont;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }
    }
}
