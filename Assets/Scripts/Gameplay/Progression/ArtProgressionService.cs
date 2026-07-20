using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Modifiers;
using UnityEngine;

namespace DemonKing.Gameplay.Progression
{
    public enum ArtLearnStatus
    {
        Succeeded = 0,
        InvalidArtId = 1,
        DefinitionNotFound = 2,
        AlreadyLearned = 3
    }

    public readonly struct ArtLearnResult
    {
        internal ArtLearnResult(
            ArtLearnStatus status,
            string artId,
            int currentRank,
            IReadOnlyList<string> newlyGrantedAbilityIds)
        {
            Status = status;
            ArtId = artId ?? string.Empty;
            CurrentRank = currentRank;
            NewlyGrantedAbilityIds = newlyGrantedAbilityIds ?? Array.Empty<string>();
        }

        public ArtLearnStatus Status { get; }
        public string ArtId { get; }
        public int CurrentRank { get; }
        public IReadOnlyList<string> NewlyGrantedAbilityIds { get; }
        public bool Succeeded => Status == ArtLearnStatus.Succeeded;
    }

    public enum ArtMasteryAwardStatus
    {
        Succeeded = 0,
        InvalidArtId = 1,
        DefinitionNotFound = 2,
        ArtNotLearned = 3,
        InvalidAmount = 4,
        InvalidEffect = 5,
        AbilityNotOwnedByArt = 6,
        AlreadyAwarded = 7
    }

    public readonly struct ArtMasteryAwardResult
    {
        internal ArtMasteryAwardResult(
            ArtMasteryAwardStatus status,
            string artId,
            long appliedMasteryPoints,
            int previousRank,
            int currentRank,
            IReadOnlyList<string> newlyGrantedAbilityIds)
        {
            Status = status;
            ArtId = artId ?? string.Empty;
            AppliedMasteryPoints = appliedMasteryPoints;
            PreviousRank = previousRank;
            CurrentRank = currentRank;
            NewlyGrantedAbilityIds = newlyGrantedAbilityIds ?? Array.Empty<string>();
        }

        public ArtMasteryAwardStatus Status { get; }
        public string ArtId { get; }
        public long AppliedMasteryPoints { get; }
        public int PreviousRank { get; }
        public int CurrentRank { get; }
        public IReadOnlyList<string> NewlyGrantedAbilityIds { get; }
        public bool Succeeded => Status == ArtMasteryAwardStatus.Succeeded;
        public bool DidRankUp => CurrentRank > PreviousRank;
    }

    /// <summary>
    /// Art習得、熟練度、ランクに応じたAbility付与を調停します。
    /// Abilityの効果処理を知らず、共通の効果成立通知だけを受け取ります。
    /// </summary>
    public sealed class ArtProgressionService
    {
        private const string ArtIdPrefix = "art.";
        private const int RememberedExecutionCapacity = 4096;

        private readonly GameObject owner;
        private readonly CharacterProgressionState progressionState;
        private readonly AbilityController abilityController;
        private readonly Dictionary<string, ArtDefinition> definitions =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, ArtMasteryTable> masteryTables =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, ArtAbilityBinding> abilityBindings =
            new(StringComparer.Ordinal);
        private readonly HashSet<Guid> awardedExecutionIds = new();
        private readonly Queue<Guid> awardedExecutionOrder = new();
        private readonly IReadOnlyList<IArtMasteryModifierSource> masteryModifierSources;

        public ArtProgressionService(
            GameObject owner,
            CharacterProgressionState progressionState,
            AbilityController abilityController,
            IEnumerable<ArtDefinition> artDefinitions,
            IEnumerable<IArtMasteryModifierSource> masteryModifierSources = null)
        {
            this.owner = owner != null
                ? owner
                : throw new ArgumentNullException(nameof(owner));
            this.progressionState = progressionState ??
                throw new ArgumentNullException(nameof(progressionState));
            this.abilityController = abilityController != null
                ? abilityController
                : throw new ArgumentNullException(nameof(abilityController));

            if (abilityController.gameObject != owner)
            {
                throw new ArgumentException(
                    "Artの使用者とAbilityControllerのGameObjectが一致していません。",
                    nameof(abilityController));
            }

            RegisterDefinitions(artDefinitions);
            this.masteryModifierSources = masteryModifierSources == null
                ? Array.Empty<IArtMasteryModifierSource>()
                : new List<IArtMasteryModifierSource>(masteryModifierSources);
            SynchronizeGrantedAbilities();
        }

