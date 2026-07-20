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
    /// 恒久機能のロジックは持たず、Prototypeシーン向けの生成と依存注入だけを担当します。
    /// Feature間のイベント調停はPrototypeTrainingAreaCoordinatorへ委譲します。
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

            GameObject coordinatorObject = new("訓練エリア制御");
            coordinatorObject.transform.SetParent(parent, false);
            PrototypeTrainingAreaCoordinator coordinator =
                coordinatorObject.AddComponent<PrototypeTrainingAreaCoordinator>();
            coordinator.Initialize(
                npc,
                dummyRespawner,
                acquisitionService,
                fireMagicTrainingGrant,
                dialogueLog,
                rewardService);
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
    }
}
