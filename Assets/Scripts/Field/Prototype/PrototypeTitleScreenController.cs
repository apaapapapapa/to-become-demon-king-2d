using System;
using System.Collections.Generic;
using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Field.Prototype
{
    internal enum PrototypeTitlePage
    {
        Main = 0,
        NewGameSlots = 1,
        LoadGameSlots = 2
    }

    internal readonly struct PrototypeGameStartRequest
    {
        public PrototypeGameStartRequest(SaveSlotId slotId, bool isNewGame)
        {
            SlotId = slotId;
            IsNewGame = isNewGame;
        }

        public SaveSlotId SlotId { get; }
        public bool IsNewGame { get; }
    }

    /// <summary>
    /// Title Screenの入力、ページ遷移、Save Slot選択、Game Session開始を調停します。
    /// Save内容の復元やGameplay構築は既存PrototypeApplicationInstaller / PrototypeGameSessionへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class PrototypeTitleScreenController : MonoBehaviour
    {
        private const int MainMenuItemCount = 5;
        private const int NewGameIndex = 0;
        private const int ContinueIndex = 1;
        private const int LoadGameIndex = 2;
        private const int SettingsIndex = 3;
        private const int QuitIndex = 4;

        [SerializeField, Min(0.05f)] private float initialNavigationDelay = 0.32f;
        [SerializeField, Min(0.05f)] private float navigationRepeatInterval = 0.12f;

        private readonly List<SaveSlotMetadata> slotMetadata = new(LocalSaveSlotStore.SlotCount);

        private PrototypeProjectAssets projectAssets;
        private LocalSaveSlotStore slotStore;
        private PlayerInputReader inputReader;
        private PrototypeTitleScreenView view;
        private Action<PrototypeGameStartRequest> startGameHandler;
        private Action quitHandler;
        private float nextNavigationTime;
        private int navigationDirection;
        private bool subscribed;
        private bool initialized;
        private bool started;
        private bool messageIsError;
        private string message = string.Empty;

        public PrototypeTitlePage CurrentPage { get; private set; } = PrototypeTitlePage.Main;
        public int MainSelectionIndex { get; private set; }
        public int SlotSelectionIndex { get; private set; }
        public IReadOnlyList<SaveSlotMetadata> SlotMetadata => slotMetadata;
        public bool ContinueAvailable => FindLatestReadySlotIndex() >= 0;

        public void Initialize(
            PrototypeProjectAssets assets,
            LocalSaveSlotStore saveSlotStore,
            PlayerInputReader reader,
            PrototypeTitleScreenView titleView,
            Action<PrototypeGameStartRequest> onStartGame = null,
            Action onQuit = null)
        {
            projectAssets = assets != null
                ? assets
                : throw new ArgumentNullException(nameof(assets));
            slotStore = saveSlotStore ?? throw new ArgumentNullException(nameof(saveSlotStore));
            inputReader = reader != null
                ? reader
                : throw new ArgumentNullException(nameof(reader));
            view = titleView != null
                ? titleView
                : throw new ArgumentNullException(nameof(titleView));
            startGameHandler = onStartGame ?? StartGame;
            quitHandler = onQuit ?? UnityEngine.Application.Quit;

            inputReader.EnableUiInput();
            RefreshSlotMetadata();
            SubscribeInput();
            initialized = true;
            Render();
        }

        private void OnEnable()
        {
            SubscribeInput();
        }

        private void OnDisable()
        {
            UnsubscribeInput();
        }

        private void Update()
        {
            if (!initialized || started || inputReader == null)
            {
                return;
            }

            int direction = ResolveNavigationDirection(inputReader.Navigate);
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

        public bool MoveSelection(int direction)
        {
            if (!initialized || started || direction == 0)
            {
                return false;
            }

            if (CurrentPage == PrototypeTitlePage.Main)
            {
                MainSelectionIndex = WrapIndex(
                    MainSelectionIndex + Math.Sign(direction),
                    MainMenuItemCount);
            }
            else
            {
                SlotSelectionIndex = WrapIndex(
                    SlotSelectionIndex + Math.Sign(direction),
                    LocalSaveSlotStore.SlotCount);
            }

            ClearMessage();
            Render();
            return true;
        }

        public void SubmitSelection()
        {
            if (!initialized || started)
            {
                return;
            }

            if (CurrentPage == PrototypeTitlePage.Main)
            {
                SubmitMainSelection();
                return;
            }

            SubmitSlotSelection();
        }

        public bool Cancel()
        {
            if (!initialized || started || CurrentPage == PrototypeTitlePage.Main)
            {
                return false;
            }

            CurrentPage = PrototypeTitlePage.Main;
            navigationDirection = 0;
            ClearMessage();
            RefreshSlotMetadata();
            Render();
            return true;
        }

        private void SubmitMainSelection()
        {
            switch (MainSelectionIndex)
            {
                case NewGameIndex:
                    OpenNewGameSlots();
                    break;
                case ContinueIndex:
                    ContinueGame();
                    break;
                case LoadGameIndex:
                    OpenLoadGameSlots();
                    break;
                case SettingsIndex:
                    SetMessage("SettingsはP0では表示導線のみです。設定項目は後続タスクで実装します。", false);
                    Render();
                    break;
                case QuitIndex:
                    quitHandler?.Invoke();
                    break;
            }
        }

        private void OpenNewGameSlots()
        {
            RefreshSlotMetadata();
            CurrentPage = PrototypeTitlePage.NewGameSlots;
            int emptyIndex = FindFirstSlotIndex(SaveSlotStatus.Empty);
            SlotSelectionIndex = emptyIndex >= 0 ? emptyIndex : 0;
            if (emptyIndex < 0)
            {
                SetMessage("空きSave Slotがありません。既存Slotの上書きはP0では行いません。", true);
            }
            else
            {
                ClearMessage();
            }

            Render();
        }

        private void ContinueGame()
        {
            RefreshSlotMetadata();
            int latestReadyIndex = FindLatestReadySlotIndex();
            if (latestReadyIndex < 0)
            {
                SetMessage("Continue可能なSaveがありません。", true);
                Render();
                return;
            }

            StartSelectedGame(latestReadyIndex, isNewGame: false);
        }

        private void OpenLoadGameSlots()
        {
            RefreshSlotMetadata();
            CurrentPage = PrototypeTitlePage.LoadGameSlots;
            int latestReadyIndex = FindLatestReadySlotIndex();
            SlotSelectionIndex = latestReadyIndex >= 0 ? latestReadyIndex : 0;
            ClearMessage();
            Render();
        }

        private void SubmitSlotSelection()
        {
            RefreshSlotMetadata();
            if (SlotSelectionIndex < 0 || SlotSelectionIndex >= slotMetadata.Count)
            {
                SetMessage("Save Slot選択が不正です。", true);
                Render();
                return;
            }

            SaveSlotMetadata selected = slotMetadata[SlotSelectionIndex];
            if (CurrentPage == PrototypeTitlePage.NewGameSlots)
            {
                if (selected.Status != SaveSlotStatus.Empty)
                {
                    SetMessage("New Gameは空きSlotを選択してください。既存Saveは上書きしません。", true);
                    Render();
                    return;
                }

                StartSelectedGame(SlotSelectionIndex, isNewGame: true);
                return;
            }

            if (selected.Status != SaveSlotStatus.Ready)
            {
                SetMessage(FormatLoadError(selected), true);
                Render();
                return;
            }

            StartSelectedGame(SlotSelectionIndex, isNewGame: false);
        }

        private void StartSelectedGame(int slotIndex, bool isNewGame)
        {
            if (slotIndex < 0 || slotIndex >= slotMetadata.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }

            SaveSlotMetadata selected = slotMetadata[slotIndex];
            started = true;
            inputReader.DisableInput();
            UnsubscribeInput();
            startGameHandler?.Invoke(new PrototypeGameStartRequest(selected.SlotId, isNewGame));
        }

        private void StartGame(PrototypeGameStartRequest request)
        {
            ISaveService saveService = slotStore.CreateSaveService(request.SlotId);
            if (request.IsNewGame)
            {
                saveService = new FreshGameSaveService(saveService);
            }

            gameObject.SetActive(false);
            new PrototypeApplicationInstaller(projectAssets, saveService).Install();
            Destroy(gameObject);
        }

        private void RefreshSlotMetadata()
        {
            slotMetadata.Clear();
            foreach (SaveSlotId slotId in slotStore.Slots)
            {
                slotMetadata.Add(slotStore.GetMetadata(slotId));
            }
        }

        private int FindLatestReadySlotIndex()
        {
            int bestIndex = -1;
            DateTime bestSavedAt = DateTime.MinValue;
            for (int index = 0; index < slotMetadata.Count; index++)
            {
                SaveSlotMetadata metadata = slotMetadata[index];
                if (metadata.Status != SaveSlotStatus.Ready)
                {
                    continue;
                }

                DateTime savedAt = metadata.LastSavedUtc ?? DateTime.MinValue;
                if (bestIndex < 0 || savedAt > bestSavedAt)
                {
                    bestIndex = index;
                    bestSavedAt = savedAt;
                }
            }

            return bestIndex;
        }

        private int FindFirstSlotIndex(SaveSlotStatus status)
        {
            for (int index = 0; index < slotMetadata.Count; index++)
            {
                if (slotMetadata[index].Status == status)
                {
                    return index;
                }
            }

            return -1;
        }

        private void Render()
        {
            if (view == null)
            {
                return;
            }

            if (CurrentPage == PrototypeTitlePage.Main)
            {
                view.ShowMainMenu(MainSelectionIndex, ContinueAvailable, message);
                return;
            }

            bool newGameMode = CurrentPage == PrototypeTitlePage.NewGameSlots;
            view.ShowSlotMenu(
                newGameMode ? "NEW GAME - SELECT EMPTY SLOT" : "LOAD GAME - SELECT SLOT",
                SlotSelectionIndex,
                slotMetadata,
                newGameMode,
                message,
                messageIsError);
        }

        private void SubscribeInput()
        {
            if (subscribed || inputReader == null || !isActiveAndEnabled)
            {
                return;
            }

            inputReader.SubmitPressed += SubmitSelection;
            inputReader.CancelPressed += HandleCancelPressed;
            subscribed = true;
        }

        private void UnsubscribeInput()
        {
            if (!subscribed || inputReader == null)
            {
                return;
            }

            inputReader.SubmitPressed -= SubmitSelection;
            inputReader.CancelPressed -= HandleCancelPressed;
            subscribed = false;
        }

        private void HandleCancelPressed()
        {
            Cancel();
        }

        private void ClearMessage()
        {
            message = string.Empty;
            messageIsError = false;
        }

        private void SetMessage(string value, bool isError)
        {
            message = value ?? string.Empty;
            messageIsError = isError;
        }

        private static string FormatLoadError(SaveSlotMetadata metadata)
        {
            switch (metadata.Status)
            {
                case SaveSlotStatus.Empty:
                    return "このSlotにはSaveがありません。";
                case SaveSlotStatus.Corrupted:
                    return "Saveが破損しているため読み込めません。既存ファイルは変更していません。";
                case SaveSlotStatus.UnsupportedVersion:
                    return $"Save Version {metadata.SaveVersion} は現在のバージョンでは読み込めません。";
                default:
                    return "このSave Slotは読み込めません。";
            }
        }

        private static int ResolveNavigationDirection(Vector2 navigate)
        {
            if (Mathf.Abs(navigate.y) <= 0.5f || Mathf.Abs(navigate.y) < Mathf.Abs(navigate.x))
            {
                return 0;
            }

            return navigate.y > 0f ? -1 : 1;
        }

        private static int WrapIndex(int value, int count)
        {
            return ((value % count) + count) % count;
        }
    }

    /// <summary>
    /// Prototype Scene起動時にTitle Screenだけを構築し、Game Sessionはユーザー選択後まで開始しません。
    /// </summary>
    internal static class PrototypeTitleScreenInstaller
    {
        public static GameObject Install(PrototypeProjectAssets projectAssets)
        {
            if (projectAssets == null)
            {
                throw new ArgumentNullException(nameof(projectAssets));
            }

            GameObject root = new("Title Runtime", typeof(RectTransform));
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            root.AddComponent<GraphicRaycaster>();

            PrototypeTitleScreenView view = root.AddComponent<PrototypeTitleScreenView>();
            view.Initialize(projectAssets.UiFont);
            PlayerInputReader inputReader = root.AddComponent<PlayerInputReader>();
            PrototypeTitleScreenController controller = root.AddComponent<PrototypeTitleScreenController>();
            controller.Initialize(
                projectAssets,
                LocalSaveSlotStore.CreateDefault(),
                inputReader,
                view);
            return root;
        }
    }
}
