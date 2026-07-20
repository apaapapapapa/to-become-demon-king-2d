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
        }
    }
}
