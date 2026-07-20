using System.Collections;
using System.Linq;
using DemonKing.Gameplay.Characters;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    public sealed class FieldPhysics3DPlayModeTests
    {
        [UnityTest]
        public IEnumerator CharacterPhysicsBody3D_ConfiguresPure3DPhysicsAndLocksElevation()
        {
            GameObject actor = new("3D Physics Actor");
            CharacterPhysicsBody3D physicsBody = actor.AddComponent<CharacterPhysicsBody3D>();
            yield return null;

            Assert.That(physicsBody.Body, Is.Not.Null);
            Assert.That(physicsBody.CollisionVolume, Is.Not.Null);
            Assert.That(physicsBody.Body.useGravity, Is.False);
            Assert.That(actor.GetComponent<Rigidbody2D>(), Is.Null);
            Assert.That(actor.GetComponents<Collider2D>(), Is.Empty);
            Assert.That(
                (physicsBody.Body.constraints & RigidbodyConstraints.FreezePositionZ) != 0,
                Is.True);

            physicsBody.SetElevationLocked(false);

            Assert.That(physicsBody.IsElevationLocked, Is.False);
            Assert.That(
                (physicsBody.Body.constraints & RigidbodyConstraints.FreezePositionZ) != 0,
                Is.False);

            Object.Destroy(actor);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterElevationMotor_Jumpで上昇して地面へ着地する()
        {
            GameObject actor = new("Jump Actor");
            actor.transform.position = new Vector3(100f, 100f, 0f);
            CharacterElevationMotor elevationMotor = actor.AddComponent<CharacterElevationMotor>();
            yield return null;

            Assert.That(elevationMotor.IsGrounded, Is.True);
            Assert.That(elevationMotor.TryJump(), Is.True);

            for (int frame = 0; frame < 5; frame++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(elevationMotor.Elevation, Is.GreaterThan(0f));
            Assert.That(elevationMotor.Mode, Is.EqualTo(CharacterElevationMode.Airborne));

            for (int frame = 0; frame < 180 && !elevationMotor.IsGrounded; frame++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(elevationMotor.IsGrounded, Is.True);

            // Groundedへの状態遷移とRigidbody poseの更新は同じFixedUpdateで行われるため、
            // 次のPhysics stepまで待って確定後のElevationを検証します。
            yield return new WaitForFixedUpdate();

            Assert.That(elevationMotor.IsGrounded, Is.True);
            Assert.That(elevationMotor.Elevation, Is.EqualTo(0f).Within(0.001f));
            Assert.That(elevationMotor.VerticalVelocity, Is.Zero.Within(0.001f));

            Object.Destroy(actor);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Flight_建物上端を超えた高度で有限高さColliderを横断できる()
        {
            GameObject building = new("Height Aware Building");
            building.transform.position = new Vector3(200f, 0f, 0f);
            BoxCollider buildingCollider = building.AddComponent<BoxCollider>();
            buildingCollider.center = new Vector3(0f, 0f, 2f);
            buildingCollider.size = new Vector3(2f, 2f, 4f);

            GameObject actor = new("Flying Actor");
            actor.transform.position = new Vector3(198f, 0f, 0f);
            CharacterElevationMotor elevationMotor = actor.AddComponent<CharacterElevationMotor>();
            Rigidbody body = actor.GetComponent<Rigidbody>();
            yield return null;

            elevationMotor.SetFlightMode(true);
            elevationMotor.SetFlightVerticalInput(1f);
            for (int frame = 0; frame < 120 && elevationMotor.Elevation < 4.25f; frame++)
            {
                yield return new WaitForFixedUpdate();
            }

            elevationMotor.SetFlightVerticalInput(0f);
            Assert.That(elevationMotor.IsFlying, Is.True);
            Assert.That(elevationMotor.Elevation, Is.GreaterThan(4f));

            Physics.SyncTransforms();
            Collider[] aerialHits = Physics.OverlapSphere(
                new Vector3(200f, 0f, elevationMotor.Elevation + 0.36f),
                0.3f,
                ~0,
                QueryTriggerInteraction.Collide);
            Assert.That(aerialHits.Contains(buildingCollider), Is.False);

            for (int frame = 0; frame < 45; frame++)
            {
                Vector3 next = body.position;
                next.x += 0.1f;
                body.MovePosition(next);
                yield return new WaitForFixedUpdate();
            }

            Assert.That(body.position.x, Is.GreaterThan(201.5f));
            Assert.That(elevationMotor.Elevation, Is.GreaterThan(4f));

            Object.Destroy(actor);
            Object.Destroy(building);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FiniteHeightObstacle_IsNotDetectedAboveItsTop()
        {
            GameObject building = new("Height Aware Building");
            BoxCollider buildingCollider = building.AddComponent<BoxCollider>();
            buildingCollider.center = new Vector3(0f, 0f, 2f);
            buildingCollider.size = new Vector3(2f, 2f, 4f);
            yield return null;

            Physics.SyncTransforms();
            Collider[] groundHits = Physics.OverlapSphere(
                new Vector3(0f, 0f, 0.25f),
                0.25f,
                ~0,
                QueryTriggerInteraction.Collide);
            Collider[] aerialHits = Physics.OverlapSphere(
                new Vector3(0f, 0f, 5.25f),
                0.25f,
                ~0,
                QueryTriggerInteraction.Collide);

            Assert.That(groundHits.Contains(buildingCollider), Is.True);
            Assert.That(aerialHits.Contains(buildingCollider), Is.False);

            Object.Destroy(building);
            yield return null;
        }
    }
}
