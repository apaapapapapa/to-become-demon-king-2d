using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DemonKing.Core.Input
{
    /// <summary>
    /// プレイヤー操作に必要な論理入力を一か所で管理します。
    /// デバイス固有のキーやボタンはInput Actionsアセットへ閉じ込め、ゲームプレイ側には値とイベントだけを公開します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private const string DefaultInputActionsResourcePath = "Input/PlayerControls";

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";

        private InputActionAsset runtimeInputActions;
        private InputActionMap playerActionMap;
        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction interactAction;
        private InputAction dodgeAction;
        private InputAction pauseAction;

        public event Action AttackPressed;
        public event Action InteractPressed;
        public event Action DodgePressed;
        public event Action PausePressed;

        public Vector2 Move => moveAction == null
            ? Vector2.zero
            : Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        private void Awake()
        {
            InitializeActions();
        }

        private void OnEnable()
        {
            playerActionMap?.Enable();
        }

        private void OnDisable()
        {
            playerActionMap?.Disable();
        }

        private void OnDestroy()
        {
            UnsubscribeCallbacks();

            if (runtimeInputActions != null)
            {
                Destroy(runtimeInputActions);
            }
        }

        private void InitializeActions()
        {
            InputActionAsset source = inputActions != null
                ? inputActions
                : Resources.Load<InputActionAsset>(DefaultInputActionsResourcePath);

            if (source == null)
            {
                Debug.LogError(
                    $"Input Action Assetが見つかりません。Prefab参照またはResources/{DefaultInputActionsResourcePath}.inputactionsを確認してください。",
                    this);
                return;
            }

            // アセット本体のEnable状態を他の利用者と共有しないよう、プレイヤー個体専用の複製を使用します。
            runtimeInputActions = Instantiate(source);
            playerActionMap = runtimeInputActions.FindActionMap(actionMapName, throwIfNotFound: false);

            if (playerActionMap == null)
            {
                Debug.LogError($"入力アクションマップ '{actionMapName}' が見つかりません。", this);
                return;
            }

            moveAction = FindAction("Move");
            attackAction = FindAction("Attack");
            interactAction = FindAction("Interact");
            dodgeAction = FindAction("Dodge");
            pauseAction = FindAction("Pause");

            SubscribeCallbacks();
        }

        private InputAction FindAction(string actionName)
        {
            InputAction action = playerActionMap.FindAction(actionName, throwIfNotFound: false);
            if (action == null)
            {
                Debug.LogError($"入力アクション '{actionMapName}/{actionName}' が見つかりません。", this);
            }

            return action;
        }

        private void SubscribeCallbacks()
        {
            if (attackAction != null) attackAction.performed += OnAttackPerformed;
            if (interactAction != null) interactAction.performed += OnInteractPerformed;
            if (dodgeAction != null) dodgeAction.performed += OnDodgePerformed;
            if (pauseAction != null) pauseAction.performed += OnPausePerformed;
        }

        private void UnsubscribeCallbacks()
        {
            if (attackAction != null) attackAction.performed -= OnAttackPerformed;
            if (interactAction != null) interactAction.performed -= OnInteractPerformed;
            if (dodgeAction != null) dodgeAction.performed -= OnDodgePerformed;
            if (pauseAction != null) pauseAction.performed -= OnPausePerformed;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context) => AttackPressed?.Invoke();
        private void OnInteractPerformed(InputAction.CallbackContext context) => InteractPressed?.Invoke();
        private void OnDodgePerformed(InputAction.CallbackContext context) => DodgePressed?.Invoke();
        private void OnPausePerformed(InputAction.CallbackContext context) => PausePressed?.Invoke();
    }
}
