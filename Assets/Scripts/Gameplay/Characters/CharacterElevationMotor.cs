using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    public enum CharacterElevationMode
    {
        Grounded,
        Airborne,
        Flying
    }

    /// <summary>
    /// フィールドのElevation軸（Z）だけを制御し、Jump / Fall / Flightを管理します。
    /// X/Y平面移動はCharacterPlanarMotorへ委譲し、Unity標準重力は使用しません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterPhysicsBody3D))]
    public sealed class CharacterElevationMotor : MonoBehaviour
    {
        private const float SupportNormalThreshold = 0.5f;

        [SerializeField, Min(0.1f)] private float jumpSpeed = 5f;
        [SerializeField, Min(0.1f)] private float fallAcceleration = 12f;
        [SerializeField, Min(0.1f)] private float flightSpeed = 3f;
        [SerializeField, Min(0.1f)] private float maxFlightElevation = 8f;
        [SerializeField] private float groundElevation;
        [SerializeField, Min(0.001f)] private float groundTolerance = 0.02f;

        private CharacterPhysicsBody3D physicsBody;
        private Rigidbody body;
        private float verticalVelocity;
        private float flightVerticalInput;
        private float lastSupportContactFixedTime = float.NegativeInfinity;

        public CharacterElevationMode Mode { get; private set; }
        public float Elevation => body == null ? transform.position.z : body.position.z;
        public float VerticalVelocity => verticalVelocity;
        public bool IsGrounded => Mode == CharacterElevationMode.Grounded;
        public bool IsFlying => Mode == CharacterElevationMode.Flying;

        private void Awake()
        {
            physicsBody = GetComponent<CharacterPhysicsBody3D>();
            physicsBody.EnsureConfigured();
            body = physicsBody.Body;

            if (body.position.z <= groundElevation + groundTolerance)
            {
                SnapToGround();
            }
            else
            {
                Mode = CharacterElevationMode.Airborne;
                physicsBody.SetElevationLocked(false);
            }
        }

        private void FixedUpdate()
        {
            if (body == null)
            {
                return;
            }

            switch (Mode)
            {
                case CharacterElevationMode.Grounded:
                    UpdateGrounded();
                    break;
                case CharacterElevationMode.Airborne:
                    UpdateAirborne();
                    break;
                case CharacterElevationMode.Flying:
                    UpdateFlying();
                    break;
            }
        }

        public bool TryJump()
        {
            if (Mode != CharacterElevationMode.Grounded)
            {
                return false;
            }

            Mode = CharacterElevationMode.Airborne;
            verticalVelocity = jumpSpeed;
            lastSupportContactFixedTime = float.NegativeInfinity;
            physicsBody.SetElevationLocked(false);
            return true;
        }

        public void ToggleFlight()
        {
            SetFlightMode(Mode != CharacterElevationMode.Flying);
        }

        public void SetFlightMode(bool enabled)
        {
            if (enabled)
            {
                Mode = CharacterElevationMode.Flying;
                verticalVelocity = 0f;
                physicsBody.SetElevationLocked(false);
                return;
            }

            flightVerticalInput = 0f;
            verticalVelocity = 0f;
            if (Elevation <= groundElevation + groundTolerance)
            {
                SnapToGround();
            }
            else
            {
                Mode = CharacterElevationMode.Airborne;
                physicsBody.SetElevationLocked(false);
            }
        }

        public void SetFlightVerticalInput(float input)
        {
            flightVerticalInput = Mathf.Clamp(input, -1f, 1f);
        }

        private void UpdateGrounded()
        {
            verticalVelocity = 0f;

            if (Elevation <= groundElevation + groundTolerance)
            {
                if (!physicsBody.IsElevationLocked)
                {
                    SnapToGround();
                }

                return;
            }

            if (HasRecentSupportContact())
            {
                return;
            }

            Mode = CharacterElevationMode.Airborne;
            physicsBody.SetElevationLocked(false);
        }

        private void UpdateAirborne()
        {
            physicsBody.SetElevationLocked(false);
            verticalVelocity -= fallAcceleration * Time.fixedDeltaTime;

            Vector3 next = body.position;
            next.z += verticalVelocity * Time.fixedDeltaTime;
            if (next.z <= groundElevation)
            {
                SnapToGround();
                return;
            }

            body.MovePosition(next);
        }

        private void UpdateFlying()
        {
            physicsBody.SetElevationLocked(false);
            verticalVelocity = 0f;

            Vector3 next = body.position;
            next.z = Mathf.Clamp(
                next.z + flightVerticalInput * flightSpeed * Time.fixedDeltaTime,
                groundElevation,
                maxFlightElevation);
            body.MovePosition(next);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (Mode == CharacterElevationMode.Flying || verticalVelocity > 0f)
            {
                return;
            }

            for (int index = 0; index < collision.contactCount; index++)
            {
                ContactPoint contact = collision.GetContact(index);
                if (contact.normal.z < SupportNormalThreshold)
                {
                    continue;
                }

                lastSupportContactFixedTime = Time.fixedTime;
                verticalVelocity = 0f;
                Mode = CharacterElevationMode.Grounded;
                physicsBody.SetElevationLocked(true);
                return;
            }
        }

        private bool HasRecentSupportContact()
        {
            return Time.fixedTime - lastSupportContactFixedTime <= Time.fixedDeltaTime * 1.5f;
        }

        private void SnapToGround()
        {
            verticalVelocity = 0f;
            flightVerticalInput = 0f;
            Mode = CharacterElevationMode.Grounded;

            // Rigidbody interpolation中の直前位置をFreezePositionZが保持しないよう、
            // 先にElevationを固定してからGround Elevationへ明示的にテレポートします。
            physicsBody.SetElevationLocked(true);
            Vector3 position = body.position;
            position.z = groundElevation;
            body.position = position;
        }
    }
}
