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
    /// <summary>
    /// PrototypeのGameplay Featureが共有するRuntime Service群です。
    /// World構築側が個別Controllerを探索し続けないよう、Composition Rootで一度だけ生成します。
    /// </summary>
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
            var gameplayEventHub = new GameplayEventHub();
            var questProgressionService = new QuestProgressionService(questDefinitions);
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
