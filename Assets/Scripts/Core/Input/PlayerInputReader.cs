using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DemonKing.Core.Input
{
    /// <summary>
    /// プレイヤー操作に必要な論理入力と入力コンテキストを一か所で管理します。
    /// Gameplay / UI / Disabledのいずれかだけを有効にし、画面状態に応じて入力先を明確に切り替えます。
    /// Ability系の物理Actionは論理AbilitySlotへ変換して通知します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private const string DefaultInputActionsResourcePath = "Input/PlayerControls";

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string gameplayActionMapName = "Gameplay";
        [SerializeField] private string uiActionMapName = "UI";
        [SerializeField] private PlayerInputContext initialContext = PlayerInputContext.Gameplay;

        private InputActionAsset runtimeInputActions;
        private InputActionMap gameplayActionMap;
        private InputActionMap uiActionMap;

        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction artAction;
        private InputAction interactAction;
        private InputAction dodgeAction;
        private InputAction jumpAction;
        private InputAction flightToggleAction;
        private InputAction descendAction;
        private InputAction evolutionAction;
        private InputAction gameplayPauseAction;

        private InputAction navigateAction;
        private InputAction submitAction;
        private InputAction cancelAction;
        private InputAction uiPauseAction;

        private bool componentEnabled;

        public event Action<AbilitySlot> AbilitySlotPressed;

        [Obsolete("Use AbilitySlotPressed instead.")]
        public event Action AttackPressed;

        [Obsolete("Use AbilitySlotPressed instead.")]
        public event Action ArtPressed;

        public event Action InteractPressed;
        public event Action DodgePressed;
        public event Action JumpPressed;
        public event Action FlightTogglePressed;
        public event Action EvolutionPressed;
        public event Action PausePressed;
        public event Action SubmitPressed;
        public event Action CancelPressed;
        public event Action<PlayerInputContext> ContextChanged;

        public PlayerInputContext CurrentContext { get; private set; } = PlayerInputContext.Gameplay;

        public Vector2 Move =>
            CurrentContext == PlayerInputContext.Gameplay && moveAction != null
                ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
                : Vector2.zero;

        /// <summary>
        /// 飛行時の高さ入力です。Jumpを押している間は+1、Descendを押している間は-1になります。
        /// 地上ではJumpの押下イベントだけを使用し、この値はCharacterElevationMotorの飛行中だけ参照します。
        /// </summary>
        public float FlightElevationInput
        {
            get
            {
                if (CurrentContext != PlayerInputContext.Gameplay)
                {
                    return 0f;
                }

                float ascend = jumpAction == null ? 0f : jumpAction.ReadValue<float>();
                float descend = descendAction == null ? 0f : descendAction.ReadValue<float>();
                return Mathf.Clamp(ascend - descend, -1f, 1f);
            }
        }

        public Vector2 Navigate =>
            CurrentContext == PlayerInputContext.UI && navigateAction != null
                ? Vector2.ClampMagnitude(navigateAction.ReadValue<Vector2>(), 1f)
                : Vector2.zero;

        public bool IsGameplayInputEnabled => gameplayActionMap != null && gameplayActionMap.enabled;
        public bool IsUiInputEnabled => uiActionMap != null && uiActionMap.enabled;

        private void Awake()
        {
            CurrentContext = initialContext;
            InitializeActions();
        }

        private void OnEnable()
        {
            componentEnabled = true;
            ApplyContext();
        }

        private void OnDisable()
        {
            componentEnabled = false;
            DisableAllMaps();
        }

        private void OnDestroy()
        {
            UnsubscribeCallbacks();

            if (runtimeInputActions != null)
            {
                Destroy(runtimeInputActions);
            }
        }

        public void SetContext(PlayerInputContext context)
        {
            if (CurrentContext == context)
            {
                ApplyContext();
                return;
            }

            CurrentContext = context;
            ApplyContext();
            ContextChanged?.Invoke(CurrentContext);
        }

        public void EnableGameplayInput()
        {
            SetContext(PlayerInputContext.Gameplay);
        }

        public void EnableUiInput()
        {
            SetContext(PlayerInputContext.UI);
        }

        public void DisableInput()
        {
            SetContext(PlayerInputContext.Disabled);
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
            gameplayActionMap = runtimeInputActions.FindActionMap(gameplayActionMapName, throwIfNotFound: false);
            uiActionMap = runtimeInputActions.FindActionMap(uiActionMapName, throwIfNotFound: false);

            if (gameplayActionMap == null)
            {
                Debug.LogError($"入力アクションマップ '{gameplayActionMapName}' が見つかりません。", this);
            }

            if (uiActionMap == null)
            {
                Debug.LogError($"入力アクションマップ '{uiActionMapName}' が見つかりません。", this);
            }

            moveAction = FindAction(gameplayActionMap, "Move");
            attackAction = FindAction(gameplayActionMap, "Attack");
            artAction = FindAction(gameplayActionMap, "Art");
            interactAction = FindAction(gameplayActionMap, "Interact");
            dodgeAction = FindAction(gameplayActionMap, "Dodge");
            jumpAction = FindAction(gameplayActionMap, "Jump");
            flightToggleAction = FindAction(gameplayActionMap, "FlightToggle");
            descendAction = FindAction(gameplayActionMap, "Descend");
            evolutionAction = FindAction(gameplayActionMap, "Evolution");
            gameplayPauseAction = FindAction(gameplayActionMap, "Pause");

            navigateAction = FindAction(uiActionMap, "Navigate");
            submitAction = FindAction(uiActionMap, "Submit");
            cancelAction = FindAction(uiActionMap, "Cancel");
            uiPauseAction = FindAction(uiActionMap, "Pause");

            SubscribeCallbacks();
        }

        private InputAction FindAction(InputActionMap map, string actionName)
        {
            if (map == null)
            {
                return null;
            }

            InputAction action = map.FindAction(actionName, throwIfNotFound: false);
            if (action == null)
            {
                Debug.LogError($"入力アクション '{map.name}/{actionName}' が見つかりません。", this);
            }

            return action;
        }

        private void ApplyContext()
        {
            DisableAllMaps();

            if (!componentEnabled)
            {
                return;
            }

            switch (CurrentContext)
            {
                case PlayerInputContext.Gameplay:
                    gameplayActionMap?.Enable();
                    break;
                case PlayerInputContext.UI:
                    uiActionMap?.Enable();
                    break;
                case PlayerInputContext.Disabled:
                    break;
                default:
                    Debug.LogWarning($"未対応の入力コンテキストです: {CurrentContext}", this);
                    break;
            }
        }

        private void DisableAllMaps()
        {
            gameplayActionMap?.Disable();
            uiActionMap?.Disable();
        }

        private void SubscribeCallbacks()
        {
            if (attackAction != null) attackAction.performed += OnAttackPerformed;
            if (artAction != null) artAction.performed += OnArtPerformed;
            if (interactAction != null) interactAction.performed += OnInteractPerformed;
            if (dodgeAction != null) dodgeAction.performed += OnDodgePerformed;
            if (jumpAction != null) jumpAction.performed += OnJumpPerformed;
            if (flightToggleAction != null) flightToggleAction.performed += OnFlightTogglePerformed;
            if (evolutionAction != null) evolutionAction.performed += OnEvolutionPerformed;
            if (gameplayPauseAction != null) gameplayPauseAction.performed += OnPausePerformed;
            if (submitAction != null) submitAction.performed += OnSubmitPerformed;
            if (cancelAction != null) cancelAction.performed += OnCancelPerformed;
            if (uiPauseAction != null) uiPauseAction.performed += OnPausePerformed;
        }

        private void UnsubscribeCallbacks()
        {
            if (attackAction != null) attackAction.performed -= OnAttackPerformed;
            if (artAction != null) artAction.performed -= OnArtPerformed;
            if (interactAction != null) interactAction.performed -= OnInteractPerformed;
            if (dodgeAction != null) dodgeAction.performed -= OnDodgePerformed;
            if (jumpAction != null) jumpAction.performed -= OnJumpPerformed;
            if (flightToggleAction != null) flightToggleAction.performed -= OnFlightTogglePerformed;
            if (evolutionAction != null) evolutionAction.performed -= OnEvolutionPerformed;
            if (gameplayPauseAction != null) gameplayPauseAction.performed -= OnPausePerformed;
            if (submitAction != null) submitAction.performed -= OnSubmitPerformed;
            if (cancelAction != null) cancelAction.performed -= OnCancelPerformed;
            if (uiPauseAction != null) uiPauseAction.performed -= OnPausePerformed;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AbilitySlotPressed?.Invoke(AbilitySlot.Primary);
#pragma warning disable CS0618
            AttackPressed?.Invoke();
#pragma warning restore CS0618
        }

        private void OnArtPerformed(InputAction.CallbackContext context)
        {
            AbilitySlotPressed?.Invoke(AbilitySlot.Action1);
#pragma warning disable CS0618
            ArtPressed?.Invoke();
#pragma warning restore CS0618
        }

        private void OnInteractPerformed(InputAction.CallbackContext context) => InteractPressed?.Invoke();
        private void OnDodgePerformed(InputAction.CallbackContext context) => DodgePressed?.Invoke();
        private void OnJumpPerformed(InputAction.CallbackContext context) => JumpPressed?.Invoke();
        private void OnFlightTogglePerformed(InputAction.CallbackContext context) => FlightTogglePressed?.Invoke();
        private void OnEvolutionPerformed(InputAction.CallbackContext context) => EvolutionPressed?.Invoke();
        private void OnPausePerformed(InputAction.CallbackContext context) => PausePressed?.Invoke();
        private void OnSubmitPerformed(InputAction.CallbackContext context) => SubmitPressed?.Invoke();
        private void OnCancelPerformed(InputAction.CallbackContext context) => CancelPressed?.Invoke();
    }
}
