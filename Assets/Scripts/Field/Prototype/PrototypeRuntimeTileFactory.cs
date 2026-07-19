using UnityEngine;
using UnityEngine.Tilemaps;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 外部タイルセットへ移行するまでの間、実際のTilemapへ配置する最小Tileを実行時に生成します。
    /// 地形表現はTilemap側へ移し、1マスごとのGameObject生成は行いません。
    /// </summary>
    internal sealed class PrototypeRuntimeTileFactory
    {
        private Sprite diamondSprite;
        private Tile grassTile;
        private Tile pathTile;
        private Tile collisionTile;

        public Tile GrassTile => grassTile ??= CreateVisualTile("草地タイル");
        public Tile PathTile => pathTile ??= CreateVisualTile("小道タイル");
        public Tile CollisionTile => collisionTile ??= CreateCollisionTile();

        private Tile CreateVisualTile(string name)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = name;
            tile.sprite = DiamondSprite;
            tile.color = Color.white;
            tile.flags = TileFlags.None;
            tile.colliderType = Tile.ColliderType.None;
            return tile;
        }

        private static Tile CreateCollisionTile()
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = "衝突タイル";
            tile.sprite = null;
            tile.color = Color.clear;
            tile.flags = TileFlags.None;
            tile.colliderType = Tile.ColliderType.Grid;
            return tile;
        }

        private Sprite DiamondSprite
        {
            get
            {
                if (diamondSprite == null)
                {
                    diamondSprite = CreateDiamondSprite();
                }

                return diamondSprite;
            }
        }

        private static Sprite CreateDiamondSprite()
        {
            const int width = 32;
            const int height = 16;
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
            {
                name = "アイソメトリック菱形タイル",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = Mathf.Abs((x + 0.5f - width * 0.5f) / (width * 0.5f));
                    float ny = Mathf.Abs((y + 0.5f - height * 0.5f) / (height * 0.5f));
                    texture.SetPixel(x, y, nx + ny <= 1f ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
        }
    }
}
