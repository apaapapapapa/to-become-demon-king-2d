using DemonKing.Gameplay.Interaction;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Interaction機能を確認するための試作NPCです。
    /// 会話システム導入前のため、現在は相互作用結果をConsoleへ出力します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(GroupYSorter))]
    public sealed class PrototypeNpcInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "見習い魔術師";

        private void Awake()
        {
            CircleCollider2D interactionCollider = GetComponent<CircleCollider2D>();
            interactionCollider.isTrigger = true;
            interactionCollider.radius = 0.55f;
            interactionCollider.offset = new Vector2(0f, 0.35f);

            if (GetComponentInChildren<SpriteRenderer>(includeInactive: true) == null)
            {
                CreateVisuals();
            }
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        public bool CanInteract(GameObject interactor)
        {
            return enabled && gameObject.activeInHierarchy && interactor != null;
        }

        public void Interact(GameObject interactor)
        {
            Debug.Log($"{displayName}：『魔王を目指しているの？ まずは訓練用スライムで腕試ししてみて。』", this);
        }

        private void CreateVisuals()
        {
            var shapes = new RuntimeShapeFactory();
            shapes.CreateEllipse("NPCの影", new Vector2(0f, -0.34f), new Vector2(0.88f, 0.24f),
                new Color(0.05f, 0.12f, 0.13f, 0.6f), -2, transform);
            shapes.CreatePatch("ローブ", new Vector2(0f, 0.20f), new Vector2(0.58f, 0.92f),
                new Color(0.30f, 0.34f, 0.68f), 0, transform);
            shapes.CreateEllipse("顔", new Vector2(0f, 0.76f), new Vector2(0.46f, 0.46f),
                new Color(0.95f, 0.76f, 0.58f), 1, transform);
            shapes.CreateDiamond("帽子", new Vector2(0f, 1.08f), new Vector2(0.72f, 0.42f),
                new Color(0.19f, 0.17f, 0.42f), 2, transform);
        }
    }
}
