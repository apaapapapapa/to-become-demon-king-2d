using UnityEngine;
using UnityEngine.InputSystem;

namespace DemonKing.Field
{
    /// <summary>Responsive top-down movement with a small squash-and-stretch animation.</summary>
    public sealed class SlimeController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.4f;
        private Vector2 fieldExtents = new(7.9f, 4.95f);
        private Vector2 input;
        private float animationTime;

        public void Configure(Vector2 extents) => fieldExtents = extents;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            input = keyboard == null ? Vector2.zero : new Vector2(
                (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0) -
                (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0),
                (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0) -
                (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0)).normalized;

            Vector3 next = transform.position + (Vector3)(input * moveSpeed * Time.deltaTime);
            next.x = Mathf.Clamp(next.x, -fieldExtents.x, fieldExtents.x);
            next.y = Mathf.Clamp(next.y, -fieldExtents.y, fieldExtents.y);
            transform.position = next;

            animationTime += Time.deltaTime * (input.sqrMagnitude > 0 ? 10f : 3f);
            float bounce = input.sqrMagnitude > 0 ? Mathf.Abs(Mathf.Sin(animationTime)) : 0;
            transform.localScale = new Vector3(1f + bounce * 0.08f, 1f - bounce * 0.08f, 1f);
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
            GUI.Label(new Rect(Screen.width / 2f - 220, Screen.height - 42, 440, 26),
                "WASD / 矢印キー でスライムを移動", hint);
        }
    }
}
