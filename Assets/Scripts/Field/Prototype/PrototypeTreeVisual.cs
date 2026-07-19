using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 木Prefabへプロジェクト管理の静的Spriteアートを適用します。
    /// RuntimeShapeFactoryによる仮図形生成は行いません。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeTreeVisual : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = ResolveRenderer();
            ApplySprite();
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        public void SetSprite(Sprite newSprite)
        {
            sprite = newSprite;
            spriteRenderer ??= ResolveRenderer();
            ApplySprite();
        }

        private SpriteRenderer ResolveRenderer()
        {
            SpriteRenderer existing = GetComponentInChildren<SpriteRenderer>(includeInactive: true);
            if (existing != null)
            {
                return existing;
            }

            GameObject art = new("Art");
            art.transform.SetParent(transform, false);
            SpriteRenderer renderer = art.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = SortingLayerNames.World;
            return renderer;
        }

        private void ApplySprite()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
    }
}
