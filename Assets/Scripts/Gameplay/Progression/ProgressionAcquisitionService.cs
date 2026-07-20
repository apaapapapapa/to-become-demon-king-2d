using System;
using System.Collections.Generic;
using DemonKing.Gameplay.Progression.Configuration;

namespace DemonKing.Gameplay.Progression
{
    public readonly struct ProgressionGrantResult
    {
        private readonly string grantId;
        private readonly IReadOnlyList<string> learnedArtIds;
        private readonly IReadOnlyList<string> unlockedSkillIds;

        internal ProgressionGrantResult(
            string grantId,
            IReadOnlyList<string> learnedArtIds,
            IReadOnlyList<string> unlockedSkillIds)
        {
            this.grantId = grantId ?? string.Empty;
            this.learnedArtIds = learnedArtIds ?? Array.Empty<string>();
            this.unlockedSkillIds = unlockedSkillIds ?? Array.Empty<string>();
        }

        public string GrantId => grantId ?? string.Empty;
        public IReadOnlyList<string> LearnedArtIds =>
            learnedArtIds ?? Array.Empty<string>();
        public IReadOnlyList<string> UnlockedSkillIds =>
            unlockedSkillIds ?? Array.Empty<string>();
        public bool WasGranted => LearnedArtIds.Count + UnlockedSkillIds.Count > 0;
    }

    /// <summary>
    /// 複数の取得元から送られるArt習得とSkill取得を共通サービスへ中継します。
    /// 訓練条件、報酬条件、アイテム消費などの取得元固有ルールは扱いません。
    /// </summary>
    public sealed class ProgressionAcquisitionService
    {
        private readonly ArtProgressionController artController;
        private readonly SkillProgressionController skillController;

        public ProgressionAcquisitionService(
            ArtProgressionController artController,
            SkillProgressionController skillController)
        {
            this.artController = artController != null && artController.IsInitialized
                ? artController
                : throw new ArgumentException(
                    "初期化済みのArtProgressionControllerが必要です。",
                    nameof(artController));
            this.skillController = skillController != null && skillController.IsInitialized
                ? skillController
                : throw new ArgumentException(
                    "初期化済みのSkillProgressionControllerが必要です。",
                    nameof(skillController));
        }

        public event Action<ProgressionGrantResult> ProgressionGranted;

        public ProgressionGrantResult Grant(ProgressionGrantDefinition definition)
        {
            if (definition == null || !definition.IsConfigured)
            {
                throw new ArgumentException(
                    "正しく設定されたProgressionGrantDefinitionが必要です。",
                    nameof(definition));
            }

            var learnedArtIds = new List<string>();
            foreach (ArtDefinition artDefinition in definition.LearnedArts)
            {
                ArtLearnResult result = artController.Learn(artDefinition.ArtId);
                if (result.Succeeded)
                {
                    learnedArtIds.Add(result.ArtId);
                }
            }

            var unlockedSkillIds = new List<string>();
            foreach (SkillDefinition skillDefinition in definition.UnlockedSkills)
            {
                SkillUnlockResult result = skillController.Unlock(skillDefinition.SkillId);
                if (result.Succeeded)
                {
                    unlockedSkillIds.Add(result.SkillId);
                }
            }

            var grantResult = new ProgressionGrantResult(
                definition.GrantId,
                learnedArtIds,
                unlockedSkillIds);
            if (grantResult.WasGranted)
            {
                ProgressionGranted?.Invoke(grantResult);
            }

            return grantResult;
        }
    }
}
