using System;
using System.Collections.Generic;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Progression.Configuration;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// 現在のCharacter DefinitionとProgression Stateから入力Slotへ割当可能なAbilityを判定します。
    /// PresentationやSave DTOには依存しません。
    /// </summary>
    public static class AbilityLoadoutEligibility
    {
        public readonly struct Entry
        {
            public Entry(ArtDefinition sourceArt, AbilityDefinition ability)
            {
                SourceArt = sourceArt;
                Ability = ability;
            }

            public ArtDefinition SourceArt { get; }
            public AbilityDefinition Ability { get; }
        }

        public static IReadOnlyList<Entry> GetAssignableAbilities(
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            ValidateContext(characterDefinition, progressionState);

            var result = new List<Entry>();
            var abilityIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (ArtDefinition artDefinition in characterDefinition.ArtDefinitions)
            {
                if (artDefinition == null ||
                    !progressionState.TryGetArtProgress(
                        artDefinition.ArtId,
                        out ArtProgressState progressState))
                {
                    continue;
                }

                int currentRank = artDefinition.CreateMasteryTable()
                    .GetRankForTotalMasteryPoints(progressState.MasteryPoints);
                foreach (ArtAbilityUnlockEntry unlockEntry in artDefinition.AbilityUnlocks)
                {
                    AbilityDefinition abilityDefinition = unlockEntry?.AbilityDefinition;
                    if (abilityDefinition == null ||
                        unlockEntry.RequiredRank > currentRank ||
                        !abilityIds.Add(abilityDefinition.AbilityId))
                    {
                        continue;
                    }

                    result.Add(new Entry(artDefinition, abilityDefinition));
                }
            }

            return result;
        }

        public static bool CanAssign(
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState,
            string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return false;
            }

            foreach (Entry entry in GetAssignableAbilities(characterDefinition, progressionState))
            {
                if (string.Equals(entry.Ability.AbilityId, abilityId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateContext(
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            if (characterDefinition == null)
            {
                throw new ArgumentNullException(nameof(characterDefinition));
            }

            if (progressionState == null)
            {
                throw new ArgumentNullException(nameof(progressionState));
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
        }
    }
}
