using DemonKing.Presentation.Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプシーンのTilemap描画順を、プロジェクト共通のSorting Layerルールへ合わせます。
    /// シーン移行中も実行時生成SpriteとTilemapの前後関係が一貫するようにします。
    /// </summary>
    internal static class PrototypeSortingConfigurator
    {
        public static void Configure()
        {
            GameObject gridObject = GameObject.Find("Grid");
            if (gridObject == null)
            {
                return;
            }

            ConfigureTilemap(gridObject.transform, "Ground", SortingLayerNames.Ground, 0, TilemapRenderer.Mode.Chunk);
            ConfigureTilemap(gridObject.transform, "Props", SortingLayerNames.World, 0, TilemapRenderer.Mode.Individual);
            ConfigureTilemap(gridObject.transform, "Foreground", SortingLayerNames.Foreground, 0, TilemapRenderer.Mode.Chunk);
        }

        private static void ConfigureTilemap(
            Transform grid,
            string childName,
            string sortingLayerName,
            int sortingOrder,
            TilemapRenderer.Mode mode)
        {
            Transform child = grid.Find(childName);
            if (child == null)
            {
                Debug.LogWarning($"Tilemap '{childName}' が見つからないため、描画順設定をスキップします。", grid);
                return;
            }

            TilemapRenderer renderer = child.GetComponent<TilemapRenderer>();
            if (renderer == null)
            {
                Debug.LogWarning($"Tilemap '{childName}' にTilemapRendererがないため、描画順設定をスキップします。", child);
                return;
            }

            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
            renderer.mode = mode;
        }
    }
}
