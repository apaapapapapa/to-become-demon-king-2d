using DemonKing.Core.Input;
using DemonKing.Core.Math;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// 3D Physics上でX/Yフィールド平面の通常移動だけを担当します。
    /// ZはElevationとしてCharacterPhysicsBody3Dが管理し、通常移動では変更しません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterPhysicsBody3D))]
    public sealed class CharacterPlanarMotor : MonoBehaviour
    {
        private const float DefaultMoveSpeed = 3.4f;

        [SerializeField] private CharacterStatsDefinition statsDefinition;

        private MoveInputReader inputReader;
        private Rigidbody body;
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
            CharacterPhysicsBody3D physicsBody = GetComponent<CharacterPhysicsBody3D>();
            physicsBody.EnsureConfigured();
            body = physicsBody.Body;
        }

        private void Update()
        {
            currentInput = inputReader == null ? Vector2.zero : inputReader.Move;
        }

        private void FixedUpdate()
        {
            if (body == null || movementLocked)
            {
                return;
            }

            Vector3 next = body.position +
                           FieldSpace3D.PlanarDelta(currentInput) *
                           (MoveSpeed * Time.fixedDeltaTime);
            next.z = body.position.z;

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
