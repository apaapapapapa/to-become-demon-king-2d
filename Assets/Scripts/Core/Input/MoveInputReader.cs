using UnityEngine;
using UnityEngine.InputSystem;

namespace DemonKing.Core.Input
{
    /// <summary>
    /// プレイヤー移動の論理入力を一か所に集約します。
    /// 入力バインディングは.inputactionsアセットで管理し、ゲームプレイ側はMoveの値だけを参照します。
    /// </summary>
    public sealed class MoveInputReader : MonoBehaviour
    {
        private const string DefaultResourcesPath = "Input/PlayerControls";

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string resourcesPath = DefaultResourcesPath;

        private InputActionAsset runtimeInputActions;
        private InputAction moveAction;

        public Vector2 Move => moveAction == null
            ? Vector2.zero
            : Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        private void Awake()
        {
            InitializeActions();
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
            if (runtimeInputActions != null)
            {
                Destroy(runtimeInputActions);
            }
        }

        private void InitializeActions()
        {
            InputActionAsset source = inputActions;
            if (source == null)
            {
                source = Resources.Load<InputActionAsset>(resourcesPath);
            }

            if (source == null)
            {
                Debug.LogError(
                    $"入力アクションアセットが見つかりません。Resources/{resourcesPath}.inputactions を確認してください。",
                    this);
                return;
            }

            // アセット本体のEnable状態を他の利用者と共有しないよう、実行時専用の複製を使用します。
            runtimeInputActions = Instantiate(source);
            moveAction = runtimeInputActions.FindAction($"{actionMapName}/{moveActionName}", throwIfNotFound: false);

            if (moveAction == null)
            {
                Debug.LogError(
                    $"入力アクション '{actionMapName}/{moveActionName}' が見つかりません。",
                    this);
            }
        }
    }
}
