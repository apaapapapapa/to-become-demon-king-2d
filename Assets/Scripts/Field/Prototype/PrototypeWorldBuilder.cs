using DemonKing.Gameplay.Characters;
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

        public PrototypeWorldBuilder(
            Vector3 playerSpawnPosition,
            int playableTileRadius,
            PrototypeProjectAssets projectAssets)
        {
            this.playerSpawnPosition = playerSpawnPosition;
            this.playableTileRadius = Mathf.Max(4, playableTileRadius);
            this.projectAssets = projectAssets;
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

            RewardService rewardService = CreateRewardService(player);
            if (rewardService != null)
            {
                new PrototypeGameplayFeatureInstaller().Install(
                    world,
                    rewardService,
                    projectAssets.TrainingDummyReward);
            }

            terrain.BuildForeground(world);

            PrototypeCameraInstaller.Configure(Camera.main, player == null ? null : player.transform);
            return new PrototypeWorldBuildResult(world, player, rewardService);
        }

        private static RewardService CreateRewardService(GameObject player)
        {
            CharacterRuntimeContextHost contextHost = player == null
                ? null
                : player.GetComponent<CharacterRuntimeContextHost>();
            if (contextHost == null || !contextHost.IsInitialized)
            {
                Debug.LogError("プレイヤーのCharacterRuntimeContextが見つからないため、報酬処理を初期化できません。");
                return null;
            }

            return new RewardService(contextHost.Context);
        }
    }
}
