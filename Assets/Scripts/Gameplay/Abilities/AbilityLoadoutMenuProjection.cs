using System;
using System.Collections.Generic;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Progression.Configuration;

namespace DemonKing.Gameplay.Abilities
{
    public enum AbilityLoadoutMenuEntryKind
    {
        ArtAbility = 0,
        PassiveSkill = 1
    }

    /// <summary>
    /// Loadout選択画面へ渡すuGUI非依存の表示要素です。
    /// Art由来Abilityは入力割当可能、現在の受動Skillは一覧表示のみとします。
    /// </summary>
    public readonly struct AbilityLoadoutMenuEntry
    {
        internal AbilityLoadoutMenuEntry(
            AbilityLoadoutMenuEntryKind kind,
            string sourceContentId,
            string sourceDisplayName,
            string displayName,
            string description,
            string abilityId)
        {
            Kind = kind;
            SourceContentId = sourceContentId ?? string.Empty;
            SourceDisplayName = sourceDisplayName ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            AbilityId = abilityId ?? string.Empty;
        }

        public AbilityLoadoutMenuEntryKind Kind { get; }
        public string SourceContentId { get; }
        public string SourceDisplayName { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string AbilityId { get; }
        public bool CanAssign =>
            Kind == AbilityLoadoutMenuEntryKind.ArtAbility &&
            !string.IsNullOrEmpty(AbilityId);
    }

    /// <summary>
    /// Character DefinitionとRuntime Progression StateからLoadout選択候補を構築します。
    /// 取得済みArtの現在Rankで解放済みのAbilityと、取得済み受動Skillだけを公開します。
    /// </summary>
    public static class AbilityLoadoutMenuProjection
    {
        public static IReadOnlyList<AbilityLoadoutMenuEntry> Build(
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

            var entries = new List<AbilityLoadoutMenuEntry>();
            AppendArtAbilities(entries, characterDefinition, progressionState);
            AppendPassiveSkills(entries, characterDefinition, progressionState);
            return entries;
        }

        public static string ResolveAbilityDisplayName(
            CharacterDefinition characterDefinition,
            string abilityId)
        {
            if (characterDefinition == null || string.IsNullOrWhiteSpace(abilityId))
            {
                return string.Empty;
            }

            foreach (AbilityDefinition definition in characterDefinition.AbilityDefinitions)
            {
                if (definition != null &&
                    string.Equals(definition.AbilityId, abilityId, StringComparison.Ordinal))
                {
                    return definition.DisplayName;
                }
            }

            foreach (ArtDefinition artDefinition in characterDefinition.ArtDefinitions)
            {
                if (artDefinition == null)
                {
                    continue;
                }

                foreach (ArtAbilityUnlockEntry unlock in artDefinition.AbilityUnlocks)
                {
                    AbilityDefinition definition = unlock?.AbilityDefinition;
                    if (definition != null &&
                        string.Equals(definition.AbilityId, abilityId, StringComparison.Ordinal))
                    {
                        return definition.DisplayName;
                    }
                }
            }

            return abilityId;
        }

        private static void AppendArtAbilities(
            ICollection<AbilityLoadoutMenuEntry> entries,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
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

                foreach (ArtAbilityUnlockEntry unlock in artDefinition.AbilityUnlocks)
                {
                    AbilityDefinition abilityDefinition = unlock?.AbilityDefinition;
                    if (abilityDefinition == null || unlock.RequiredRank > currentRank)
                    {
                        continue;
                    }

                    entries.Add(new AbilityLoadoutMenuEntry(
                        AbilityLoadoutMenuEntryKind.ArtAbility,
                        artDefinition.ArtId,
                        artDefinition.DisplayName,
                        abilityDefinition.DisplayName,
                        abilityDefinition.Description,
                        abilityDefinition.AbilityId));
                }
            }
        }

        private static void AppendPassiveSkills(
            ICollection<AbilityLoadoutMenuEntry> entries,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            foreach (SkillDefinition skillDefinition in characterDefinition.SkillDefinitions)
            {
                if (skillDefinition == null ||
                    !progressionState.IsSkillUnlocked(skillDefinition.SkillId))
                {
                    continue;
                }

                entries.Add(new AbilityLoadoutMenuEntry(
                    AbilityLoadoutMenuEntryKind.PassiveSkill,
                    skillDefinition.SkillId,
                    skillDefinition.DisplayName,
                    skillDefinition.DisplayName,
                    skillDefinition.Description,
                    string.Empty));
            }
        }
    }
}
