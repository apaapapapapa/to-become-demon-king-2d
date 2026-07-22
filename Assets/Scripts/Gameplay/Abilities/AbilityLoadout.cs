using System;
using System.Collections.Generic;
using DemonKing.Core.Input;
using DemonKing.Domain;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// 論理Ability SlotからStable Ability IDへのRuntime割当を管理します。
    /// Input SystemやAbility Definitionの所有関係には依存しません。
    /// 編集可能Slotでは同一Abilityを複数Slotへ保持せず、再割当は移動として扱います。
    /// </summary>
    public sealed class AbilityLoadout
    {
        private readonly Dictionary<AbilitySlot, string> abilityIdsBySlot = new();

        public event Action<AbilitySlot, string> SlotChanged;

        public IReadOnlyDictionary<AbilitySlot, string> Slots => abilityIdsBySlot;

        public bool Assign(AbilitySlot slot, string abilityId)
        {
            string normalizedId = NormalizeAbilityId(abilityId);
            if (AbilityLoadoutPolicy.IsEditableSlot(slot))
            {
                return AssignEditable(slot, normalizedId);
            }

            return AssignCore(slot, normalizedId);
        }

        public bool Clear(AbilitySlot slot)
        {
            if (!abilityIdsBySlot.Remove(slot))
            {
                return false;
            }

            SlotChanged?.Invoke(slot, string.Empty);
            return true;
        }

        public void ClearAll()
        {
            if (abilityIdsBySlot.Count == 0)
            {
                return;
            }

            AbilitySlot[] slots = new AbilitySlot[abilityIdsBySlot.Count];
            abilityIdsBySlot.Keys.CopyTo(slots, 0);
            abilityIdsBySlot.Clear();
            foreach (AbilitySlot slot in slots)
            {
                SlotChanged?.Invoke(slot, string.Empty);
            }
        }

        public bool TryResolve(AbilitySlot slot, out string abilityId)
        {
            return abilityIdsBySlot.TryGetValue(slot, out abilityId);
        }

        private bool AssignEditable(AbilitySlot targetSlot, string normalizedId)
        {
            bool changed = false;
            foreach (AbilitySlot slot in AbilityLoadoutPolicy.EditableSlots)
            {
                if (slot == targetSlot ||
                    !abilityIdsBySlot.TryGetValue(slot, out string existingAbilityId) ||
                    !string.Equals(existingAbilityId, normalizedId, StringComparison.Ordinal))
                {
                    continue;
                }

                changed |= Clear(slot);
            }

            changed |= AssignCore(targetSlot, normalizedId);
            return changed;
        }

        private bool AssignCore(AbilitySlot slot, string normalizedId)
        {
            if (abilityIdsBySlot.TryGetValue(slot, out string existing) &&
                string.Equals(existing, normalizedId, StringComparison.Ordinal))
            {
                return false;
            }

            abilityIdsBySlot[slot] = normalizedId;
            SlotChanged?.Invoke(slot, normalizedId);
            return true;
        }

        private static string NormalizeAbilityId(string abilityId)
        {
            string normalizedId = StableContentId.Normalize(abilityId);
            if (!StableContentId.IsValid(normalizedId) ||
                !normalizedId.StartsWith("ability.", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Ability Slotへ割り当てるStable Ability IDが不正です: {abilityId}",
                    nameof(abilityId));
            }

            return normalizedId;
        }
    }
}
