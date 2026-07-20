using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// キャラクターの3D Physics本体を構成します。
    /// X/Yは既存のフィールド平面、ZはElevationとして扱い、現行の地上移動ではZを固定します。
    /// Jump / Flight実装時はSetElevationLocked(false)で高さ方向の制御を解放できます。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterPhysicsBody3D : MonoBehaviour
    {
        private const float DefaultRadius = 0.36f;
        private const float DefaultHeight = 0.72f;
        private static readonly Vector2 DefaultPlanarCenterOffset = new(0f, -0.12f);

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
            if (Body == null)
            {
                Body = gameObject.AddComponent<Rigidbody>();
            }

            Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            SetElevationLocked(lockElevation);

            CollisionVolume = GetComponent<CapsuleCollider>();
            if (CollisionVolume == null)
            {
                CollisionVolume = gameObject.AddComponent<CapsuleCollider>();
            }

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

            DisableLegacyPhysics2D();
        }

        public void SetElevationLocked(bool locked)
        {
            lockElevation = locked;
            if (Body == null)
            {
                Body = GetComponent<Rigidbody>();
                if (Body == null)
                {
                    return;
                }
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

        private void DisableLegacyPhysics2D()
        {
            Rigidbody2D legacyBody = GetComponent<Rigidbody2D>();
            if (legacyBody != null)
            {
                legacyBody.simulated = false;
            }

            foreach (Collider2D legacyCollider in GetComponents<Collider2D>())
            {
                legacyCollider.enabled = false;
            }
        }
    }
}
