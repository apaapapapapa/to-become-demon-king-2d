using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 校舎、柵、案内物、街灯など、人の手で作られたフィールド要素を構築します。
    /// </summary>
    internal sealed class ArchitectureBuilder
    {
        private readonly RuntimeShapeFactory shapes;
        private readonly AmbientEffectController ambientEffects;

        public ArchitectureBuilder(RuntimeShapeFactory shapes, AmbientEffectController ambientEffects)
        {
            this.shapes = shapes;
            this.ambientEffects = ambientEffects;
        }

        public void Build(Transform parent)
        {
            CreateFence(parent);
            CreateCottage(parent);
            CreateLandmarks(parent);
            CreateLampposts(parent);
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

        private void CreateCottage(Transform parent)
        {
            Vector2 basePosition = new(-4.55f, 1.15f);
            int order = PrototypeWorldMath.SortOrder(basePosition.y);

            shapes.CreateEllipse("校舎の影", basePosition + new Vector2(0.25f, -0.15f), new Vector2(4.7f, 0.8f),
                new Color(0.06f, 0.14f, 0.13f, 0.62f), order - 5, parent);
            shapes.CreatePatch("校舎の側壁", basePosition + new Vector2(1.35f, 0.82f), new Vector2(1.35f, 1.75f),
                new Color(0.67f, 0.54f, 0.39f), order - 1, parent);
            shapes.CreatePatch("校舎の正面", basePosition + new Vector2(-0.25f, 0.86f), new Vector2(3.25f, 1.82f),
                PrototypePalette.Wall, order, parent);
            shapes.CreatePatch("壁の陰", basePosition + new Vector2(-0.25f, 0.12f), new Vector2(3.25f, 0.28f),
                new Color(0.58f, 0.43f, 0.31f), order + 1, parent);

            shapes.CreatePatch("横梁", basePosition + new Vector2(-0.25f, 1.18f), new Vector2(3.25f, 0.12f),
                PrototypePalette.Wood, order + 2, parent);
            shapes.CreatePatch("左の柱", basePosition + new Vector2(-1.45f, 0.88f), new Vector2(0.13f, 1.72f),
                PrototypePalette.Wood, order + 2, parent);
            shapes.CreatePatch("中央の柱", basePosition + new Vector2(0.05f, 0.88f), new Vector2(0.12f, 1.72f),
                PrototypePalette.Wood, order + 2, parent);

            shapes.CreateDiamond("大屋根の影", basePosition + new Vector2(0.18f, 2.00f), new Vector2(4.55f, 1.82f),
                new Color(0.24f, 0.12f, 0.19f), order + 3, parent);
            shapes.CreateDiamond("大屋根", basePosition + new Vector2(0f, 2.10f), new Vector2(4.42f, 1.78f),
                PrototypePalette.Roof, order + 4, parent);
            shapes.CreatePatch("屋根の明るい縁", basePosition + new Vector2(-0.35f, 2.55f), new Vector2(2.55f, 0.12f),
                PrototypePalette.RoofLight, order + 5, parent, rotation: 11f);
            shapes.CreatePatch("屋根の暗い縁", basePosition + new Vector2(0.74f, 1.65f), new Vector2(2.7f, 0.11f),
                new Color(0.28f, 0.13f, 0.20f), order + 5, parent, rotation: -11f);

            shapes.CreatePatch("煙突", basePosition + new Vector2(1.15f, 2.75f), new Vector2(0.42f, 1.18f),
                new Color(0.39f, 0.28f, 0.25f), order + 3, parent);
            shapes.CreatePatch("煙突の笠", basePosition + new Vector2(1.15f, 3.35f), new Vector2(0.56f, 0.16f),
                new Color(0.22f, 0.16f, 0.17f), order + 4, parent);

            CreateWindow(basePosition + new Vector2(-0.92f, 0.92f), order + 3, parent);
            CreateWindow(basePosition + new Vector2(0.72f, 0.92f), order + 3, parent);
            shapes.CreatePatch("玄関", basePosition + new Vector2(1.37f, 0.59f), new Vector2(0.65f, 1.22f),
                new Color(0.25f, 0.20f, 0.23f), order + 3, parent);
            shapes.CreateEllipse("扉の金具", basePosition + new Vector2(1.18f, 0.62f), new Vector2(0.09f, 0.09f),
                new Color(0.94f, 0.67f, 0.28f), order + 4, parent);

            shapes.CreatePatch("花箱", basePosition + new Vector2(-0.92f, 0.39f), new Vector2(0.93f, 0.20f),
                PrototypePalette.Wood, order + 4, parent);
            CreateFlowerCluster(basePosition + new Vector2(-0.92f, 0.53f), order + 5, parent);
        }

        private void CreateWindow(Vector2 position, int order, Transform parent)
        {
            shapes.CreatePatch("窓枠", position, new Vector2(0.78f, 0.72f), PrototypePalette.Wood, order, parent);
            shapes.CreatePatch("暖かな窓", position, new Vector2(0.61f, 0.55f),
                new Color(0.96f, 0.74f, 0.35f), order + 1, parent);
            shapes.CreatePatch("窓の縦桟", position, new Vector2(0.07f, 0.56f), PrototypePalette.Wood, order + 2, parent);
            shapes.CreatePatch("窓の横桟", position, new Vector2(0.62f, 0.07f), PrototypePalette.Wood, order + 2, parent);
        }

        private void CreateFlowerCluster(Vector2 position, int order, Transform parent)
        {
            for (int i = -2; i <= 2; i++)
            {
                Vector2 flower = position + new Vector2(i * 0.16f, Mathf.Abs(i) * -0.025f);
                shapes.CreateEllipse("花箱の花", flower, new Vector2(0.16f, 0.13f),
                    i % 2 == 0 ? new Color(0.96f, 0.55f, 0.66f) : new Color(0.98f, 0.79f, 0.32f),
                    order, parent);
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

        private void CreateLampposts(Transform parent)
        {
            CreateLamppost(new Vector2(-1.10f, 2.08f), 0.2f, parent);
            CreateLamppost(new Vector2(2.15f, -2.10f), 2.1f, parent);
        }

        private void CreateLamppost(Vector2 basePosition, float phase, Transform parent)
        {
            int order = PrototypeWorldMath.SortOrder(basePosition.y);
            shapes.CreateEllipse("街灯の影", basePosition + new Vector2(0.12f, -0.10f), new Vector2(0.82f, 0.24f),
                new Color(0.05f, 0.13f, 0.13f, 0.55f), order - 2, parent);
            shapes.CreatePatch("街灯の支柱", basePosition + Vector2.up * 0.78f, new Vector2(0.11f, 1.62f),
                new Color(0.19f, 0.19f, 0.22f), order, parent);
            shapes.CreatePatch("街灯の笠", basePosition + Vector2.up * 1.62f, new Vector2(0.54f, 0.18f),
                new Color(0.24f, 0.20f, 0.25f), order + 1, parent);
            shapes.CreateDiamond("街灯の灯り", basePosition + Vector2.up * 1.45f, new Vector2(0.34f, 0.40f),
                new Color(1f, 0.73f, 0.31f), order + 2, parent);
            GameObject glow = shapes.CreateEllipse("街灯の光輪", basePosition + Vector2.up * 1.44f, new Vector2(1.35f, 1.35f),
                new Color(1f, 0.65f, 0.25f, 0.13f), order + 3, parent);
            ambientEffects.Register(glow, Vector2.zero, 0f, 2.4f, phase);
        }
    }
}
