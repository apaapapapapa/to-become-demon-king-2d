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
            int diedCount = 0;
            health.Died += _ => diedCount++;

            yield return null;

            health.TakeDamage(99, null);
            health.TakeDamage(1, null);

            Assert.That(health.CurrentHealth, Is.EqualTo(0));
            Assert.That(health.IsAlive, Is.False);
            Assert.That(diedCount, Is.EqualTo(1));

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
            Assert.That(cameraObject.transform.position.y, Is.EqualTo(2.35f).Within(0.001f));
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
