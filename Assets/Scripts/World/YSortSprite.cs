using UnityEngine;

namespace DemonKing.World
{
    /// <summary>
    /// 画面下側のオブジェクトが手前に描画されるよう、SpriteRenderer に一貫した Y座標ベースの並び替えを適用します。
    /// 現在の実行時生成スライム試作以外で、今後追加する動的なワールドオブジェクトに使用します。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class YSortSprite : MonoBehaviour
    {
        [SerializeField, Min(1)] private int precision = 100;
        [SerializeField] private int orderOffset;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * precision) + orderOffset;
        }
    }
}
