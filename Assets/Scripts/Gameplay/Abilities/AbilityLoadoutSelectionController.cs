using System;
using System.Collections.Generic;
using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// Ability Loadout画面の開閉、候補選択、Action Slot選択とRuntime割当を管理します。
    /// Modal所有権とInput Context / Time Scale切替はModalUiCoordinatorへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AbilityLoadoutSelectionController : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float initialNavigationDelay = 0.32f;
        [SerializeField, Min(0.05f)] private float navigationRepeatInterval = 0.12f;

        private readonly List<AbilityLoadoutMenuEntry> entries = new();
        private PlayerInputReader inputReader;
        private ModalUiCoordinator modalUiCoordinator;
        private AbilityLoadoutController loadoutController;
        private CharacterDefinition characterDefinition;
        private CharacterProgressionState progressionState;
        private float nextNavigationTime;
        private int navigationCommand;
        private bool subscribed;

        public event Action StateChanged;

        public bool IsInitialized =>
            inputReader != null &&
            modalUiCoordinator != null &&
            loadoutController != null &&
            characterDefinition != null &&
            progressionState != null;

        public bool IsOpen { get; private set; }
        public int SelectedEntryIndex { get; private set; } = -1;
        public int SelectedSlotIndex { get; private set; }
        public IReadOnlyList<AbilityLoadoutMenuEntry> Entries => entries;
        public IReadOnlyList<AbilitySlot> Slots => AbilityLoadoutPolicy.EditableSlots;
        public string LastActionMessage { get; private set; } = string.Empty;

        public AbilityLoadoutMenuEntry? SelectedEntry =>
            SelectedEntryIndex >= 0 && SelectedEntryIndex < entries.Count
                ? entries[SelectedEntryIndex]
                : null;

        public AbilitySlot SelectedSlot => AbilityLoadoutPolicy.GetEditableSlot(SelectedSlotIndex);

        public void Initialize(
            PlayerInputReader reader,
            AbilityLoadoutController controller,
            CharacterDefinition definition,
            CharacterProgressionState state)
        {
            Initialize(
                reader,
                ModalUiCoordinator.GetOrCreate(reader),
                controller,
                definition,
                state);
        }

        public void Initialize(
            PlayerInputReader reader,
            ModalUiCoordinator coordinator,
            AbilityLoadoutController controller,
            CharacterDefinition definition,
            CharacterProgressionState state)
        {
            RestoreGameplayState();
            UnsubscribeInput();
            inputReader = reader != null
                ? reader
                : throw new ArgumentNullException(nameof(reader));
            modalUiCoordinator = coordinator != null
                ? coordinator
                : throw new ArgumentNullException(nameof(coordinator));
            modalUiCoordinator.Initialize(inputReader);
            loadoutController = controller != null && controller.IsInitialized
                ? controller
                : throw new ArgumentException(
                    "初期化済みのAbilityLoadoutControllerが必要です。",
                    nameof(controller));
            characterDefinition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
            progressionState = state ?? throw new ArgumentNullException(nameof(state));

            RefreshEntries();
            SubscribeInput();
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

            Vector2 navigate = inputReader.Navigate;
            int command = ResolveNavigationCommand(navigate);
            if (command == 0)
            {
                navigationCommand = 0;
                return;
            }

            float now = Time.unscaledTime;
            if (command != navigationCommand)
            {
                navigationCommand = command;
                ApplyNavigationCommand(command);
                nextNavigationTime = now + initialNavigationDelay;
                return;
            }

            if (now >= nextNavigationTime)
            {
                ApplyNavigationCommand(command);
                nextNavigationTime = now + navigationRepeatInterval;
            }
        }

        public bool OpenMenu()
        {
            EnsureInitialized();
            if (IsOpen)
            {
                return false;
            }

            RefreshEntries();
            if (!modalUiCoordinator.TryOpen(this, 0f))
            {
                return false;
            }

            IsOpen = true;
            navigationCommand = 0;
            LastActionMessage = string.Empty;
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

        public bool MoveEntrySelection(int direction)
        {
            if (!IsOpen || entries.Count <= 1 || direction == 0)
            {
                return false;
            }

            int nextIndex =
                (SelectedEntryIndex + Math.Sign(direction) + entries.Count) % entries.Count;
            if (nextIndex == SelectedEntryIndex)
            {
                return false;
            }

            SelectedEntryIndex = nextIndex;
            LastActionMessage = string.Empty;
            StateChanged?.Invoke();
            return true;
        }

        public bool MoveSlotSelection(int direction)
        {
            int slotCount = AbilityLoadoutPolicy.EditableSlots.Count;
            if (!IsOpen || slotCount == 0 || direction == 0)
            {
                return false;
            }

            int nextIndex =
                (SelectedSlotIndex + Math.Sign(direction) + slotCount) % slotCount;
            if (nextIndex == SelectedSlotIndex)
            {
                return false;
            }

            SelectedSlotIndex = nextIndex;
            LastActionMessage = string.Empty;
            StateChanged?.Invoke();
            return true;
        }

        public bool AssignSelected()
        {
            if (!IsOpen)
            {
                return false;
            }

            AbilityLoadoutMenuEntry? selected = SelectedEntry;
            if (!selected.HasValue)
            {
                LastActionMessage = "割り当て可能なArt Abilityをまだ取得していません。";
                StateChanged?.Invoke();
                return false;
            }

            AbilityLoadoutMenuEntry entry = selected.Value;
            if (!entry.CanAssign)
            {
                LastActionMessage = "受動Skillは取得中ずっと有効なため、入力割当は不要です。";
                StateChanged?.Invoke();
                return false;
            }

            if (!AbilityLoadoutEligibility.CanAssign(
                    characterDefinition,
                    progressionState,
                    entry.AbilityId))
            {
                LastActionMessage = "現在の進行状態では、このAbilityを割り当てられません。";
                StateChanged?.Invoke();
                return false;
            }

            bool changed = loadoutController.Assign(SelectedSlot, entry.AbilityId);
            LastActionMessage = changed
                ? $"{entry.DisplayName} を {FormatSlot(SelectedSlot)} に割り当てました。"
                : $"{entry.DisplayName} はすでに {FormatSlot(SelectedSlot)} に割り当て済みです。";
            StateChanged?.Invoke();
            return changed;
        }

        public string GetAssignedAbilityDisplayName(AbilitySlot slot)
        {
            if (loadoutController == null ||
                !loadoutController.TryResolve(slot, out string abilityId))
            {
                return "未設定";
            }

            string displayName = AbilityLoadoutMenuProjection.ResolveAbilityDisplayName(
                characterDefinition,
                abilityId);
            return string.IsNullOrWhiteSpace(displayName) ? abilityId : displayName;
        }

        public void RefreshEntries()
        {
            if (characterDefinition == null || progressionState == null)
            {
                return;
            }

            AbilityLoadoutMenuEntry? previousSelection = SelectedEntry;
            entries.Clear();
            entries.AddRange(AbilityLoadoutMenuProjection.Build(
                characterDefinition,
                progressionState));

            SelectedEntryIndex = entries.Count == 0 ? -1 : 0;
            if (!previousSelection.HasValue)
            {
                return;
            }

            AbilityLoadoutMenuEntry previous = previousSelection.Value;
            for (int index = 0; index < entries.Count; index++)
            {
                AbilityLoadoutMenuEntry candidate = entries[index];
                if (candidate.Kind == previous.Kind &&
                    string.Equals(
                        candidate.SourceContentId,
                        previous.SourceContentId,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        candidate.AbilityId,
                        previous.AbilityId,
                        StringComparison.Ordinal))
                {
                    SelectedEntryIndex = index;
                    break;
                }
            }
        }

        private void SubscribeInput()
        {
            if (subscribed || inputReader == null || !isActiveAndEnabled)
            {
                return;
            }

            inputReader.LoadoutPressed += HandleLoadoutPressed;
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

            inputReader.LoadoutPressed -= HandleLoadoutPressed;
            inputReader.SubmitPressed -= HandleSubmitPressed;
            inputReader.CancelPressed -= HandleCancelPressed;
            subscribed = false;
        }

        private void HandleLoadoutPressed()
        {
            OpenMenu();
        }

        private void HandleSubmitPressed()
        {
            if (IsOpen)
            {
                AssignSelected();
            }
        }

        private void HandleCancelPressed()
        {
            if (IsOpen)
            {
                CloseMenu();
            }
        }

        private static int ResolveNavigationCommand(Vector2 navigate)
        {
            float horizontal = navigate.x;
            float vertical = navigate.y;
            if (Mathf.Abs(vertical) > 0.5f && Mathf.Abs(vertical) >= Mathf.Abs(horizontal))
            {
                return vertical > 0f ? -1 : 1;
            }

            if (Mathf.Abs(horizontal) > 0.5f)
            {
                return horizontal > 0f ? 2 : -2;
            }

            return 0;
        }

        private void ApplyNavigationCommand(int command)
        {
            if (Mathf.Abs(command) == 1)
            {
                MoveEntrySelection(command);
            }
            else if (Mathf.Abs(command) == 2)
            {
                MoveSlotSelection(command > 0 ? 1 : -1);
            }
        }

        private void RestoreGameplayState()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            navigationCommand = 0;
            modalUiCoordinator?.TryClose(this);
        }

        private static string FormatSlot(AbilitySlot slot)
        {
            return slot switch
            {
                AbilitySlot.Action1 => "Action1",
                AbilitySlot.Action2 => "Action2",
                AbilitySlot.Action3 => "Action3",
                AbilitySlot.Action4 => "Action4",
                _ => slot.ToString()
            };
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Ability Loadout選択が初期化されていません。");
            }
        }
    }
}
