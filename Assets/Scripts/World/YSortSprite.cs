using UnityEngine;

namespace DemonKing.World
{
    /// <summary>
    /// Applies deterministic Y-based sorting to a SpriteRenderer so lower objects render in front.
    /// Use this for future dynamic world objects outside the current runtime slime prototype.
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
