using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Core.Application;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Save;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Quests;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// PrototypeのRuntime Stateを1つのGameSaveDataへ集約し、ローカルSaveへ書き込みます。
    /// Gameplay Featureは保存先を知らず、Compositionだけが各状態を束ねます。
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class PrototypeLocalSaveCoordinator : MonoBehaviour
    {
        private const float AutosaveIntervalSeconds = 15f;

        private ISaveService saveService;
        private CharacterProgressionState progressionState;
        private AbilityLoadoutController loadoutController;
        private QuestProgressionService questProgressionService;
        private ProgressionGrantConsumptionState grantConsumptionState;
        private bool savingEnabled;
        private bool initialized;
        private float nextAutosaveTime;

        public bool IsInitialized => initialized;
        public bool SavingEnabled => savingEnabled;

        public void Initialize(
            ISaveService service,
            CharacterProgressionState characterProgressionState,
            AbilityLoadoutController abilityLoadoutController,
            QuestProgressionService questService,
            ProgressionGrantConsumptionState consumptionState,
            bool enableSaving)
        {
            saveService = service ?? throw new ArgumentNullException(nameof(service));
            progressionState = characterProgressionState ??
                throw new ArgumentNullException(nameof(characterProgressionState));
            loadoutController = abilityLoadoutController != null && abilityLoadoutController.IsInitialized
                ? abilityLoadoutController
                : throw new ArgumentException(
                    "初期化済みAbilityLoadoutControllerが必要です。",
                    nameof(abilityLoadoutController));
            questProgressionService = questService ?? throw new ArgumentNullException(nameof(questService));
            grantConsumptionState = consumptionState ?? throw new ArgumentNullException(nameof(consumptionState));
            savingEnabled = enableSaving;
            initialized = true;
            nextAutosaveTime = Time.unscaledTime + AutosaveIntervalSeconds;
        }

        public bool SaveNow()
        {
            if (!initialized || !savingEnabled)
            {
                return false;
            }

            try
            {
                GameSaveData saveData = CharacterProgressionSaveMapper.ToGameSaveData(progressionState);
                saveData.player.abilityLoadout =
                    AbilityLoadoutSaveMapper.ToSaveData(loadoutController.Loadout);
                saveData.quests = QuestProgressSaveMapper.ToSaveData(questProgressionService.States);
                saveData.world = new WorldSaveData
                {
                    consumedProgressionGrantIds = grantConsumptionState.ConsumedGrantIds
                        .OrderBy(grantId => grantId, StringComparer.Ordinal)
                        .ToList()
                };

                saveService.Save(saveData);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"ローカルSaveの書き込みに失敗しました。\n{exception}", this);
                return false;
            }
        }

        private void Update()
        {
            if (!initialized || !savingEnabled || Time.unscaledTime < nextAutosaveTime)
            {
                return;
            }

            SaveNow();
            nextAutosaveTime = Time.unscaledTime + AutosaveIntervalSeconds;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveNow();
            }
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }
    }
}
