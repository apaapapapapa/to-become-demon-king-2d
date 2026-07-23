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
    /// プロトタイプ起動時のアプリケーション構成を組み立てます。
    /// Application全体の構築順序を調停し、Feature固有のSave復元処理はGame Sessionへ委譲します。
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
            this.projectAssets = projectAssets;
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

            PrototypeSceneConfigurator.Configure(Camera.main);
            PrototypeSortingConfigurator.Configure();

            GameObject applicationRoot = new("Application Runtime");
            var dialogueLog = new DialogueLog();
            PrototypeGameSessionResult sessionResult = new PrototypeGameSession(
                    projectAssets,
                    settings,
                    dialogueLog,
                    saveService)
                .Start(applicationRoot);
            PrototypeWorldBuildResult worldResult = sessionResult.WorldResult;

            PlayerInputReader inputReader = worldResult.Player == null
                ? null
                : worldResult.Player.GetComponent<PlayerInputReader>();

            ModalUiCoordinator modalUiCoordinator =
                applicationRoot.AddComponent<ModalUiCoordinator>();
            GamePauseController pauseController =
                applicationRoot.AddComponent<GamePauseController>();

            if (inputReader == null)
            {
                Debug.LogError("PlayerInputReaderが見つからないため、Modal UI入力を初期化できません。");
            }
            else
            {
                modalUiCoordinator.Initialize(inputReader);
                pauseController.Initialize(
                    inputReader,
                    modalUiCoordinator,
                    settings.PausedTimeScale);
            }

            EvolutionSelectionController evolutionSelectionController = null;
            EvolutionProgressionController evolutionProgressionController =
                worldResult.Player == null
                    ? null
                    : worldResult.Player.GetComponent<EvolutionProgressionController>();
            if (inputReader != null && evolutionProgressionController != null)
            {
                evolutionSelectionController =
                    applicationRoot.AddComponent<EvolutionSelectionController>();
                evolutionSelectionController.Initialize(
                    inputReader,
                    modalUiCoordinator,
                    evolutionProgressionController);
            }
            else
            {
                Debug.LogError("Evolution選択を初期化するためのPlayerコンポーネントが見つかりません。");
            }

            AbilityLoadoutController loadoutController = worldResult.Player == null
                ? null
                : worldResult.Player.GetComponent<AbilityLoadoutController>();
            AbilityLoadoutSelectionController abilityLoadoutSelectionController =
                worldResult.Player == null
                    ? null
                    : worldResult.Player.GetComponent<AbilityLoadoutSelectionController>();
            if (inputReader == null ||
                loadoutController == null ||
                abilityLoadoutSelectionController == null)
            {
                Debug.LogError("Ability Loadout選択を初期化するためのPlayerコンポーネントが見つかりません。");
            }
            else
            {
                abilityLoadoutSelectionController.Initialize(
                    inputReader,
                    modalUiCoordinator,
                    loadoutController,
                    projectAssets.PlayerCharacter,
                    sessionResult.ProgressionState);
            }

            PrototypeUiInstaller.Create(
                projectAssets.UiFont,
                projectAssets.PauseMenuPrefab,
                projectAssets.EvolutionMenuPrefab,
                projectAssets.AbilityLoadoutMenuPrefab,
                pauseController,
                dialogueLog,
                evolutionSelectionController,
                abilityLoadoutSelectionController,
                worldResult.QuestProgressionService);

            return applicationRoot;
        }
    }
}
