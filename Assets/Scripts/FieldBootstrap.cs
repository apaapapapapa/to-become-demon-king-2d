using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>Builds the first playable meadow without requiring imported art assets.</summary>
    public sealed class FieldBootstrap : MonoBehaviour
    {
        private static readonly Color Grass = new(0.28f, 0.55f, 0.25f);
        private static readonly Color GrassLight = new(0.36f, 0.65f, 0.29f);

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Camera camera = Camera.main;
            camera.backgroundColor = new Color(0.11f, 0.24f, 0.20f);
            camera.orthographicSize = 6.2f;

            Transform world = new GameObject("Playable Meadow").transform;
            CreatePatch("Grass field", Vector2.zero, new Vector2(17.5f, 11.5f), Grass, -20, world);

            // A winding, warm dirt trail makes the large play area easy to read.
            for (int i = -8; i <= 8; i++)
            {
                float y = Mathf.Sin(i * 0.55f) * 0.65f - 0.4f;
                CreatePatch("Path", new Vector2(i, y), new Vector2(1.25f, 1.8f),
                    new Color(0.63f, 0.48f, 0.28f), -12, world);
            }

            // Layered grass tufts, flowers, stones and trees give the field depth.
            var random = new System.Random(1837);
            for (int i = 0; i < 85; i++)
            {
                float x = Next(random, -8.2f, 8.2f);
                float y = Next(random, -5.25f, 5.25f);
                if (Mathf.Abs(y - Mathf.Sin(x * 0.55f) * 0.65f + 0.4f) < 1.1f) continue;
                Color color = i % 9 == 0 ? new Color(0.96f, 0.78f, 0.30f) : GrassLight;
                CreatePatch(i % 9 == 0 ? "Wildflower" : "Grass tuft", new Vector2(x, y),
                    i % 9 == 0 ? new Vector2(0.13f, 0.13f) : new Vector2(0.10f, 0.28f), color, -6, world);
            }

            CreatePond(world);
            CreateLandmarks(world);

            GameObject slime = CreateSlime(world);
            slime.transform.position = new Vector3(0, -1.5f, -1);
            slime.AddComponent<SlimeController>().Configure(new Vector2(7.9f, 4.95f));
        }

        private static void CreatePond(Transform parent)
        {
            CreatePatch("Pond bank", new Vector2(5.9f, 3.35f), new Vector2(4.25f, 2.6f),
                new Color(0.24f, 0.43f, 0.22f), -10, parent);
            CreatePatch("Pond", new Vector2(5.9f, 3.35f), new Vector2(3.7f, 2.15f),
                new Color(0.17f, 0.55f, 0.66f), -9, parent);
            for (int i = 0; i < 4; i++)
                CreatePatch("Water glint", new Vector2(4.8f + i * 0.7f, 3.25f + (i % 2) * 0.35f),
                    new Vector2(0.4f, 0.07f), new Color(0.52f, 0.86f, 0.82f), -8, parent);
        }

        private static void CreateLandmarks(Transform parent)
        {
            Vector2[] trees = { new(-7.5f, 4.5f), new(-6.2f, 4.8f), new(-7.7f, -4.4f), new(7.4f, -4.3f) };
            foreach (Vector2 p in trees)
            {
                CreatePatch("Tree shadow", p + new Vector2(0.15f, -0.4f), new Vector2(1.15f, 0.42f),
                    new Color(0.13f, 0.30f, 0.19f), -5, parent);
                CreatePatch("Tree trunk", p + Vector2.down * 0.15f, new Vector2(0.34f, 0.85f),
                    new Color(0.39f, 0.24f, 0.12f), -3, parent);
                CreatePatch("Tree crown", p + Vector2.up * 0.45f, new Vector2(1.25f, 1.35f),
                    new Color(0.12f, 0.42f, 0.20f), -2, parent);
                CreatePatch("Tree highlight", p + new Vector2(-0.2f, 0.7f), new Vector2(0.55f, 0.5f),
                    new Color(0.29f, 0.62f, 0.24f), -1, parent);
            }

            CreatePatch("Ancient stone", new Vector2(-5.2f, -2.7f), new Vector2(0.8f, 1.15f),
                new Color(0.46f, 0.52f, 0.47f), -4, parent);
            CreatePatch("Stone rune", new Vector2(-5.2f, -2.6f), new Vector2(0.12f, 0.55f),
                new Color(0.62f, 0.83f, 0.58f), -3, parent);
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
