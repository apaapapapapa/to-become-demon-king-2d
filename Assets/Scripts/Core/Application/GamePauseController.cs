using System;
using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Core.Application
{
    /// <summary>
    /// ゲーム全体のポーズ状態を管理します。
    /// Time.timeScaleとInput Contextの切り替えだけを担当し、具体的なUI表示には依存しません。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GamePauseController : MonoBehaviour
    {
        private PlayerInputReader inputReader;
        private float pausedTimeScale;
        private float resumeTimeScale = 1f;
        private bool subscribed;

        public event Action<bool> PauseStateChanged;

        public bool IsPaused { get; private set; }

        public void Initialize(PlayerInputReader reader, float pauseTimeScale = 0f)
        {
            UnsubscribeInput();
            inputReader = reader;
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
            RestoreTimeScaleWithoutChangingInput();
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
            if (IsPaused)
            {
                return;
            }

            resumeTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
            IsPaused = true;

            inputReader?.EnableUiInput();
            Time.timeScale = pausedTimeScale;
            PauseStateChanged?.Invoke(true);
        }

        public void ResumeGame()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = resumeTimeScale;
            inputReader?.EnableGameplayInput();
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

        private void RestoreTimeScaleWithoutChangingInput()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = resumeTimeScale;
        }
    }
}
