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
        private readonly List<string> unlockedSkillIds;
        private readonly List<string> unlockedEvolutionNodeIds;

        private CharacterProgressionState(
            string characterDefinitionId,
            int level,
            long currentExperience,
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
            this.unlockedSkillIds = CopyDistinctIds(unlockedSkillIds, nameof(unlockedSkillIds));
            this.unlockedEvolutionNodeIds = CopyDistinctIds(
                unlockedEvolutionNodeIds,
                nameof(unlockedEvolutionNodeIds));
        }

        public string CharacterDefinitionId { get; }
        public int Level { get; }
        public long CurrentExperience { get; }
        public IReadOnlyList<string> UnlockedSkillIds => unlockedSkillIds;
        public IReadOnlyList<string> UnlockedEvolutionNodeIds => unlockedEvolutionNodeIds;

        public static CharacterProgressionState CreateInitial(string characterDefinitionId)
        {
            return new CharacterProgressionState(
                characterDefinitionId,
                level: 1,
                currentExperience: 0,
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
            return new CharacterProgressionState(
                characterDefinitionId,
                level,
                currentExperience,
                unlockedSkillIds,
                unlockedEvolutionNodeIds);
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
    }
}
