using UnityEngine;
using UnityEngine.Tilemaps;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 背景と前景の演出を維持しつつ、歩行地形と小道は実際のIsometric Tilemapへ構築します。
    /// 1マスごとのGameObject生成を廃止し、地形データの所有先をGround Tilemapへ統一します。
    /// </summary>
    internal sealed class TerrainBuilder
    {
        private readonly RuntimeShapeFactory shapes;
        private readonly PrototypeTilemapContext tilemaps;
        private readonly PrototypeRuntimeTileFactory tiles;
        private readonly int playableRadius;

        public TerrainBuilder(
            RuntimeShapeFactory shapes,
            PrototypeTilemapContext tilemaps,
            PrototypeRuntimeTileFactory tiles,
            int playableRadius)
        {
            this.shapes = shapes;
            this.tilemaps = tilemaps;
            this.tiles = tiles;
            this.playableRadius = Mathf.Max(4, playableRadius);
        }

        public void BuildBase(Transform parent)
        {
            CreateBackdrop(parent);
            PaintGround();
            PaintPath(parent);
            CreateGroundEdge(parent);
        }

        public void BuildForeground(Transform parent)
        {
            CreateForegroundFrame(parent);
        }

        private void CreateBackdrop(Transform parent)
        {
            shapes.CreatePatch("深緑の背景", Vector2.zero, new Vector2(28f, 16f),
                new Color(0.07f, 0.15f, 0.17f), -5000, parent);
            shapes.CreatePatch("夕暮れの霞", new Vector2(0f, 4.2f), new Vector2(26f, 3.1f),
                new Color(0.37f, 0.31f, 0.36f, 0.45f), -4990, parent);
            shapes.CreateEllipse("沈む光", new Vector2(5.8f, 4.35f), new Vector2(5.6f, 5.6f),
                new Color(0.92f, 0.55f, 0.35f, 0.10f), -4980, parent);
            shapes.CreateEllipse("夕日の芯", new Vector2(5.8f, 4.35f), new Vector2(2.2f, 2.2f),
                new Color(1f, 0.72f, 0.43f, 0.13f), -4979, parent);

            for (int i = 0; i < 9; i++)
            {
                float x = -10f + i * 2.5f;
                float height = 1.5f + (i % 3) * 0.42f;
                shapes.CreatePatch("遠景の幹", new Vector2(x, 3.2f), new Vector2(0.34f, height),
                    new Color(0.08f, 0.19f, 0.18f), -4960, parent);
                shapes.CreateEllipse("遠景の樹冠", new Vector2(x, 4.05f), new Vector2(2.4f, 1.55f),
                    new Color(0.09f, 0.24f, 0.21f), -4959, parent);
            }
        }

        private void PaintGround()
        {
            Tilemap ground = tilemaps.Ground;
            ground.ClearAllTiles();

            for (int x = -playableRadius; x <= playableRadius; x++)
            {
                for (int y = -playableRadius; y <= playableRadius; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) > playableRadius)
                    {
                        continue;
                    }

                    Vector3Int cell = new(x, y, 0);
                    float variation = 0.12f + ((x * 17 + y * 31) & 3) * 0.055f;
                    Color color = Color.Lerp(PrototypePalette.Grass, PrototypePalette.GrassLight, variation);
                    ground.SetTile(cell, tiles.GrassTile);
                    ground.SetColor(cell, color);
                }
            }
        }

        private void PaintPath(Transform parent)
        {
            Tilemap ground = tilemaps.Ground;

            for (int x = -playableRadius + 2; x <= playableRadius - 2; x++)
            {
                int centerY = PrototypeWorldMath.PathY(x);
                for (int width = -1; width <= 1; width++)
                {
                    int tileY = centerY + width;
                    if (Mathf.Abs(x) + Mathf.Abs(tileY) > playableRadius - 1)
                    {
                        continue;
                    }

                    Vector3Int cell = new(x, tileY, 0);
                    Color color = ((x + tileY) & 1) == 0 ? PrototypePalette.Path : PrototypePalette.PathLight;
                    ground.SetTile(cell, tiles.PathTile);
                    ground.SetColor(cell, color);
                }

                // 小石はタイル地形の上に置く装飾なので、移行期間中は軽量なSprite演出として残します。
                if (x % 3 == 0)
                {
                    Vector2 pebble = PrototypeWorldMath.Iso(x, centerY) + new Vector2(0.18f, 0.03f);
                    shapes.CreateEllipse("小道の小石", pebble, new Vector2(0.12f, 0.055f),
                        new Color(0.48f, 0.34f, 0.23f), PrototypeWorldMath.GroundOrder + 92, parent);
                }
            }
        }

        private void CreateGroundEdge(Transform parent)
        {
            shapes.CreatePatch("地面の影", new Vector2(0.16f, -0.24f), new Vector2(15.7f, 7.95f),
                new Color(0.035f, 0.08f, 0.10f, 0.72f), PrototypeWorldMath.GroundOrder - 30, parent);
            shapes.CreatePatch("地面の厚み", new Vector2(0f, -0.16f), new Vector2(15.35f, 7.78f),
                new Color(0.25f, 0.27f, 0.18f), PrototypeWorldMath.GroundOrder - 20, parent);
            shapes.CreatePatch("手前の土の断面", new Vector2(0f, -3.88f), new Vector2(15.2f, 0.27f),
                new Color(0.29f, 0.20f, 0.15f), PrototypeWorldMath.GroundOrder + 1, parent);
            shapes.CreatePatch("断面の明るい縁", new Vector2(0f, -3.73f), new Vector2(15.2f, 0.06f),
                new Color(0.56f, 0.43f, 0.25f), PrototypeWorldMath.GroundOrder + 2, parent);
        }

        private void CreateForegroundFrame(Transform parent)
        {
            shapes.CreateEllipse("左手前の葉影", new Vector2(-7.45f, -4.18f), new Vector2(4.1f, 2.65f),
                new Color(0.05f, 0.17f, 0.15f, 0.94f), 3000, parent);
            shapes.CreateEllipse("左手前の葉", new Vector2(-6.72f, -3.88f), new Vector2(2.55f, 1.55f),
                new Color(0.10f, 0.31f, 0.22f), 3001, parent);
            shapes.CreateEllipse("右手前の葉影", new Vector2(7.45f, -4.18f), new Vector2(4.1f, 2.65f),
                new Color(0.05f, 0.17f, 0.15f, 0.94f), 3000, parent);
            shapes.CreateEllipse("右手前の葉", new Vector2(6.75f, -3.86f), new Vector2(2.45f, 1.48f),
                new Color(0.12f, 0.35f, 0.23f), 3001, parent);
        }
    }
}
