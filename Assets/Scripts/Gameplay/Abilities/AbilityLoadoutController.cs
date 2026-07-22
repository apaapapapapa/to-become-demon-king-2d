using System;
using System.Collections.Generic;
using DemonKing.Core.Input;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// プレイヤー個体のAbility Loadout Runtime Stateを保持します。
    /// 初期割当はCharacterDefinitionとRuntime Progression Stateから構築し、
    /// その後はRuntime Loadoutだけを更新します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AbilityLoadoutController : MonoBehaviour
    {
        public AbilityLoadout Loadout { get; private set; }
        public bool IsInitialized => Loadout != null;

        /// <summary>
        /// 既存利用側との互換用初期化です。すべてのArtをランク1相当で習得済みとして扱い、
        /// 共通Eligibilityを通して初期割当を構築します。
        /// 実ゲームプレイヤーはProgression Stateを受け取るoverloadを使用します。
        /// </summary>
        public void Initialize(CharacterDefinition characterDefinition)
        {
            if (characterDefinition == null)
            {
                throw new ArgumentNullException(nameof(characterDefinition));
            }

            CharacterProgressionState compatibilityState =
                CharacterProgressionState.CreateInitial(characterDefinition.CharacterId);
            foreach (var artDefinition in characterDefinition.ArtDefinitions)
            {
                if (artDefinition != null)
                {
                    compatibilityState.TryLearnArt(artDefinition.ArtId, out _);
                }
            }

            Initialize(characterDefinition, compatibilityState);
        }

        public void Initialize(
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            if (progressionState == null)
            {
                throw new ArgumentNullException(nameof(progressionState));
            }

            if (characterDefinition == null)
            {
                throw new ArgumentNullException(nameof(characterDefinition));
            }

            if (!string.Equals(
                    characterDefinition.CharacterId,
                    progressionState.CharacterDefinitionId,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "CharacterDefinitionとProgression StateのCharacter IDが一致していません。",
                    nameof(progressionState));
            }

            InitializeInternal(characterDefinition, progressionState);
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

        private void InitializeInternal(
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
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

            foreach (AbilityLoadoutEligibility.Entry eligible in
                     AbilityLoadoutEligibility.GetAssignableAbilities(
                         characterDefinition,
                         progressionState))
            {
                TryAssignNextActionSlot(
                    loadout,
                    eligible.Ability,
                    assignedAbilityIds,
                    ref nextActionSlot);
            }

            Initialize(loadout);
        }

        private static void TryAssignNextActionSlot(
            AbilityLoadout loadout,
            AbilityDefinition definition,
            ISet<string> assignedAbilityIds,
            ref int nextActionSlot)
        {
            if (definition == null ||
                nextActionSlot >= AbilityLoadoutPolicy.EditableSlots.Count ||
                !assignedAbilityIds.Add(definition.AbilityId))
            {
                return;
            }

            loadout.Assign(
                AbilityLoadoutPolicy.GetEditableSlot(nextActionSlot),
                definition.AbilityId);
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
