using UnityEngine;
using UnityEngine.InputSystem;

namespace DemonKing.Core.Input
{
    /// <summary>
    /// プレイヤー移動の論理入力を一か所に集約します。
    /// ゲームプレイ側はキーボードやゲームパッドの具体的なキー配置を意識せず、Move の値だけを参照します。
    /// </summary>
    public sealed class MoveInputReader : MonoBehaviour
    {
        private InputAction moveAction;

        public Vector2 Move => moveAction == null
            ? Vector2.zero
            : Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        private void Awake()
        {
            moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");

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
        }

        private void OnEnable()
        {
            moveAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
        }
    }
}
