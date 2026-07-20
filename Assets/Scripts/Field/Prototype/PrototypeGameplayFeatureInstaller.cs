using System;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Rewards;
using DemonKing.Gameplay.Rewards.Configuration;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// InteractionとCombatの最小プレイ可能ループを確認するため、試作NPCと訓練用ダミーを配置します。
    /// 恒久機能のロジックは持たず、Prototypeシーン向けの組み立てだけを担当します。
    /// </summary>
    internal sealed class PrototypeGameplayFeatureInstaller
    {
        public void Install(
            Transform parent,
            RewardService rewardService,
            RewardDefinition trainingDummyReward,
            ProgressionAcquisitionService acquisitionService,
            ProgressionGrantDefinition fireMagicTrainingGrant,
            DialogueLog dialogueLog)
        {
            if (rewardService == null)
            {
                throw new ArgumentNullException(nameof(rewardService));
            }

            if (trainingDummyReward == null || !trainingDummyReward.IsConfigured)
            {
                throw new ArgumentException(
                    "訓練用ダミーの報酬定義が正しく設定されていません。",
                    nameof(trainingDummyReward));
            }

            if (dialogueLog == null)
            {
                throw new ArgumentNullException(nameof(dialogueLog));
            }

            if (acquisitionService == null)
            {
                throw new ArgumentNullException(nameof(acquisitionService));
            }

            if (fireMagicTrainingGrant == null || !fireMagicTrainingGrant.IsConfigured)
            {
                throw new ArgumentException(
                    "火炎魔法の訓練取得定義が正しく設定されていません。",
                    nameof(fireMagicTrainingGrant));
            }

            PrototypeNpcInteractable npc = CreateNpc(parent, dialogueLog);
            var dummyRespawner = new PrototypeCombatDummyRespawner(
                parent,
                new Vector3(1.45f, -0.45f, 0f),
                dummy => ConfigureCombatDummy(dummy, rewardService, trainingDummyReward));
            dummyRespawner.SpawnOrRestore();
            npc.Interacted += () => dummyRespawner.SpawnOrRestore();
            npc.DialogueCompleted += _ =>
            {
                ProgressionGrantResult result = acquisitionService.Grant(fireMagicTrainingGrant);
                dialogueLog.ShowLine(
                    "見習い魔術師",
                    result.WasGranted
                        ? "火炎魔法を習得した！ Kキー／ゲームパッドYで火炎弾を放てる。"
                        : "火炎魔法はもう身についている。実戦で熟練を重ねよう。");
            };

            rewardService.RewardGranted += LogGrantedReward;
        }

        private static PrototypeNpcInteractable CreateNpc(Transform parent, DialogueLog dialogueLog)
        {
            GameObject npc = new("見習い魔術師");
            npc.transform.SetParent(parent, false);
            npc.transform.localPosition = new Vector3(-0.85f, 0.35f, 0f);
            PrototypeNpcInteractable interactable = npc.AddComponent<PrototypeNpcInteractable>();
            interactable.ConfigureDialogueLog(dialogueLog);
            return interactable;
        }

        private static void ConfigureCombatDummy(
            PrototypeCombatDummy dummy,
            RewardService rewardService,
            RewardDefinition trainingDummyReward)
        {
            dummy.ConfigureReward(trainingDummyReward);
            dummy.gameObject.AddComponent<PrototypeMonsterDefeatEffect>();
            dummy.Defeated += context =>
            {
                RewardGrantResult result = rewardService.GrantDefeatReward(
                    context,
                    trainingDummyReward);
                if (!result.WasGranted)
                {
                    Debug.LogWarning($"撃破報酬を付与できませんでした: {result.FailureReason}");
                }
            };
        }

        private static void LogGrantedReward(RewardGrantResult result)
        {
            string progressionSummary = result.ProgressionGrantResult.WasGranted
                ? $" Art {string.Join(", ", result.ProgressionGrantResult.LearnedArtIds)}" +
                  $" Skill {string.Join(", ", result.ProgressionGrantResult.UnlockedSkillIds)}"
                : string.Empty;
            Debug.Log(
                $"経験値を{result.GrantedExperience}獲得。" +
                $" レベル {result.LevelUpResult.PreviousLevel} → {result.LevelUpResult.CurrentLevel}、" +
                $"累積経験値 {result.LevelUpResult.CurrentExperience}{progressionSummary}");
        }
    }
}
