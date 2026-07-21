using System;
using System.Collections.Generic;
using DemonKing.Core.Input;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Save;
using DemonKing.Gameplay.Characters.Configuration;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// Runtime Ability LoadoutとSave DTOを相互変換します。
    /// PrimaryはDefinition由来の予約枠なので保存せず、ユーザー編集可能なAction1〜4だけを対象とします。
    /// </summary>
    public static class AbilityLoadoutSaveMapper
    {
        private static readonly AbilitySlot[] EditableSlots =
        {
            AbilitySlot.Action1,
            AbilitySlot.Action2,
            AbilitySlot.Action3,
            AbilitySlot.Action4
        };

        public static AbilityLoadoutSaveData ToSaveData(AbilityLoadout loadout)
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }

            var saveData = new AbilityLoadoutSaveData();
            foreach (AbilitySlot slot in EditableSlots)
            {
                if (!loadout.TryResolve(slot, out string abilityId))
                {
                    continue;
                }

                saveData.slots.Add(new AbilitySlotSaveData
                {
                    slot = (int)slot,
                    abilityId = abilityId
                });
            }

            return saveData;
        }

        public static void ApplySavedAssignments(
            AbilityLoadoutController controller,
            AbilityLoadoutSaveData saveData,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            if (controller == null || !controller.IsInitialized)
            {
                throw new ArgumentException(
                    "初期化済みAbilityLoadoutControllerが必要です。",
                    nameof(controller));
            }

            if (characterDefinition == null)
            {
                throw new ArgumentNullException(nameof(characterDefinition));
            }

            if (progressionState == null)
            {
                throw new ArgumentNullException(nameof(progressionState));
            }

            foreach (AbilitySlot slot in EditableSlots)
            {
                controller.Clear(slot);
            }

            if (saveData?.slots == null)
            {
                return;
            }

            var assignableAbilityIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (AbilityLoadoutMenuEntry entry in
                     AbilityLoadoutMenuProjection.Build(characterDefinition, progressionState))
            {
                if (entry.CanAssign)
                {
                    assignableAbilityIds.Add(entry.AbilityId);
                }
            }

            var assignedAbilityIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (AbilitySlotSaveData savedSlot in saveData.slots)
            {
                if (savedSlot == null ||
                    !Enum.IsDefined(typeof(AbilitySlot), savedSlot.slot))
                {
                    continue;
                }

                AbilitySlot slot = (AbilitySlot)savedSlot.slot;
                if (!IsEditableSlot(slot) ||
                    !assignableAbilityIds.Contains(savedSlot.abilityId ?? string.Empty) ||
                    !assignedAbilityIds.Add(savedSlot.abilityId))
                {
                    continue;
                }

                controller.Assign(slot, savedSlot.abilityId);
            }
        }

        private static bool IsEditableSlot(AbilitySlot slot)
        {
            foreach (AbilitySlot editableSlot in EditableSlots)
            {
                if (editableSlot == slot)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
