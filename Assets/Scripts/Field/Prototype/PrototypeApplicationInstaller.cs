using System;
using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Progression;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプ起動時のApplication寿命を構築します。
    /// Application RootはField Sceneより長く保持し、FieldごとのPlayer / UI参照はBinderへ委譲します。
    /// </summary>
    internal sealed class PrototypeApplicationInstaller
    {
        private readonly PrototypeProjectAssets projectAssets;
        private readonly ISaveService saveService;

        public PrototypeApplicationInstaller(PrototypeProjectAssets projectAssets)
            : this(
                projectAssets,
                LocalSaveSlotStore.CreateDefault().CreateSaveService(SaveSlotId.Slot1))
        {
        }

        public PrototypeApplicationInstaller(
            PrototypeProjectAssets projectAssets,
            ISaveService saveService)
        {
            this.projectAssets = projectAssets != null
                ? projectAssets
                : throw new ArgumentNullException(nameof(projectAssets));
            this.saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public GameObject Install()
        {
            PrototypeApplicationSettings settings = projectAssets.ApplicationSettings;
            if (settings == null)
            {
                Debug.LogError("PrototypeApplicationSettingsが設定されていません。");
                return null;
            }

            PrototypeSortingConfigurator.Configure();

            GameObject applicationRoot = new("Application Runtime");
            UnityEngine.Object.DontDestroyOnLoad(applicationRoot);

            var dialogueLog = new DialogueLog();
            var gameSession = new PrototypeGameSession(
                projectAssets,
                settings,
                dialogueLog,
                saveService);

            PrototypeApplicationFieldBinder fieldBinder =
                applicationRoot.AddComponent<PrototypeApplicationFieldBinder>();
            fieldBinder.Initialize(projectAssets, settings, dialogueLog);

            PrototypeFieldTransitionService transitionService =
                applicationRoot.AddComponent<PrototypeFieldTransitionService>();
            transitionService.Initialize(gameSession, fieldBinder);
            gameSession.SetTransitionRequester(transitionService);

            PrototypeGameSessionResult sessionResult = gameSession.Start(applicationRoot);
            fieldBinder.Bind(sessionResult);
            return applicationRoot;
        }
    }

    /// <summary>
    /// Field再構築後にApplication常駐Controllerを新Playerへ再接続し、Field UIを再生成します。
    /// Progression / Quest等のSession Stateそのものは所有しません。
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class PrototypeApplicationFieldBinder : MonoBehaviour
    {
        private PrototypeProjectAssets projectAssets;
        private PrototypeApplicationSettings settings;
        private DialogueLog dialogueLog;
        private ModalUiCoordinator modalUiCoordinator;
        private GamePauseController pauseController;
        private EvolutionSelectionController evolutionSelectionController;
        private GameObject uiRoot;
        private bool initialized;

        public PlayerInputReader CurrentInputReader { get; private set; }
        public GameObject CurrentUiRoot => uiRoot;
        public bool HasOpenModal => modalUiCoordinator != null && modalUiCoordinator.HasOpenModal;

        public void Initialize(
            PrototypeProjectAssets assets,
            PrototypeApplicationSettings applicationSettings,
            DialogueLog sharedDialogueLog)
        {
            projectAssets = assets != null
                ? assets
                : throw new ArgumentNullException(nameof(assets));
            settings = applicationSettings != null
                ? applicationSettings
                : throw new ArgumentNullException(nameof(applicationSettings));
            dialogueLog = sharedDialogueLog ?? throw new ArgumentNullException(nameof(sharedDialogueLog));

            modalUiCoordinator = GetComponent<ModalUiCoordinator>() ??
                gameObject.AddComponent<ModalUiCoordinator>();
            pauseController = GetComponent<GamePauseController>() ??
                gameObject.AddComponent<GamePauseController>();
            evolutionSelectionController = GetComponent<EvolutionSelectionController>() ??
                gameObject.AddComponent<EvolutionSelectionController>();
            initialized = true;
        }

        public void Bind(PrototypeGameSessionResult sessionResult)
        {
            if (!initialized)
            {
                throw new InvalidOperationException("PrototypeApplicationFieldBinderが初期化されていません。");
            }

            PrototypeWorldBuildResult worldResult = sessionResult.WorldResult;
            GameObject player = worldResult.Player;
            PlayerInputReader inputReader = player == null
                ? null
                : player.GetComponent<PlayerInputReader>();
            if (inputReader == null)
            {
                throw new InvalidOperationException(
                    "Field PlayerにPlayerInputReaderがないためApplication UIを再接続できません。");
            }

            if (modalUiCoordinator.HasOpenModal)
            {
                throw new InvalidOperationException(
                    "Modal UIが開いている間はField Runtimeを再バインドできません。");
            }

            CurrentInputReader = inputReader;
            modalUiCoordinator.Initialize(inputReader);
            pauseController.Initialize(
                inputReader,
                modalUiCoordinator,
                settings.PausedTimeScale);

            EvolutionProgressionController evolutionProgressionController =
                player.GetComponent<EvolutionProgressionController>();
            if (evolutionProgressionController == null || !evolutionProgressionController.IsInitialized)
            {
                throw new InvalidOperationException(
                    "Field PlayerのEvolutionProgressionControllerが初期化されていません。");
            }

            evolutionSelectionController.Initialize(
                inputReader,
                modalUiCoordinator,
                evolutionProgressionController);

            AbilityLoadoutController loadoutController =
                player.GetComponent<AbilityLoadoutController>();
            AbilityLoadoutSelectionController abilityLoadoutSelectionController =
                player.GetComponent<AbilityLoadoutSelectionController>();
            if (loadoutController == null ||
                !loadoutController.IsInitialized ||
                abilityLoadoutSelectionController == null)
            {
                throw new InvalidOperationException(
                    "Field PlayerのAbility Loadout Runtimeが初期化されていません。");
            }

            abilityLoadoutSelectionController.Initialize(
                inputReader,
                modalUiCoordinator,
                loadoutController,
                projectAssets.PlayerCharacter,
                sessionResult.ProgressionState);

            if (uiRoot != null)
            {
                Destroy(uiRoot);
            }

            uiRoot = PrototypeUiInstaller.Create(
                projectAssets.UiFont,
                projectAssets.PauseMenuPrefab,
                projectAssets.EvolutionMenuPrefab,
                projectAssets.AbilityLoadoutMenuPrefab,
                pauseController,
                dialogueLog,
                evolutionSelectionController,
                abilityLoadoutSelectionController,
                worldResult.QuestProgressionService);

            inputReader.EnableGameplayInput();
        }
    }
}
