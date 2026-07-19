using System.Collections.Generic;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// 外部アートアセットを使わず、温かい学園ファンタジー調のアイソメトリック2.5Dフィールドを構築します。
    /// 奥行きで前後する建物や植生、夕暮れの光、動く水面と蛍を重ね、既存の試作をすぐ遊べる状態に保ちます。
    /// </summary>
    public sealed class FieldBootstrap : MonoBehaviour
    {
        private static readonly Color Grass = new(0.31f, 0.52f, 0.28f);
        private static readonly Color GrassLight = new(0.43f, 0.66f, 0.34f);
        private static readonly Color DeepGreen = new(0.10f, 0.22f, 0.20f);
        private static readonly Color Path = new(0.72f, 0.52f, 0.31f);
        private static readonly Color PathLight = new(0.86f, 0.69f, 0.43f);
        private static readonly Color Wood = new(0.32f, 0.20f, 0.16f);
        private static readonly Color Roof = new(0.49f, 0.22f, 0.34f);
        private static readonly Color RoofLight = new(0.68f, 0.32f, 0.40f);
        private static readonly Color Wall = new(0.84f, 0.72f, 0.52f);
        private const float IsoYScale = 0.5f;
        private const int GroundOrder = -1000;

        private readonly List<AmbientElement> ambientElements = new();
        private float ambientTime;

        private sealed class AmbientElement
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 BasePosition;
            public Vector2 Direction;
            public Color BaseColor;
            public float Amplitude;
            public float Speed;
            public float Phase;
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.antiAliasing = 0;

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.075f, 0.16f, 0.18f);
                camera.orthographicSize = 5.8f;
                camera.allowMSAA = false;
                camera.transform.position = new Vector3(0f, 0.35f, -10f);
            }

            Transform world = new GameObject("夕映えの学園草原").transform;
            CreateBackdrop(world);
            CreateGround(world);
            CreatePath(world);
            CreateFence(world);
            CreateCottage(world);
            CreatePond(world);
            CreateTrees(world);
            CreateDecorations(world);
            CreateLandmarks(world);
            CreateLampposts(world);
            CreateAtmosphere(world);
            CreateForegroundFrame(world);

            GameObject slime = CreateSlime(world);
            slime.transform.localPosition = new Vector3(0f, -1.35f, -1f);
            slime.AddComponent<SlimeController>().Configure(new Vector2(7.15f, 3.45f));
        }

        private void Update()
        {
            ambientTime += Time.deltaTime;
            foreach (AmbientElement element in ambientElements)
            {
                float wave = Mathf.Sin(ambientTime * element.Speed + element.Phase);
                Vector2 drift = element.Direction * (wave * element.Amplitude);
                element.Transform.localPosition = element.BasePosition + new Vector3(drift.x, drift.y, 0f);

                Color color = element.BaseColor;
                color.a *= 0.72f + (wave + 1f) * 0.14f;
                element.Renderer.color = color;
            }
        }

        private static void CreateBackdrop(Transform parent)
        {
            CreatePatch("深緑の背景", Vector2.zero, new Vector2(28f, 16f),
                new Color(0.07f, 0.15f, 0.17f), -5000, parent);
            CreatePatch("夕暮れの霞", new Vector2(0f, 4.2f), new Vector2(26f, 3.1f),
                new Color(0.37f, 0.31f, 0.36f, 0.45f), -4990, parent);
            CreateEllipse("沈む光", new Vector2(5.8f, 4.35f), new Vector2(5.6f, 5.6f),
                new Color(0.92f, 0.55f, 0.35f, 0.10f), -4980, parent);
            CreateEllipse("夕日の芯", new Vector2(5.8f, 4.35f), new Vector2(2.2f, 2.2f),
                new Color(1f, 0.72f, 0.43f, 0.13f), -4979, parent);

            for (int i = 0; i < 9; i++)
            {
                float x = -10f + i * 2.5f;
                float height = 1.5f + (i % 3) * 0.42f;
                CreatePatch("遠景の幹", new Vector2(x, 3.2f), new Vector2(0.34f, height),
                    new Color(0.08f, 0.19f, 0.18f), -4960, parent);
                CreateEllipse("遠景の樹冠", new Vector2(x, 4.05f), new Vector2(2.4f, 1.55f),
                    new Color(0.09f, 0.24f, 0.21f), -4959, parent);
            }
        }

        private static void CreateGround(Transform parent)
        {
            CreatePatch("地面の影", new Vector2(0.16f, -0.24f), new Vector2(15.7f, 7.95f),
                new Color(0.035f, 0.08f, 0.10f, 0.72f), GroundOrder - 30, parent);
            CreatePatch("地面の厚み", new Vector2(0f, -0.16f), new Vector2(15.35f, 7.78f),
                new Color(0.25f, 0.27f, 0.18f), GroundOrder - 20, parent);

            for (int x = -15; x <= 15; x++)
            {
                for (int y = -15; y <= 15; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) > 15)
                    {
                        continue;
                    }

                    Vector2 position = Iso(x, y);
                    float variation = 0.12f + ((x * 17 + y * 31) & 3) * 0.055f;
                    Color color = Color.Lerp(Grass, GrassLight, variation);
                    CreateDiamond("草地タイル", position, new Vector2(1.03f, 0.53f),
                        color, GroundOrder, parent);
                }
            }

            CreatePatch("手前の土の断面", new Vector2(0f, -3.88f), new Vector2(15.2f, 0.27f),
                new Color(0.29f, 0.20f, 0.15f), GroundOrder + 1, parent);
            CreatePatch("断面の明るい縁", new Vector2(0f, -3.73f), new Vector2(15.2f, 0.06f),
                new Color(0.56f, 0.43f, 0.25f), GroundOrder + 2, parent);
        }

        private static void CreatePath(Transform parent)
        {
            for (int x = -13; x <= 13; x++)
            {
                int centerY = PathY(x);
                for (int width = -1; width <= 1; width++)
                {
                    int tileY = centerY + width;
                    if (Mathf.Abs(x) + Mathf.Abs(tileY) > 14)
                    {
                        continue;
                    }

                    Vector2 position = Iso(x, tileY);
                    CreateDiamond("小道の影", position + Vector2.down * 0.045f, new Vector2(1.06f, 0.55f),
                        new Color(0.34f, 0.25f, 0.18f), GroundOrder + 89, parent);
                    Color color = ((x + tileY) & 1) == 0 ? Path : PathLight;
                    CreateDiamond("小道タイル", position, new Vector2(1.02f, 0.50f),
                        color, GroundOrder + 90, parent);
                }

                if (x % 3 == 0)
                {
                    Vector2 pebble = Iso(x, centerY) + new Vector2(0.18f, 0.03f);
                    CreateEllipse("小道の小石", pebble, new Vector2(0.12f, 0.055f),
                        new Color(0.48f, 0.34f, 0.23f), GroundOrder + 92, parent);
                }
            }
        }

        private static void CreateFence(Transform parent)
        {
            const float baseY = 2.72f;
            int order = SortOrder(baseY);
            CreatePatch("柵の下段", new Vector2(0.75f, baseY + 0.30f), new Vector2(6.5f, 0.11f),
                new Color(0.38f, 0.25f, 0.18f), order, parent, rotation: -2f);
            CreatePatch("柵の上段", new Vector2(0.75f, baseY + 0.62f), new Vector2(6.5f, 0.12f),
                new Color(0.50f, 0.33f, 0.21f), order + 1, parent, rotation: -2f);

            for (int i = 0; i < 8; i++)
            {
                float x = -2.45f + i * 0.92f;
                CreatePatch("柵柱", new Vector2(x, baseY + 0.43f), new Vector2(0.14f, 0.92f),
                    Wood, order + 2, parent);
                CreateDiamond("柵柱の飾り", new Vector2(x, baseY + 0.93f), new Vector2(0.22f, 0.15f),
                    new Color(0.61f, 0.42f, 0.25f), order + 3, parent);
            }
        }

        private static void CreateCottage(Transform parent)
        {
            Vector2 basePosition = new(-4.55f, 1.15f);
            int order = SortOrder(basePosition.y);

            CreateEllipse("校舎の影", basePosition + new Vector2(0.25f, -0.15f), new Vector2(4.7f, 0.8f),
                new Color(0.06f, 0.14f, 0.13f, 0.62f), order - 5, parent);
            CreatePatch("校舎の側壁", basePosition + new Vector2(1.35f, 0.82f), new Vector2(1.35f, 1.75f),
                new Color(0.67f, 0.54f, 0.39f), order - 1, parent);
            CreatePatch("校舎の正面", basePosition + new Vector2(-0.25f, 0.86f), new Vector2(3.25f, 1.82f),
                Wall, order, parent);
            CreatePatch("壁の陰", basePosition + new Vector2(-0.25f, 0.12f), new Vector2(3.25f, 0.28f),
                new Color(0.58f, 0.43f, 0.31f), order + 1, parent);

            CreatePatch("横梁", basePosition + new Vector2(-0.25f, 1.18f), new Vector2(3.25f, 0.12f),
                Wood, order + 2, parent);
            CreatePatch("左の柱", basePosition + new Vector2(-1.45f, 0.88f), new Vector2(0.13f, 1.72f),
                Wood, order + 2, parent);
            CreatePatch("中央の柱", basePosition + new Vector2(0.05f, 0.88f), new Vector2(0.12f, 1.72f),
                Wood, order + 2, parent);

            CreateDiamond("大屋根の影", basePosition + new Vector2(0.18f, 2.00f), new Vector2(4.55f, 1.82f),
                new Color(0.24f, 0.12f, 0.19f), order + 3, parent);
            CreateDiamond("大屋根", basePosition + new Vector2(0f, 2.10f), new Vector2(4.42f, 1.78f),
                Roof, order + 4, parent);
            CreatePatch("屋根の明るい縁", basePosition + new Vector2(-0.35f, 2.55f), new Vector2(2.55f, 0.12f),
                RoofLight, order + 5, parent, rotation: 11f);
            CreatePatch("屋根の暗い縁", basePosition + new Vector2(0.74f, 1.65f), new Vector2(2.7f, 0.11f),
                new Color(0.28f, 0.13f, 0.20f), order + 5, parent, rotation: -11f);

            CreatePatch("煙突", basePosition + new Vector2(1.15f, 2.75f), new Vector2(0.42f, 1.18f),
                new Color(0.39f, 0.28f, 0.25f), order + 3, parent);
            CreatePatch("煙突の笠", basePosition + new Vector2(1.15f, 3.35f), new Vector2(0.56f, 0.16f),
                new Color(0.22f, 0.16f, 0.17f), order + 4, parent);

            CreateWindow(basePosition + new Vector2(-0.92f, 0.92f), order + 3, parent);
            CreateWindow(basePosition + new Vector2(0.72f, 0.92f), order + 3, parent);
            CreatePatch("玄関", basePosition + new Vector2(1.37f, 0.59f), new Vector2(0.65f, 1.22f),
                new Color(0.25f, 0.20f, 0.23f), order + 3, parent);
            CreateEllipse("扉の金具", basePosition + new Vector2(1.18f, 0.62f), new Vector2(0.09f, 0.09f),
                new Color(0.94f, 0.67f, 0.28f), order + 4, parent);

            CreatePatch("花箱", basePosition + new Vector2(-0.92f, 0.39f), new Vector2(0.93f, 0.20f),
                Wood, order + 4, parent);
            CreateFlowerCluster(basePosition + new Vector2(-0.92f, 0.53f), order + 5, parent);
        }

        private static void CreateWindow(Vector2 position, int order, Transform parent)
        {
            CreatePatch("窓枠", position, new Vector2(0.78f, 0.72f), Wood, order, parent);
            CreatePatch("暖かな窓", position, new Vector2(0.61f, 0.55f),
                new Color(0.96f, 0.74f, 0.35f), order + 1, parent);
            CreatePatch("窓の縦桟", position, new Vector2(0.07f, 0.56f), Wood, order + 2, parent);
            CreatePatch("窓の横桟", position, new Vector2(0.62f, 0.07f), Wood, order + 2, parent);
        }

        private void CreatePond(Transform parent)
        {
            Vector2 center = new(4.45f, 0.55f);
            CreateDiamond("池の影", center + new Vector2(0.12f, -0.10f), new Vector2(5.1f, 2.42f),
                new Color(0.05f, 0.16f, 0.18f, 0.68f), GroundOrder + 110, parent);
            CreateDiamond("池の岸", center, new Vector2(5f, 2.30f),
                new Color(0.33f, 0.43f, 0.25f), GroundOrder + 111, parent);
            CreateDiamond("池", center, new Vector2(4.22f, 1.75f),
                new Color(0.16f, 0.47f, 0.55f), GroundOrder + 112, parent);
            CreateDiamond("池の浅瀬", center + new Vector2(-0.18f, 0.08f), new Vector2(3.72f, 1.38f),
                new Color(0.28f, 0.64f, 0.63f, 0.58f), GroundOrder + 113, parent);

            for (int i = 0; i < 5; i++)
            {
                Vector2 position = center + new Vector2(-1.15f + i * 0.55f, -0.25f + (i % 2) * 0.22f);
                GameObject glint = CreatePatch("水面のきらめき", position, new Vector2(0.38f, 0.055f),
                    new Color(0.69f, 0.92f, 0.80f, 0.76f), GroundOrder + 115, parent);
                AddAmbient(glint, Vector2.right, 0.12f, 0.75f + i * 0.08f, i * 0.9f);
            }

            CreateEllipse("睡蓮の葉", center + new Vector2(1.18f, 0.08f), new Vector2(0.42f, 0.18f),
                new Color(0.25f, 0.52f, 0.26f), GroundOrder + 116, parent);
            CreateEllipse("睡蓮の花", center + new Vector2(1.22f, 0.19f), new Vector2(0.14f, 0.10f),
                new Color(0.96f, 0.70f, 0.75f), GroundOrder + 117, parent);

            for (int i = -3; i <= 3; i++)
            {
                Vector2 plank = center + new Vector2(i * 0.34f, -0.02f * i);
                CreatePatch("橋板", plank, new Vector2(0.28f, 0.82f),
                    i % 2 == 0 ? new Color(0.48f, 0.31f, 0.20f) : new Color(0.57f, 0.38f, 0.23f),
                    GroundOrder + 128, parent, rotation: -3f);
            }
            CreatePatch("橋の上手すり", center + new Vector2(0f, 0.42f), new Vector2(2.65f, 0.08f),
                Wood, GroundOrder + 129, parent, rotation: -3f);
            CreatePatch("橋の下手すり", center + new Vector2(0f, -0.43f), new Vector2(2.65f, 0.08f),
                Wood, GroundOrder + 129, parent, rotation: -3f);
        }

        private static void CreateTrees(Transform parent)
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

        private static void CreateTree(Vector2 basePosition, Transform parent)
        {
            int order = SortOrder(basePosition.y);
            CreateEllipse("木の影", basePosition + new Vector2(0.22f, -0.14f), new Vector2(1.85f, 0.48f),
                new Color(0.05f, 0.15f, 0.14f, 0.62f), order - 3, parent);
            CreatePatch("木の幹", basePosition + new Vector2(0f, 0.72f), new Vector2(0.38f, 1.55f),
                new Color(0.31f, 0.20f, 0.14f), order, parent);
            CreatePatch("幹の光", basePosition + new Vector2(-0.09f, 0.82f), new Vector2(0.10f, 1.20f),
                new Color(0.55f, 0.37f, 0.20f), order + 1, parent);

            CreateEllipse("樹冠の影", basePosition + new Vector2(0.12f, 1.72f), new Vector2(2.10f, 1.72f),
                new Color(0.08f, 0.29f, 0.22f), order + 2, parent);
            CreateEllipse("左の樹冠", basePosition + new Vector2(-0.52f, 1.82f), new Vector2(1.45f, 1.35f),
                new Color(0.12f, 0.42f, 0.25f), order + 3, parent);
            CreateEllipse("右の樹冠", basePosition + new Vector2(0.48f, 1.98f), new Vector2(1.58f, 1.43f),
                new Color(0.16f, 0.49f, 0.27f), order + 3, parent);
            CreateEllipse("樹冠の光", basePosition + new Vector2(-0.32f, 2.28f), new Vector2(0.92f, 0.68f),
                new Color(0.39f, 0.68f, 0.34f), order + 4, parent);
            CreateEllipse("葉のきらめき", basePosition + new Vector2(-0.52f, 2.43f), new Vector2(0.31f, 0.20f),
                new Color(0.65f, 0.82f, 0.45f, 0.72f), order + 5, parent);
        }

        private static void CreateBush(Vector2 basePosition, Transform parent)
        {
            int order = SortOrder(basePosition.y);
            CreateEllipse("茂みの影", basePosition + Vector2.down * 0.08f, new Vector2(1.45f, 0.42f),
                new Color(0.06f, 0.18f, 0.15f, 0.58f), order - 1, parent);
            CreateEllipse("茂み", basePosition + Vector2.up * 0.22f, new Vector2(1.38f, 0.88f),
                new Color(0.14f, 0.42f, 0.24f), order, parent);
            CreateEllipse("茂みの光", basePosition + new Vector2(-0.28f, 0.39f), new Vector2(0.64f, 0.38f),
                new Color(0.36f, 0.64f, 0.31f), order + 1, parent);
        }

        private static void CreateDecorations(Transform parent)
        {
            var random = new System.Random(1837);
            Vector2 cottage = new(-4.55f, 1.15f);
            Vector2 pond = new(4.45f, 0.55f);

            for (int i = 0; i < 74; i++)
            {
                int x = random.Next(-14, 15);
                int y = random.Next(-14, 15);
                if (Mathf.Abs(x) + Mathf.Abs(y) > 14 || Mathf.Abs(y - PathY(x)) <= 1)
                {
                    continue;
                }

                Vector2 position = Iso(x, y) + new Vector2(Next(random, -0.28f, 0.28f), Next(random, -0.11f, 0.11f));
                if ((position - cottage).sqrMagnitude < 7f || (position - pond).sqrMagnitude < 6f)
                {
                    continue;
                }

                int order = SortOrder(position.y);
                if (i % 7 == 0)
                {
                    CreateFlower(position, order, i % 14 == 0, parent);
                }
                else
                {
                    CreatePatch("草の影", position + Vector2.down * 0.06f, new Vector2(0.18f, 0.08f),
                        new Color(0.12f, 0.29f, 0.18f, 0.55f), order - 1, parent);
                    CreatePatch("草の房", position + Vector2.up * 0.08f, new Vector2(0.10f, 0.30f),
                        GrassLight, order, parent, rotation: i % 2 == 0 ? -8f : 8f);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 position = new(-6.1f + i * 0.34f, 0.02f + (i % 2) * 0.15f);
                CreateFlower(position, SortOrder(position.y), i % 2 == 0, parent);
            }
        }

        private static void CreateFlower(Vector2 position, int order, bool pink, Transform parent)
        {
            CreatePatch("花の茎", position + Vector2.up * 0.08f, new Vector2(0.055f, 0.25f),
                new Color(0.20f, 0.48f, 0.23f), order, parent);
            Color petal = pink ? new Color(0.96f, 0.54f, 0.65f) : new Color(0.94f, 0.78f, 0.30f);
            CreateEllipse("花びら", position + Vector2.up * 0.23f, new Vector2(0.17f, 0.14f),
                petal, order + 1, parent);
            CreateEllipse("花芯", position + Vector2.up * 0.23f, new Vector2(0.06f, 0.06f),
                new Color(1f, 0.89f, 0.45f), order + 2, parent);
        }

        private static void CreateFlowerCluster(Vector2 position, int order, Transform parent)
        {
            for (int i = -2; i <= 2; i++)
            {
                Vector2 flower = position + new Vector2(i * 0.16f, Mathf.Abs(i) * -0.025f);
                CreateEllipse("花箱の花", flower, new Vector2(0.16f, 0.13f),
                    i % 2 == 0 ? new Color(0.96f, 0.55f, 0.66f) : new Color(0.98f, 0.79f, 0.32f),
                    order, parent);
            }
        }

        private static void CreateLandmarks(Transform parent)
        {
            Vector2 stone = new(-1.95f, -2.18f);
            int stoneOrder = SortOrder(stone.y);
            CreateEllipse("古代石の影", stone + new Vector2(0.12f, -0.16f), new Vector2(1.18f, 0.34f),
                new Color(0.06f, 0.15f, 0.14f, 0.58f), stoneOrder - 2, parent);
            CreatePatch("古代石", stone + Vector2.up * 0.48f, new Vector2(0.74f, 1.22f),
                new Color(0.38f, 0.44f, 0.41f), stoneOrder, parent, rotation: -4f);
            CreatePatch("古代石の光", stone + new Vector2(-0.18f, 0.60f), new Vector2(0.12f, 0.70f),
                new Color(0.65f, 0.79f, 0.62f, 0.72f), stoneOrder + 1, parent, rotation: -4f);
            CreatePatch("石のルーン", stone + Vector2.up * 0.52f, new Vector2(0.09f, 0.48f),
                new Color(0.57f, 0.92f, 0.72f), stoneOrder + 2, parent);

            Vector2 sign = new(-0.62f, -0.45f);
            int signOrder = SortOrder(sign.y);
            CreatePatch("案内板の柱", sign + Vector2.up * 0.42f, new Vector2(0.13f, 0.88f),
                Wood, signOrder, parent);
            CreatePatch("案内板", sign + Vector2.up * 0.82f, new Vector2(1.18f, 0.48f),
                new Color(0.53f, 0.34f, 0.20f), signOrder + 1, parent, rotation: -2f);
            CreateDiamond("案内板の矢印", sign + new Vector2(0.30f, 0.82f), new Vector2(0.28f, 0.20f),
                PathLight, signOrder + 2, parent);
        }

        private void CreateLampposts(Transform parent)
        {
            CreateLamppost(new Vector2(-1.10f, 2.08f), 0.2f, parent);
            CreateLamppost(new Vector2(2.15f, -2.10f), 2.1f, parent);
        }

        private void CreateLamppost(Vector2 basePosition, float phase, Transform parent)
        {
            int order = SortOrder(basePosition.y);
            CreateEllipse("街灯の影", basePosition + new Vector2(0.12f, -0.10f), new Vector2(0.82f, 0.24f),
                new Color(0.05f, 0.13f, 0.13f, 0.55f), order - 2, parent);
            CreatePatch("街灯の支柱", basePosition + Vector2.up * 0.78f, new Vector2(0.11f, 1.62f),
                new Color(0.19f, 0.19f, 0.22f), order, parent);
            CreatePatch("街灯の笠", basePosition + Vector2.up * 1.62f, new Vector2(0.54f, 0.18f),
                new Color(0.24f, 0.20f, 0.25f), order + 1, parent);
            CreateDiamond("街灯の灯り", basePosition + Vector2.up * 1.45f, new Vector2(0.34f, 0.40f),
                new Color(1f, 0.73f, 0.31f), order + 2, parent);
            GameObject glow = CreateEllipse("街灯の光輪", basePosition + Vector2.up * 1.44f, new Vector2(1.35f, 1.35f),
                new Color(1f, 0.65f, 0.25f, 0.13f), order + 3, parent);
            AddAmbient(glow, Vector2.zero, 0f, 2.4f, phase);
        }

        private void CreateAtmosphere(Transform parent)
        {
            var random = new System.Random(420);
            for (int i = 0; i < 16; i++)
            {
                Vector2 position = new(Next(random, -6.7f, 6.7f), Next(random, -2.8f, 3.0f));
                float size = Next(random, 0.07f, 0.13f);
                GameObject firefly = CreateEllipse("漂う光", position, new Vector2(size, size),
                    new Color(1f, 0.87f, 0.39f, 0.84f), 1450 + i, parent);
                Vector2 direction = new Vector2(Next(random, -0.45f, 0.45f), 1f).normalized;
                AddAmbient(firefly, direction, Next(random, 0.08f, 0.22f),
                    Next(random, 0.55f, 1.15f), Next(random, 0f, Mathf.PI * 2f));
            }

            CreatePatch("薄い夕霧", new Vector2(0f, 3.65f), new Vector2(18f, 0.32f),
                new Color(0.73f, 0.64f, 0.68f, 0.08f), 1400, parent);
        }

        private static void CreateForegroundFrame(Transform parent)
        {
            CreateEllipse("左手前の葉影", new Vector2(-7.45f, -4.18f), new Vector2(4.1f, 2.65f),
                new Color(0.05f, 0.17f, 0.15f, 0.94f), 3000, parent);
            CreateEllipse("左手前の葉", new Vector2(-6.72f, -3.88f), new Vector2(2.55f, 1.55f),
                new Color(0.10f, 0.31f, 0.22f), 3001, parent);
            CreateEllipse("右手前の葉影", new Vector2(7.45f, -4.18f), new Vector2(4.1f, 2.65f),
                new Color(0.05f, 0.17f, 0.15f, 0.94f), 3000, parent);
            CreateEllipse("右手前の葉", new Vector2(6.75f, -3.86f), new Vector2(2.45f, 1.48f),
                new Color(0.12f, 0.35f, 0.23f), 3001, parent);
        }

        private static GameObject CreateSlime(Transform parent)
        {
            GameObject root = new("スライム");
            root.transform.SetParent(parent, false);
            CreateEllipse("影", new Vector2(0f, -0.38f), new Vector2(1.18f, 0.34f),
                new Color(0.05f, 0.16f, 0.14f, 0.70f), -2, root.transform);
            CreateEllipse("輪郭", new Vector2(0f, 0.02f), new Vector2(1.18f, 0.94f),
                new Color(0.08f, 0.31f, 0.25f), 0, root.transform);
            CreateEllipse("からだ", new Vector2(0f, 0.06f), new Vector2(1.04f, 0.82f),
                new Color(0.31f, 0.86f, 0.53f), 1, root.transform);
            CreateEllipse("下側の色", new Vector2(0f, -0.20f), new Vector2(0.83f, 0.25f),
                new Color(0.17f, 0.63f, 0.43f), 2, root.transform);
            CreateEllipse("つや", new Vector2(-0.25f, 0.29f), new Vector2(0.28f, 0.18f),
                new Color(0.76f, 1f, 0.78f), 3, root.transform);
            CreateEllipse("左目", new Vector2(-0.20f, 0.05f), new Vector2(0.09f, 0.15f),
                new Color(0.04f, 0.11f, 0.10f), 4, root.transform);
            CreateEllipse("右目", new Vector2(0.20f, 0.05f), new Vector2(0.09f, 0.15f),
                new Color(0.04f, 0.11f, 0.10f), 4, root.transform);
            return root;
        }

        private void AddAmbient(GameObject target, Vector2 direction, float amplitude, float speed, float phase)
        {
            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            ambientElements.Add(new AmbientElement
            {
                Transform = target.transform,
                Renderer = renderer,
                BasePosition = target.transform.localPosition,
                Direction = direction,
                BaseColor = renderer.color,
                Amplitude = amplitude,
                Speed = speed,
                Phase = phase
            });
        }

        private static int PathY(int x) => Mathf.RoundToInt(Mathf.Sin(x * 0.38f) * 1.65f);

        private static Vector2 Iso(int x, int y) => new((x - y) * 0.5f, (x + y) * 0.5f * IsoYScale);

        private static int SortOrder(float worldY) => -Mathf.RoundToInt(worldY * 100f);

        private static GameObject CreateDiamond(string name, Vector2 position, Vector2 size, Color color,
            int order, Transform parent)
        {
            return CreatePatch(name, position, size, color, order, parent, DiamondSprite);
        }

        private static GameObject CreateEllipse(string name, Vector2 position, Vector2 size, Color color,
            int order, Transform parent)
        {
            return CreatePatch(name, position, size, color, order, parent, CircleSprite);
        }

        private static GameObject CreatePatch(string name, Vector2 position, Vector2 size, Color color,
            int order, Transform parent, Sprite sprite = null, float rotation = 0f)
        {
            GameObject patch = new(name);
            patch.transform.SetParent(parent, false);
            patch.transform.localPosition = new Vector3(position.x, position.y, 0f);
            patch.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            patch.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = patch.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite != null ? sprite : SquareSprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return patch;
        }

        private static Sprite squareSprite;
        private static Sprite circleSprite;
        private static Sprite diamondSprite;

        private static Sprite SquareSprite
        {
            get
            {
                if (squareSprite == null)
                {
                    squareSprite = CreateShapeSprite("四角ピクセル", 0);
                }

                return squareSprite;
            }
        }

        private static Sprite CircleSprite
        {
            get
            {
                if (circleSprite == null)
                {
                    circleSprite = CreateShapeSprite("丸ピクセル", 1);
                }

                return circleSprite;
            }
        }

        private static Sprite DiamondSprite
        {
            get
            {
                if (diamondSprite == null)
                {
                    diamondSprite = CreateShapeSprite("菱形ピクセル", 2);
                }

                return diamondSprite;
            }
        }

        private static Sprite CreateShapeSprite(string name, int shape)
        {
            const int size = 16;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
                    float ny = (y + 0.5f - size * 0.5f) / (size * 0.5f);
                    bool visible = shape switch
                    {
                        1 => nx * nx + ny * ny <= 1f,
                        2 => Mathf.Abs(nx) + Mathf.Abs(ny) <= 1f,
                        _ => true
                    };
                    texture.SetPixel(x, y, visible ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static float Next(System.Random random, float min, float max) =>
            min + (float)random.NextDouble() * (max - min);
    }
}
