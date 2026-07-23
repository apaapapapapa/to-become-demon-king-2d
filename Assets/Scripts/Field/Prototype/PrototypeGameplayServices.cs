using System.Collections.Generic;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Content;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using DemonKing.Gameplay.Rewards;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    internal sealed class PrototypeGameplayServices
    {
        public PrototypeGameplayServices(
            ProgressionAcquisitionService progressionAcquisitionService,
            RewardService rewardService,
            GameplayEventHub gameplayEventHub,
            QuestProgressionService questProgressionService,
            GameContentCatalog gameContentCatalog)
        {
            ProgressionAcquisitionService = progressionAcquisitionService;
            RewardService = rewardService;
            GameplayEventHub = gameplayEventHub;
            QuestProgressionService = questProgressionService;
            GameContentCatalog = gameContentCatalog;
        }

        public ProgressionAcquisitionService ProgressionAcquisitionService { get; }
        public RewardService RewardService { get; }
        public GameplayEventHub GameplayEventHub { get; }
        public QuestProgressionService QuestProgressionService { get; }
        public GameContentCatalog GameContentCatalog { get; }
    }

    internal static class PrototypeGameplayServicesFactory
    {
        public static bool TryCreate(
            GameObject player,
            IEnumerable<QuestDefinition> questDefinitions,
            GameContentCatalog gameContentCatalog,
            out PrototypeGameplayServices services)
        {
            return TryCreate(
                player,
                questDefinitions,
                gameContentCatalog,
                sharedQuestProgressionService: null,
                sharedGameplayEventHub: null,
                out services);
        }

        public static bool TryCreate(
            GameObject player,
            IEnumerable<QuestDefinition> questDefinitions,
            GameContentCatalog gameContentCatalog,
            QuestProgressionService sharedQuestProgressionService,
            out PrototypeGameplayServices services)
        {
            return TryCreate(
                player,
                questDefinitions,
                gameContentCatalog,
                sharedQuestProgressionService,
                sharedGameplayEventHub: null,
                out services);
        }

        /// <summary>
        /// Game Session所有のQuest Service / Gameplay Event HubをField Runtimeへ再接続できます。
        /// Hubを共有する場合、Quest / Story等の購読はSession側で一度だけ行います。
        /// </summary>
        public static bool TryCreate(
            GameObject player,
            IEnumerable<QuestDefinition> questDefinitions,
            GameContentCatalog gameContentCatalog,
            QuestProgressionService sharedQuestProgressionService,
            GameplayEventHub sharedGameplayEventHub,
            out PrototypeGameplayServices services)
        {
            services = null;
            if (player == null)
            {
                Debug.LogError("Gameplay Serviceを初期化できません。プレイヤーが生成されていません。");
                return false;
            }

            if (gameContentCatalog == null)
            {
                Debug.LogError("Gameplay Serviceを初期化できません。GameContentCatalogが生成されていません。");
                return false;
            }

            ArtProgressionController artController = player.GetComponent<ArtProgressionController>();
            SkillProgressionController skillController = player.GetComponent<SkillProgressionController>();
            if (artController == null || skillController == null ||
                !artController.IsInitialized || !skillController.IsInitialized)
            {
                Debug.LogError("Art・Skill取得サービスを初期化できません。プレイヤーの進行Controllerを確認してください。");
                return false;
            }

            CharacterRuntimeContextHost contextHost = player.GetComponent<CharacterRuntimeContextHost>();
            if (contextHost == null || !contextHost.IsInitialized)
            {
                Debug.LogError("プレイヤーのCharacterRuntimeContextが見つからないため、報酬処理を初期化できません。");
                return false;
            }

            var acquisitionService = new ProgressionAcquisitionService(artController, skillController);
            var rewardService = new RewardService(contextHost.Context, acquisitionService);
            QuestProgressionService questProgressionService =
                sharedQuestProgressionService ?? new QuestProgressionService(questDefinitions);
            GameplayEventHub gameplayEventHub =
                sharedGameplayEventHub ?? new GameplayEventHub();

            if (sharedGameplayEventHub == null)
            {
                gameplayEventHub.Published += questProgressionService.Handle;
            }

            services = new PrototypeGameplayServices(
                acquisitionService,
                rewardService,
                gameplayEventHub,
                questProgressionService,
                gameContentCatalog);
            return true;
        }
    }
}
