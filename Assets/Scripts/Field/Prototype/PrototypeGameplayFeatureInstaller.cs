using System;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.AI;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Dialogue.Configuration;
using DemonKing.Gameplay.Rewards;
using DemonKing.Gameplay.Spawning;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// InteractionとCombatの最小プレイ可能ループを確認するため、試作NPCと訓練用スライムを配置します。
    /// 恒久機能のロジックは持たず、Prototypeシーン向けの生成と依存注入だけを担当します。
    /// </summary>
    internal sealed class PrototypeGameplayFeatureInstaller
    {
        public void Install(
            Transform parent,
            GameObject player,
            PrototypeGameplayServices gameplayServices,
            TrainingScenarioDefinition scenario,
            DialogueLog dialogueLog)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (gameplayServices == null)
            {
                throw new ArgumentNullException(nameof(gameplayServices));
            }

            if (scenario == null || !scenario.IsConfigured)
            {
                throw new ArgumentException(
                    "訓練シナリオ定義が正しく設定されていません。",
                    nameof(scenario));
            }

            if (dialogueLog == null)
            {
                throw new ArgumentNullException(nameof(dialogueLog));
            }

            PrototypeNpcInteractable npc = CreateNpc(parent, dialogueLog, scenario.OfferDialogue);
            var dummyFactory = new PrototypeCombatDummyFactory(
                parent,
                new Vector3(1.45f, -0.45f, 0f),
                dummy => ConfigureCombatDummy(
                    dummy,
                    player,
                    scenario,
                    gameplayServices.RewardService));
            var dummyLifecycle = new SpawnLifecycle<PrototypeCombatDummy>(
                dummyFactory.Spawn,
                dummy => dummy != null && dummy.IsAlive,
                dummy => dummy.RestoreToFull());

            GameObject controllerObject = new("訓練エリア制御");
            controllerObject.transform.SetParent(parent, false);

            TrainingQuestFlowController questFlow =
                controllerObject.AddComponent<TrainingQuestFlowController>();
            questFlow.Initialize(
                npc,
                dummyLifecycle,
                gameplayServices.ProgressionAcquisitionService,
                dialogueLog,
                gameplayServices.GameplayEventHub,
                gameplayServices.QuestProgressionService,
                scenario);

            TrainingDummyEventBridge dummyEventBridge =
                controllerObject.AddComponent<TrainingDummyEventBridge>();
            dummyEventBridge.Initialize(
                dummyLifecycle,
                gameplayServices.GameplayEventHub);

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
            GameObject player,
            TrainingScenarioDefinition scenario,
            RewardService rewardService)
        {
            dummy.ConfigureReward(scenario.DefeatReward);
            dummy.gameObject.AddComponent<PrototypeMonsterDefeatEffect>();

            EnemyAiController enemyAi = dummy.GetComponent<EnemyAiController>();
            if (enemyAi == null)
            {
                enemyAi = dummy.gameObject.AddComponent<EnemyAiController>();
            }

            enemyAi.Configure(scenario.EnemyAiDefinition, player);
            dummy.Defeated += context =>
            {
                RewardGrantResult result = rewardService.GrantDefeatReward(
                    context,
                    scenario.DefeatReward);
                if (!result.WasGranted)
                {
                    Debug.LogWarning($"撃破報酬を付与できませんでした: {result.FailureReason}");
                }
            };
        }
    }
}
