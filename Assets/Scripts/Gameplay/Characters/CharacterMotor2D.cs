using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// キャラクターの2D移動だけを担当します。
    /// 入力取得や見た目のアニメーションを分離し、将来のNPC・敵・別プレイヤー実装でも移動ロジックを再利用できる形にします。
    /// </summary>
    [RequireComponent(typeof(MoveInputReader))]
    public sealed class CharacterMotor2D : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.4f;
        [SerializeField] private bool clampToBounds = true;
        [SerializeField] private Vector2 fieldExtents = new(7.9f, 4.95f);

        private MoveInputReader inputReader;

        public Vector2 CurrentInput => inputReader == null ? Vector2.zero : inputReader.Move;

        private void Awake()
        {
            inputReader = GetComponent<MoveInputReader>();
        }

        private void Update()
        {
            Vector2 input = CurrentInput;
            Vector3 next = transform.position + (Vector3)(input * moveSpeed * Time.deltaTime);

            if (clampToBounds)
            {
                next.x = Mathf.Clamp(next.x, -fieldExtents.x, fieldExtents.x);
                next.y = Mathf.Clamp(next.y, -fieldExtents.y, fieldExtents.y);
            }

            transform.position = next;
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
