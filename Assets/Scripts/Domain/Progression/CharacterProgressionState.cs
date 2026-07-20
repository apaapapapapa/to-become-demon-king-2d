using System;
using System.Collections.Generic;

namespace DemonKing.Domain.Progression
{
    /// <summary>
    /// キャラクターの成長に関する実行時状態です。
    /// ScriptableObjectのDefinitionを変更せず、保存データから復元できる純C#状態として保持します。
    /// </summary>
    public sealed class CharacterProgressionState
    {
        private readonly List<ArtProgressState> artProgressStates;
        private readonly List<string> unlockedSkillIds;
        private readonly List<string> unlockedEvolutionNodeIds;

        private CharacterProgressionState(
            string characterDefinitionId,
            int level,
            long currentExperience,
            IEnumerable<ArtProgressState> artProgressStates,
            IEnumerable<string> unlockedSkillIds,
            IEnumerable<string> unlockedEvolutionNodeIds)
        {
            CharacterDefinitionId = StableContentId.Require(
                characterDefinitionId,
                nameof(characterDefinitionId));

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "レベルは1以上である必要があります。");
            }

            if (currentExperience < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentExperience),
                    "現在経験値は0以上である必要があります。");
            }

            Level = level;
            CurrentExperience = currentExperience;
            this.artProgressStates = CopyArtProgressStates(
                artProgressStates,
                nameof(artProgressStates));
            this.unlockedSkillIds = CopyDistinctIds(unlockedSkillIds, nameof(unlockedSkillIds));
            this.unlockedEvolutionNodeIds = CopyDistinctIds(
                unlockedEvolutionNodeIds,
                nameof(unlockedEvolutionNodeIds));
        }

        public string CharacterDefinitionId { get; }
        public int Level { get; private set; }
        public long CurrentExperience { get; private set; }
        public IReadOnlyList<ArtProgressState> ArtProgressStates => artProgressStates;
        public IReadOnlyList<string> UnlockedSkillIds => unlockedSkillIds;
        public IReadOnlyList<string> UnlockedEvolutionNodeIds => unlockedEvolutionNodeIds;

        public static CharacterProgressionState CreateInitial(string characterDefinitionId)
        {
            return new CharacterProgressionState(
                characterDefinitionId,
                level: 1,
                currentExperience: 0,
                artProgressStates: Array.Empty<ArtProgressState>(),
                unlockedSkillIds: Array.Empty<string>(),
                unlockedEvolutionNodeIds: Array.Empty<string>());
        }

        public static CharacterProgressionState Restore(
            string characterDefinitionId,
            int level,
            long currentExperience,
            IEnumerable<string> unlockedSkillIds,
            IEnumerable<string> unlockedEvolutionNodeIds)
        {
            return Restore(
                characterDefinitionId,
                level,
                currentExperience,
                unlockedSkillIds,
                unlockedEvolutionNodeIds,
                Array.Empty<ArtProgressState>());
        }

        public static CharacterProgressionState Restore(
            string characterDefinitionId,
            int level,
            long currentExperience,
            IEnumerable<string> unlockedSkillIds,
            IEnumerable<string> unlockedEvolutionNodeIds,
            IEnumerable<ArtProgressState> artProgressStates)
        {
            return new CharacterProgressionState(
                characterDefinitionId,
                level,
                currentExperience,
                artProgressStates,
                unlockedSkillIds,
                unlockedEvolutionNodeIds);
        }

        public bool TryLearnArt(string artId, out ArtProgressState progressState)
        {
            if (TryGetArtProgress(artId, out progressState))
            {
                return false;
            }

            progressState = ArtProgressState.CreateLearned(artId);
            artProgressStates.Add(progressState);
            return true;
        }

        public bool TryUnlockSkill(string skillId)
        {
            string normalizedId = StableContentId.Require(skillId, nameof(skillId));
            if (!normalizedId.StartsWith("skill.", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Skill IDはskill.*形式である必要があります。",
                    nameof(skillId));
            }

            if (unlockedSkillIds.Contains(normalizedId))
            {
                return false;
            }

            unlockedSkillIds.Add(normalizedId);
            return true;
        }

        public bool IsSkillUnlocked(string skillId)
        {
            string normalizedId = StableContentId.Normalize(skillId);
            return unlockedSkillIds.Contains(normalizedId);
        }

        public bool TryUnlockEvolutionNode(string evolutionNodeId)
        {
            string normalizedId = StableContentId.Require(
                evolutionNodeId,
                nameof(evolutionNodeId));
            if (!normalizedId.StartsWith("evolution.", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Evolution Node IDはevolution.*形式である必要があります。",
                    nameof(evolutionNodeId));
            }

            if (unlockedEvolutionNodeIds.Contains(normalizedId))
            {
                return false;
            }

            unlockedEvolutionNodeIds.Add(normalizedId);
            return true;
        }

        public bool IsEvolutionNodeUnlocked(string evolutionNodeId)
        {
            string normalizedId = StableContentId.Normalize(evolutionNodeId);
            return unlockedEvolutionNodeIds.Contains(normalizedId);
        }

        public bool TryGetArtProgress(string artId, out ArtProgressState progressState)
        {
            string normalizedId = StableContentId.Normalize(artId);
            foreach (ArtProgressState candidate in artProgressStates)
            {
                if (string.Equals(candidate.ArtId, normalizedId, StringComparison.Ordinal))
                {
                    progressState = candidate;
                    return true;
                }
            }

            progressState = null;
            return false;
        }

        /// <summary>
        /// 累積経験値を加算し、同時にレベルを更新します。
        /// 最大レベル時の余剰経験値はExperienceTableの方針に従います。
        /// </summary>
        public LevelUpResult GainExperience(long amount, ExperienceTable experienceTable)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "加算経験値は0以上である必要があります。");
            }

            if (experienceTable == null)
            {
                throw new ArgumentNullException(nameof(experienceTable));
            }

            int previousLevel = Level;
            long previousExperience = CurrentExperience;
            long nextExperience = AddWithoutOverflow(previousExperience, amount);

            if (!experienceTable.KeepOverflowAtMaxLevel)
            {
                long maximumExperience = experienceTable.GetCumulativeExperienceForLevel(
                    experienceTable.MaxLevel);
                nextExperience = Math.Min(nextExperience, maximumExperience);
            }

            CurrentExperience = nextExperience;
            Level = experienceTable.GetLevelForTotalExperience(CurrentExperience);

            return new LevelUpResult(
                amount,
                Math.Max(0, CurrentExperience - previousExperience),
                previousLevel,
                Level,
                previousExperience,
                CurrentExperience,
                Level == experienceTable.MaxLevel);
        }

        private static long AddWithoutOverflow(long left, long right)
        {
            return left > long.MaxValue - right
                ? long.MaxValue
                : left + right;
        }

        private static List<string> CopyDistinctIds(IEnumerable<string> source, string parameterName)
        {
            var result = new List<string>();
            var knownIds = new HashSet<string>(StringComparer.Ordinal);

            if (source == null)
            {
                return result;
            }

            foreach (string value in source)
            {
                string id = StableContentId.Require(value, parameterName);
                if (knownIds.Add(id))
                {
                    result.Add(id);
                }
            }

            return result;
        }

        private static List<ArtProgressState> CopyArtProgressStates(
            IEnumerable<ArtProgressState> source,
            string parameterName)
        {
            var result = new List<ArtProgressState>();
            var knownIds = new HashSet<string>(StringComparer.Ordinal);

            if (source == null)
            {
                return result;
            }

            foreach (ArtProgressState value in source)
            {
                if (value == null)
                {
                    throw new ArgumentException(
                        "Art進捗にnullを含めることはできません。",
                        parameterName);
                }

                if (!knownIds.Add(value.ArtId))
                {
                    throw new ArgumentException(
                        $"Art進捗IDが重複しています: {value.ArtId}",
                        parameterName);
                }

                result.Add(ArtProgressState.Restore(value.ArtId, value.MasteryPoints));
            }

            return result;
        }
    }
}
