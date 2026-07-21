using System.Collections;
using DemonKing.Core.Application;
using DemonKing.Core.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DemonKing.Tests.PlayMode
{
    public sealed class ModalUiCoordinatorPlayModeTests
    {
        [UnityTest]
        public IEnumerator ModalUiCoordinator_同時Openを拒否しCloseで直前状態へ復元する()
        {
            Time.timeScale = 0.6f;
            GameObject root = new("Modal Coordinator Test");
            PlayerInputReader inputReader = root.AddComponent<PlayerInputReader>();
            ModalUiCoordinator coordinator = root.AddComponent<ModalUiCoordinator>();
            coordinator.Initialize(inputReader);
            var firstOwner = new object();
            var secondOwner = new object();

            Assert.That(coordinator.TryOpen(firstOwner, 0f), Is.True);
            Assert.That(coordinator.TryOpen(secondOwner, 0f), Is.False);
            Assert.That(coordinator.IsOwnedBy(firstOwner), Is.True);
            Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.UI));
            Assert.That(Time.timeScale, Is.Zero);

            Assert.That(coordinator.TryClose(firstOwner), Is.True);
            Assert.That(coordinator.HasOpenModal, Is.False);
            Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.Gameplay));
            Assert.That(Time.timeScale, Is.EqualTo(0.6f).Within(0.001f));

            Object.Destroy(root);
            yield return null;
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator ModalUiCoordinator_Disable時にInputContextとTimeScaleを復元する()
        {
            Time.timeScale = 0.75f;
            GameObject root = new("Modal Disable Test");
            PlayerInputReader inputReader = root.AddComponent<PlayerInputReader>();
            ModalUiCoordinator coordinator = root.AddComponent<ModalUiCoordinator>();
            coordinator.Initialize(inputReader);
            var owner = new object();

            Assert.That(coordinator.TryOpen(owner, 0f), Is.True);
            coordinator.enabled = false;

            Assert.That(coordinator.HasOpenModal, Is.False);
            Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.Gameplay));
            Assert.That(Time.timeScale, Is.EqualTo(0.75f).Within(0.001f));

            Object.Destroy(root);
            yield return null;
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator ModalUiCoordinator_Destroy時にInputContextとTimeScaleを復元する()
        {
            Time.timeScale = 0.5f;
            GameObject root = new("Modal Destroy Test");
            PlayerInputReader inputReader = root.AddComponent<PlayerInputReader>();
            ModalUiCoordinator coordinator = root.AddComponent<ModalUiCoordinator>();
            coordinator.Initialize(inputReader);
            var owner = new object();

            Assert.That(coordinator.TryOpen(owner, 0f), Is.True);
            Object.Destroy(coordinator);
            yield return null;

            Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.Gameplay));
            Assert.That(Time.timeScale, Is.EqualTo(0.5f).Within(0.001f));

            Object.Destroy(root);
            yield return null;
            Time.timeScale = 1f;
        }
    }
}
