using DemonKing.Core.Application;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// GamePauseControllerの状態を表示するuGUIベースのポーズ画面です。
    /// ポーズ状態の決定やTime.timeScale操作は行わず、表示だけを担当します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class PauseMenuView : MonoBehaviour
    {
        private static readonly Color OverlayColor = new(0.01f, 0.02f, 0.025f, 0.72f);
        private static readonly Color PanelColor = new(0.045f, 0.10f, 0.12f, 0.96f);
        private static readonly Color TitleColor = new(1f, 0.89f, 0.66f, 1f);
        private static readonly Color HintColor = new(0.91f, 0.94f, 0.84f, 1f);

        private GamePauseController pauseController;
        private Font uiFont;
        private GameObject pauseRoot;

        public void Initialize(Font font, GamePauseController controller)
        {
            Unbind();
            uiFont = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pauseController = controller;

            BuildHierarchy();

            if (pauseController != null)
            {
                pauseController.PauseStateChanged += HandlePauseStateChanged;
                SetVisible(pauseController.IsPaused);
            }
            else
            {
                SetVisible(false);
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (pauseController != null)
            {
                pauseController.PauseStateChanged -= HandlePauseStateChanged;
                pauseController = null;
            }
        }

        private void HandlePauseStateChanged(bool paused)
        {
            SetVisible(paused);
        }

        private void SetVisible(bool visible)
        {
            if (pauseRoot != null)
            {
                pauseRoot.SetActive(visible);
            }
        }

        private void BuildHierarchy()
        {
            Transform existing = transform.Find("Pause Menu");
            if (existing != null)
            {
                pauseRoot = existing.gameObject;
                return;
            }

            RectTransform overlay = CreateRect("Pause Menu", transform);
            StretchToParent(overlay);
            pauseRoot = overlay.gameObject;

            Image overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.color = OverlayColor;
            overlayImage.raycastTarget = true;

            RectTransform panel = CreateRect("Pause Panel", overlay);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(460f, 220f);

            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = PanelColor;
            panelImage.raycastTarget = false;

            CreateText(
                "Pause Title",
                panel,
                "ポーズ",
                fontSize: 34,
                fontStyle: FontStyle.Bold,
                color: TitleColor,
                anchoredPosition: new Vector2(0f, 35f),
                size: new Vector2(400f, 70f));

            CreateText(
                "Pause Hint",
                panel,
                "Esc／Start／キャンセル でゲームに戻る",
                fontSize: 18,
                fontStyle: FontStyle.Normal,
                color: HintColor,
                anchoredPosition: new Vector2(0f, -42f),
                size: new Vector2(410f, 52f));
        }

        private void CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            FontStyle fontStyle,
            Color color,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = uiFont;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = TextAnchor.MiddleCenter;
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
    }
}
