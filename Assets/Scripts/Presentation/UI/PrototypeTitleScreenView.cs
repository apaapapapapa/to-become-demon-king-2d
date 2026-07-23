using System;
using System.Collections.Generic;
using DemonKing.Core.Application;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// P0 Title Screenの表示だけを担当します。
    /// Slot選択やゲーム開始判断はController側に置き、ViewはSave RuntimeやGame Sessionを参照しません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class PrototypeTitleScreenView : MonoBehaviour
    {
        private static readonly Color BackgroundColor = new(0.018f, 0.028f, 0.032f, 1f);
        private static readonly Color PanelColor = new(0.035f, 0.075f, 0.085f, 0.96f);
        private static readonly Color TitleColor = new(1f, 0.86f, 0.58f, 1f);
        private static readonly Color NormalColor = new(0.90f, 0.94f, 0.86f, 1f);
        private static readonly Color SelectedColor = new(1f, 0.64f, 0.32f, 1f);
        private static readonly Color DisabledColor = new(0.42f, 0.48f, 0.45f, 1f);
        private static readonly Color ErrorColor = new(1f, 0.56f, 0.48f, 1f);

        private readonly Text[] mainMenuTexts = new Text[5];
        private readonly Text[] slotTexts = new Text[LocalSaveSlotStore.SlotCount];

        private Font uiFont;
        private GameObject mainMenuRoot;
        private GameObject slotMenuRoot;
        private Text slotHeaderText;
        private Text statusText;
        private bool initialized;

        public void Initialize(Font font)
        {
            if (initialized)
            {
                return;
            }

            uiFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildHierarchy();
            initialized = true;
        }

        public void ShowMainMenu(int selectedIndex, bool continueAvailable, string message)
        {
            EnsureInitialized();
            mainMenuRoot.SetActive(true);
            slotMenuRoot.SetActive(false);

            string[] labels = { "NEW GAME", "CONTINUE", "LOAD GAME", "SETTINGS", "QUIT" };
            for (int index = 0; index < mainMenuTexts.Length; index++)
            {
                bool selected = index == selectedIndex;
                bool enabled = index != 1 || continueAvailable;
                mainMenuTexts[index].text = $"{(selected ? "▶ " : "  ")}{labels[index]}";
                mainMenuTexts[index].color = !enabled
                    ? DisabledColor
                    : selected ? SelectedColor : NormalColor;
            }

            SetStatus(message, isError: false);
        }

        public void ShowSlotMenu(
            string header,
            int selectedIndex,
            IReadOnlyList<SaveSlotMetadata> metadata,
            bool newGameMode,
            string message,
            bool messageIsError)
        {
            EnsureInitialized();
            mainMenuRoot.SetActive(false);
            slotMenuRoot.SetActive(true);
            slotHeaderText.text = header ?? string.Empty;

            for (int index = 0; index < slotTexts.Length; index++)
            {
                SaveSlotMetadata slot = metadata != null && index < metadata.Count
                    ? metadata[index]
                    : null;
                bool selected = index == selectedIndex;
                string summary = FormatSlotSummary(slot);
                slotTexts[index].text = $"{(selected ? "▶ " : "  ")}{summary}";

                bool selectable = slot != null &&
                    (newGameMode
                        ? slot.Status == SaveSlotStatus.Empty
                        : slot.Status == SaveSlotStatus.Ready);
                slotTexts[index].color = !selectable
                    ? DisabledColor
                    : selected ? SelectedColor : NormalColor;
            }

            SetStatus(message, messageIsError);
        }

        private void BuildHierarchy()
        {
            RectTransform root = GetComponent<RectTransform>();
            if (root != null)
            {
                StretchToParent(root);
            }

            Image background = gameObject.AddComponent<Image>();
            background.color = BackgroundColor;
            background.raycastTarget = false;

            CreateText(
                "Game Title",
                transform,
                "TO BECOME DEMON KING",
                52,
                TitleColor,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -115f),
                new Vector2(900f, 80f));

            CreateText(
                "Game Subtitle",
                transform,
                "弱き魔物は、人を信じるのか。魔王になるのか。",
                20,
                NormalColor,
                FontStyle.Normal,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -190f),
                new Vector2(900f, 48f));

            RectTransform panel = CreateRect("Title Panel", transform);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = new Vector2(0f, -35f);
            panel.sizeDelta = new Vector2(880f, 570f);
            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = PanelColor;
            panelImage.raycastTarget = false;

            mainMenuRoot = CreateRect("Main Menu", panel).gameObject;
            StretchToParent(mainMenuRoot.GetComponent<RectTransform>());
            for (int index = 0; index < mainMenuTexts.Length; index++)
            {
                mainMenuTexts[index] = CreateText(
                    $"Main Menu {index}",
                    mainMenuRoot.transform,
                    string.Empty,
                    30,
                    NormalColor,
                    FontStyle.Bold,
                    TextAnchor.MiddleLeft,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(-115f, 145f - (index * 70f)),
                    new Vector2(470f, 58f));
            }

            slotMenuRoot = CreateRect("Slot Menu", panel).gameObject;
            StretchToParent(slotMenuRoot.GetComponent<RectTransform>());
            slotHeaderText = CreateText(
                "Slot Header",
                slotMenuRoot.transform,
                string.Empty,
                30,
                TitleColor,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -34f),
                new Vector2(-90f, 62f));

            for (int index = 0; index < slotTexts.Length; index++)
            {
                slotTexts[index] = CreateText(
                    $"Save Slot {index + 1}",
                    slotMenuRoot.transform,
                    string.Empty,
                    22,
                    NormalColor,
                    FontStyle.Normal,
                    TextAnchor.MiddleLeft,
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -118f - (index * 105f)),
                    new Vector2(-90f, 88f));
            }

            statusText = CreateText(
                "Status",
                panel,
                string.Empty,
                18,
                NormalColor,
                FontStyle.Normal,
                TextAnchor.MiddleCenter,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 52f),
                new Vector2(-90f, 70f));

            CreateText(
                "Input Hint",
                transform,
                "↑↓ / Left Stick : 選択     Enter / A : 決定     Esc / B : 戻る",
                17,
                NormalColor,
                FontStyle.Normal,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 34f),
                new Vector2(900f, 44f));
        }

        private static string FormatSlotSummary(SaveSlotMetadata metadata)
        {
            if (metadata == null)
            {
                return "SLOT ?   読み込み不可";
            }

            switch (metadata.Status)
            {
                case SaveSlotStatus.Empty:
                    return $"SLOT {metadata.SlotId.Value}   EMPTY";
                case SaveSlotStatus.Corrupted:
                    return $"SLOT {metadata.SlotId.Value}   SAVE DATA ERROR";
                case SaveSlotStatus.UnsupportedVersion:
                    return $"SLOT {metadata.SlotId.Value}   UNSUPPORTED SAVE v{metadata.SaveVersion}";
                case SaveSlotStatus.Ready:
                    string savedAt = metadata.LastSavedUtc.HasValue
                        ? metadata.LastSavedUtc.Value.ToLocalTime().ToString("yyyy/MM/dd HH:mm")
                        : "----/--/-- --:--";
                    TimeSpan playTime = TimeSpan.FromSeconds(metadata.PlayTimeSeconds);
                    return $"SLOT {metadata.SlotId.Value}   Lv.{metadata.Level}   " +
                           $"{metadata.CurrentFieldId}   {playTime.TotalHours:00}:{playTime.Minutes:00}:{playTime.Seconds:00}   {savedAt}";
                default:
                    return $"SLOT {metadata.SlotId.Value}   UNKNOWN";
            }
        }

        private void SetStatus(string message, bool isError)
        {
            statusText.text = message ?? string.Empty;
            statusText.color = isError ? ErrorColor : NormalColor;
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
            GameObject child = new(name, typeof(RectTransform));
            RectTransform rect = child.GetComponent<RectTransform>();
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

        private void EnsureInitialized()
        {
            if (!initialized)
            {
                throw new InvalidOperationException("PrototypeTitleScreenViewが初期化されていません。");
            }
        }
    }
}
