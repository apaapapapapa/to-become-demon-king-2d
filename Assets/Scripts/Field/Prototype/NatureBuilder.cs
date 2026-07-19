using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 池、木、茂み、草花など、自然物を構築します。
    /// </summary>
    internal sealed class NatureBuilder
    {
        private readonly RuntimeShapeFactory shapes;
        private readonly AmbientEffectController ambientEffects;

        public NatureBuilder(RuntimeShapeFactory shapes, AmbientEffectController ambientEffects)
        {
            this.shapes = shapes;
            this.ambientEffects = ambientEffects;
        }

        public void Build(Transform parent)
        {
            CreatePond(parent);
            CreateTrees(parent);
            CreateDecorations(parent);
        }

        private void CreatePond(Transform parent)
        {
            Vector2 center = new(4.45f, 0.55f);
            shapes.CreateDiamond("池の影", center + new Vector2(0.12f, -0.10f), new Vector2(5.1f, 2.42f),
                new Color(0.05f, 0.16f, 0.18f, 0.68f), PrototypeWorldMath.GroundOrder + 110, parent);
            shapes.CreateDiamond("池の岸", center, new Vector2(5f, 2.30f),
                new Color(0.33f, 0.43f, 0.25f), PrototypeWorldMath.GroundOrder + 111, parent);
            shapes.CreateDiamond("池", center, new Vector2(4.22f, 1.75f),
                new Color(0.16f, 0.47f, 0.55f), PrototypeWorldMath.GroundOrder + 112, parent);
            shapes.CreateDiamond("池の浅瀬", center + new Vector2(-0.18f, 0.08f), new Vector2(3.72f, 1.38f),
                new Color(0.28f, 0.64f, 0.63f, 0.58f), PrototypeWorldMath.GroundOrder + 113, parent);

            for (int i = 0; i < 5; i++)
            {
                Vector2 position = center + new Vector2(-1.15f + i * 0.55f, -0.25f + (i % 2) * 0.22f);
                GameObject glint = shapes.CreatePatch("水面のきらめき", position, new Vector2(0.38f, 0.055f),
                    new Color(0.69f, 0.92f, 0.80f, 0.76f), PrototypeWorldMath.GroundOrder + 115, parent);
                ambientEffects.Register(glint, Vector2.right, 0.12f, 0.75f + i * 0.08f, i * 0.9f);
            }

            shapes.CreateEllipse("睡蓮の葉", center + new Vector2(1.18f, 0.08f), new Vector2(0.42f, 0.18f),
                new Color(0.25f, 0.52f, 0.26f), PrototypeWorldMath.GroundOrder + 116, parent);
            shapes.CreateEllipse("睡蓮の花", center + new Vector2(1.22f, 0.19f), new Vector2(0.14f, 0.10f),
                new Color(0.96f, 0.70f, 0.75f), PrototypeWorldMath.GroundOrder + 117, parent);

            for (int i = -3; i <= 3; i++)
            {
                Vector2 plank = center + new Vector2(i * 0.34f, -0.02f * i);
                shapes.CreatePatch("橋板", plank, new Vector2(0.28f, 0.82f),
                    i % 2 == 0 ? new Color(0.48f, 0.31f, 0.20f) : new Color(0.57f, 0.38f, 0.23f),
                    PrototypeWorldMath.GroundOrder + 128, parent, rotation: -3f);
            }

            shapes.CreatePatch("橋の上手すり", center + new Vector2(0f, 0.42f), new Vector2(2.65f, 0.08f),
                PrototypePalette.Wood, PrototypeWorldMath.GroundOrder + 129, parent, rotation: -3f);
            shapes.CreatePatch("橋の下手すり", center + new Vector2(0f, -0.43f), new Vector2(2.65f, 0.08f),
                PrototypePalette.Wood, PrototypeWorldMath.GroundOrder + 129, parent, rotation: -3f);
        }

        private void CreateTrees(Transform parent)
        {
            Vector2[] positions =
            {
                new(-6.95f, 2.18f),
                new(-6.65f, -1.55f),
                new(-3.15f, -2.95f),
                new(2.25f, 3.05f),
                new(6.75f, 2.33f),
                new(6.62f, -1.62f)
            };

            foreach (Vector2 position in positions)
            {
                CreateTree(position, parent);
            }

            CreateBush(new Vector2(-2.55f, 2.65f), parent);
            CreateBush(new Vector2(3.38f, 2.83f), parent);
            CreateBush(new Vector2(6.1f, -2.7f), parent);
        }

        private void CreateTree(Vector2 basePosition, Transform parent)
        {
            int order = PrototypeWorldMath.SortOrder(basePosition.y);
            shapes.CreateEllipse("木の影", basePosition + new Vector2(0.22f, -0.14f), new Vector2(1.85f, 0.48f),
                new Color(0.05f, 0.15f, 0.14f, 0.62f), order - 3, parent);
            shapes.CreatePatch("木の幹", basePosition + new Vector2(0f, 0.72f), new Vector2(0.38f, 1.55f),
                new Color(0.31f, 0.20f, 0.14f), order, parent);
            shapes.CreatePatch("幹の光", basePosition + new Vector2(-0.09f, 0.82f), new Vector2(0.10f, 1.20f),
                new Color(0.55f, 0.37f, 0.20f), order + 1, parent);

            shapes.CreateEllipse("樹冠の影", basePosition + new Vector2(0.12f, 1.72f), new Vector2(2.10f, 1.72f),
                new Color(0.08f, 0.29f, 0.22f), order + 2, parent);
            shapes.CreateEllipse("左の樹冠", basePosition + new Vector2(-0.52f, 1.82f), new Vector2(1.45f, 1.35f),
                new Color(0.12f, 0.42f, 0.25f), order + 3, parent);
            shapes.CreateEllipse("右の樹冠", basePosition + new Vector2(0.48f, 1.98f), new Vector2(1.58f, 1.43f),
                new Color(0.16f, 0.49f, 0.27f), order + 3, parent);
            shapes.CreateEllipse("樹冠の光", basePosition + new Vector2(-0.32f, 2.28f), new Vector2(0.92f, 0.68f),
                new Color(0.39f, 0.68f, 0.34f), order + 4, parent);
            shapes.CreateEllipse("葉のきらめき", basePosition + new Vector2(-0.52f, 2.43f), new Vector2(0.31f, 0.20f),
                new Color(0.65f, 0.82f, 0.45f, 0.72f), order + 5, parent);
        }

        private void CreateBush(Vector2 basePosition, Transform parent)
        {
            int order = PrototypeWorldMath.SortOrder(basePosition.y);
            shapes.CreateEllipse("茂みの影", basePosition + Vector2.down * 0.08f, new Vector2(1.45f, 0.42f),
                new Color(0.06f, 0.18f, 0.15f, 0.58f), order - 1, parent);
            shapes.CreateEllipse("茂み", basePosition + Vector2.up * 0.22f, new Vector2(1.38f, 0.88f),
                new Color(0.14f, 0.42f, 0.24f), order, parent);
            shapes.CreateEllipse("茂みの光", basePosition + new Vector2(-0.28f, 0.39f), new Vector2(0.64f, 0.38f),
                new Color(0.36f, 0.64f, 0.31f), order + 1, parent);
        }

        private void CreateDecorations(Transform parent)
        {
            var random = new System.Random(1837);
            Vector2 cottage = new(-4.55f, 1.15f);
            Vector2 pond = new(4.45f, 0.55f);

            for (int i = 0; i < 74; i++)
            {
                int x = random.Next(-14, 15);
                int y = random.Next(-14, 15);
                if (Mathf.Abs(x) + Mathf.Abs(y) > 14 || Mathf.Abs(y - PrototypeWorldMath.PathY(x)) <= 1)
                {
                    continue;
                }

                Vector2 position = PrototypeWorldMath.Iso(x, y) + new Vector2(
                    PrototypeWorldMath.Next(random, -0.28f, 0.28f),
                    PrototypeWorldMath.Next(random, -0.11f, 0.11f));
                if ((position - cottage).sqrMagnitude < 7f || (position - pond).sqrMagnitude < 6f)
                {
                    continue;
                }

                int order = PrototypeWorldMath.SortOrder(position.y);
                if (i % 7 == 0)
                {
                    CreateFlower(position, order, i % 14 == 0, parent);
                }
                else
                {
                    shapes.CreatePatch("草の影", position + Vector2.down * 0.06f, new Vector2(0.18f, 0.08f),
                        new Color(0.12f, 0.29f, 0.18f, 0.55f), order - 1, parent);
                    shapes.CreatePatch("草の房", position + Vector2.up * 0.08f, new Vector2(0.10f, 0.30f),
                        PrototypePalette.GrassLight, order, parent, rotation: i % 2 == 0 ? -8f : 8f);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 position = new(-6.1f + i * 0.34f, 0.02f + (i % 2) * 0.15f);
                CreateFlower(position, PrototypeWorldMath.SortOrder(position.y), i % 2 == 0, parent);
            }
        }

        private void CreateFlower(Vector2 position, int order, bool pink, Transform parent)
        {
            shapes.CreatePatch("花の茎", position + Vector2.up * 0.08f, new Vector2(0.055f, 0.25f),
                new Color(0.20f, 0.48f, 0.23f), order, parent);
            Color petal = pink ? new Color(0.96f, 0.54f, 0.65f) : new Color(0.94f, 0.78f, 0.30f);
            shapes.CreateEllipse("花びら", position + Vector2.up * 0.23f, new Vector2(0.17f, 0.14f),
                petal, order + 1, parent);
            shapes.CreateEllipse("花芯", position + Vector2.up * 0.23f, new Vector2(0.06f, 0.06f),
                new Color(1f, 0.89f, 0.45f), order + 2, parent);
        }
    }
}
