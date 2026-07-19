using System.Collections;
using DemonKing.Gameplay.Combat;
using DemonKing.Presentation.CameraSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DemonKing.Tests.PlayMode
{
    /// <summary>
    /// 実行時ライフサイクルが必要なGameplayとPresentationの最小回帰テストです。
    /// </summary>
    public sealed class GameplayAndCameraPlayModeTests
    {
        [UnityTest]
        public IEnumerator Health_致死ダメージでHPが0になり死亡イベントを1回通知する()
        {
            GameObject target = new("Health Test Target");
            Health health = target.AddComponent<Health>();
            health.ConfigureCombatIdentity("character.test.target", "reward.test.target");
            int diedCount = 0;
            DefeatContext diedContext = null;
            health.Died += context =>
            {
                diedCount++;
                diedContext = context;
            };

            yield return null;

            var request = new DamageRequest(
                99,
                sourceActorId: "character.test.attacker",
                abilityId: "ability.test",
                tags: DamageTags.Skill);
            DamageResult lethalResult = health.ApplyDamage(request);
            DamageResult ignoredResult = health.ApplyDamage(new DamageRequest(1));

            Assert.That(health.CurrentHealth, Is.EqualTo(0));
            Assert.That(health.IsAlive, Is.False);
            Assert.That(diedCount, Is.EqualTo(1));
            Assert.That(lethalResult.AppliedAmount, Is.EqualTo(3));
            Assert.That(lethalResult.Target, Is.SameAs(target));
            Assert.That(lethalResult.WasDefeated, Is.True);
            Assert.That(ignoredResult.WasApplied, Is.False);
            Assert.That(diedContext, Is.Not.Null);
            Assert.That(diedContext.AttackerActorId, Is.EqualTo("character.test.attacker"));
            Assert.That(diedContext.AbilityId, Is.EqualTo("ability.test"));
            Assert.That(diedContext.RewardDefinitionId, Is.EqualTo("reward.test.target"));

            Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator CameraFollow2D_ターゲットへ追従しZ座標を維持する()
        {
            GameObject cameraObject = new("Camera Follow Test");
            cameraObject.transform.position = new Vector3(5f, 5f, -10f);
            CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();

            GameObject target = new("Camera Target");
            target.transform.position = new Vector3(1f, 2f, 0f);

            follow.SetTarget(target.transform, snapImmediately: true);

            Assert.That(cameraObject.transform.position.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(cameraObject.transform.position.y, Is.EqualTo(3.7f).Within(0.001f));
            Assert.That(cameraObject.transform.position.z, Is.EqualTo(-10f).Within(0.001f));

            target.transform.position = new Vector3(3f, 2f, 0f);
            float beforeX = cameraObject.transform.position.x;
            yield return null;

            Assert.That(cameraObject.transform.position.x, Is.GreaterThan(beforeX));
            Assert.That(cameraObject.transform.position.z, Is.EqualTo(-10f).Within(0.001f));

            Object.Destroy(cameraObject);
            Object.Destroy(target);
        }
    }
}
