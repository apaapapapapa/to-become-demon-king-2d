using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// インポート済みのアートアセットを使わず、アイソメトリック風の軽量な試作ワールドを構築します。
    /// 実際の Isometric Tilemap とピクセルアートを準備している間も、プロジェクトをすぐに遊べる状態に保ちます。
    /// </summary>
    public sealed class FieldBootstrap : MonoBehaviour
    {
        private static readonly Color Grass = new(0.28f, 0.55f, 0.25f);
        private static readonly Color GrassLight = new(0.36f, 0.65f, 0.29f);
        private const float IsoYScale = 0.5f;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Camera camera = Camera.main;
            camera.backgroundColor = new Color(0.11f, 0.24f, 0.20f);
            camera.orthographicSize = 6.2f;

            Transform world = new GameObject("Isometric Prototype Meadow").transform;
            CreateDiamondGround(world);
            CreateDiamondPath(world);
            CreateDecorations(world);
            CreatePond(world);
            CreateLandmarks(world);

            GameObject slime = CreateSlime(world);
            slime.transform.position = new Vector3(0, -1.5f, -1);
            slime.AddComponent<SlimeController>().Configure(new Vector2(7.9f, 4.95f));
        }

        private static void CreateDiamondGround(Transform parent)
        {
            for (int x = -9; x <= 9; x++)
            {
                for (int y = -9; y <= 9; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) > 10) continue;
                    Vector2 position = Iso(x, y);
                    Color color = (x + y) % 2 == 0 ? Grass : new Color(0.30f, 0.58f, 0.26f);
                    CreateDiamond("Grass Tile", position, new Vector2(1.02f, 0.52f), color, -100, parent);
                }
            }
        }

        private static void CreateDiamondPath(Transform parent)
        {
            for (int i = -8; i <= 8; i++)
            {
                int gridY = Mathf.RoundToInt(Mathf.Sin(i * 0.55f));
                CreateDiamond("Path Tile", Iso(i, gridY), new Vector2(1.04f, 0.54f),
                    new Color(0.63f, 0.48f, 0.28f), -80, parent);
            }
        }

        private static void CreateDecorations(Transform parent)
        {
            var random = new System.Random(1837);
            for (int i = 0; i < 65; i++)
            {
                int x = random.Next(-8, 9);
                int y = random.Next(-8, 9);
                if (Mathf.Abs(x) + Mathf.Abs(y) > 9 || Mathf.Abs(y - Mathf.RoundToInt(Mathf.Sin(x * 0.55f))) <= 1) continue;

                Vector2 p = Iso(x, y) + new Vector2(Next(random, -0.28f, 0.28f), Next(random, -0.12f, 0.12f));
                bool flower = i % 8 == 0;
                CreatePatch(flower ? "Wildflower" : "Grass tuft", p,
                    flower ? new Vector2(0.13f, 0.13f) : new Vector2(0.10f, 0.28f),
                    flower ? new Color(0.96f, 0.78f, 0.30f) : GrassLight,
                    SortOrder(p.y), parent);
            }
        }

        private static void CreatePond(Transform parent)
        {
            Vector2 center = Iso(5, -1);
            CreateDiamond("Pond bank", center, new Vector2(4.4f, 2.2f), new Color(0.24f, 0.43f, 0.22f), -70, parent);
            CreateDiamond("Pond", center, new Vector2(3.7f, 1.75f), new Color(0.17f, 0.55f, 0.66f), -69, parent);
            for (int i = 0; i < 4; i++)
                CreatePatch("Water glint", center + new Vector2(-0.9f + i * 0.6f, (i % 2) * 0.18f),
                    new Vector2(0.4f, 0.06f), new Color(0.52f, 0.86f, 0.82f), -68, parent);
        }

        private static void CreateLandmarks(Transform parent)
        {
            Vector2[] trees = { Iso(-6, 2), Iso(-5, 4), Iso(-2, -6), Iso(6, -2) };
            foreach (Vector2 p in trees)
            {
                int order = SortOrder(p.y);
                CreatePatch("Tree shadow", p + new Vector2(0.15f, -0.35f), new Vector2(1.15f, 0.35f),
                    new Color(0.13f, 0.30f, 0.19f), order - 2, parent);
                CreatePatch("Tree trunk", p + Vector2.up * 0.25f, new Vector2(0.34f, 0.85f),
                    new Color(0.39f, 0.24f, 0.12f), order, parent);
                CreatePatch("Tree crown", p + Vector2.up * 0.9f, new Vector2(1.25f, 1.35f),
                    new Color(0.12f, 0.42f, 0.20f), order + 1, parent);
                CreatePatch("Tree highlight", p + new Vector2(-0.2f, 1.15f), new Vector2(0.55f, 0.5f),
                    new Color(0.29f, 0.62f, 0.24f), order + 2, parent);
            }

            Vector2 stone = Iso(-4, -1);
            int stoneOrder = SortOrder(stone.y);
            CreatePatch("Ancient stone", stone + Vector2.up * 0.3f, new Vector2(0.8f, 1.15f),
                new Color(0.46f, 0.52f, 0.47f), stoneOrder, parent);
            CreatePatch("Stone rune", stone + Vector2.up * 0.4f, new Vector2(0.12f, 0.55f),
                new Color(0.62f, 0.83f, 0.58f), stoneOrder + 1, parent);
        }

        private static GameObject CreateSlime(Transform parent)
        {
            GameObject root = new("Slime");
            root.transform.SetParent(parent);
            CreatePatch("Shadow", new Vector2(0, -0.36f), new Vector2(1.05f, 0.34f),
                new Color(0.10f, 0.23f, 0.16f, 0.65f), 8, root.transform);
            CreatePatch("Body", new Vector2(0, 0.05f), new Vector2(1.05f, 0.9f),
                new Color(0.26f, 0.86f, 0.52f), 10, root.transform);
            CreatePatch("Shine", new Vector2(-0.24f, 0.27f), new Vector2(0.25f, 0.18f),
                new Color(0.69f, 1f, 0.74f), 11, root.transform);
            CreatePatch("Left eye", new Vector2(-0.2f, 0.04f), new Vector2(0.10f, 0.16f), Color.black, 12, root.transform);
            CreatePatch("Right eye", new Vector2(0.2f, 0.04f), new Vector2(0.10f, 0.16f), Color.black, 12, root.transform);
            return root;
        }

        private static Vector2 Iso(int x, int y) => new((x - y) * 0.5f, (x + y) * 0.5f * IsoYScale);

        private static int SortOrder(float worldY) => -Mathf.RoundToInt(worldY * 100f);

        private static GameObject CreateDiamond(string name, Vector2 position, Vector2 size, Color color,
            int order, Transform parent)
        {
            GameObject patch = CreatePatch(name, position, size, color, order, parent);
            patch.transform.localRotation = Quaternion.Euler(0, 0, 45f);
            return patch;
        }

        private static GameObject CreatePatch(string name, Vector2 position, Vector2 size, Color color,
            int order, Transform parent)
        {
            GameObject patch = new(name);
            patch.transform.SetParent(parent, false);
            patch.transform.localPosition = new Vector3(position.x, position.y, 0);
            patch.transform.localScale = new Vector3(size.x, size.y, 1);
            SpriteRenderer renderer = patch.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return patch;
        }

        private static Sprite runtimeSprite;
        private static Sprite RuntimeSprite
        {
            get
            {
                if (runtimeSprite != null) return runtimeSprite;
                Texture2D texture = new(1, 1) { name = "Runtime Pixel" };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                runtimeSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
                return runtimeSprite;
            }
        }

        private static float Next(System.Random random, float min, float max) =>
            min + (float)random.NextDouble() * (max - min);
    }
}
