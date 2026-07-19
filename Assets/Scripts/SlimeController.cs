using UnityEngine;
using UnityEngine.InputSystem;

namespace DemonKing.Field
{
    /// <summary>
    /// Responsive prototype movement driven by a logical Input Action rather than direct key polling.
    /// Supports keyboard and gamepad while preserving the current runtime meadow prototype.
    /// Also applies Y-based sorting to the slime's child renderers for future 2.5D depth behavior.
    /// </summary>
    public sealed class SlimeController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.4f;
        [SerializeField, Min(1)] private int sortingPrecision = 100;

        private readonly InputAction moveAction = new("Move", InputActionType.Value, expectedControlType: "Vector2");
        private Vector2 fieldExtents = new(7.9f, 4.95f);
        private Vector2 input;
        private float animationTime;
        private SpriteRenderer[] spriteRenderers;
        private int[] relativeSortingOrders;

        public void Configure(Vector2 extents) => fieldExtents = extents;

        private void Awake()
        {
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            moveAction.AddBinding("<Gamepad>/leftStick");

            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            relativeSortingOrders = new int[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                relativeSortingOrders[i] = spriteRenderers[i].sortingOrder;
            }
        }

        private void OnEnable() => moveAction.Enable();

        private void OnDisable() => moveAction.Disable();

        private void OnDestroy() => moveAction.Dispose();

        private void Update()
        {
            input = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

            Vector3 next = transform.position + (Vector3)(input * moveSpeed * Time.deltaTime);
            next.x = Mathf.Clamp(next.x, -fieldExtents.x, fieldExtents.x);
            next.y = Mathf.Clamp(next.y, -fieldExtents.y, fieldExtents.y);
            transform.position = next;

            animationTime += Time.deltaTime * (input.sqrMagnitude > 0 ? 10f : 3f);
            float bounce = input.sqrMagnitude > 0 ? Mathf.Abs(Mathf.Sin(animationTime)) : 0;
            transform.localScale = new Vector3(1f + bounce * 0.08f, 1f - bounce * 0.08f, 1f);

            UpdateSortingOrder();
        }

        private void UpdateSortingOrder()
        {
            int yOrder = -Mathf.RoundToInt(transform.position.y * sortingPrecision);
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].sortingOrder = yOrder + relativeSortingOrders[i];
            }
        }

        private void OnGUI()
        {
            GUIStyle title = new(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.94f, 1f, 0.83f) }
            };
            GUIStyle hint = new(title) { fontSize = 14, fontStyle = FontStyle.Normal };
            GUI.Label(new Rect(Screen.width / 2f - 180, 18, 360, 32), "はじまりの草原", title);
            GUI.Label(new Rect(Screen.width / 2f - 260, Screen.height - 42, 520, 26),
                "WASD / 矢印キー / ゲームパッド左スティック でスライムを移動", hint);
        }
    }
}
