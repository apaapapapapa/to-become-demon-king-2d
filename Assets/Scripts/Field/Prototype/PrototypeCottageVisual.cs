using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 校舎Prefabへプロジェクト管理の静的Spriteアートを適用します。
    /// RuntimeShapeFactoryによる仮図形生成は行いません。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeCottageVisual : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;

        private void Awake()
        {
            CreateSpriteRendererIfNeeded();
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        private void CreateSpriteRendererIfNeeded()
        {
            if (GetComponentInChildren<SpriteRenderer>(includeInactive: true) != null)
            {
                return;
            }

            GameObject art = new("Art");
            art.transform.SetParent(transform, false);
            SpriteRenderer renderer = art.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = SortingLayerNames.World;
        }
    }
}
