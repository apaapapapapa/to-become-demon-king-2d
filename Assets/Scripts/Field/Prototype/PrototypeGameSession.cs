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

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Save読込からField構築、Runtime復元、保存開始までのGame Session起動順序を管理します。
    /// Field Definition / Entry Pointの解決とField Runtime CompositionはField境界へ委譲します。
    /// </summary>
    internal sealed class PrototypeGameSession
    {
        private readonly PrototypeProjectAssets projectAssets;
        private readonly PrototypeApplicationSettings settings;
        private readonly DialogueLog dialogueLog;
        private readonly ISaveService saveService;

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
        }

        public PrototypeGameSessionResult Start(GameObject applicationRoot)
        {
            if (applicationRoot == null)
            {
                throw new ArgumentNullException(nameof(applicationRoot));
            }

            PrototypeFieldCatalog fieldCatalog =
                PrototypeFieldCatalog.CreateInitial(settings, projectAssets);
            PrototypeSaveSession saveSession = PrototypeSaveSession.Load(
                saveService,
                projectAssets.PlayerCharacter.CharacterId,
                fieldCatalog.InitialField.DefaultLocation);

            ResolveFieldLocation(
                fieldCatalog,
                saveSession.CurrentFieldLocation,
                out PrototypeFieldDefinition fieldDefinition,
                out FieldEntryPoint entryPoint);

            PrototypeWorldBuildResult worldResult = new PrototypeWorldBuilder(
                    fieldDefinition,
                    entryPoint,
                    dialogueLog,
                    saveSession.ProgressionState,
                    saveSession.GrantConsumptionState)
                .Build();

            new PrototypeGameSaveRestorer(projectAssets).Restore(saveSession, worldResult);
            StartSaving(applicationRoot, saveSession, worldResult);

            return new PrototypeGameSessionResult(
                worldResult,
                saveSession.ProgressionState,
                worldResult.FieldLocation);
        }

        private static void ResolveFieldLocation(
            PrototypeFieldCatalog fieldCatalog,
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

        private void StartSaving(
            GameObject applicationRoot,
            PrototypeSaveSession saveSession,
            PrototypeWorldBuildResult worldResult)
        {
            AbilityLoadoutController loadoutController = worldResult.Player == null
                ? null
                : worldResult.Player.GetComponent<AbilityLoadoutController>();
            if (loadoutController == null || worldResult.QuestProgressionService == null)
            {
                Debug.LogError(
                    "Save Snapshotを構築するためのRuntime Stateが揃っていないため、保存を開始できません。");
                return;
            }

            var snapshotProvider = new PrototypeGameSaveSnapshotProvider(
                saveSession.ProgressionState,
                loadoutController,
                worldResult.QuestProgressionService,
                saveSession.GrantConsumptionState,
                worldResult.FieldLocation);

            PrototypeLocalSaveCoordinator saveCoordinator =
                applicationRoot.AddComponent<PrototypeLocalSaveCoordinator>();
            saveCoordinator.Initialize(
                saveService,
                snapshotProvider,
                saveSession.SavingEnabled);
            saveCoordinator.SaveNow();
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
    /// 保存タイミングや保存先は知りません。
    /// </summary>
    internal sealed class PrototypeGameSaveSnapshotProvider : IGameSaveSnapshotProvider
    {
        private readonly CharacterProgressionState progressionState;
        private readonly AbilityLoadoutController loadoutController;
        private readonly QuestProgressionService questProgressionService;
        private readonly ProgressionGrantConsumptionState grantConsumptionState;
        private readonly FieldLocation fieldLocation;

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
            this.loadoutController = loadoutController != null && loadoutController.IsInitialized
                ? loadoutController
                : throw new ArgumentException(
                    "初期化済みAbilityLoadoutControllerが必要です。",
                    nameof(loadoutController));
            this.questProgressionService = questProgressionService ??
                throw new ArgumentNullException(nameof(questProgressionService));
            this.grantConsumptionState = grantConsumptionState ??
                throw new ArgumentNullException(nameof(grantConsumptionState));
            this.fieldLocation = fieldLocation.IsValid
                ? fieldLocation
                : throw new ArgumentException(
                    "Field Locationが不正です。",
                    nameof(fieldLocation));
        }

        public GameSaveData CreateSnapshot()
        {
            PlayerSaveData playerSaveData = CharacterProgressionSaveMapper.ToSaveData(
                progressionState);
            playerSaveData.abilityLoadout =
                AbilityLoadoutSaveMapper.ToSaveData(loadoutController.Loadout);

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
    }
}
