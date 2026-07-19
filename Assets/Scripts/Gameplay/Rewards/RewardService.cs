using System;
using System.Collections.Generic;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Rewards.Configuration;

namespace DemonKing.Gameplay.Rewards
{
    public enum RewardGrantFailureReason
    {
        None = 0,
        InvalidDefeatContext = 1,
        InvalidRewardDefinition = 2,
        AttackerMismatch = 3,
        RewardDefinitionMismatch = 4,
        AlreadyGranted = 5
    }

    /// <summary>
    /// 報酬付与の成否と、経験値による成長結果をまとめます。
    /// </summary>
    public readonly struct RewardGrantResult
    {
        internal RewardGrantResult(
            bool wasGranted,
            string rewardDefinitionId,
            RewardGrantFailureReason failureReason,
            LevelUpResult levelUpResult)
        {
            WasGranted = wasGranted;
            RewardDefinitionId = rewardDefinitionId ?? string.Empty;
            FailureReason = failureReason;
            LevelUpResult = levelUpResult;
        }

        public bool WasGranted { get; }
        public string RewardDefinitionId { get; }
        public RewardGrantFailureReason FailureReason { get; }
        public LevelUpResult LevelUpResult { get; }
        public long GrantedExperience => LevelUpResult.AppliedExperience;
    }

    /// <summary>
    /// 撃破結果と報酬定義を検証し、プレイヤーの成長状態へ報酬を反映します。
    /// 敵やHealthから経験値の加算処理を分離するためのGameplayサービスです。
    /// </summary>
    public sealed class RewardService
    {
        private readonly CharacterRuntimeContext recipient;
        private readonly ExperienceTable experienceTable;
        private readonly HashSet<Guid> rewardedDefeatIds = new HashSet<Guid>();

        public RewardService(CharacterRuntimeContext recipient)
        {
            this.recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));

            if (recipient.Definition.ExperienceTableDefinition == null ||
                !recipient.Definition.ExperienceTableDefinition.IsConfigured)
            {
                throw new ArgumentException(
                    "報酬受取キャラクターの経験値テーブルが正しく設定されていません。",
                    nameof(recipient));
            }

            experienceTable = recipient.Definition.ExperienceTableDefinition.CreateRuntimeTable();
        }

        public event Action<RewardGrantResult> RewardGranted;

        public RewardGrantResult GrantDefeatReward(
            DefeatContext defeatContext,
            RewardDefinition rewardDefinition)
        {
            if (defeatContext == null || defeatContext.DefeatId == Guid.Empty)
            {
                return Rejected(
                    rewardDefinition == null ? string.Empty : rewardDefinition.RewardId,
                    RewardGrantFailureReason.InvalidDefeatContext);
            }

            if (rewardDefinition == null || !rewardDefinition.IsConfigured)
            {
                return Rejected(
                    rewardDefinition == null ? string.Empty : rewardDefinition.RewardId,
                    RewardGrantFailureReason.InvalidRewardDefinition);
            }

            if (!string.Equals(
                    defeatContext.AttackerActorId,
                    recipient.Definition.CharacterId,
                    StringComparison.Ordinal))
            {
                return Rejected(
                    rewardDefinition.RewardId,
                    RewardGrantFailureReason.AttackerMismatch);
            }

            if (!string.Equals(
                    defeatContext.RewardDefinitionId,
                    rewardDefinition.RewardId,
                    StringComparison.Ordinal))
            {
                return Rejected(
                    rewardDefinition.RewardId,
                    RewardGrantFailureReason.RewardDefinitionMismatch);
            }

            if (rewardedDefeatIds.Contains(defeatContext.DefeatId))
            {
                return Rejected(
                    rewardDefinition.RewardId,
                    RewardGrantFailureReason.AlreadyGranted);
            }

            LevelUpResult levelUpResult = recipient.ProgressionState.GainExperience(
                rewardDefinition.Experience,
                experienceTable);
            rewardedDefeatIds.Add(defeatContext.DefeatId);

            var result = new RewardGrantResult(
                true,
                rewardDefinition.RewardId,
                RewardGrantFailureReason.None,
                levelUpResult);
            RewardGranted?.Invoke(result);
            return result;
        }

        private static RewardGrantResult Rejected(
            string rewardDefinitionId,
            RewardGrantFailureReason failureReason)
        {
            return new RewardGrantResult(
                false,
                rewardDefinitionId,
                failureReason,
                default);
        }
    }
}
