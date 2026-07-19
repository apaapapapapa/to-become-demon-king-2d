using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 柵やランドマークなどの軽量な試作要素を構築し、校舎と街灯はPrefabとして配置します。
    /// 大きなワールド要素の見た目はBuilderから分離し、Prefab単位で差し替え可能にします。
    /// </summary>
    internal sealed class ArchitectureBuilder
    {
        private readonly RuntimeShapeFactory shapes;
        private readonly PrototypeWorldPrefabFactory prefabs;

        public ArchitectureBuilder(RuntimeShapeFactory shapes, PrototypeWorldPrefabFactory prefabs)
        {
            this.shapes = shapes;
            this.prefabs = prefabs;
        }

        public void BuildStructures(Transform parent)
        {
            CreateFence(parent);
            prefabs.CreateCottage(new Vector2(-4.55f, 1.15f), parent);
        }

        public void BuildLandmarksAndLighting(Transform parent)
        {
            CreateLandmarks(parent);
            prefabs.CreateLamppost(new Vector2(-1.10f, 2.08f), parent);
            prefabs.CreateLamppost(new Vector2(2.15f, -2.10f), parent);
        }

        private void CreateFence(Transform parent)
        {
            const float baseY = 2.72f;
            int order = PrototypeWorldMath.SortOrder(baseY);
            shapes.CreatePatch("柵の下段", new Vector2(0.75f, baseY + 0.30f), new Vector2(6.5f, 0.11f),
                new Color(0.38f, 0.25f, 0.18f), order, parent, rotation: -2f);
            shapes.CreatePatch("柵の上段", new Vector2(0.75f, baseY + 0.62f), new Vector2(6.5f, 0.12f),
                new Color(0.50f, 0.33f, 0.21f), order + 1, parent, rotation: -2f);

            for (int i = 0; i < 8; i++)
            {
                float x = -2.45f + i * 0.92f;
                shapes.CreatePatch("柵柱", new Vector2(x, baseY + 0.43f), new Vector2(0.14f, 0.92f),
                    PrototypePalette.Wood, order + 2, parent);
                shapes.CreateDiamond("柵柱の飾り", new Vector2(x, baseY + 0.93f), new Vector2(0.22f, 0.15f),
                    new Color(0.61f, 0.42f, 0.25f), order + 3, parent);
            }
        }

        private void CreateLandmarks(Transform parent)
        {
            Vector2 stone = new(-1.95f, -2.18f);
            int stoneOrder = PrototypeWorldMath.SortOrder(stone.y);
            shapes.CreateEllipse("古代石の影", stone + new Vector2(0.12f, -0.16f), new Vector2(1.18f, 0.34f),
                new Color(0.06f, 0.15f, 0.14f, 0.58f), stoneOrder - 2, parent);
            shapes.CreatePatch("古代石", stone + Vector2.up * 0.48f, new Vector2(0.74f, 1.22f),
                new Color(0.38f, 0.44f, 0.41f), stoneOrder, parent, rotation: -4f);
            shapes.CreatePatch("古代石の光", stone + new Vector2(-0.18f, 0.60f), new Vector2(0.12f, 0.70f),
                new Color(0.65f, 0.79f, 0.62f, 0.72f), stoneOrder + 1, parent, rotation: -4f);
            shapes.CreatePatch("石のルーン", stone + Vector2.up * 0.52f, new Vector2(0.09f, 0.48f),
                new Color(0.57f, 0.92f, 0.72f), stoneOrder + 2, parent);

            Vector2 sign = new(-0.62f, -0.45f);
            int signOrder = PrototypeWorldMath.SortOrder(sign.y);
            shapes.CreatePatch("案内板の柱", sign + Vector2.up * 0.42f, new Vector2(0.13f, 0.88f),
                PrototypePalette.Wood, signOrder, parent);
            shapes.CreatePatch("案内板", sign + Vector2.up * 0.82f, new Vector2(1.18f, 0.48f),
                new Color(0.53f, 0.34f, 0.20f), signOrder + 1, parent, rotation: -2f);
            shapes.CreateDiamond("案内板の矢印", sign + new Vector2(0.30f, 0.82f), new Vector2(0.28f, 0.20f),
                PrototypePalette.PathLight, signOrder + 2, parent);
        }
    }
}
