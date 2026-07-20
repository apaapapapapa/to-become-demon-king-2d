using DemonKing.Core.Input;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// Dodge入力を受け取り、3D Physics上のX/Y平面で短時間の回避移動を行います。
    /// Elevation（Z）は変更せず、通常移動とはCharacterPlanarMotorのロックで排他制御します。
    /// 最終的な3軸移動の合成はCharacterPhysicsBody3Dへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterPhysicsBody3D))]
    public class CharacterDodge : MonoBehaviour
    {
        private static readonly Vector2 DefaultDirection = Vector2.down;

        [SerializeField] private DodgeDefinition dodgeDefinition;

        private PlayerInputReader playerInput;
        private MoveInputReader moveInput;
        private CharacterPlanarMotor motor;
        private CharacterPhysicsBody3D physicsBody;

        private Vector2 lastMoveDirection = DefaultDirection;
        private Vector2 dodgeDirection;
        private float dodgeTimeRemaining;
        private float cooldownRemaining;

        public bool IsDodging { get; private set; }
        public float CooldownRemaining => Mathf.Max(0f, cooldownRemaining);

        protected virtual void Awake()
        {
            playerInput = GetComponent<PlayerInputReader>();
            moveInput = GetComponent<MoveInputReader>();
            motor = GetComponent<CharacterPlanarMotor>();
            if (motor == null)
            {
                motor = gameObject.AddComponent<CharacterPlanarMotor>();
            }

            physicsBody = GetComponent<CharacterPhysicsBody3D>();
            physicsBody.EnsureConfigured();
        }

        protected virtual void OnEnable()
        {
            if (playerInput != null)
            {
                playerInput.DodgePressed += HandleDodgePressed;
            }
        }

        protected virtual void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.DodgePressed -= HandleDodgePressed;
            }

            FinishDodge();
        }

        protected virtual void Update()
        {
            Vector2 move = moveInput == null ? Vector2.zero : moveInput.Move;
            if (move.sqrMagnitude > 0.0001f)
            {
                lastMoveDirection = move.normalized;
            }

            if (cooldownRemaining > 0f)
            {
                cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!IsDodging || physicsBody == null || dodgeDefinition == null)
            {
                return;
            }

            physicsBody.QueuePlanarDelta(
                dodgeDirection * (dodgeDefinition.DodgeSpeed * Time.fixedDeltaTime));

            dodgeTimeRemaining -= Time.fixedDeltaTime;
            if (dodgeTimeRemaining <= 0f)
            {
                FinishDodge();
            }
        }

        public void Configure(DodgeDefinition definition)
        {
            dodgeDefinition = definition;
        }

        /// <summary>
        /// 指定したフィールド平面方向への回避開始を試みます。
        /// PlayModeテストやAI制御からも利用できるよう、入力イベントとは分離しています。
        /// </summary>
        public bool TryDodge(Vector2 direction)
        {
            if (dodgeDefinition == null || IsDodging || cooldownRemaining > 0f)
            {
                return false;
            }

            if (playerInput != null && playerInput.CurrentContext != PlayerInputContext.Gameplay)
            {
                return false;
            }

            Vector2 resolvedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : lastMoveDirection;
            if (resolvedDirection.sqrMagnitude <= 0.0001f)
            {
                resolvedDirection = DefaultDirection;
            }

            dodgeDirection = resolvedDirection;
            dodgeTimeRemaining = dodgeDefinition.Duration;
            cooldownRemaining = dodgeDefinition.Cooldown;
            IsDodging = true;
            motor?.SetMovementLocked(true);
            return true;
        }

        private void HandleDodgePressed()
        {
            Vector2 move = moveInput == null ? Vector2.zero : moveInput.Move;
            TryDodge(move);
        }

        private void FinishDodge()
        {
            if (!IsDodging)
            {
                return;
            }

            IsDodging = false;
            dodgeTimeRemaining = 0f;
            motor?.SetMovementLocked(false);
        }
    }
}
