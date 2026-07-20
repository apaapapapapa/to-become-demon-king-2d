using UnityEngine;
using UnityEngine.Tilemaps;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Collision Tilemapを配置マーカーとして構築し、実際の移動衝突は3D BoxColliderへ変換します。
    /// X/Yはフィールド平面、ZはElevationです。有限高さの障害物はCollider上端を超えれば上空通過できます。
    /// </summary>
    internal sealed class CollisionMapBuilder
    {
        private const float BoundaryCollisionHeight = 1000f;
        private const float BuildingCollisionHeight = 4f;
        private const float PlanarFillRatio = 0.9f;
        private const string PhysicsRootName = "Physics3D";

        private readonly PrototypeTilemapContext tilemaps;
        private readonly PrototypeRuntimeTileFactory tiles;
        private readonly int playableRadius;

        private Transform physicsRoot;

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
            RebuildPhysicsRoot(collision.transform);

            // 外周2セルは高さ方向にも十分大きいHard Boundaryとして扱います。
            for (int x = -playableRadius; x <= playableRadius; x++)
            {
                for (int y = -playableRadius; y <= playableRadius; y++)
                {
                    int distance = Mathf.Abs(x) + Mathf.Abs(y);
                    if (distance < playableRadius - 1 || distance > playableRadius)
                    {
                        continue;
                    }

                    AddCollisionCell(
                        new Vector3Int(x, y, 0),
                        BoundaryCollisionHeight,
                        "外周");
                }
            }

            // 校舎は有限高さの3D障害物として構築します。
            // 将来、ActorのElevationがCollider上端を超えれば追加の例外判定なしで上空通過できます。
            AddWorldObstacle(
                new Vector2(-4.55f, 1.15f),
                halfWidth: 2,
                halfHeight: 1,
                BuildingCollisionHeight,
                "校舎");
        }

        private void RebuildPhysicsRoot(Transform collisionTransform)
        {
            Transform existing = collisionTransform.Find(PhysicsRootName);
            if (existing != null)
            {
                existing.gameObject.SetActive(false);
                Object.Destroy(existing.gameObject);
            }

            GameObject root = new(PhysicsRootName);
            physicsRoot = root.transform;
            physicsRoot.SetParent(collisionTransform, false);
        }

        private void AddWorldObstacle(
            Vector2 worldPosition,
            int halfWidth,
            int halfHeight,
            float collisionHeight,
            string label)
        {
            Vector3Int center = tilemaps.Collision.WorldToCell(worldPosition);
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    AddCollisionCell(
                        center + new Vector3Int(x, y, 0),
                        collisionHeight,
                        label);
                }
            }
        }

        private void AddCollisionCell(
            Vector3Int cell,
            float collisionHeight,
            string label)
        {
            Tilemap collision = tilemaps.Collision;
            collision.SetTile(cell, tiles.CollisionTile);

            GameObject volume = new($"{label} Collision {cell.x},{cell.y}");
            volume.transform.SetParent(physicsRoot, false);
            Vector3 localCenter = collision.GetCellCenterLocal(cell);
            volume.transform.localPosition = new Vector3(localCenter.x, localCenter.y, 0f);

            BoxCollider collider = volume.AddComponent<BoxCollider>();
            Vector3 cellSize = tilemaps.Grid.cellSize;
            collider.center = new Vector3(0f, 0f, collisionHeight * 0.5f);
            collider.size = new Vector3(
                Mathf.Max(0.05f, cellSize.x * PlanarFillRatio),
                Mathf.Max(0.05f, cellSize.y * PlanarFillRatio),
                collisionHeight);
        }
    }
}
