using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 試作フィールド全体の構築順序を管理する構成ルートです。
    /// Tilemap、衝突、Prefab、演出、プレイヤーを組み合わせます。
    /// </summary>
    internal sealed class PrototypeWorldBuilder
    {
        private readonly Vector3 playerSpawnPosition;
        private readonly int playableTileRadius;

        public PrototypeWorldBuilder(Vector3 playerSpawnPosition, int playableTileRadius)
        {
            this.playerSpawnPosition = playerSpawnPosition;
            this.playableTileRadius = Mathf.Max(4, playableTileRadius);
        }

        public Transform Build()
        {
            Transform world = new GameObject("夕映えの学園草原").transform;
            AmbientEffectController ambientEffects = world.gameObject.AddComponent<AmbientEffectController>();

            var shapes = new RuntimeShapeFactory();
            PrototypeTilemapContext tilemaps = PrototypeTilemapContext.Resolve();
            var tiles = new PrototypeRuntimeTileFactory();
            var prefabs = new PrototypeWorldPrefabFactory();
            var terrain = new TerrainBuilder(shapes, tilemaps, tiles, playableTileRadius);
            var architecture = new ArchitectureBuilder(shapes, prefabs);

            terrain.BuildBase(world);
            new CollisionMapBuilder(tilemaps, tiles, playableTileRadius).Build();
            architecture.BuildStructures(world);
            new NatureBuilder(shapes, ambientEffects, prefabs).Build(world);
            architecture.BuildLandmarksAndLighting(world);
            new AtmosphereBuilder(shapes, ambientEffects).Build(world);
            terrain.BuildForeground(world);
            new PrototypePlayerSpawner(playerSpawnPosition).Spawn(world);

            return world;
        }
    }
}
