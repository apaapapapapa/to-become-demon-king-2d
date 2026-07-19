using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// キャラクターの2D移動だけを担当します。
    /// 入力取得や見た目のアニメーションを分離し、Rigidbody2D経由で移動と衝突を成立させます。
    /// </summary>
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CharacterMotor2D : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.4f;
        [SerializeField] private bool clampToBounds = true;
        [SerializeField] private Vector2 fieldExtents = new(7.9f, 4.95f);

        private MoveInputReader inputReader;
        private Rigidbody2D body;
        private Vector2 currentInput;

        public Vector2 CurrentInput => currentInput;

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
            // Input Systemの値はUpdateで取得し、物理移動はFixedUpdateで適用します。
            currentInput = inputReader == null ? Vector2.zero : inputReader.Move;
        }

        private void FixedUpdate()
        {
            if (body == null)
            {
                return;
            }

            Vector2 next = body.position + currentInput * (moveSpeed * Time.fixedDeltaTime);

            if (clampToBounds)
            {
                next.x = Mathf.Clamp(next.x, -fieldExtents.x, fieldExtents.x);
                next.y = Mathf.Clamp(next.y, -fieldExtents.y, fieldExtents.y);
            }

            body.MovePosition(next);
        }

        public void Configure(float speed, Vector2 extents)
        {
            SetMoveSpeed(speed);
            SetBounds(extents);
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0.1f, speed);
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
