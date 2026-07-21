using System;
using System.Collections.Generic;

namespace DemonKing.Domain.Progression
{
    /// <summary>
    /// 一度きりProgression Grantの消費済み状態をStable Grant IDで保持します。
    /// フィールド上のGameObjectライフサイクルから独立し、Saveから復元できます。
    /// </summary>
    public sealed class ProgressionGrantConsumptionState
    {
        private readonly HashSet<string> consumedGrantIds = new(StringComparer.Ordinal);

        private ProgressionGrantConsumptionState(IEnumerable<string> consumedGrantIds)
        {
            if (consumedGrantIds == null)
            {
                return;
            }

            foreach (string grantId in consumedGrantIds)
            {
                this.consumedGrantIds.Add(RequireGrantId(grantId, nameof(consumedGrantIds)));
            }
        }

        public event Action<string> GrantConsumed;

        public IReadOnlyCollection<string> ConsumedGrantIds => consumedGrantIds;

        public static ProgressionGrantConsumptionState CreateInitial()
        {
            return new ProgressionGrantConsumptionState(Array.Empty<string>());
        }

        public static ProgressionGrantConsumptionState Restore(IEnumerable<string> consumedGrantIds)
        {
            return new ProgressionGrantConsumptionState(consumedGrantIds);
        }

        public bool IsConsumed(string grantId)
        {
            string normalizedId = StableContentId.Normalize(grantId);
            return consumedGrantIds.Contains(normalizedId);
        }

        public bool TryConsume(string grantId)
        {
            string normalizedId = RequireGrantId(grantId, nameof(grantId));
            if (!consumedGrantIds.Add(normalizedId))
            {
                return false;
            }

            GrantConsumed?.Invoke(normalizedId);
            return true;
        }

        private static string RequireGrantId(string grantId, string parameterName)
        {
            string normalizedId = StableContentId.Require(grantId, parameterName);
            if (!normalizedId.StartsWith("grant.", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Progression Grant IDはgrant.*形式である必要があります。",
                    parameterName);
            }

            return normalizedId;
        }
    }
}
