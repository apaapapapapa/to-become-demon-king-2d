using UnityEngine;
using UnityEngine.InputSystem;

namespace DemonKing.Field
{
    /// <summary>
    /// キーを直接監視せず、論理 Input Action で応答性の高い試作用移動を実現します。
    /// 現在の実行時生成の草原試作を維持しながら、キーボードとゲームパッドに対応します。
    /// 将来の 2.5D 奥行き表現に備え、スライムの子 Renderer に Y座標ベースの並び替えも適用します。
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
        private Texture2D panelTexture;
        private Texture2D accentTexture;

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

            panelTexture = CreateSolidTexture(new Color(0.045f, 0.10f, 0.12f, 0.86f));
            accentTexture = CreateSolidTexture(new Color(0.93f, 0.57f, 0.31f, 0.95f));
        }

        private void OnEnable() => moveAction.Enable();

        private void OnDisable() => moveAction.Disable();

        private void OnDestroy()
        {
            moveAction.Dispose();
            if (panelTexture != null) Destroy(panelTexture);
            if (accentTexture != null) Destroy(accentTexture);
        }

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
            transform.localRotation = Quaternion.Euler(0f, 0f, -input.x * 3f);

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
            float scale = Mathf.Clamp(Screen.height / 720f, 0.82f, 1.22f);

            GUIStyle title = new(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(20f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.89f, 0.66f) }
            };
            GUIStyle subtitle = new(title)
            {
                fontSize = Mathf.RoundToInt(12f * scale),
                fontStyle = FontStyle.Normal,
                normal = { textColor = new Color(0.72f, 0.86f, 0.72f) }
            };
            GUIStyle hint = new(subtitle)
            {
                fontSize = Mathf.RoundToInt(13f * scale),
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.91f, 0.94f, 0.84f) }
            };

            Rect locationPanel = new(22f * scale, 22f * scale, 286f * scale, 76f * scale);
            GUI.DrawTexture(locationPanel, panelTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(locationPanel.x, locationPanel.y, 5f * scale, locationPanel.height),
                accentTexture, ScaleMode.StretchToFill);
            GUI.Label(new Rect(locationPanel.x + 18f * scale, locationPanel.y + 8f * scale,
                locationPanel.width - 24f * scale, 34f * scale), "夕映えの学園草原", title);
            GUI.Label(new Rect(locationPanel.x + 18f * scale, locationPanel.y + 40f * scale,
                locationPanel.width - 24f * scale, 24f * scale), "魔法学園・西の庭", subtitle);

            float controlsWidth = 510f * scale;
            Rect controlsPanel = new((Screen.width - controlsWidth) * 0.5f, Screen.height - 54f * scale,
                controlsWidth, 34f * scale);
            GUI.DrawTexture(controlsPanel, panelTexture, ScaleMode.StretchToFill);
            GUI.Label(controlsPanel, "移動　WASD／矢印キー／ゲームパッド左スティック", hint);
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
