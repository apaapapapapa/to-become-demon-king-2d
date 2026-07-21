using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// Questの一時通知だけを表示し、Realtime基準の表示寿命を管理します。
    /// 常設Trackerの表示対象やQuest状態は保持しません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class QuestNotificationView : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.05f, 0.075f, 0.12f, 0.92f);
        private static readonly Color TextColor = new(0.98f, 0.82f, 0.42f, 1f);
        private const float DefaultDurationSeconds = 2.5f;

        private Font uiFont;
        private GameObject notificationPanel;
        private Text notificationText;
        private Coroutine hideCoroutine;
        private float durationSeconds = DefaultDurationSeconds;

        public bool IsVisible => notificationPanel != null && notificationPanel.activeSelf;
        public string DisplayedText => notificationText == null ? string.Empty : notificationText.text;

        public void Initialize(Font font, float displayDurationSeconds = DefaultDurationSeconds)
        {
            StopPendingHide();
            uiFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            durationSeconds = Mathf.Max(0f, displayDurationSeconds);
            BuildHierarchy();
            Hide();
        }

        public void Show(string message)
        {
            if (notificationPanel == null || notificationText == null)
            {
                return;
            }

            StopPendingHide();
            notificationText.text = message ?? string.Empty;
            notificationPanel.SetActive(true);
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        public void Hide()
        {
            StopPendingHide();
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            StopPendingHide();
        }

        private IEnumerator HideAfterDelay()
        {
            if (durationSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(durationSeconds);
            }

            hideCoroutine = null;
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }

        private void StopPendingHide()
        {
            if (hideCoroutine == null)
            {
                return;
            }

            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        private void BuildHierarchy()
        {
            if (notificationPanel != null)
            {
                Destroy(notificationPanel);
            }

            GameObject panelObject = new("Quest Notification", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(transform, false);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 1f);
            panel.anchorMax = new Vector2(0.5f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);
            panel.anchoredPosition = new Vector2(0f, -28f);
            panel.sizeDelta = new Vector2(520f, 110f);
            Image image = panelObject.GetComponent<Image>();
            image.color = PanelColor;
            image.raycastTarget = false;
            notificationPanel = panelObject;

            GameObject textObject = new("Quest Notification Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panel, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(12f, 6f);
            textRect.offsetMax = new Vector2(-12f, -6f);

            notificationText = textObject.GetComponent<Text>();
            notificationText.font = uiFont;
            notificationText.fontSize = 19;
            notificationText.fontStyle = FontStyle.Bold;
            notificationText.alignment = TextAnchor.MiddleCenter;
            notificationText.color = TextColor;
            notificationText.raycastTarget = false;
        }
    }
}
