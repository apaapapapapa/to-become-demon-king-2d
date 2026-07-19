using DemonKing.Core.Input;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// キャラクターの2D通常移動だけを担当します。
    /// 移動速度はCharacterStatsDefinitionから取得し、回避など別の移動制御中は外側から一時ロックできます。
    /// </summary>
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CharacterMotor2D : MonoBehaviour
    {
        private const float DefaultMoveSpeed = 3.4f;

        [SerializeField] private CharacterStatsDefinition statsDefinition;

        private MoveInputReader inputReader;
        private Rigidbody2D body;
        private Vector2 currentInput;
        private bool clampToBounds;
        private bool movementLocked;
        private Vector2 fieldExtents;

        public Vector2 CurrentInput => currentInput;
        public float MoveSpeed => statsDefinition == null ? DefaultMoveSpeed : statsDefinition.MoveSpeed;
        public bool IsMovementLocked => movementLocked;

        private void Awake()
        {
            inputReader = GetComponent<MoveInputReader>();
            body = GetComponent<Rigidbody2D>();

            // 2D見下ろし／アイソメトリック移動では重力や回転を使用しないため、実行時にも前提を固定します。
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void Update()
        {
            // 入力値は常に保持し、通常移動を止める必要があるかはFixedUpdateで判断します。
            currentInput = inputReader == null ? Vector2.zero : inputReader.Move;
        }

        private void FixedUpdate()
        {
            if (body == null || movementLocked)
            {
                return;
            }

            Vector2 next = body.position + currentInput * (MoveSpeed * Time.fixedDeltaTime);

            if (clampToBounds)
            {
                next.x = Mathf.Clamp(next.x, -fieldExtents.x, fieldExtents.x);
                next.y = Mathf.Clamp(next.y, -fieldExtents.y, fieldExtents.y);
            }

            body.MovePosition(next);
        }

        public void Configure(CharacterStatsDefinition definition)
        {
            statsDefinition = definition;
        }

        public void SetMovementLocked(bool locked)
        {
            movementLocked = locked;
        }

        public void SetBounds(Vector2 extents)
        {
            fieldExtents = new Vector2(Mathf.Abs(extents.x), Mathf.Abs(extents.y));
            clampToBounds = true;
        }

        public void DisableBounds()
        {
            clampToBounds = false;
        }
    }
}
