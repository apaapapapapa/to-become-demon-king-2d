using System;
using System.Collections.Generic;

namespace DemonKing.Domain.Progression
{
    /// <summary>
    /// 各レベルへ到達するための累積経験値を保持する、Unity非依存の経験値テーブルです。
    /// </summary>
    public sealed class ExperienceTable
    {
        private readonly long[] cumulativeExperienceByLevel;

        public ExperienceTable(
            IEnumerable<long> cumulativeExperienceByLevel,
            bool keepOverflowAtMaxLevel)
        {
            if (cumulativeExperienceByLevel == null)
            {
                throw new ArgumentNullException(nameof(cumulativeExperienceByLevel));
            }

            var values = new List<long>(cumulativeExperienceByLevel);
            Validate(values);

            this.cumulativeExperienceByLevel = values.ToArray();
            KeepOverflowAtMaxLevel = keepOverflowAtMaxLevel;
        }

        public int MaxLevel => cumulativeExperienceByLevel.Length;
        public bool KeepOverflowAtMaxLevel { get; }

        public long GetCumulativeExperienceForLevel(int level)
        {
            if (level < 1 || level > MaxLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            return cumulativeExperienceByLevel[level - 1];
        }

        public int GetLevelForTotalExperience(long totalExperience)
        {
            if (totalExperience < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalExperience));
            }

            int lower = 0;
            int upper = cumulativeExperienceByLevel.Length - 1;
            int matchedIndex = 0;

            while (lower <= upper)
            {
                int middle = lower + ((upper - lower) / 2);
                if (cumulativeExperienceByLevel[middle] <= totalExperience)
                {
                    matchedIndex = middle;
                    lower = middle + 1;
                }
                else
                {
                    upper = middle - 1;
                }
            }

            return matchedIndex + 1;
        }

        private static void Validate(IReadOnlyList<long> values)
        {
            if (values.Count == 0)
            {
                throw new ArgumentException(
                    "経験値テーブルにはレベル1以上の定義が必要です。",
                    nameof(values));
            }

            if (values[0] != 0)
            {
                throw new ArgumentException(
                    "レベル1の累積経験値は0である必要があります。",
                    nameof(values));
            }

            for (int index = 1; index < values.Count; index++)
            {
                if (values[index] <= values[index - 1])
                {
                    throw new ArgumentException(
                        "累積経験値はレベル順に厳密な単調増加である必要があります。",
                        nameof(values));
                }
            }
        }
    }
}
