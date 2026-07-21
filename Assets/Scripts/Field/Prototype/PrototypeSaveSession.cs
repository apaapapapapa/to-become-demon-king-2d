using System;
using DemonKing.Core.Application;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Save;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 起動時のSave読込結果をRuntime Stateへ変換して保持します。
    /// 不正・未対応Saveは上書きせず、新規状態で起動しつつ保存を無効化します。
    /// </summary>
    internal sealed class PrototypeSaveSession
    {
        private PrototypeSaveSession(
            GameSaveData saveData,
            CharacterProgressionState progressionState,
            ProgressionGrantConsumptionState grantConsumptionState,
            bool wasLoaded,
            bool hasSavedLoadout,
            bool savingEnabled)
        {
            SaveData = saveData;
            ProgressionState = progressionState;
            GrantConsumptionState = grantConsumptionState;
            WasLoaded = wasLoaded;
            HasSavedLoadout = hasSavedLoadout;
            SavingEnabled = savingEnabled;
        }

        public GameSaveData SaveData { get; }
        public CharacterProgressionState ProgressionState { get; }
        public ProgressionGrantConsumptionState GrantConsumptionState { get; }
        public bool WasLoaded { get; }
        public bool HasSavedLoadout { get; }
        public bool SavingEnabled { get; }

        public static PrototypeSaveSession Load(
            ISaveService saveService,
            string expectedCharacterId)
        {
            if (saveService == null)
            {
                throw new ArgumentNullException(nameof(saveService));
            }

            try
            {
                if (!saveService.TryLoad(out GameSaveData rawSave))
                {
                    return CreateFresh(expectedCharacterId, savingEnabled: true);
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

                return new PrototypeSaveSession(
                    saveData,
                    progressionState,
                    consumptionState,
                    wasLoaded: true,
                    hasSavedLoadout: sourceVersion >= 3,
                    savingEnabled: true);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"ローカルSaveを復元できませんでした。既存ファイルを保護するため、この起動中の保存を無効化します。\n{exception}");
                return CreateFresh(expectedCharacterId, savingEnabled: false);
            }
        }

        private static PrototypeSaveSession CreateFresh(
            string characterId,
            bool savingEnabled)
        {
            return new PrototypeSaveSession(
                saveData: null,
                CharacterProgressionState.CreateInitial(characterId),
                ProgressionGrantConsumptionState.CreateInitial(),
                wasLoaded: false,
                hasSavedLoadout: false,
                savingEnabled: savingEnabled);
        }
    }
}