        public event Action<ArtLearnResult> ArtLearned;
        public event Action<ArtMasteryAwardResult> MasteryAwarded;

        public ArtLearnResult Learn(string artId)
        {
            string normalizedId = StableContentId.Normalize(artId);
            if (!IsValidArtId(normalizedId))
            {
                return RejectedLearn(ArtLearnStatus.InvalidArtId, normalizedId);
            }

            if (!definitions.TryGetValue(normalizedId, out ArtDefinition definition))
            {
                return RejectedLearn(ArtLearnStatus.DefinitionNotFound, normalizedId);
            }

            if (!progressionState.TryLearnArt(normalizedId, out ArtProgressState progressState))
            {
                int existingRank = masteryTables[normalizedId]
                    .GetRankForTotalMasteryPoints(progressState.MasteryPoints);
                return new ArtLearnResult(
                    ArtLearnStatus.AlreadyLearned,
                    normalizedId,
                    existingRank,
                    Array.Empty<string>());
            }

            int currentRank = masteryTables[normalizedId]
                .GetRankForTotalMasteryPoints(progressState.MasteryPoints);
            IReadOnlyList<string> grantedAbilityIds = GrantUnlockedAbilities(
                definition,
                currentRank);
            var result = new ArtLearnResult(
                ArtLearnStatus.Succeeded,
                normalizedId,
                currentRank,
                grantedAbilityIds);
            ArtLearned?.Invoke(result);
            return result;
        }

        public ArtMasteryAwardResult AwardMastery(string artId, long amount)
        {
            string normalizedId = StableContentId.Normalize(artId);
            if (!IsValidArtId(normalizedId))
            {
                return RejectedMastery(
                    ArtMasteryAwardStatus.InvalidArtId,
                    normalizedId);
            }

            if (amount <= 0)
            {
                return RejectedMastery(
                    ArtMasteryAwardStatus.InvalidAmount,
                    normalizedId);
            }

            if (!definitions.TryGetValue(normalizedId, out ArtDefinition definition))
            {
                return RejectedMastery(
                    ArtMasteryAwardStatus.DefinitionNotFound,
                    normalizedId);
            }

            if (!progressionState.TryGetArtProgress(normalizedId, out ArtProgressState progressState))
            {
                return RejectedMastery(
                    ArtMasteryAwardStatus.ArtNotLearned,
                    normalizedId);
            }

            ArtMasteryTable masteryTable = masteryTables[normalizedId];
            amount = ResolveMasteryPoints(definition, amount);
            int previousRank = masteryTable.GetRankForTotalMasteryPoints(
                progressState.MasteryPoints);
            long appliedPoints = progressState.GainMastery(amount);
            int currentRank = masteryTable.GetRankForTotalMasteryPoints(
                progressState.MasteryPoints);
            IReadOnlyList<string> grantedAbilityIds = GrantUnlockedAbilities(
                definition,
                currentRank);
            var result = new ArtMasteryAwardResult(
                ArtMasteryAwardStatus.Succeeded,
                normalizedId,
                appliedPoints,
                previousRank,
                currentRank,
                grantedAbilityIds);
            MasteryAwarded?.Invoke(result);
            return result;
        }

        public ArtMasteryAwardResult AwardMastery(AbilityEffectResolved effect)
        {
            if (!effect.WasApplied ||
                effect.ExecutionId == Guid.Empty ||
                effect.User != owner)
            {
                return RejectedMastery(
                    ArtMasteryAwardStatus.InvalidEffect,
                    string.Empty);
            }

            if (!abilityBindings.TryGetValue(
                    StableContentId.Normalize(effect.AbilityId),
                    out ArtAbilityBinding binding))
            {
                return RejectedMastery(
                    ArtMasteryAwardStatus.AbilityNotOwnedByArt,
                    string.Empty);
            }

            if (!progressionState.TryGetArtProgress(
                    binding.Definition.ArtId,
                    out _))
            {
                return RejectedMastery(
                    ArtMasteryAwardStatus.ArtNotLearned,
                    binding.Definition.ArtId);
            }

            if (!RememberAwardedExecution(effect.ExecutionId))
            {
                return RejectedMastery(
                    ArtMasteryAwardStatus.AlreadyAwarded,
                    binding.Definition.ArtId);
            }

            return AwardMastery(
                binding.Definition.ArtId,
                binding.Entry.MasteryPointsPerEffectiveUse);
        }

