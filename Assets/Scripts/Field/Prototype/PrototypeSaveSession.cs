using System;
using DemonKing.Core.Application;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Save;
using DemonKing.Field.Composition;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 起動時のSave読込結果をGame Session Runtime Stateへ変換して保持します。
    /// 不正・未対応Saveは上書きせず、新規状態で起動しつつ保存を無効化します。
    /// </summary>
    internal sealed class PrototypeSaveSession
    {
        private PrototypeSaveSession(
            GameSaveData saveData,
            CharacterProgressionState progressionState,
            ProgressionGrantConsumptionState grantConsumptionState,
            FieldLocation currentFieldLocation,
            bool wasLoaded,
            bool hasSavedLoadout,
            bool savingEnabled)
        {
            SaveData = saveData;
            ProgressionState = progressionState;
            GrantConsumptionState = grantConsumptionState;
            CurrentFieldLocation = currentFieldLocation;
            WasLoaded = wasLoaded;
            HasSavedLoadout = hasSavedLoadout;
            SavingEnabled = savingEnabled;
        }

        public GameSaveData SaveData { get; }
        public CharacterProgressionState ProgressionState { get; }
        public ProgressionGrantConsumptionState GrantConsumptionState { get; }
        public FieldLocation CurrentFieldLocation { get; }
        public bool WasLoaded { get; }
        public bool HasSavedLoadout { get; }
        public bool SavingEnabled { get; }

        public static PrototypeSaveSession Load(
            ISaveService saveService,
            string expectedCharacterId)
        {
            return Load(
                saveService,
                expectedCharacterId,
                new FieldLocation(
                    PrototypeFieldDefinition.DefaultFieldId,
                    PrototypeFieldDefinition.DefaultEntryPointId));
        }

        public static PrototypeSaveSession Load(
            ISaveService saveService,
            string expectedCharacterId,
            FieldLocation defaultFieldLocation)
        {
            if (saveService == null)
            {
                throw new ArgumentNullException(nameof(saveService));
            }

            if (!defaultFieldLocation.IsValid)
            {
                throw new ArgumentException(
                    "Default Field Locationが不正です。",
                    nameof(defaultFieldLocation));
            }

            try
            {
                if (!saveService.TryLoad(out GameSaveData rawSave))
                {
                    return CreateFresh(
                        expectedCharacterId,
                        defaultFieldLocation,
                        savingEnabled: true);
                }

                int sourceVersion = rawSave.version;
                GameSaveData saveData = GameSaveDataMigrator.MigrateToCurrent(rawSave);
                CharacterProgressionState progressionState =
                    CharacterProgressionSaveMapper.FromSaveData(saveData.player);

                if (!string.Equals(
                        progressionState.CharacterDefinitionId,
                        expectedCharacterId,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"SaveのCharacter IDが現在のPlayer Definitionと一致しません: " +
                        $"{progressionState.CharacterDefinitionId}");
                }

                ProgressionGrantConsumptionState consumptionState =
                    ProgressionGrantConsumptionState.Restore(
                        saveData.world.consumedProgressionGrantIds);
                FieldLocation fieldLocation = ResolveFieldLocation(
                    saveData.world,
                    defaultFieldLocation);

                return new PrototypeSaveSession(
                    saveData,
                    progressionState,
                    consumptionState,
                    fieldLocation,
                    wasLoaded: true,
                    hasSavedLoadout: sourceVersion >= 3,
                    savingEnabled: true);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"ローカルSaveを復元できませんでした。既存ファイルを保護するため、この起動中の保存を無効化します。\n{exception}");
                return CreateFresh(
                    expectedCharacterId,
                    defaultFieldLocation,
                    savingEnabled: false);
            }
        }

        private static FieldLocation ResolveFieldLocation(
            WorldSaveData worldSaveData,
            FieldLocation defaultFieldLocation)
        {
            if (worldSaveData == null ||
                string.IsNullOrWhiteSpace(worldSaveData.currentFieldId) ||
                string.IsNullOrWhiteSpace(worldSaveData.entryPointId))
            {
                return defaultFieldLocation;
            }

            return new FieldLocation(
                worldSaveData.currentFieldId,
                worldSaveData.entryPointId);
        }

        private static PrototypeSaveSession CreateFresh(
            string characterId,
            FieldLocation defaultFieldLocation,
            bool savingEnabled)
        {
            return new PrototypeSaveSession(
                saveData: null,
                CharacterProgressionState.CreateInitial(characterId),
                ProgressionGrantConsumptionState.CreateInitial(),
                defaultFieldLocation,
                wasLoaded: false,
                hasSavedLoadout: false,
                savingEnabled: savingEnabled);
        }
    }
}
