using System;
using System.Collections.Generic;
using DemonKing.Core.Input;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Progression
{
    public readonly struct EvolutionSelectionEntry
    {
        internal EvolutionSelectionEntry(
            EvolutionDefinition definition,
            EvolutionEvaluationResult evaluation)
        {
            Definition = definition;
            Evaluation = evaluation;
        }

        public EvolutionDefinition Definition { get; }
        public EvolutionEvaluationResult Evaluation { get; }
    }

    /// <summary>
    /// Evolution選択画面の開閉、選択位置、確定要求を管理します。
    /// 条件判定はEvolutionProgressionControllerへ委譲し、具体的なuGUI表示を知りません。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EvolutionSelectionController : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float initialNavigationDelay = 0.32f;
        [SerializeField, Min(0.05f)] private float navigationRepeatInterval = 0.12f;

        private readonly List<EvolutionSelectionEntry> entries = new();
        private PlayerInputReader inputReader;
        private EvolutionProgressionController progressionController;
        private PlayerInputContext previousInputContext;
        private float resumeTimeScale = 1f;
        private float nextNavigationTime;
        private int navigationDirection;
        private bool subscribed;

        public event Action StateChanged;

        public bool IsInitialized => inputReader != null && progressionController != null;
        public bool IsOpen { get; private set; }
        public int SelectedIndex { get; private set; }
        public IReadOnlyList<EvolutionSelectionEntry> Entries => entries;
        public EvolutionApplyResult? LastApplyResult { get; private set; }

        public EvolutionSelectionEntry? SelectedEntry =>
            SelectedIndex >= 0 && SelectedIndex < entries.Count
                ? entries[SelectedIndex]
                : null;

        public void Initialize(
            PlayerInputReader reader,
            EvolutionProgressionController controller)
        {
            UnsubscribeInput();
            inputReader = reader != null
                ? reader
                : throw new ArgumentNullException(nameof(reader));
            progressionController = controller != null && controller.IsInitialized
                ? controller
                : throw new ArgumentException(
                    "初期化済みのEvolutionProgressionControllerが必要です。",
                    nameof(controller));
            SubscribeInput();
            RefreshEntries();
        }

        private void OnEnable()
        {
            SubscribeInput();
        }

        private void OnDisable()
        {
            UnsubscribeInput();
            RestoreGameplayState();
        }

        private void Update()
        {
            if (!IsOpen || inputReader == null)
            {
                return;
            }

            float vertical = inputReader.Navigate.y;
            int direction = vertical > 0.5f ? -1 : vertical < -0.5f ? 1 : 0;
            if (direction == 0)
            {
                navigationDirection = 0;
                return;
            }

            float now = Time.unscaledTime;
            if (direction != navigationDirection)
            {
                navigationDirection = direction;
                MoveSelection(direction);
                nextNavigationTime = now + initialNavigationDelay;
                return;
            }

            if (now >= nextNavigationTime)
            {
                MoveSelection(direction);
                nextNavigationTime = now + navigationRepeatInterval;
            }
        }

        public bool OpenMenu()
        {
            EnsureInitialized();
            if (IsOpen || entries.Count == 0 ||
                inputReader.CurrentContext != PlayerInputContext.Gameplay)
            {
                return false;
            }

            RefreshEntries();
            previousInputContext = inputReader.CurrentContext;
            resumeTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            IsOpen = true;
            LastApplyResult = null;
            navigationDirection = 0;
            Time.timeScale = 0f;
            inputReader.EnableUiInput();
            StateChanged?.Invoke();
            return true;
        }

        public bool CloseMenu()
        {
            if (!IsOpen)
            {
                return false;
            }

            RestoreGameplayState();
            StateChanged?.Invoke();
            return true;
        }

        public bool MoveSelection(int direction)
        {
            if (!IsOpen || entries.Count <= 1 || direction == 0)
            {
                return false;
            }

            int nextIndex = (SelectedIndex + Math.Sign(direction) + entries.Count) % entries.Count;
            if (nextIndex == SelectedIndex)
            {
                return false;
            }

            SelectedIndex = nextIndex;
            LastApplyResult = null;
            StateChanged?.Invoke();
            return true;
        }

        public bool ConfirmSelection()
        {
            if (!IsOpen || SelectedIndex < 0 || SelectedIndex >= entries.Count)
            {
                return false;
            }

            EvolutionSelectionEntry selected = entries[SelectedIndex];
            LastApplyResult = progressionController.Evolve(
                selected.Definition.EvolutionNodeId);
            RefreshEntries();

            if (LastApplyResult.Value.Succeeded)
            {
                RestoreGameplayState();
            }

            StateChanged?.Invoke();
            return LastApplyResult.Value.Succeeded;
        }

        public void RefreshEntries()
        {
            if (progressionController == null || !progressionController.IsInitialized)
            {
                return;
            }

            string selectedNodeId = SelectedEntry?.Definition.EvolutionNodeId;
            entries.Clear();
            foreach (EvolutionDefinition definition in progressionController.Definitions)
            {
                entries.Add(new EvolutionSelectionEntry(
                    definition,
                    progressionController.Evaluate(definition.EvolutionNodeId)));
            }

            SelectedIndex = 0;
            if (!string.IsNullOrEmpty(selectedNodeId))
            {
                for (int index = 0; index < entries.Count; index++)
                {
                    if (string.Equals(
                            entries[index].Definition.EvolutionNodeId,
                            selectedNodeId,
                            StringComparison.Ordinal))
                    {
                        SelectedIndex = index;
                        break;
                    }
                }
            }
        }

        private void SubscribeInput()
        {
            if (subscribed || inputReader == null || !isActiveAndEnabled)
            {
                return;
            }

            inputReader.EvolutionPressed += HandleEvolutionPressed;
            inputReader.SubmitPressed += HandleSubmitPressed;
            inputReader.CancelPressed += HandleCancelPressed;
            subscribed = true;
        }

        private void UnsubscribeInput()
        {
            if (!subscribed || inputReader == null)
            {
                return;
            }

            inputReader.EvolutionPressed -= HandleEvolutionPressed;
            inputReader.SubmitPressed -= HandleSubmitPressed;
            inputReader.CancelPressed -= HandleCancelPressed;
            subscribed = false;
        }

        private void HandleEvolutionPressed()
        {
            OpenMenu();
        }

        private void HandleSubmitPressed()
        {
            if (IsOpen)
            {
                ConfirmSelection();
            }
        }

        private void HandleCancelPressed()
        {
            if (IsOpen)
            {
                CloseMenu();
            }
        }

        private void RestoreGameplayState()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            navigationDirection = 0;
            Time.timeScale = resumeTimeScale;
            if (inputReader != null)
            {
                inputReader.SetContext(previousInputContext);
            }
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Evolution選択が初期化されていません。");
            }
        }
    }
}
