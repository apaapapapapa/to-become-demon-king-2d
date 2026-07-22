using System.Text;
using DemonKing.Core.Input;
using DemonKing.Gameplay.Abilities;
using UnityEngine;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// Ability Loadout選択状態をPrefabベースのuGUIへ反映します。
    /// 候補生成、入力割当、Input Context制御、Hierarchy構築は行いません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class AbilityLoadoutMenuView : MonoBehaviour
    {
        private AbilityLoadoutSelectionController selectionController;
        private AbilityLoadoutMenuLayout layout;

        public void Initialize(
            Font font,
            AbilityLoadoutSelectionController controller,
            AbilityLoadoutMenuLayout menuLayout)
        {
            Unbind();
            selectionController = controller;
            layout = menuLayout;
            if (layout == null || !layout.IsConfigured)
            {
                Debug.LogError("AbilityLoadoutMenuLayoutが正しく設定されていません。", this);
                return;
            }

            Font resolvedFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            layout.ApplyFont(resolvedFont);

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
            if (layout == null)
            {
                return;
            }

            bool visible = selectionController != null && selectionController.IsOpen;
            layout.Root.SetActive(visible);
            if (!visible)
            {
                return;
            }

            layout.EntriesText.text = BuildEntryList();
            layout.SlotsText.text = BuildSlotList();

            AbilityLoadoutMenuEntry? selected = selectionController.SelectedEntry;
            if (!selected.HasValue)
            {
                layout.DetailTitleText.text = "取得済みArt / Skillはありません";
                layout.DetailDescriptionText.text =
                    "ArtやSkillを取得すると、この画面から確認・入力割当できるようになります。";
                layout.DetailStatusText.text = selectionController.LastActionMessage;
                return;
            }

            AbilityLoadoutMenuEntry entry = selected.Value;
            layout.DetailTitleText.text = entry.DisplayName;
            layout.DetailDescriptionText.text = entry.Description;
            layout.DetailStatusText.text = BuildEntryStatus(entry);
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
