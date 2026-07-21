using System;
using System.Collections.Generic;
using DemonKing.Core.Input;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// プレイヤー個体のAbility Loadout Runtime Stateを保持します。
    /// 初期割当はCharacterDefinitionから構築し、その後はUI等がRuntime Loadoutだけを更新します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AbilityLoadoutController : MonoBehaviour
    {
        private static readonly AbilitySlot[] EditableActionSlots =
        {
            AbilitySlot.Action1,
            AbilitySlot.Action2,
            AbilitySlot.Action3,
            AbilitySlot.Action4
        };

        public AbilityLoadout Loadout { get; private set; }
        public bool IsInitialized => Loadout != null;

        public void Initialize(CharacterDefinition characterDefinition)
        {
            if (characterDefinition == null)
            {
                throw new ArgumentNullException(nameof(characterDefinition));
            }

            var loadout = new AbilityLoadout();
            var assignedAbilityIds = new HashSet<string>(StringComparer.Ordinal);

            IReadOnlyList<AbilityDefinition> directAbilities = characterDefinition.AbilityDefinitions;
            if (directAbilities.Count > 0 && directAbilities[0] != null)
            {
                loadout.Assign(AbilitySlot.Primary, directAbilities[0].AbilityId);
                assignedAbilityIds.Add(directAbilities[0].AbilityId);
            }

            int nextActionSlot = 0;
            for (int index = 1; index < directAbilities.Count; index++)
            {
                TryAssignNextActionSlot(
                    loadout,
                    directAbilities[index],
                    assignedAbilityIds,
                    ref nextActionSlot);
            }

            foreach (ArtDefinition artDefinition in characterDefinition.ArtDefinitions)
            {
                if (artDefinition == null)
                {
                    continue;
                }

                foreach (ArtAbilityUnlockEntry unlockEntry in artDefinition.AbilityUnlocks)
                {
                    if (unlockEntry == null || unlockEntry.RequiredRank != 1)
                    {
                        continue;
                    }

                    TryAssignNextActionSlot(
                        loadout,
                        unlockEntry.AbilityDefinition,
                        assignedAbilityIds,
                        ref nextActionSlot);
                }
            }

            Initialize(loadout);
        }

        public void Initialize(AbilityLoadout loadout)
        {
            Loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
        }

        public bool Assign(AbilitySlot slot, string abilityId)
        {
            EnsureInitialized();
            return Loadout.Assign(slot, abilityId);
        }

        public bool Clear(AbilitySlot slot)
        {
            EnsureInitialized();
            return Loadout.Clear(slot);
        }

        public bool TryResolve(AbilitySlot slot, out string abilityId)
        {
            abilityId = string.Empty;
            return Loadout != null && Loadout.TryResolve(slot, out abilityId);
        }

        private static void TryAssignNextActionSlot(
            AbilityLoadout loadout,
            AbilityDefinition definition,
            ISet<string> assignedAbilityIds,
            ref int nextActionSlot)
        {
            if (definition == null ||
                nextActionSlot >= EditableActionSlots.Length ||
                !assignedAbilityIds.Add(definition.AbilityId))
            {
                return;
            }

            loadout.Assign(EditableActionSlots[nextActionSlot], definition.AbilityId);
            nextActionSlot++;
        }

        private void EnsureInitialized()
        {
            if (Loadout == null)
            {
                throw new InvalidOperationException("Ability Loadoutが初期化されていません。");
            }
        }
    }
}
