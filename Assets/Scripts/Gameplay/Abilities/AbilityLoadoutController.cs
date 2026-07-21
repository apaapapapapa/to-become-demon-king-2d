using System;
using System.Collections.Generic;
using DemonKing.Core.Input;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
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
        /// 既存利用側との互換用初期化です。ArtのRuntime習得状態を持たないため、
        /// CharacterDefinition上のランク1 Abilityまでを初期割当します。
        /// 実ゲームプレイヤーはProgression Stateを受け取るoverloadを使用します。
        /// </summary>
        public void Initialize(CharacterDefinition characterDefinition)
        {
            InitializeInternal(characterDefinition, progressionState: null);
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

                int currentRank = ResolveInitialArtRank(artDefinition, progressionState);
                if (currentRank <= 0)
                {
                    continue;
                }

                foreach (ArtAbilityUnlockEntry unlockEntry in artDefinition.AbilityUnlocks)
                {
                    if (unlockEntry == null || unlockEntry.RequiredRank > currentRank)
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

        private static int ResolveInitialArtRank(
            ArtDefinition artDefinition,
            CharacterProgressionState progressionState)
        {
            if (progressionState == null)
            {
                return 1;
            }

            if (!progressionState.TryGetArtProgress(
                    artDefinition.ArtId,
                    out ArtProgressState progressState))
            {
                return 0;
            }

            return artDefinition.CreateMasteryTable()
                .GetRankForTotalMasteryPoints(progressState.MasteryPoints);
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
