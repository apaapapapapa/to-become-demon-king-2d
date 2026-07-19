using DemonKing.Core.Math;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.World
{
    /// <summary>
    /// 画面下側のオブジェクトが手前に描画されるよう、単一のSpriteRendererへY座標ベースの描画順を適用します。
    /// 複数SpriteRendererを持つオブジェクトにはGroupYSorterを使用します。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class YSortSprite : MonoBehaviour
    {
        [SerializeField] private string sortingLayerName = SortingLayerNames.World;
        [SerializeField, Min(1)] private int precision = 100;
        [SerializeField] private int orderOffset;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = sortingLayerName;
        }

        private void LateUpdate()
        {
            spriteRenderer.sortingOrder = WorldSortOrder.FromWorldY(
                transform.position.y,
                precision,
                orderOffset);
        }
    }
}
