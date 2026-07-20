using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Content;
using UnityEngine;

namespace DemonKing.Gameplay.Progression.Configuration
{
    /// <summary>
    /// Art内でAbilityを解放するランクと、効果成立時の熟練ポイントを定義します。
    /// </summary>
    [Serializable]
    public sealed class ArtAbilityUnlockEntry
    {
        [SerializeField] private AbilityDefinition abilityDefinition;
        [SerializeField, Min(1)] private int requiredRank = 1;
        [SerializeField, Min(1)] private long masteryPointsPerEffectiveUse = 1;

        public AbilityDefinition AbilityDefinition => abilityDefinition;
        public int RequiredRank => requiredRank;
        public long MasteryPointsPerEffectiveUse => masteryPointsPerEffectiveUse;

        internal bool IsConfigured(int maxRank)
        {
            return abilityDefinition != null &&
                   abilityDefinition.IsConfigured &&
                   requiredRank >= 1 &&
                   requiredRank <= maxRank &&
                   masteryPointsPerEffectiveUse > 0;
        }

        internal void Normalize(int maxRank)
        {
            requiredRank = Mathf.Clamp(requiredRank, 1, Mathf.Max(1, maxRank));
            masteryPointsPerEffectiveUse = Math.Max(1, masteryPointsPerEffectiveUse);
        }
    }

    /// <summary>
    /// 複数Abilityの段階解放と熟練ランクを表す静的なArtコンテンツ定義です。
    /// 習得状態や熟練ポイントはArtProgressStateへ分離します。
    /// </summary>
    [CreateAssetMenu(fileName = "Art", menuName = "Demon King/Gameplay/Progression/Art")]
    public sealed class ArtDefinition : ScriptableObject, IGameContentDefinition
    {
        private const string ArtIdPrefix = "art.";

        [Header("Identity")]
        [SerializeField] private string artId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField, TextArea] private string encyclopediaDescription = string.Empty;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool visibleInEncyclopedia = true;
        [SerializeField] private string category = string.Empty;

        [Header("Mastery")]
        [SerializeField] private long[] cumulativeMasteryPointsByRank = { 0 };
        [SerializeField] private ArtAbilityUnlockEntry[] abilityUnlocks =
            Array.Empty<ArtAbilityUnlockEntry>();

        public string ContentId => artId;
        public string ArtId => artId;
        public string DisplayName => displayName;
        public string Description => description;
        public string EncyclopediaDescription => encyclopediaDescription;
        public Sprite Icon => icon;
        public bool VisibleInEncyclopedia => visibleInEncyclopedia;
        public string Category => category;
        public IReadOnlyList<long> CumulativeMasteryPointsByRank =>
            cumulativeMasteryPointsByRank ?? Array.Empty<long>();
        public IReadOnlyList<ArtAbilityUnlockEntry> AbilityUnlocks =>
            abilityUnlocks ?? Array.Empty<ArtAbilityUnlockEntry>();
        public int MaxRank => cumulativeMasteryPointsByRank?.Length ?? 0;

        public bool IsConfigured
        {
            get
            {
                if (!StableContentId.IsValid(artId) ||
                    !artId.StartsWith(ArtIdPrefix, StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(displayName) ||
                    !HasValidMasteryThresholds() ||
                    abilityUnlocks == null ||
                    abilityUnlocks.Length == 0)
                {
                    return false;
                }

                bool hasInitialAbility = false;
                var abilityIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (ArtAbilityUnlockEntry entry in abilityUnlocks)
                {
                    if (entry == null ||
                        !entry.IsConfigured(MaxRank) ||
                        !abilityIds.Add(entry.AbilityDefinition.AbilityId))
                    {
                        return false;
                    }

                    hasInitialAbility |= entry.RequiredRank == 1;
                }

                return hasInitialAbility;
            }
        }

        public ArtMasteryTable CreateMasteryTable()
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException(
                    $"ArtDefinitionが正しく設定されていません: {name}");
            }

            return new ArtMasteryTable(cumulativeMasteryPointsByRank);
        }

        private bool HasValidMasteryThresholds()
        {
            if (cumulativeMasteryPointsByRank == null ||
                cumulativeMasteryPointsByRank.Length == 0 ||
                cumulativeMasteryPointsByRank[0] != 0)
            {
                return false;
            }

            for (int index = 1; index < cumulativeMasteryPointsByRank.Length; index++)
            {
                if (cumulativeMasteryPointsByRank[index] <=
                    cumulativeMasteryPointsByRank[index - 1])
                {
                    return false;
                }
            }

            return true;
        }

        private void OnValidate()
        {
            artId = StableContentId.Normalize(artId);
            displayName = displayName?.Trim() ?? string.Empty;
            description = description?.Trim() ?? string.Empty;
            encyclopediaDescription = encyclopediaDescription?.Trim() ?? string.Empty;
            category = category?.Trim() ?? string.Empty;

            if (cumulativeMasteryPointsByRank == null ||
                cumulativeMasteryPointsByRank.Length == 0)
            {
                cumulativeMasteryPointsByRank = new long[] { 0 };
            }

            cumulativeMasteryPointsByRank[0] = 0;
            for (int index = 1; index < cumulativeMasteryPointsByRank.Length; index++)
            {
                cumulativeMasteryPointsByRank[index] = Math.Max(
                    cumulativeMasteryPointsByRank[index - 1] + 1,
                    cumulativeMasteryPointsByRank[index]);
            }

            abilityUnlocks ??= Array.Empty<ArtAbilityUnlockEntry>();
            foreach (ArtAbilityUnlockEntry entry in abilityUnlocks)
            {
                entry?.Normalize(MaxRank);
            }
        }
    }
}
