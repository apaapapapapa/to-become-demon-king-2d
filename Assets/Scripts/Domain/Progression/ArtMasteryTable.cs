using System;
using System.Collections.Generic;

namespace DemonKing.Domain.Progression
{
    /// <summary>
    /// Artランクごとの累積熟練ポイント閾値を評価するUnity非依存テーブルです。
    /// </summary>
    public sealed class ArtMasteryTable
    {
        private readonly long[] cumulativeMasteryPointsByRank;

        public ArtMasteryTable(IEnumerable<long> cumulativeMasteryPointsByRank)
        {
            if (cumulativeMasteryPointsByRank == null)
            {
                throw new ArgumentNullException(nameof(cumulativeMasteryPointsByRank));
            }

            var values = new List<long>(cumulativeMasteryPointsByRank);
            Validate(values);
            this.cumulativeMasteryPointsByRank = values.ToArray();
        }

        public int MaxRank => cumulativeMasteryPointsByRank.Length;

        public long GetCumulativeMasteryPointsForRank(int rank)
        {
            if (rank < 1 || rank > MaxRank)
            {
                throw new ArgumentOutOfRangeException(nameof(rank));
            }

            return cumulativeMasteryPointsByRank[rank - 1];
        }

        public int GetRankForTotalMasteryPoints(long masteryPoints)
        {
            if (masteryPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(masteryPoints));
            }

            int lower = 0;
            int upper = cumulativeMasteryPointsByRank.Length - 1;
            int matchedIndex = 0;

            while (lower <= upper)
            {
                int middle = lower + ((upper - lower) / 2);
                if (cumulativeMasteryPointsByRank[middle] <= masteryPoints)
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
                    "Art熟練テーブルにはランク1以上の定義が必要です。",
                    nameof(values));
            }

            if (values[0] != 0)
            {
                throw new ArgumentException(
                    "Artランク1の累積熟練ポイントは0である必要があります。",
                    nameof(values));
            }

            for (int index = 1; index < values.Count; index++)
            {
                if (values[index] <= values[index - 1])
                {
                    throw new ArgumentException(
                        "Art熟練ポイント閾値はランク順に厳密な単調増加である必要があります。",
                        nameof(values));
                }
            }
        }
    }
}
