using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 試作フィールド全体の構築順序を管理する構成ルートです。
    /// 各Builderの具体的な描画処理は持たず、依存関係の組み立てだけを担当します。
    /// </summary>
    internal sealed class PrototypeWorldBuilder
    {
        public Transform Build()
        {
            Transform world = new GameObject("夕映えの学園草原").transform;
            AmbientEffectController ambientEffects = world.gameObject.AddComponent<AmbientEffectController>();
            var shapes = new RuntimeShapeFactory();
            var terrain = new TerrainBuilder(shapes);
            var architecture = new ArchitectureBuilder(shapes, ambientEffects);

            // 既存プロトタイプと同じ生成順を維持し、同一SortingOrder時の見え方の変化を避けます。
            terrain.BuildBase(world);
            architecture.BuildStructures(world);
            new NatureBuilder(shapes, ambientEffects).Build(world);
            architecture.BuildLandmarksAndLighting(world);
            new AtmosphereBuilder(shapes, ambientEffects).Build(world);
            terrain.BuildForeground(world);
            new PrototypePlayerSpawner().Spawn(world);

            return world;
        }
    }
}
