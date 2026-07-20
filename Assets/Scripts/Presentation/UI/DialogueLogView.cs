using System.Text;
using DemonKing.Gameplay.Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// セッション内の直近のNPC発言を、プレイを止めずにuGUIへ表示します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class DialogueLogView : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.035f, 0.07f, 0.09f, 0.92f);
        private static readonly Color AccentColor = new(0.42f, 0.80f, 0.74f, 0.95f);
        private static readonly Color TitleColor = new(0.75f, 0.95f, 0.88f, 1f);
        private static readonly Color TextColor = new(0.96f, 0.94f, 0.84f, 1f);

        private DialogueLog dialogueLog;
        private Font uiFont;
        private GameObject panelRoot;
        private Text logText;

        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;
        public string DisplayedText => logText == null ? string.Empty : logText.text;

        public void Initialize(Font font, DialogueLog log)
        {
            Unbind();
            uiFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dialogueLog = log;

            BuildHierarchy();

            if (dialogueLog != null)
            {
                dialogueLog.LineAdded += HandleLineAdded;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (dialogueLog != null)
            {
                dialogueLog.LineAdded -= HandleLineAdded;
                dialogueLog = null;
            }
        }

        private void HandleLineAdded(DialogueLine line)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (panelRoot == null || logText == null)
            {
                return;
            }

            bool hasLines = dialogueLog != null && dialogueLog.Lines.Count > 0;
            panelRoot.SetActive(hasLines);
            if (!hasLines)
            {
                logText.text = string.Empty;
                return;
            }

            var builder = new StringBuilder();
            foreach (DialogueLine line in dialogueLog.Lines)
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(line.Speaker);
                builder.Append("：");
                builder.Append(line.Text);
            }

            logText.text = builder.ToString();
        }

        private void BuildHierarchy()
        {
            Transform existing = transform.Find("Dialogue Log");
            if (existing != null)
            {
                panelRoot = existing.gameObject;
                logText = existing.Find("Dialogue Log Text")?.GetComponent<Text>();
                return;
            }

            RectTransform panel = CreateRect("Dialogue Log", transform);
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.zero;
            panel.pivot = Vector2.zero;
            panel.anchoredPosition = new Vector2(22f, 82f);
            panel.sizeDelta = new Vector2(760f, 220f);
            panelRoot = panel.gameObject;

            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = PanelColor;
            panelImage.raycastTarget = false;

            RectTransform accent = CreateRect("Dialogue Accent", panel);
            accent.anchorMin = new Vector2(0f, 0f);
            accent.anchorMax = new Vector2(0f, 1f);
            accent.pivot = new Vector2(0f, 0.5f);
            accent.anchoredPosition = Vector2.zero;
            accent.sizeDelta = new Vector2(5f, 0f);
            Image accentImage = accent.gameObject.AddComponent<Image>();
            accentImage.color = AccentColor;
            accentImage.raycastTarget = false;

            CreateText(
                "Dialogue Log Title",
                panel,
                "会話ログ",
                fontSize: 18,
                color: TitleColor,
                fontStyle: FontStyle.Bold,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                pivot: new Vector2(0.5f, 1f),
                anchoredPosition: new Vector2(16f, -8f),
                size: new Vector2(-32f, 32f));

            logText = CreateText(
                "Dialogue Log Text",
                panel,
                string.Empty,
                fontSize: 18,
                color: TextColor,
                fontStyle: FontStyle.Normal,
                anchorMin: Vector2.zero,
                anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f),
                anchoredPosition: new Vector2(16f, -18f),
                size: new Vector2(-32f, -54f));
            logText.alignment = TextAnchor.UpperLeft;
            logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            logText.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            Color color,
            FontStyle fontStyle,
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
            text.alignment = TextAnchor.MiddleLeft;
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
