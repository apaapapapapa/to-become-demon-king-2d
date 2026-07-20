using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Progression;
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
            RewardService rewardService)
        {
            ProgressionAcquisitionService = progressionAcquisitionService;
            RewardService = rewardService;
        }

        public ProgressionAcquisitionService ProgressionAcquisitionService { get; }
        public RewardService RewardService { get; }
    }

    internal static class PrototypeGameplayServicesFactory
    {
        public static bool TryCreate(GameObject player, out PrototypeGameplayServices services)
        {
            services = null;
            if (player == null)
            {
                Debug.LogError("Gameplay Serviceを初期化できません。プレイヤーが生成されていません。");
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
            services = new PrototypeGameplayServices(acquisitionService, rewardService);
            return true;
        }
    }
}
