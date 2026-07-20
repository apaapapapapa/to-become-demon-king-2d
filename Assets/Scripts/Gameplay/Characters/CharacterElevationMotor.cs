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
    /// X/Y平面移動はCharacterPlanarMotorへ委譲し、最終的な3軸移動の合成はCharacterPhysicsBody3Dへ委譲します。
    /// Unity標準重力は使用しません。
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
        private float verticalVelocity;
        private float flightVerticalInput;
        private float lastSupportContactFixedTime = float.NegativeInfinity;
        private bool groundedOnBasePlane;

        public CharacterElevationMode Mode { get; private set; }
        public float Elevation => physicsBody == null ? transform.position.z : physicsBody.Elevation;
        public float VerticalVelocity => verticalVelocity;
        public bool IsGrounded => Mode == CharacterElevationMode.Grounded;
        public bool IsFlying => Mode == CharacterElevationMode.Flying;

        private void Awake()
        {
            physicsBody = GetComponent<CharacterPhysicsBody3D>();
            physicsBody.EnsureConfigured();

            if (Elevation <= groundElevation + groundTolerance)
            {
                SnapToGround();
            }
            else
            {
                groundedOnBasePlane = false;
                Mode = CharacterElevationMode.Airborne;
                physicsBody.SetElevationLocked(false);
            }
        }

        private void FixedUpdate()
        {
            if (physicsBody == null)
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

            groundedOnBasePlane = false;
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
                groundedOnBasePlane = false;
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
                groundedOnBasePlane = false;
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

            if (groundedOnBasePlane)
            {
                physicsBody.SetElevationLocked(true);
                if (Mathf.Abs(Elevation - groundElevation) > groundTolerance)
                {
                    physicsBody.SetElevationImmediate(groundElevation);
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

            float delta = verticalVelocity * Time.fixedDeltaTime;
            if (Elevation + delta <= groundElevation)
            {
                SnapToGround();
                return;
            }

            physicsBody.QueueElevationDelta(delta);
        }

        private void UpdateFlying()
        {
            physicsBody.SetElevationLocked(false);
            verticalVelocity = 0f;

            float targetElevation = Mathf.Clamp(
                Elevation + flightVerticalInput * flightSpeed * Time.fixedDeltaTime,
                groundElevation,
                maxFlightElevation);
            physicsBody.QueueElevationDelta(targetElevation - Elevation);
        }

        private void OnCollisionEnter(Collision collision)
        {
            ResolveSupportContact(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            ResolveSupportContact(collision);
        }

        private void ResolveSupportContact(Collision collision)
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

                groundedOnBasePlane = false;
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
            groundedOnBasePlane = true;
            verticalVelocity = 0f;
            flightVerticalInput = 0f;
            Mode = CharacterElevationMode.Grounded;
            physicsBody.SetElevationLocked(true);
            physicsBody.SetElevationImmediate(groundElevation);
        }
    }
}
