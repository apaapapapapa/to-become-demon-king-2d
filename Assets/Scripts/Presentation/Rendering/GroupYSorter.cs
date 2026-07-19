using DemonKing.Core.Math;
using UnityEngine;

namespace DemonKing.Presentation.Rendering
{
    /// <summary>
    /// 子階層にある複数のSpriteRendererの相対的な描画順を保ちながら、ルートのY座標に応じて一括で並び替えます。
    /// 複数パーツで構成されるキャラクターや動的オブジェクト向けです。
    /// 描画順精度はこのコンポーネントが所有します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GroupYSorter : MonoBehaviour
    {
        [SerializeField] private string sortingLayerName = SortingLayerNames.World;
        [SerializeField, Min(1)] private int precision = 100;

        private SpriteRenderer[] spriteRenderers;
        private int[] relativeSortingOrders;

        private void Awake()
        {
            RefreshRenderers();
        }

        private void OnEnable()
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                RefreshRenderers();
            }
        }

        private void LateUpdate()
        {
            if (spriteRenderers == null)
            {
                return;
            }

            int baseOrder = WorldSortOrder.FromWorldY(transform.position.y, precision);
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].sortingOrder = baseOrder + relativeSortingOrders[i];
                }
            }
        }

        public void RefreshRenderers()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            relativeSortingOrders = new int[spriteRenderers.Length];

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].sortingLayerName = sortingLayerName;
                relativeSortingOrders[i] = spriteRenderers[i].sortingOrder;
            }
        }
    }
}
