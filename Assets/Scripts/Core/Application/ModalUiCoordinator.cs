using System;
using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Core.Application
{
    /// <summary>
    /// モーダルUIの所有権と、Input Context / Time Scaleの退避・復元を一元管理します。
    /// 同時に所有できるModalは1つだけです。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ModalUiCoordinator : MonoBehaviour
    {
        private PlayerInputReader inputReader;
        private object currentOwner;
        private PlayerInputContext previousInputContext = PlayerInputContext.Gameplay;
        private float previousTimeScale = 1f;
        private bool initialized;

        public bool IsInitialized => initialized;
        public bool HasOpenModal => currentOwner != null;

        public void Initialize(PlayerInputReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (HasOpenModal && inputReader != reader)
            {
                throw new InvalidOperationException(
                    "Modalを所有している間はPlayerInputReaderを差し替えられません。");
            }

            inputReader = reader;
            initialized = true;
        }

        /// <summary>
        /// 既存の呼び出し側を段階的に共通Coordinatorへ移行するため、
        /// Playerと同じGameObject上のCoordinatorを取得または生成します。
        /// Composition Rootでは明示的に共有Coordinatorを注入してください。
        /// </summary>
        public static ModalUiCoordinator GetOrCreate(PlayerInputReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            ModalUiCoordinator coordinator = reader.GetComponent<ModalUiCoordinator>();
            if (coordinator == null)
            {
                coordinator = reader.gameObject.AddComponent<ModalUiCoordinator>();
            }

            coordinator.Initialize(reader);
            return coordinator;
        }

        public bool TryOpen(object owner, float modalTimeScale = 0f)
        {
            EnsureInitialized();
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (currentOwner != null || inputReader.CurrentContext != PlayerInputContext.Gameplay)
            {
                return false;
            }

            previousInputContext = inputReader.CurrentContext;
            previousTimeScale = Time.timeScale;
            currentOwner = owner;

            inputReader.EnableUiInput();
            Time.timeScale = Mathf.Clamp(modalTimeScale, 0f, 1f);
            return true;
        }

        public bool TryClose(object owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (!ReferenceEquals(currentOwner, owner))
            {
                return false;
            }

            RestorePreviousState();
            return true;
        }

        public bool IsOwnedBy(object owner)
        {
            return owner != null && ReferenceEquals(currentOwner, owner);
        }

        private void OnDisable()
        {
            RestorePreviousState();
        }

        private void OnDestroy()
        {
            RestorePreviousState();
        }

        private void RestorePreviousState()
        {
            if (currentOwner == null)
            {
                return;
            }

            currentOwner = null;
            Time.timeScale = previousTimeScale;
            if (inputReader != null)
            {
                inputReader.SetContext(previousInputContext);
            }
        }

        private void EnsureInitialized()
        {
            if (!initialized || inputReader == null)
            {
                throw new InvalidOperationException("ModalUiCoordinatorが初期化されていません。");
            }
        }
    }
}
