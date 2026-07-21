using System;
using DemonKing.Core.Application;
using DemonKing.Domain.Save;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Prototypeの保存タイミングを管理します。
    /// Runtime StateからGameSaveDataを構築する責務はIGameSaveSnapshotProviderへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class PrototypeLocalSaveCoordinator : MonoBehaviour
    {
        private const float AutosaveIntervalSeconds = 15f;

        private ISaveService saveService;
        private IGameSaveSnapshotProvider snapshotProvider;
        private bool savingEnabled;
        private bool initialized;
        private float nextAutosaveTime;

        public bool IsInitialized => initialized;
        public bool SavingEnabled => savingEnabled;

        public void Initialize(
            ISaveService service,
            IGameSaveSnapshotProvider provider,
            bool enableSaving)
        {
            saveService = service ?? throw new ArgumentNullException(nameof(service));
            snapshotProvider = provider ?? throw new ArgumentNullException(nameof(provider));
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
                GameSaveData saveData = snapshotProvider.CreateSnapshot();
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
