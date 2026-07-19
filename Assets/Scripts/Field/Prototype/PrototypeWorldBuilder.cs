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

            new TerrainBuilder(shapes).Build(world);
            new ArchitectureBuilder(shapes, ambientEffects).Build(world);
            new NatureBuilder(shapes, ambientEffects).Build(world);
            new AtmosphereBuilder(shapes, ambientEffects).Build(world);
            new PrototypePlayerSpawner(shapes).Spawn(world);

            return world;
        }
    }
}
