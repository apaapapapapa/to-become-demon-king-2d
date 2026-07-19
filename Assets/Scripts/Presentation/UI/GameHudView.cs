using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// プレイ中の常設HUDをCanvas（uGUI）で表示します。
    /// ゲームルールは持たず、画面上の表示階層と見た目だけを担当します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class GameHudView : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.045f, 0.10f, 0.12f, 0.88f);
        private static readonly Color AccentColor = new(0.93f, 0.57f, 0.31f, 0.95f);
        private static readonly Color TitleColor = new(1f, 0.89f, 0.66f, 1f);
        private static readonly Color SubtitleColor = new(0.72f, 0.86f, 0.72f, 1f);
        private static readonly Color HintColor = new(0.91f, 0.94f, 0.84f, 1f);

        private Font uiFont;

        private void Awake()
        {
            BuildHierarchy();
        }

        private void BuildHierarchy()
        {
            if (transform.Find("HUD") != null)
            {
                return;
            }

            uiFont = LoadUiFont();

            RectTransform hudRoot = CreateRect("HUD", transform);
            StretchToParent(hudRoot);

            BuildLocationPanel(hudRoot);
            BuildControlsPanel(hudRoot);
        }

        private void BuildLocationPanel(Transform parent)
        {
            RectTransform panel = CreatePanel(
                "Location Panel",
                parent,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                pivot: new Vector2(0f, 1f),
                anchoredPosition: new Vector2(22f, -22f),
                size: new Vector2(320f, 82f));

            RectTransform accent = CreateRect("Accent", panel);
            accent.anchorMin = new Vector2(0f, 0f);
            accent.anchorMax = new Vector2(0f, 1f);
            accent.pivot = new Vector2(0f, 0.5f);
            accent.anchoredPosition = Vector2.zero;
            accent.sizeDelta = new Vector2(5f, 0f);
            Image accentImage = accent.gameObject.AddComponent<Image>();
            accentImage.color = AccentColor;
            accentImage.raycastTarget = false;

            CreateText(
                "Location Title",
                panel,
                "夕映えの学園草原",
                fontSize: 22,
                color: TitleColor,
                fontStyle: FontStyle.Bold,
                alignment: TextAnchor.MiddleLeft,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                pivot: new Vector2(0.5f, 1f),
                anchoredPosition: new Vector2(13f, -8f),
                size: new Vector2(-26f, 36f));

            CreateText(
                "Location Subtitle",
                panel,
                "魔法学園・西の庭",
                fontSize: 14,
                color: SubtitleColor,
                fontStyle: FontStyle.Normal,
                alignment: TextAnchor.MiddleLeft,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 0f),
                pivot: new Vector2(0.5f, 0f),
                anchoredPosition: new Vector2(13f, 8f),
                size: new Vector2(-26f, 28f));
        }

        private void BuildControlsPanel(Transform parent)
        {
            RectTransform panel = CreatePanel(
                "Controls Panel",
                parent,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                pivot: new Vector2(0.5f, 0f),
                anchoredPosition: new Vector2(0f, 20f),
                size: new Vector2(860f, 44f));

            CreateText(
                "Controls Text",
                panel,
                "移動  WASD／矢印／左スティック    攻撃  J    話す・調べる  E    回避  Shift    ポーズ  Esc",
                fontSize: 16,
                color: HintColor,
                fontStyle: FontStyle.Normal,
                alignment: TextAnchor.MiddleCenter,
                anchorMin: Vector2.zero,
                anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f),
                anchoredPosition: Vector2.zero,
                size: Vector2.zero);
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

        private void CreateText(
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
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static Font LoadUiFont()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Font osFont = Font.CreateDynamicFontFromOSFont(
                new[]
                {
                    "Yu Gothic UI",
                    "Yu Gothic",
                    "Meiryo",
                    "Hiragino Sans",
                    "Noto Sans CJK JP",
                    "Arial"
                },
                18);
            if (osFont != null)
            {
                return osFont;
            }
#endif

            Font builtInFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (builtInFont == null)
            {
                Debug.LogWarning(
                    "uGUI用フォントを取得できませんでした。本番配布前に日本語対応フォントをプロジェクトアセットとして設定してください。");
            }

            return builtInFont;
        }
    }
}
