using System.Collections;
using System.Text;
using DemonKing.Domain.Quests;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// 受注済みQuestの進捗を常設表示し、受注・進捗・報告可能・完了の状態遷移を非モーダル通知で提示します。
    /// Questの進行ルールは持たず、QuestProgressionServiceの状態とイベントだけを表示します。
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
        private const float NotificationDurationSeconds = 2.5f;

        private QuestProgressionService questService;
        private Font uiFont;
        private GameObject trackerPanel;
        private GameObject notificationPanel;
        private Text statusText;
        private Text titleText;
        private Text objectiveText;
        private Text notificationText;
        private string displayedQuestId = string.Empty;
        private Coroutine hideNotificationCoroutine;

        public bool IsVisible => trackerPanel != null && trackerPanel.activeSelf;
        public bool IsNotificationVisible => notificationPanel != null && notificationPanel.activeSelf;
        public string DisplayedStatusText => statusText == null ? string.Empty : statusText.text;
        public string DisplayedQuestTitle => titleText == null ? string.Empty : titleText.text;
        public string DisplayedObjectiveText => objectiveText == null ? string.Empty : objectiveText.text;
        public string DisplayedNotificationText => notificationText == null ? string.Empty : notificationText.text;

        public void Initialize(Font font, QuestProgressionService service)
        {
            Unbind();
            uiFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            questService = service;

            BuildHierarchy();

            if (questService != null)
            {
                questService.QuestAccepted += HandleQuestAccepted;
                questService.ProgressChanged += HandleProgressChanged;
                questService.QuestReadyToTurnIn += HandleQuestReadyToTurnIn;
                questService.QuestCompleted += HandleQuestCompleted;
                displayedQuestId = ResolveInitialDisplayedQuestId();
            }

            Refresh();
            HideNotification();
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
                questService = null;
            }

            if (hideNotificationCoroutine != null)
            {
                StopCoroutine(hideNotificationCoroutine);
                hideNotificationCoroutine = null;
            }
        }

        private void HandleQuestAccepted(QuestProgressState state)
        {
            displayedQuestId = state.QuestId;
            Refresh();
            ShowNotification($"クエスト受注\n{ResolveQuestName(state.QuestId)}");
        }

        private void HandleProgressChanged(QuestProgressUpdate update)
        {
            displayedQuestId = update.QuestId;
            Refresh();

            if (update.QuestReadyToTurnIn)
            {
                return;
            }

            string objectiveName = ResolveObjectiveName(update.QuestId, update.ObjectiveId);
            ShowNotification($"クエスト進捗\n{objectiveName}  {update.CurrentCount}");
        }

        private void HandleQuestReadyToTurnIn(QuestProgressState state)
        {
            displayedQuestId = state.QuestId;
            Refresh();
            ShowNotification($"目標達成\n{ResolveQuestName(state.QuestId)}\n見習い魔術師に報告しよう");
        }

        private void HandleQuestCompleted(QuestProgressState state)
        {
            displayedQuestId = state.QuestId;
            Refresh();
            ShowNotification($"クエスト完了\n{ResolveQuestName(state.QuestId)}");
        }

        private void Refresh()
        {
            if (trackerPanel == null || statusText == null || titleText == null || objectiveText == null)
            {
                return;
            }

            if (questService == null ||
                string.IsNullOrWhiteSpace(displayedQuestId) ||
                !questService.TryGetState(displayedQuestId, out QuestProgressState state) ||
                !state.IsAccepted ||
                !questService.TryGetDefinition(displayedQuestId, out QuestDefinition definition))
            {
                trackerPanel.SetActive(false);
                statusText.text = string.Empty;
                titleText.text = string.Empty;
                objectiveText.text = string.Empty;
                return;
            }

            trackerPanel.SetActive(true);
            if (state.IsCompleted)
            {
                statusText.text = "完了";
                statusText.color = CompletedColor;
            }
            else if (state.IsReadyToTurnIn)
            {
                statusText.text = "報告可能";
                statusText.color = ReadyColor;
            }
            else
            {
                statusText.text = "受注中";
                statusText.color = ActiveColor;
            }

            titleText.text = definition.DisplayName;
            objectiveText.text = BuildObjectiveText(definition, state);
        }

        private string ResolveInitialDisplayedQuestId()
        {
            string readyQuestId = string.Empty;
            string completedQuestId = string.Empty;
            foreach (QuestProgressState state in questService.States)
            {
                if (state.IsActive)
                {
                    return state.QuestId;
                }

                if (state.IsReadyToTurnIn && string.IsNullOrWhiteSpace(readyQuestId))
                {
                    readyQuestId = state.QuestId;
                }

                if (state.IsCompleted && string.IsNullOrWhiteSpace(completedQuestId))
                {
                    completedQuestId = state.QuestId;
                }
            }

            return !string.IsNullOrWhiteSpace(readyQuestId) ? readyQuestId : completedQuestId;
        }

        private string ResolveQuestName(string questId)
        {
            return questService != null && questService.TryGetDefinition(questId, out QuestDefinition definition)
                ? definition.DisplayName
                : questId;
        }

        private string ResolveObjectiveName(string questId, string objectiveId)
        {
            if (questService != null && questService.TryGetDefinition(questId, out QuestDefinition definition))
            {
                foreach (QuestObjectiveDefinition objective in definition.Objectives)
                {
                    if (objective.ObjectiveId == objectiveId)
                    {
                        return objective.DisplayName;
                    }
                }
            }

            return objectiveId;
        }

        private static string BuildObjectiveText(QuestDefinition definition, QuestProgressState state)
        {
            var builder = new StringBuilder();
            foreach (QuestObjectiveDefinition objectiveDefinition in definition.Objectives)
            {
                if (!state.TryGetObjective(objectiveDefinition.ObjectiveId, out ObjectiveProgressState objectiveState))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(objectiveState.IsCompleted ? "✓ " : "□ ");
                builder.Append(objectiveDefinition.DisplayName);
                builder.Append("  ");
                builder.Append(objectiveState.CurrentCount);
                builder.Append('/');
                builder.Append(objectiveState.RequiredCount);
            }

            return builder.ToString();
        }

        private void ShowNotification(string message)
        {
            if (notificationPanel == null || notificationText == null)
            {
                return;
            }

            if (hideNotificationCoroutine != null)
            {
                StopCoroutine(hideNotificationCoroutine);
            }

            notificationText.text = message;
            notificationPanel.SetActive(true);
            hideNotificationCoroutine = StartCoroutine(HideNotificationAfterDelay());
        }

        private IEnumerator HideNotificationAfterDelay()
        {
            yield return new WaitForSecondsRealtime(NotificationDurationSeconds);
            HideNotification();
        }

        private void HideNotification()
        {
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }

            hideNotificationCoroutine = null;
        }

        private void BuildHierarchy()
        {
            BuildTrackerPanel();
            BuildNotificationPanel();
        }

        private void BuildTrackerPanel()
        {
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
                "Quest Status",
                panel,
                string.Empty,
                15,
                ActiveColor,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-18f, -8f),
                new Vector2(-36f, 26f));

            titleText = CreateText(
                "Quest Title",
                panel,
                string.Empty,
                22,
                TextColor,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(18f, -38f),
                new Vector2(-36f, 38f));

            objectiveText = CreateText(
                "Quest Objectives",
                panel,
                string.Empty,
                17,
                TextColor,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                new Vector2(18f, -32f),
                new Vector2(-36f, -82f));
            objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
            objectiveText.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private void BuildNotificationPanel()
        {
            RectTransform panel = CreatePanel(
                "Quest Notification",
                transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -28f),
                new Vector2(520f, 110f));
            notificationPanel = panel.gameObject;

            notificationText = CreateText(
                "Quest Notification Text",
                panel,
                string.Empty,
                19,
                ActiveColor,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(-24f, -12f));
        }

        private RectTransform CreatePanel(
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
            string value,
            int fontSize,
            Color color,
            FontStyle fontStyle,
            TextAnchor alignment,
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

            Text text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = uiFont;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }
    }
}