        public bool TryGetCurrentRank(string artId, out int rank)
        {
            string normalizedId = StableContentId.Normalize(artId);
            if (definitions.TryGetValue(normalizedId, out _) &&
                progressionState.TryGetArtProgress(normalizedId, out ArtProgressState progressState))
            {
                rank = masteryTables[normalizedId]
                    .GetRankForTotalMasteryPoints(progressState.MasteryPoints);
                return true;
            }

            rank = 0;
            return false;
        }

        private void RegisterDefinitions(IEnumerable<ArtDefinition> artDefinitions)
        {
            if (artDefinitions == null)
            {
                throw new ArgumentNullException(nameof(artDefinitions));
            }

            foreach (ArtDefinition definition in artDefinitions)
            {
                if (definition == null || !definition.IsConfigured)
                {
                    throw new ArgumentException(
                        "正しく設定されたArtDefinitionだけを登録できます。",
                        nameof(artDefinitions));
                }

                if (!definitions.TryAdd(definition.ArtId, definition))
                {
                    throw new ArgumentException(
                        $"Art IDが重複しています: {definition.ArtId}",
                        nameof(artDefinitions));
                }

                masteryTables.Add(definition.ArtId, definition.CreateMasteryTable());
                foreach (ArtAbilityUnlockEntry entry in definition.AbilityUnlocks)
                {
                    string abilityId = entry.AbilityDefinition.AbilityId;
                    if (!abilityBindings.TryAdd(
                            abilityId,
                            new ArtAbilityBinding(definition, entry)))
                    {
                        throw new ArgumentException(
                            $"複数Artへ同じAbilityを登録できません: {abilityId}",
                            nameof(artDefinitions));
                    }
                }
            }
        }

        private void SynchronizeGrantedAbilities()
        {
            foreach (ArtProgressState progressState in progressionState.ArtProgressStates)
            {
                if (!definitions.TryGetValue(progressState.ArtId, out ArtDefinition definition))
                {
                    continue;
                }

                int rank = masteryTables[progressState.ArtId]
                    .GetRankForTotalMasteryPoints(progressState.MasteryPoints);
                GrantUnlockedAbilities(definition, rank);
            }
        }

        private IReadOnlyList<string> GrantUnlockedAbilities(
            ArtDefinition definition,
            int currentRank)
        {
            var grantedAbilityIds = new List<string>();
            foreach (ArtAbilityUnlockEntry entry in definition.AbilityUnlocks)
            {
                if (entry.RequiredRank <= currentRank &&
                    abilityController.GrantAbility(entry.AbilityDefinition))
                {
                    grantedAbilityIds.Add(entry.AbilityDefinition.AbilityId);
                }
            }

            return grantedAbilityIds;
        }

        private bool RememberAwardedExecution(Guid executionId)
        {
            if (!awardedExecutionIds.Add(executionId))
            {
                return false;
            }

            awardedExecutionOrder.Enqueue(executionId);
            if (awardedExecutionOrder.Count > RememberedExecutionCapacity)
            {
                awardedExecutionIds.Remove(awardedExecutionOrder.Dequeue());
            }

            return true;
        }

        private long ResolveMasteryPoints(ArtDefinition definition, long baseAmount)
        {
            NumericModifier modifier = NumericModifier.Identity;
            foreach (IArtMasteryModifierSource source in masteryModifierSources)
            {
                if (source != null)
                {
                    modifier = modifier.Combine(source.GetArtMasteryModifier(definition));
                }
            }

            return modifier.Apply(baseAmount, minimumValue: 1);
        }

        private static bool IsValidArtId(string artId)
        {
            return StableContentId.IsValid(artId) &&
                   artId.StartsWith(ArtIdPrefix, StringComparison.Ordinal);
        }

        private static ArtLearnResult RejectedLearn(ArtLearnStatus status, string artId)
        {
            return new ArtLearnResult(status, artId, 0, Array.Empty<string>());
        }

        private static ArtMasteryAwardResult RejectedMastery(
            ArtMasteryAwardStatus status,
            string artId)
        {
            return new ArtMasteryAwardResult(
                status,
                artId,
                0,
                0,
                0,
                Array.Empty<string>());
        }

        private readonly struct ArtAbilityBinding
        {
            public ArtAbilityBinding(
                ArtDefinition definition,
                ArtAbilityUnlockEntry entry)
            {
                Definition = definition;
                Entry = entry;
            }

            public ArtDefinition Definition { get; }
            public ArtAbilityUnlockEntry Entry { get; }
        }
    }
}
