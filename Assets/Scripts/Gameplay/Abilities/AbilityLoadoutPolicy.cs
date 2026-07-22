using System;
using System.Collections.Generic;
using DemonKing.Core.Input;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// Ability Loadoutの編集可能Slotと予約Slotのルールを一元管理します。
    /// UI、Save、Runtime Controllerは個別のSlot一覧を保持せず、このPolicyを参照します。
    /// </summary>
    public static class AbilityLoadoutPolicy
    {
        private static readonly AbilitySlot[] EditableActionSlots =
        {
            AbilitySlot.Action1,
            AbilitySlot.Action2,
            AbilitySlot.Action3,
            AbilitySlot.Action4
        };

        private static readonly IReadOnlyList<AbilitySlot> EditableActionSlotsView =
            Array.AsReadOnly(EditableActionSlots);

        public static IReadOnlyList<AbilitySlot> EditableSlots => EditableActionSlotsView;

        public static bool IsEditableSlot(AbilitySlot slot)
        {
            foreach (AbilitySlot editableSlot in EditableActionSlots)
            {
                if (editableSlot == slot)
                {
                    return true;
                }
            }

            return false;
        }

        public static int IndexOfEditableSlot(AbilitySlot slot)
        {
            for (int index = 0; index < EditableActionSlots.Length; index++)
            {
                if (EditableActionSlots[index] == slot)
                {
                    return index;
                }
            }

            return -1;
        }

        public static AbilitySlot GetEditableSlot(int index)
        {
            if (index < 0 || index >= EditableActionSlots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return EditableActionSlots[index];
        }
    }
}
