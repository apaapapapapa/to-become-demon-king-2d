using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Presentation.Characters
{
    /// <summary>
    /// 移動量に応じた簡易的な潰れ・伸び表現を担当します。
    /// ゲームプレイの移動処理とは分離し、将来Animatorや本番スプライトへ置き換えやすくします。
    /// </summary>
    [RequireComponent(typeof(MoveInputReader))]
    public sealed class CharacterSquashAnimator : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float stretchAmount = 0.08f;
        [SerializeField, Min(0f)] private float movingFrequency = 10f;
        [SerializeField, Min(0f)] private float idleFrequency = 3f;
        [SerializeField, Min(0f)] private float tiltAngle = 3f;

        private MoveInputReader inputReader;
        private Vector3 baseScale;
        private Quaternion baseRotation;
        private float animationTime;

        private void Awake()
        {
            inputReader = GetComponent<MoveInputReader>();
            baseScale = transform.localScale;
            baseRotation = transform.localRotation;
        }

        private void Update()
        {
            Vector2 input = inputReader.Move;
            bool isMoving = input.sqrMagnitude > 0.0001f;

            animationTime += Time.deltaTime * (isMoving ? movingFrequency : idleFrequency);
            float bounce = isMoving ? Mathf.Abs(Mathf.Sin(animationTime)) : 0f;

            transform.localScale = new Vector3(
                baseScale.x * (1f + bounce * stretchAmount),
                baseScale.y * (1f - bounce * stretchAmount),
                baseScale.z);
            transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, -input.x * tiltAngle);
        }

        private void OnDisable()
        {
            transform.localScale = baseScale;
            transform.localRotation = baseRotation;
        }
    }
}
