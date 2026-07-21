using System.Text;
using DemonKing.Core.Input;
using DemonKing.Gameplay.Abilities;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// Ability Loadout選択状態をuGUIへ反映します。
    /// 候補生成、入力割当、Input Context制御はAbilityLoadoutSelectionControllerへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class AbilityLoadoutMenuView : MonoBehaviour
    {
        private static readonly Color OverlayColor = new(0.01f, 0.02f, 0.04f, 0.82f);
        private static readonly Color PanelColor = new(0.045f, 0.09f, 0.12f, 0.98f);
        private static readonly Color AccentColor = new(0.50f, 0.88f, 0.72f, 1f);
        private static readonly Color TitleColor = new(1f, 0.89f, 0.66f, 1f);
        private static readonly Color TextColor = new(0.91f, 0.94f, 0.84f, 1f);
        private static readonly Color MutedColor = new(0.66f, 0.75f, 0.70f, 1f);

        private AbilityLoadoutSelectionController selectionController;
        private Font uiFont;
        private GameObject menuRoot;
        private Text entriesText;
        private Text slotsText;
        private Text detailTitleText;
        private Text detailDescriptionText;
        private Text detailStatusText;

        public void Initialize(Font font, AbilityLoadoutSelectionController controller)
        {
            Unbind();
            uiFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            selectionController = controller;
            BuildHierarchy();

            if (selectionController != null)
            {
                selectionController.StateChanged += Refresh;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (selectionController != null)
            {
                selectionController.StateChanged -= Refresh;
                selectionController = null;
            }
        }

        private void Refresh()
        {
            if (menuRoot == null)
            {
                return;
            }

            bool visible = selectionController != null && selectionController.IsOpen;
            menuRoot.SetActive(visible);
            if (!visible)
            {
                return;
            }

            entriesText.text = BuildEntryList();
            slotsText.text = BuildSlotList();

            AbilityLoadoutMenuEntry? selected = selectionController.SelectedEntry;
            if (!selected.HasValue)
            {
                detailTitleText.text = "取得済みArt / Skillはありません";
                detailDescriptionText.text =
                    "ArtやSkillを取得すると、この画面から確認・入力割当できるようになります。";
                detailStatusText.text = selectionController.LastActionMessage;
                return;
            }

            AbilityLoadoutMenuEntry entry = selected.Value;
            detailTitleText.text = entry.DisplayName;
            detailDescriptionText.text = entry.Description;
            detailStatusText.text = BuildEntryStatus(entry);
        }

        private string BuildEntryList()
        {
            if (selectionController.Entries.Count == 0)
            {
                return "（未取得）";
            }

            var builder = new StringBuilder();
            for (int index = 0; index < selectionController.Entries.Count; index++)
            {
                AbilityLoadoutMenuEntry entry = selectionController.Entries[index];
                string marker = index == selectionController.SelectedEntryIndex ? "▶" : "  ";
                string kind = entry.Kind == AbilityLoadoutMenuEntryKind.ArtAbility
                    ? "[ART]"
                    : "[SKILL]";

                builder.Append(marker)
                    .Append(' ')
                    .Append(kind)
                    .Append(' ')
                    .Append(entry.DisplayName);

                if (entry.Kind == AbilityLoadoutMenuEntryKind.ArtAbility)
                {
                    builder.Append("  / ").Append(entry.SourceDisplayName);
                }

                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildSlotList()
        {
            var builder = new StringBuilder();
            for (int index = 0; index < selectionController.Slots.Count; index++)
            {
                AbilitySlot slot = selectionController.Slots[index];
                string marker = index == selectionController.SelectedSlotIndex ? "▶" : "  ";
                builder.Append(marker)
                    .Append(' ')
                    .Append(FormatSlot(slot))
                    .Append(" : ")
                    .Append(selectionController.GetAssignedAbilityDisplayName(slot))
                    .AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildEntryStatus(AbilityLoadoutMenuEntry entry)
        {
            var builder = new StringBuilder();
            if (entry.CanAssign)
            {
                builder.Append("取得元: ")
                    .Append(entry.SourceDisplayName)
                    .Append('\n')
                    .Append("割当先: ")
                    .Append(FormatSlot(selectionController.SelectedSlot))
                    .Append("\nEnter / 決定で割り当て");
            }
            else
            {
                builder.Append("受動Skill / 常時有効")
                    .Append("\n入力Slotへの割当はありません。");
            }

            if (!string.IsNullOrEmpty(selectionController.LastActionMessage))
            {
                builder.Append("\n\n").Append(selectionController.LastActionMessage);
            }

            return builder.ToString();
        }

        private void BuildHierarchy()
        {
            Transform existing = transform.Find("Ability Loadout Menu");
            if (existing != null)
            {
                menuRoot = existing.gameObject;
                entriesText = existing.Find("Loadout Panel/Entries")?.GetComponent<Text>();
                slotsText = existing.Find("Loadout Panel/Slots")?.GetComponent<Text>();
                detailTitleText = existing.Find("Loadout Panel/Details/Title")?.GetComponent<Text>();
                detailDescriptionText = existing.Find("Loadout Panel/Details/Description")?.GetComponent<Text>();
                detailStatusText = existing.Find("Loadout Panel/Details/Status")?.GetComponent<Text>();
                return;
            }

            RectTransform overlay = CreateRect("Ability Loadout Menu", transform);
            StretchToParent(overlay);
            menuRoot = overlay.gameObject;
            Image overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.color = OverlayColor;
            overlayImage.raycastTarget = true;

            RectTransform panel = CreateRect("Loadout Panel", overlay);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(1120f, 700f);
            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = PanelColor;
            panelImage.raycastTarget = false;

            CreateText(
                "Heading",
                panel,
                "LOADOUT － Art / Skill と入力割当",
                32,
                FontStyle.Bold,
                TitleColor,
                TextAnchor.MiddleLeft,
                new Vector2(42f, -42f),
                new Vector2(1036f, 56f),
                new Vector2(0f, 1f));

            entriesText = CreateText(
                "Entries",
                panel,
                string.Empty,
                21,
                FontStyle.Bold,
                TextColor,
                TextAnchor.UpperLeft,
                new Vector2(42f, -126f),
                new Vector2(590f, 270f),
                new Vector2(0f, 1f));

            slotsText = CreateText(
                "Slots",
                panel,
                string.Empty,
                21,
                FontStyle.Bold,
                AccentColor,
                TextAnchor.UpperLeft,
                new Vector2(666f, -126f),
                new Vector2(412f, 270f),
                new Vector2(0f, 1f));

            RectTransform divider = CreateRect("Divider", panel);
            divider.anchorMin = new Vector2(0f, 1f);
            divider.anchorMax = new Vector2(0f, 1f);
            divider.pivot = new Vector2(0f, 1f);
            divider.anchoredPosition = new Vector2(42f, -414f);
            divider.sizeDelta = new Vector2(1036f, 3f);
            Image dividerImage = divider.gameObject.AddComponent<Image>();
            dividerImage.color = AccentColor;
            dividerImage.raycastTarget = false;

            RectTransform details = CreateRect("Details", panel);
            details.anchorMin = new Vector2(0f, 1f);
            details.anchorMax = new Vector2(0f, 1f);
            details.pivot = new Vector2(0f, 1f);
            details.anchoredPosition = new Vector2(42f, -440f);
            details.sizeDelta = new Vector2(1036f, 170f);

            detailTitleText = CreateText(
                "Title",
                details,
                string.Empty,
                26,
                FontStyle.Bold,
                AccentColor,
                TextAnchor.UpperLeft,
                Vector2.zero,
                new Vector2(420f, 42f),
                new Vector2(0f, 1f));

            detailDescriptionText = CreateText(
                "Description",
                details,
                string.Empty,
                18,
                FontStyle.Normal,
                TextColor,
                TextAnchor.UpperLeft,
                new Vector2(0f, -48f),
                new Vector2(560f, 112f),
                new Vector2(0f, 1f));

            detailStatusText = CreateText(
                "Status",
                details,
                string.Empty,
                18,
                FontStyle.Normal,
                MutedColor,
                TextAnchor.UpperLeft,
                new Vector2(590f, 0f),
                new Vector2(446f, 160f),
                new Vector2(0f, 1f));

            CreateText(
                "Footer",
                panel,
                "↑↓ / W,S 候補選択    ←→ / A,D Slot選択    Enter 決定    Esc 戻る",
                18,
                FontStyle.Normal,
                TextColor,
                TextAnchor.MiddleCenter,
                new Vector2(42f, 26f),
                new Vector2(1036f, 42f),
                new Vector2(0f, 0f));
        }

        private Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            FontStyle fontStyle,
            Color color,
            TextAnchor alignment,
            Vector2 anchoredPosition,
            Vector2 size,
            Vector2 pivot)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = pivot;
            rect.anchorMax = pivot;
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
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
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

        private static string FormatSlot(AbilitySlot slot)
        {
            return slot switch
            {
                AbilitySlot.Action1 => "Action1",
                AbilitySlot.Action2 => "Action2",
                AbilitySlot.Action3 => "Action3",
                AbilitySlot.Action4 => "Action4",
                _ => slot.ToString()
            };
        }
    }
}
