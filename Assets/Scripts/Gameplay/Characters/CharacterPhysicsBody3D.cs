using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// キャラクターの3D Physics本体と、1 FixedUpdate内の移動合成を担当します。
    /// X/Yはフィールド平面、ZはElevationとして扱います。
    /// Planar / Dodge / Elevationの各MotorはRigidbodyを直接移動せず、このComponentへ移動量を要求します。
    /// </summary>
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class CharacterPhysicsBody3D : MonoBehaviour
    {
        private const float DefaultRadius = 0.36f;
        private const float DefaultHeight = 0.72f;

        [SerializeField, Min(0.05f)] private float collisionRadius = DefaultRadius;
        [SerializeField, Min(0.1f)] private float collisionHeight = DefaultHeight;
        [SerializeField] private Vector2 planarCenterOffset = new(0f, -0.12f);
        [SerializeField, Min(0f)] private float centerElevation = DefaultHeight * 0.5f;
        [SerializeField] private bool lockElevation = true;

        private Vector2 pendingPlanarDelta;
        private float pendingElevationDelta;

        public Rigidbody Body { get; private set; }
        public CapsuleCollider CollisionVolume { get; private set; }
        public bool IsElevationLocked => lockElevation;
        public float Elevation => Body == null ? transform.position.z : Body.position.z;

        private void Awake()
        {
            EnsureConfigured();
        }

        private void FixedUpdate()
        {
            if (Body == null)
            {
                ClearPendingMovement();
                return;
            }

            float fixedDeltaTime = Mathf.Max(Time.fixedDeltaTime, Mathf.Epsilon);
            Vector3 velocity = Body.linearVelocity;
            velocity.x = pendingPlanarDelta.x / fixedDeltaTime;
            velocity.y = pendingPlanarDelta.y / fixedDeltaTime;
            velocity.z = lockElevation ? 0f : pendingElevationDelta / fixedDeltaTime;
            Body.linearVelocity = velocity;

            ClearPendingMovement();
        }

        private void OnDisable()
        {
            ClearPendingMovement();
            if (Body != null)
            {
                Body.linearVelocity = Vector3.zero;
            }
        }

        public void EnsureConfigured()
        {
            Body = GetComponent<Rigidbody>();
            CollisionVolume = GetComponent<CapsuleCollider>();

            Body.useGravity = false;
            Body.linearDamping = 0f;
            Body.angularDamping = 0f;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            SetElevationLocked(lockElevation);

            collisionRadius = Mathf.Max(0.05f, collisionRadius);
            collisionHeight = Mathf.Max(collisionRadius * 2f, collisionHeight);
            centerElevation = Mathf.Max(collisionRadius, centerElevation);

            CollisionVolume.direction = 2;
            CollisionVolume.radius = collisionRadius;
            CollisionVolume.height = collisionHeight;
            CollisionVolume.center = new Vector3(
                planarCenterOffset.x,
                planarCenterOffset.y,
                centerElevation);
            CollisionVolume.isTrigger = false;
        }

        public void QueuePlanarDelta(Vector2 delta)
        {
            pendingPlanarDelta += delta;
        }

        public void QueueElevationDelta(float delta)
        {
            if (!lockElevation)
            {
                pendingElevationDelta += delta;
            }
        }

        public void SetElevationImmediate(float elevation)
        {
            pendingElevationDelta = 0f;
            Body ??= GetComponent<Rigidbody>();
            StopElevationVelocity();

            Vector3 position = Body.position;
            position.z = elevation;
            Body.position = position;
        }

        public void SetElevationLocked(bool locked)
        {
            lockElevation = locked;
            Body ??= GetComponent<Rigidbody>();

            if (locked)
            {
                pendingElevationDelta = 0f;
                StopElevationVelocity();
            }

            RigidbodyConstraints constraints =
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationZ;
            if (locked)
            {
                constraints |= RigidbodyConstraints.FreezePositionZ;
            }

            Body.constraints = constraints;
        }

        private void StopElevationVelocity()
        {
            Vector3 velocity = Body.linearVelocity;
            velocity.z = 0f;
            Body.linearVelocity = velocity;
        }

        private void ClearPendingMovement()
        {
            pendingPlanarDelta = Vector2.zero;
            pendingElevationDelta = 0f;
        }
    }
}
