using System.Collections;
using DemonKing.Gameplay.AI;
using DemonKing.Gameplay.AI.Configuration;
using DemonKing.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    public sealed class EnemyAiPlayModeTests
    {
        private const string DefinitionResourcePath = "Settings/Gameplay/TrainingSlimeAi";

        [UnityTest]
        public IEnumerator EnemyAi_Targetを索敵すると追跡する()
        {
            EnemyAiDefinition definition = LoadDefinition();
            GameObject target = CreateDamageableTarget("Chase Target", new Vector3(1.3f, 0f, 0f));
            GameObject enemy = CreateEnemy("Chasing Enemy", Vector3.zero, definition, target);
            Rigidbody body = enemy.GetComponent<Rigidbody>();
            EnemyAiController ai = enemy.GetComponent<EnemyAiController>();
            float beforeX = body.position.x;

            for (int frame = 0; frame < 8; frame++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(body.position.x, Is.GreaterThan(beforeX));
            Assert.That(ai.State, Is.Not.EqualTo(EnemyAiState.Idle));

            Object.Destroy(enemy);
            Object.Destroy(target);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyAi_攻撃範囲内ではAbility経由でダメージを与える()
        {
            EnemyAiDefinition definition = LoadDefinition();
            GameObject target = CreateDamageableTarget("Attack Target", new Vector3(0.8f, 0f, 0f));
            Health targetHealth = target.GetComponent<Health>();
            targetHealth.ConfigureMaxHealth(5);
            GameObject enemy = CreateEnemy("Attacking Enemy", Vector3.zero, definition, target);
            EnemyAiController ai = enemy.GetComponent<EnemyAiController>();

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(ai.State, Is.EqualTo(EnemyAiState.Attacking));
            Assert.That(targetHealth.CurrentHealth, Is.EqualTo(4));

            Object.Destroy(enemy);
            Object.Destroy(target);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyAi_Targetとの高度差が大きい場合は追跡も攻撃もしない()
        {
            EnemyAiDefinition definition = LoadDefinition();
            GameObject target = CreateDamageableTarget("Flying Target", new Vector3(0.8f, 0f, 3f));
            Health targetHealth = target.GetComponent<Health>();
            targetHealth.ConfigureMaxHealth(5);
            GameObject enemy = CreateEnemy("Ground Enemy", Vector3.zero, definition, target);
            EnemyAiController ai = enemy.GetComponent<EnemyAiController>();
            Rigidbody body = enemy.GetComponent<Rigidbody>();
            Vector3 before = body.position;

            for (int frame = 0; frame < 5; frame++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(ai.State, Is.EqualTo(EnemyAiState.Idle));
            Assert.That(body.position.x, Is.EqualTo(before.x).Within(0.001f));
            Assert.That(body.position.y, Is.EqualTo(before.y).Within(0.001f));
            Assert.That(targetHealth.CurrentHealth, Is.EqualTo(5));

            Object.Destroy(enemy);
            Object.Destroy(target);
            yield return null;
        }

        private static EnemyAiDefinition LoadDefinition()
        {
            EnemyAiDefinition definition = Resources.Load<EnemyAiDefinition>(DefinitionResourcePath);
            Assert.That(definition, Is.Not.Null);
            Assert.That(definition.IsConfigured, Is.True);
            return definition;
        }

        private static GameObject CreateEnemy(
            string name,
            Vector3 position,
            EnemyAiDefinition definition,
            GameObject target)
        {
            GameObject enemy = new(name);
            enemy.transform.position = position;
            Health health = enemy.AddComponent<Health>();
            health.ConfigureCombatIdentity("character.test.enemy");
            EnemyAiController ai = enemy.AddComponent<EnemyAiController>();
            ai.Configure(definition, target);
            return enemy;
        }

        private static GameObject CreateDamageableTarget(string name, Vector3 position)
        {
            GameObject target = new(name);
            target.transform.position = position;
            SphereCollider collider = target.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.4f;
            Health health = target.AddComponent<Health>();
            health.ConfigureCombatIdentity("character.test.target");
            return target;
        }
    }
}
