using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Core.Application;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Quests;
using DemonKing.Domain.Save;
using DemonKing.Field.Composition;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Quests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Save読込からField構築、Runtime復元、保存開始までのGame Session寿命を管理します。
    /// Character Progression / Quest / Grant StateはField Sceneより長く保持し、Field切替時はScene依存Runtimeだけを再構築します。
    /// </summary>
    internal sealed class PrototypeGameSession
    {
        private readonly PrototypeProjectAssets projectAssets;
        private readonly PrototypeApplicationSettings settings;
        private readonly DialogueLog dialogueLog;
        private readonly ISaveService saveService;
        private readonly PrototypeFieldCatalog fieldCatalog;
        private readonly PrototypeGameSaveRestorer saveRestorer;

        private IPrototypeFieldTransitionRequester transitionRequester;
        private PrototypeSaveSession saveSession;
        private QuestProgressionService questProgressionService;
        private PrototypeGameSaveSnapshotProvider snapshotProvider;
        private PrototypeLocalSaveCoordinator saveCoordinator;
        private AbilityLoadoutSaveData preservedLoadout;
        private PrototypeWorldBuildResult currentWorldResult;
        private bool started;

        public PrototypeGameSession(
            PrototypeProjectAssets projectAssets,
            PrototypeApplicationSettings settings,
            DialogueLog dialogueLog,
            ISaveService saveService)
        {
            this.projectAssets = projectAssets != null
                ? projectAssets
                : throw new ArgumentNullException(nameof(projectAssets));
            this.settings = settings != null
                ? settings
                : throw new ArgumentNullException(nameof(settings));
            this.dialogueLog = dialogueLog ?? throw new ArgumentNullException(nameof(dialogueLog));
            this.saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            fieldCatalog = PrototypeFieldCatalog.CreateInitial(settings, projectAssets);
            saveRestorer = new PrototypeGameSaveRestorer(projectAssets);
        }

        public PrototypeFieldCatalog FieldCatalog => fieldCatalog;
        public bool IsStarted => started;
        public PrototypeWorldBuildResult CurrentWorldResult => currentWorldResult;
        public CharacterProgressionState ProgressionState => saveSession?.ProgressionState;
        public QuestProgressionService QuestProgressionService => questProgressionService;
        public FieldLocation CurrentFieldLocation => currentWorldResult.FieldLocation;

        public void SetTransitionRequester(IPrototypeFieldTransitionRequester requester)
        {
            if (started)
            {
                throw new InvalidOperationException("Game Session開始後はField Transition Requesterを差し替えられません。");
            }

            transitionRequester = requester ?? throw new ArgumentNullException(nameof(requester));
        }

        public PrototypeGameSessionResult Start(GameObject applicationRoot)
        {
            if (applicationRoot == null)
            {
                throw new ArgumentNullException(nameof(applicationRoot));
            }

            if (started)
            {
                throw new InvalidOperationException("PrototypeGameSessionは既に開始されています。");
            }

            saveSession = PrototypeSaveSession.Load(
                saveService,
                projectAssets.PlayerCharacter.CharacterId,
                fieldCatalog.InitialField.DefaultLocation);
            questProgressionService = new QuestProgressionService(projectAssets.QuestDefinitions);

            ResolveFieldLocation(
                saveSession.CurrentFieldLocation,
                out PrototypeFieldDefinition fieldDefinition,
                out FieldEntryPoint entryPoint);

            Scene activeFieldScene = PrototypeFieldSceneRuntime.Activate(
                fieldDefinition.SceneName,
                out Scene previousScene);
            currentWorldResult = BuildField(fieldDefinition, entryPoint);
            saveRestorer.Restore(saveSession, currentWorldResult);
            preservedLoadout = CaptureLoadout(currentWorldResult.Player);
            StartSaving(applicationRoot);
            PrototypeFieldSceneRuntime.UnloadPrevious(previousScene, activeFieldScene);

            started = true;
            return CreateResult();
        }

        /// <summary>
        /// Scene破棄前にScene依存LoadoutをSave DTOへ退避し、現在Fieldを即時保存します。
        /// Progression / Quest / Grant StateはGame Session所有のためコピーしません。
        /// </summary>
        public void PrepareForFieldTransition()
        {
            EnsureStarted();
            preservedLoadout = snapshotProvider.PrepareForFieldTransition();
            saveCoordinator?.SaveNow();
        }

        /// <summary>
        /// Active Scene切替後に指定Fieldを構築し、Session共有状態を新しいPlayer / Worldへ再接続します。
        /// </summary>
        public PrototypeGameSessionResult EnterField(FieldLocation location)
        {
            EnsureStarted();
            ResolveFieldLocation(
                location,
                out PrototypeFieldDefinition fieldDefinition,
                out FieldEntryPoint entryPoint);

            currentWorldResult = BuildField(fieldDefinition, entryPoint);
            RestorePreservedLoadout(currentWorldResult.Player);
            snapshotProvider.BindWorld(currentWorldResult.Player, currentWorldResult.FieldLocation);
            preservedLoadout = CaptureLoadout(currentWorldResult.Player);
            saveCoordinator?.SaveNow();
            return CreateResult();
        }

        public bool TryResolveField(
            FieldLocation location,
            out PrototypeFieldDefinition fieldDefinition,
            out FieldEntryPoint entryPoint)
        {
            return fieldCatalog.TryResolve(location, out fieldDefinition, out entryPoint);
        }

        public bool SaveNow()
        {
            return saveCoordinator != null && saveCoordinator.SaveNow();
        }

        private PrototypeWorldBuildResult BuildField(
            PrototypeFieldDefinition fieldDefinition,
            FieldEntryPoint entryPoint)
        {
            return new PrototypeWorldBuilder(
                    fieldDefinition,
                    entryPoint,
                    dialogueLog,
                    saveSession.ProgressionState,
                    saveSession.GrantConsumptionState,
                    questProgressionService,
                    transitionRequester)
                .Build();
        }

        private void ResolveFieldLocation(
            FieldLocation requestedLocation,
            out PrototypeFieldDefinition fieldDefinition,
            out FieldEntryPoint entryPoint)
        {
            if (fieldCatalog.TryResolve(
                    requestedLocation,
                    out fieldDefinition,
                    out entryPoint))
            {
                return;
            }

            FieldLocation fallback = fieldCatalog.InitialField.DefaultLocation;
            Debug.LogWarning(
                $"Saveで指定されたField Locationを解決できないためInitial Fieldへ戻します: " +
                $"{requestedLocation} -> {fallback}");
            if (!fieldCatalog.TryResolve(fallback, out fieldDefinition, out entryPoint))
            {
                throw new InvalidOperationException(
                    $"Initial Field Locationを解決できません: {fallback}");
            }
        }

        private void StartSaving(GameObject applicationRoot)
        {
            AbilityLoadoutController loadoutController = ResolveLoadoutController(currentWorldResult.Player);
            if (loadoutController == null || questProgressionService == null)
            {
                Debug.LogError(
                    "Save Snapshotを構築するためのRuntime Stateが揃っていないため、保存を開始できません。");
                return;
            }

            snapshotProvider = new PrototypeGameSaveSnapshotProvider(
                saveSession.ProgressionState,
                loadoutController,
                questProgressionService,
                saveSession.GrantConsumptionState,
                currentWorldResult.FieldLocation);

            saveCoordinator = applicationRoot.AddComponent<PrototypeLocalSaveCoordinator>();
            saveCoordinator.Initialize(
                saveService,
                snapshotProvider,
                saveSession.SavingEnabled);
            saveCoordinator.SaveNow();
        }

        private void RestorePreservedLoadout(GameObject player)
        {
            AbilityLoadoutController loadoutController = ResolveLoadoutController(player);
            if (loadoutController == null)
            {
                throw new InvalidOperationException("Field遷移後のPlayerにAbilityLoadoutControllerがありません。");
            }

            AbilityLoadoutSaveMapper.ApplySavedAssignments(
                loadoutController,
                preservedLoadout,
                projectAssets.PlayerCharacter,
                saveSession.ProgressionState);
        }

        private static AbilityLoadoutSaveData CaptureLoadout(GameObject player)
        {
            AbilityLoadoutController loadoutController = ResolveLoadoutController(player);
            return loadoutController == null
                ? new AbilityLoadoutSaveData()
                : AbilityLoadoutSaveMapper.ToSaveData(loadoutController.Loadout);
        }

        private static AbilityLoadoutController ResolveLoadoutController(GameObject player)
        {
            if (player == null)
            {
                return null;
            }

            AbilityLoadoutController loadoutController = player.GetComponent<AbilityLoadoutController>();
            return loadoutController != null && loadoutController.IsInitialized
                ? loadoutController
                : null;
        }

        private PrototypeGameSessionResult CreateResult()
        {
            return new PrototypeGameSessionResult(
                currentWorldResult,
                saveSession.ProgressionState,
                currentWorldResult.FieldLocation);
        }

        private void EnsureStarted()
        {
            if (!started)
            {
                throw new InvalidOperationException("PrototypeGameSessionが開始されていません。");
            }
        }
    }

    internal readonly struct PrototypeGameSessionResult
    {
        public PrototypeGameSessionResult(
            PrototypeWorldBuildResult worldResult,
            CharacterProgressionState progressionState,
            FieldLocation currentFieldLocation)
        {
            WorldResult = worldResult;
            ProgressionState = progressionState;
            CurrentFieldLocation = currentFieldLocation;
        }

        public PrototypeWorldBuildResult WorldResult { get; }
        public CharacterProgressionState ProgressionState { get; }
        public FieldLocation CurrentFieldLocation { get; }
    }

    /// <summary>
    /// Migration済みSave DTOを、構築済みRuntime Featureへ適用します。
    /// Field ComposerはSave DTOを知らず、Runtime構築だけを担当します。
    /// </summary>
    internal sealed class PrototypeGameSaveRestorer
    {
        private readonly PrototypeProjectAssets projectAssets;

        public PrototypeGameSaveRestorer(PrototypeProjectAssets projectAssets)
        {
            this.projectAssets = projectAssets != null
                ? projectAssets
                : throw new ArgumentNullException(nameof(projectAssets));
        }

        public void Restore(
            PrototypeSaveSession saveSession,
            PrototypeWorldBuildResult worldResult)
        {
            if (saveSession == null)
            {
                throw new ArgumentNullException(nameof(saveSession));
            }

            if (!saveSession.WasLoaded)
            {
                return;
            }

            RestoreAbilityLoadout(saveSession, worldResult.Player);
            RestoreQuestProgress(saveSession, worldResult.QuestProgressionService);
        }

        private void RestoreAbilityLoadout(
            PrototypeSaveSession saveSession,
            GameObject player)
        {
            if (!saveSession.HasSavedLoadout)
            {
                return;
            }

            AbilityLoadoutController loadoutController =
                player == null ? null : player.GetComponent<AbilityLoadoutController>();
            if (loadoutController == null)
            {
                Debug.LogError("Ability LoadoutのSave復元先が見つかりません。");
                return;
            }

            AbilityLoadoutSaveMapper.ApplySavedAssignments(
                loadoutController,
                saveSession.SaveData?.player?.abilityLoadout,
                projectAssets.PlayerCharacter,
                saveSession.ProgressionState);
        }

        private void RestoreQuestProgress(
            PrototypeSaveSession saveSession,
            QuestProgressionService questProgressionService)
        {
            if (questProgressionService == null)
            {
                Debug.LogError("Quest ProgressのSave復元先が見つかりません。");
                return;
            }

            IReadOnlyList<QuestProgressState> restoredStates =
                QuestProgressSaveMapper.FromSaveData(
                    projectAssets.QuestDefinitions,
                    saveSession.SaveData?.quests);
            questProgressionService.Restore(restoredStates);
        }
    }

    /// <summary>
    /// Runtime State一式から現在VersionのGameSaveData Snapshotを生成します。
    /// Field Scene切替中はLoadoutの最後のSnapshotを保持し、Scene依存Componentを参照し続けません。
    /// </summary>
    internal sealed class PrototypeGameSaveSnapshotProvider : IGameSaveSnapshotProvider
    {
        private readonly CharacterProgressionState progressionState;
        private readonly QuestProgressionService questProgressionService;
        private readonly ProgressionGrantConsumptionState grantConsumptionState;
        private AbilityLoadoutController loadoutController;
        private AbilityLoadoutSaveData cachedLoadout;
        private FieldLocation fieldLocation;

        public PrototypeGameSaveSnapshotProvider(
            CharacterProgressionState progressionState,
            AbilityLoadoutController loadoutController,
            QuestProgressionService questProgressionService,
            ProgressionGrantConsumptionState grantConsumptionState)
            : this(
                progressionState,
                loadoutController,
                questProgressionService,
                grantConsumptionState,
                new FieldLocation(
                    PrototypeFieldDefinition.DefaultFieldId,
                    PrototypeFieldDefinition.DefaultEntryPointId))
        {
        }

        public PrototypeGameSaveSnapshotProvider(
            CharacterProgressionState progressionState,
            AbilityLoadoutController loadoutController,
            QuestProgressionService questProgressionService,
            ProgressionGrantConsumptionState grantConsumptionState,
            FieldLocation fieldLocation)
        {
            this.progressionState = progressionState ??
                throw new ArgumentNullException(nameof(progressionState));
            this.questProgressionService = questProgressionService ??
                throw new ArgumentNullException(nameof(questProgressionService));
            this.grantConsumptionState = grantConsumptionState ??
                throw new ArgumentNullException(nameof(grantConsumptionState));
            BindWorld(loadoutController, fieldLocation);
        }

        public AbilityLoadoutSaveData PrepareForFieldTransition()
        {
            cachedLoadout = CaptureCurrentLoadout();
            loadoutController = null;
            return cachedLoadout;
        }

        public void BindWorld(GameObject player, FieldLocation newFieldLocation)
        {
            AbilityLoadoutController controller = player == null
                ? null
                : player.GetComponent<AbilityLoadoutController>();
            BindWorld(controller, newFieldLocation);
        }

        public void BindWorld(
            AbilityLoadoutController controller,
            FieldLocation newFieldLocation)
        {
            loadoutController = controller != null && controller.IsInitialized
                ? controller
                : throw new ArgumentException(
                    "初期化済みAbilityLoadoutControllerが必要です。",
                    nameof(controller));
            fieldLocation = newFieldLocation.IsValid
                ? newFieldLocation
                : throw new ArgumentException(
                    "Field Locationが不正です。",
                    nameof(newFieldLocation));
            cachedLoadout = AbilityLoadoutSaveMapper.ToSaveData(loadoutController.Loadout);
        }

        public GameSaveData CreateSnapshot()
        {
            PlayerSaveData playerSaveData = CharacterProgressionSaveMapper.ToSaveData(
                progressionState);
            cachedLoadout = CaptureCurrentLoadout();
            playerSaveData.abilityLoadout = cachedLoadout;

            return new GameSaveData
            {
                version = GameSaveData.CurrentVersion,
                player = playerSaveData,
                quests = QuestProgressSaveMapper.ToSaveData(questProgressionService.States),
                world = new WorldSaveData
                {
                    currentFieldId = fieldLocation.FieldId,
                    entryPointId = fieldLocation.EntryPointId,
                    consumedProgressionGrantIds = grantConsumptionState.ConsumedGrantIds
                        .OrderBy(grantId => grantId, StringComparer.Ordinal)
                        .ToList()
                }
            };
        }

        private AbilityLoadoutSaveData CaptureCurrentLoadout()
        {
            return loadoutController != null && loadoutController.IsInitialized
                ? AbilityLoadoutSaveMapper.ToSaveData(loadoutController.Loadout)
                : cachedLoadout ?? new AbilityLoadoutSaveData();
        }
    }
}
