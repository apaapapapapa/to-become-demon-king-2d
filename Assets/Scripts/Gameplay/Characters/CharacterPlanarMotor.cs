using DemonKing.Core.Input;
using DemonKing.Core.Math;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// 3D Physics上でX/Yフィールド平面の通常移動だけを担当します。
    /// ZはCharacterElevationMotorが担当し、最終的な3軸移動の合成はCharacterPhysicsBody3Dへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterPhysicsBody3D))]
    public sealed class CharacterPlanarMotor : MonoBehaviour
    {
        private const float DefaultMoveSpeed = 3.4f;

        [SerializeField] private CharacterStatsDefinition statsDefinition;

        private MoveInputReader inputReader;
        private CharacterPhysicsBody3D physicsBody;
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
            physicsBody = GetComponent<CharacterPhysicsBody3D>();
            physicsBody.EnsureConfigured();
        }

        private void Update()
        {
            currentInput = inputReader == null ? Vector2.zero : inputReader.Move;
        }

        private void FixedUpdate()
        {
            if (physicsBody == null || movementLocked)
            {
                return;
            }

            Vector2 delta = currentInput * (MoveSpeed * Time.fixedDeltaTime);
            if (clampToBounds)
            {
                Vector2 current = FieldSpace3D.ToPlanar(physicsBody.Body.position);
                Vector2 target = current + delta;
                target.x = Mathf.Clamp(target.x, -fieldExtents.x, fieldExtents.x);
                target.y = Mathf.Clamp(target.y, -fieldExtents.y, fieldExtents.y);
                delta = target - current;
            }

            physicsBody.QueuePlanarDelta(delta);
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
