using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Rewards;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 試作フィールド全体の構築順序を管理する構成ルートです。
    /// Tilemap、衝突、Prefab、演出、試作Gameplay Feature、プレイヤー、カメラを組み合わせます。
    /// </summary>
    internal sealed class PrototypeWorldBuilder
    {
        private readonly Vector3 playerSpawnPosition;
        private readonly int playableTileRadius;
        private readonly PrototypeProjectAssets projectAssets;
        private readonly DialogueLog dialogueLog;

        public PrototypeWorldBuilder(
            Vector3 playerSpawnPosition,
            int playableTileRadius,
            PrototypeProjectAssets projectAssets,
            DialogueLog dialogueLog)
        {
            this.playerSpawnPosition = playerSpawnPosition;
            this.playableTileRadius = Mathf.Max(4, playableTileRadius);
            this.projectAssets = projectAssets;
            this.dialogueLog = dialogueLog;
        }

        public PrototypeWorldBuildResult Build()
        {
            Transform world = new GameObject("夕映えの学園草原").transform;
            AmbientEffectController ambientEffects = world.gameObject.AddComponent<AmbientEffectController>();

            var shapes = new RuntimeShapeFactory();
            PrototypeTilemapContext tilemaps = PrototypeTilemapContext.Resolve();
            var tiles = new PrototypeRuntimeTileFactory(projectAssets.GrassTileSprite, projectAssets.PathTileSprite);
            var prefabs = new PrototypeWorldPrefabFactory(projectAssets);
            var terrain = new TerrainBuilder(shapes, tilemaps, tiles, playableTileRadius);
            var architecture = new ArchitectureBuilder(shapes, prefabs);

            terrain.BuildBase(world);
            new CollisionMapBuilder(tilemaps, tiles, playableTileRadius).Build();
            architecture.BuildStructures(world);
            new NatureBuilder(shapes, ambientEffects, prefabs).Build(world);
            architecture.BuildLandmarksAndLighting(world);
            new AtmosphereBuilder(shapes, ambientEffects).Build(world);
            GameObject player = new PrototypePlayerSpawner(
                    playerSpawnPosition,
                    projectAssets.PlayerCharacter)
                .Spawn(world);

            ProgressionAcquisitionService acquisitionService = CreateAcquisitionService(player);
            RewardService rewardService = acquisitionService == null
                ? null
                : CreateRewardService(player, acquisitionService);
            if (rewardService != null)
            {
                new PrototypeGameplayFeatureInstaller().Install(
                    world,
                    rewardService,
                    projectAssets.TrainingDummyReward,
                    acquisitionService,
                    projectAssets.FireMagicTrainingGrant,
                    dialogueLog);
            }

            terrain.BuildForeground(world);

            PrototypeCameraInstaller.Configure(Camera.main, player == null ? null : player.transform);
            return new PrototypeWorldBuildResult(world, player, rewardService);
        }

        private static ProgressionAcquisitionService CreateAcquisitionService(GameObject player)
        {
            ArtProgressionController artController = player == null
                ? null
                : player.GetComponent<ArtProgressionController>();
            SkillProgressionController skillController = player == null
                ? null
                : player.GetComponent<SkillProgressionController>();
            if (artController == null || skillController == null ||
                !artController.IsInitialized || !skillController.IsInitialized)
            {
                Debug.LogError("Art・Skill取得サービスを初期化できません。プレイヤーの進行Controllerを確認してください。");
                return null;
            }

            return new ProgressionAcquisitionService(artController, skillController);
        }

        private static RewardService CreateRewardService(
            GameObject player,
            ProgressionAcquisitionService acquisitionService)
        {
            CharacterRuntimeContextHost contextHost = player == null
                ? null
                : player.GetComponent<CharacterRuntimeContextHost>();
            if (contextHost == null || !contextHost.IsInitialized)
            {
                Debug.LogError("プレイヤーのCharacterRuntimeContextが見つからないため、報酬処理を初期化できません。");
                return null;
            }

            return new RewardService(contextHost.Context, acquisitionService);
        }
    }
}
