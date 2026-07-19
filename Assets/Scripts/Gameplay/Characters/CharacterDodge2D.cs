using DemonKing.Core.Input;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// Dodge入力を受け取り、Rigidbody2Dで短時間の回避移動を行います。
    /// 通常移動とは責務を分け、回避中だけCharacterMotor2Dをロックします。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterMotor2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CharacterDodge2D : MonoBehaviour
    {
        private static readonly Vector2 DefaultDirection = Vector2.down;

        [SerializeField] private DodgeDefinition dodgeDefinition;

        private PlayerInputReader playerInput;
        private MoveInputReader moveInput;
        private CharacterMotor2D motor;
        private Rigidbody2D body;

        private Vector2 lastMoveDirection = DefaultDirection;
        private Vector2 dodgeDirection;
        private float dodgeTimeRemaining;
        private float cooldownRemaining;

        public bool IsDodging { get; private set; }
        public float CooldownRemaining => Mathf.Max(0f, cooldownRemaining);

        private void Awake()
        {
            playerInput = GetComponent<PlayerInputReader>();
            moveInput = GetComponent<MoveInputReader>();
            motor = GetComponent<CharacterMotor2D>();
            body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (playerInput != null)
            {
                playerInput.DodgePressed += HandleDodgePressed;
            }
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.DodgePressed -= HandleDodgePressed;
            }

            FinishDodge();
        }

        private void Update()
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

        private void FixedUpdate()
        {
            if (!IsDodging || body == null || dodgeDefinition == null)
            {
                return;
            }

            Vector2 next = body.position + dodgeDirection * (dodgeDefinition.DodgeSpeed * Time.fixedDeltaTime);
            body.MovePosition(next);

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
        /// 指定方向への回避開始を試みます。
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
