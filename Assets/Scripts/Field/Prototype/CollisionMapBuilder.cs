using UnityEngine;
using UnityEngine.Tilemaps;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Collision Tilemapへ実際の衝突セルを配置します。
    /// プレイヤー移動はTransform境界ClampではなくTilemapCollider2Dでフィールド外周を制限します。
    /// </summary>
    internal sealed class CollisionMapBuilder
    {
        private readonly PrototypeTilemapContext tilemaps;
        private readonly PrototypeRuntimeTileFactory tiles;
        private readonly int playableRadius;

        public CollisionMapBuilder(
            PrototypeTilemapContext tilemaps,
            PrototypeRuntimeTileFactory tiles,
            int playableRadius)
        {
            this.tilemaps = tilemaps;
            this.tiles = tiles;
            this.playableRadius = Mathf.Max(4, playableRadius);
        }

        public void Build()
        {
            Tilemap collision = tilemaps.Collision;
            collision.ClearAllTiles();

            // 外周2セルを衝突帯にし、高速移動時にもフィールド外へ抜けにくい厚みを確保します。
            for (int x = -playableRadius; x <= playableRadius; x++)
            {
                for (int y = -playableRadius; y <= playableRadius; y++)
                {
                    int distance = Mathf.Abs(x) + Mathf.Abs(y);
                    if (distance < playableRadius - 1 || distance > playableRadius)
                    {
                        continue;
                    }

                    collision.SetTile(new Vector3Int(x, y, 0), tiles.CollisionTile);
                }
            }

            // 校舎の基部は通り抜け不可とし、ワールドオブジェクトとの衝突検証にも使用します。
            AddWorldObstacle(new Vector2(-4.55f, 1.15f), 2, 1);
        }

        private void AddWorldObstacle(Vector2 worldPosition, int halfWidth, int halfHeight)
        {
            Vector3Int center = tilemaps.Collision.WorldToCell(worldPosition);
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    tilemaps.Collision.SetTile(center + new Vector3Int(x, y, 0), tiles.CollisionTile);
                }
            }
        }
    }
}
