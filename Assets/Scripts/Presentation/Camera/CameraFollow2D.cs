using UnityEngine;

namespace DemonKing.Presentation.CameraSystem
{
    /// <summary>
    /// カメラを任意のTransformへ滑らかに追従させます。
    /// プレイヤー固有クラスには依存せず、追従対象は外側の構成ルートから設定します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new(0f, 1.7f, 0f);
        [SerializeField, Min(0f)] private float smoothTime = 0.18f;
        [SerializeField, Min(0.1f)] private float maxSpeed = 30f;

        private Transform target;
        private Vector3 velocity;
        private float cameraZ;

        public Transform Target => target;

        private void Awake()
        {
            cameraZ = transform.position.z;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = GetDesiredPosition();
            if (smoothTime <= 0f)
            {
                transform.position = desiredPosition;
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                smoothTime,
                maxSpeed,
                Time.deltaTime);
        }

        public void SetTarget(Transform newTarget, bool snapImmediately = true)
        {
            target = newTarget;
            velocity = Vector3.zero;

            if (snapImmediately && target != null)
            {
                SnapToTarget();
            }
        }

        public void ClearTarget()
        {
            target = null;
            velocity = Vector3.zero;
        }

        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            transform.position = GetDesiredPosition();
        }

        private Vector3 GetDesiredPosition()
        {
            Vector3 desiredPosition = target.position + offset;
            desiredPosition.z = cameraZ;
            return desiredPosition;
        }
    }
}
