using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// キャラクターの3D Physics本体を構成します。
    /// X/Yは既存のフィールド平面、ZはElevationとして扱い、現行の地上移動ではZを固定します。
    /// Jump / Flight実装時はSetElevationLocked(false)で高さ方向の制御を解放できます。
    /// </summary>
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

        public Rigidbody Body { get; private set; }
        public CapsuleCollider CollisionVolume { get; private set; }
        public bool IsElevationLocked => lockElevation;
        public float Elevation => transform.position.z;

        private void Awake()
        {
            EnsureConfigured();
        }

        public void EnsureConfigured()
        {
            Body = GetComponent<Rigidbody>();
            CollisionVolume = GetComponent<CapsuleCollider>();

            Body.useGravity = false;
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

        public void SetElevationLocked(bool locked)
        {
            lockElevation = locked;
            Body ??= GetComponent<Rigidbody>();

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
    }
}
