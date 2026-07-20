using DemonKing.Presentation.Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプシーン内のIsometric Gridと各Tilemapへの参照をまとめます。
    /// シーンに不足がある場合は最小構成を補完し、Builder側がGameObject名探索を繰り返さないようにします。
    /// Collision Tilemapは3D Collider生成用の配置マーカーとして扱います。
    /// </summary>
    internal sealed class PrototypeTilemapContext
    {
        private PrototypeTilemapContext(Grid grid, Tilemap ground, Tilemap collision, Tilemap props, Tilemap foreground)
        {
            Grid = grid;
            Ground = ground;
            Collision = collision;
            Props = props;
            Foreground = foreground;
        }

        public Grid Grid { get; }
        public Tilemap Ground { get; }
        public Tilemap Collision { get; }
        public Tilemap Props { get; }
        public Tilemap Foreground { get; }

        public static PrototypeTilemapContext Resolve()
        {
            Grid grid = Object.FindAnyObjectByType<Grid>();
            if (grid == null)
            {
                GameObject gridObject = new("Grid");
                grid = gridObject.AddComponent<Grid>();
                grid.cellLayout = GridLayout.CellLayout.Isometric;
                grid.cellSize = new Vector3(1f, 0.5f, 1f);
            }

            Tilemap ground = ResolveTilemap(
                grid.transform,
                "Ground",
                SortingLayerNames.Ground,
                PrototypeWorldMath.GroundOrder,
                true);
            Tilemap collision = ResolveTilemap(
                grid.transform,
                "Collision",
                SortingLayerNames.Ground,
                PrototypeWorldMath.GroundOrder,
                false);
            Tilemap props = ResolveTilemap(
                grid.transform,
                "Props",
                SortingLayerNames.World,
                0,
                true);
            Tilemap foreground = ResolveTilemap(
                grid.transform,
                "Foreground",
                SortingLayerNames.Foreground,
                0,
                true);

            TilemapRenderer propsRenderer = props.GetComponent<TilemapRenderer>();
            if (propsRenderer != null)
            {
                propsRenderer.mode = TilemapRenderer.Mode.Individual;
            }

            return new PrototypeTilemapContext(grid, ground, collision, props, foreground);
        }

        private static Tilemap ResolveTilemap(
            Transform grid,
            string name,
            string sortingLayer,
            int sortingOrder,
            bool rendererEnabled)
        {
            Transform child = grid.Find(name);
            GameObject tilemapObject;

            if (child == null)
            {
                tilemapObject = new GameObject(name);
                tilemapObject.transform.SetParent(grid, false);
            }
            else
            {
                tilemapObject = child.gameObject;
            }

            Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = tilemapObject.AddComponent<Tilemap>();
            }

            TilemapRenderer renderer = tilemapObject.GetComponent<TilemapRenderer>();
            if (renderer == null)
            {
                renderer = tilemapObject.AddComponent<TilemapRenderer>();
            }

            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = rendererEnabled;
            return tilemap;
        }
    }
}
