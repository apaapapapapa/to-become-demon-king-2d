using System;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Dialogue.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Rewards;
using DemonKing.Gameplay.Rewards.Configuration;
using DemonKing.Gameplay.Spawning;
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
            PrototypeGameplayServices gameplayServices,
            RewardDefinition trainingDummyReward,
            ProgressionGrantDefinition fireMagicTrainingGrant,
            DialogueDefinition apprenticeMageDialogue,
            DialogueLog dialogueLog)
        {
            if (gameplayServices == null)
            {
                throw new ArgumentNullException(nameof(gameplayServices));
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

            if (apprenticeMageDialogue == null || !apprenticeMageDialogue.IsConfigured)
            {
                throw new ArgumentException(
                    "見習い魔術師の会話定義が正しく設定されていません。",
                    nameof(apprenticeMageDialogue));
            }

            if (fireMagicTrainingGrant == null || !fireMagicTrainingGrant.IsConfigured)
            {
                throw new ArgumentException(
                    "火炎魔法の訓練取得定義が正しく設定されていません。",
                    nameof(fireMagicTrainingGrant));
            }

            PrototypeNpcInteractable npc = CreateNpc(parent, dialogueLog, apprenticeMageDialogue);
            var dummyFactory = new PrototypeCombatDummyFactory(
                parent,
                new Vector3(1.45f, -0.45f, 0f),
                dummy => ConfigureCombatDummy(dummy, gameplayServices.RewardService, trainingDummyReward));
            var dummyLifecycle = new SpawnLifecycle<PrototypeCombatDummy>(
                dummyFactory.Spawn,
                dummy => dummy != null && dummy.IsAlive,
                dummy => dummy.RestoreToFull());

            GameObject coordinatorObject = new("訓練エリア制御");
            coordinatorObject.transform.SetParent(parent, false);
            PrototypeTrainingAreaCoordinator coordinator =
                coordinatorObject.AddComponent<PrototypeTrainingAreaCoordinator>();
            coordinator.Initialize(
                npc,
                dummyLifecycle,
                gameplayServices.ProgressionAcquisitionService,
                fireMagicTrainingGrant,
                dialogueLog,
                gameplayServices.RewardService,
                gameplayServices.GameplayEventHub,
                gameplayServices.QuestProgressionService);

            dummyLifecycle.SpawnOrRestore();
        }

        private static PrototypeNpcInteractable CreateNpc(
            Transform parent,
            DialogueLog dialogueLog,
            DialogueDefinition dialogueDefinition)
        {
            GameObject npc = new("見習い魔術師");
            npc.transform.SetParent(parent, false);
            npc.transform.localPosition = new Vector3(-0.85f, 0.35f, 0f);
            PrototypeNpcInteractable interactable = npc.AddComponent<PrototypeNpcInteractable>();
            interactable.ConfigureDialogueLog(dialogueLog);
            interactable.ConfigureDialogue(dialogueDefinition);
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
