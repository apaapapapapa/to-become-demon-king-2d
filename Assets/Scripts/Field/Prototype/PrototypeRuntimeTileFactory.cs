using UnityEngine;
using UnityEngine.Tilemaps;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロジェクト管理のインポート済み地形Spriteから、実行時に使用するTileオブジェクトだけを生成します。
    /// Texture2DやSprite自体は生成せず、描画データは外部から渡されたSprite参照を正とします。
    /// </summary>
    internal sealed class PrototypeRuntimeTileFactory
    {
        private readonly Sprite grassSprite;
        private readonly Sprite pathSprite;

        private Tile grassTile;
        private Tile pathTile;
        private Tile collisionTile;

        public PrototypeRuntimeTileFactory(Sprite grassSprite, Sprite pathSprite)
        {
            this.grassSprite = grassSprite;
            this.pathSprite = pathSprite;
        }

        public Tile GrassTile => grassTile ??= CreateVisualTile("草地タイル", grassSprite);
        public Tile PathTile => pathTile ??= CreateVisualTile("小道タイル", pathSprite);
        public Tile CollisionTile => collisionTile ??= CreateCollisionTile();

        private static Tile CreateVisualTile(string name, Sprite sprite)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = name;
            tile.sprite = sprite;
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
    }
}
