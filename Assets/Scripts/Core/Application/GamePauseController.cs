using System;
using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Core.Application
{
    /// <summary>
    /// ゲーム全体のポーズ状態を管理します。
    /// Modalの所有権、Time Scale、Input Contextの退避・復元はModalUiCoordinatorへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GamePauseController : MonoBehaviour
    {
        private PlayerInputReader inputReader;
        private ModalUiCoordinator modalUiCoordinator;
        private float pausedTimeScale;
        private bool subscribed;

        public event Action<bool> PauseStateChanged;

        public bool IsPaused { get; private set; }

        public void Initialize(PlayerInputReader reader, float pauseTimeScale = 0f)
        {
            Initialize(
                reader,
                ModalUiCoordinator.GetOrCreate(reader),
                pauseTimeScale);
        }

        public void Initialize(
            PlayerInputReader reader,
            ModalUiCoordinator coordinator,
            float pauseTimeScale = 0f)
        {
            RestoreActiveState();
            UnsubscribeInput();

            inputReader = reader != null
                ? reader
                : throw new ArgumentNullException(nameof(reader));
            modalUiCoordinator = coordinator != null
                ? coordinator
                : throw new ArgumentNullException(nameof(coordinator));
            modalUiCoordinator.Initialize(inputReader);
            pausedTimeScale = Mathf.Clamp(pauseTimeScale, 0f, 1f);

            if (isActiveAndEnabled)
            {
                SubscribeInput();
            }
        }

        private void OnEnable()
        {
            SubscribeInput();
        }

        private void OnDisable()
        {
            UnsubscribeInput();
            RestoreActiveState();
        }

        public void TogglePause()
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            if (IsPaused || modalUiCoordinator == null)
            {
                return;
            }

            if (!modalUiCoordinator.TryOpen(this, pausedTimeScale))
            {
                return;
            }

            IsPaused = true;
            PauseStateChanged?.Invoke(true);
        }

        public void ResumeGame()
        {
            if (!IsPaused)
            {
                return;
            }

            modalUiCoordinator?.TryClose(this);
            IsPaused = false;
            PauseStateChanged?.Invoke(false);
        }

        private void SubscribeInput()
        {
            if (subscribed || inputReader == null)
            {
                return;
            }

            inputReader.PausePressed += HandlePausePressed;
            inputReader.CancelPressed += HandleCancelPressed;
            subscribed = true;
        }

        private void UnsubscribeInput()
        {
            if (!subscribed || inputReader == null)
            {
                return;
            }

            inputReader.PausePressed -= HandlePausePressed;
            inputReader.CancelPressed -= HandleCancelPressed;
            subscribed = false;
        }

        private void HandlePausePressed()
        {
            TogglePause();
        }

        private void HandleCancelPressed()
        {
            if (IsPaused)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// シーン破棄やコンポーネント無効化時にModal所有権を解放します。
        /// Coordinator側が先に復元済みでもPause状態だけは必ず通常状態へ戻します。
        /// </summary>
        private void RestoreActiveState()
        {
            if (!IsPaused)
            {
                return;
            }

            modalUiCoordinator?.TryClose(this);
            IsPaused = false;
            PauseStateChanged?.Invoke(false);
        }
    }
}
