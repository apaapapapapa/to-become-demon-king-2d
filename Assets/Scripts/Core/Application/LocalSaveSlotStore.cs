using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DemonKing.Domain.Save;
using UnityEngine;
using Math = System.Math;

namespace DemonKing.Core.Application
{
    /// <summary>
    /// Application層で扱う固定Save Slot IDです。
    /// Gameplay / Save DTOへSlot概念を持ち込まないため、この境界でだけ使用します。
    /// </summary>
    public readonly struct SaveSlotId : IEquatable<SaveSlotId>
    {
        public const int MinimumValue = 1;
        public const int MaximumValue = 3;

        public static SaveSlotId Slot1 => new SaveSlotId(1);
        public static SaveSlotId Slot2 => new SaveSlotId(2);
        public static SaveSlotId Slot3 => new SaveSlotId(3);

        public SaveSlotId(int value)
        {
            if (value < MinimumValue || value > MaximumValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Save Slotは{MinimumValue}から{MaximumValue}の範囲で指定してください。");
            }

            Value = value;
        }

        public int Value { get; }
        public bool IsValid => Value >= MinimumValue && Value <= MaximumValue;

        public bool Equals(SaveSlotId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is SaveSlotId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return $"Slot {Value}";
        }
    }

    public enum SaveSlotStatus
    {
        Empty = 0,
        Ready = 1,
        Corrupted = 2,
        UnsupportedVersion = 3
    }

    /// <summary>
    /// Title / Load Game UI向けの表示用Metadataです。
    /// GameSaveDataとは別境界とし、Gameplay Runtimeの復元には使用しません。
    /// </summary>
    public sealed class SaveSlotMetadata
    {
        internal SaveSlotMetadata(
            SaveSlotId slotId,
            SaveSlotStatus status,
            DateTime? lastSavedUtc,
            double playTimeSeconds,
            int level,
            string currentFieldId,
            int saveVersion)
        {
            SlotId = slotId;
            Status = status;
            LastSavedUtc = lastSavedUtc;
            PlayTimeSeconds = Math.Max(0d, playTimeSeconds);
            Level = Math.Max(0, level);
            CurrentFieldId = currentFieldId ?? string.Empty;
            SaveVersion = saveVersion;
        }

        public SaveSlotId SlotId { get; }
        public SaveSlotStatus Status { get; }
        public DateTime? LastSavedUtc { get; }
        public double PlayTimeSeconds { get; }
        public int Level { get; }
        public string CurrentFieldId { get; }
        public int SaveVersion { get; }
        public bool CanLoad => Status == SaveSlotStatus.Ready;
    }

    /// <summary>
    /// 3つのローカルSave Slotについて、Slot IDから具体的な保存先とMetadataを解決します。
    /// 選択後のGameplay側には解決済みISaveServiceだけを渡します。
    /// </summary>
    public sealed class LocalSaveSlotStore
    {
        public const int SlotCount = 3;

        private static readonly IReadOnlyList<SaveSlotId> AvailableSlots = Array.AsReadOnly(
            new[] { SaveSlotId.Slot1, SaveSlotId.Slot2, SaveSlotId.Slot3 });

        private readonly string directoryPath;
        private readonly Func<double> realtimeSecondsProvider;
        private readonly Func<DateTime> utcNowProvider;

        public LocalSaveSlotStore(string directoryPath)
            : this(
                directoryPath,
                () => Time.unscaledTime,
                () => DateTime.UtcNow)
        {
        }

        public LocalSaveSlotStore(
            string directoryPath,
            Func<double> realtimeSecondsProvider,
            Func<DateTime> utcNowProvider)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("Saveディレクトリは空にできません。", nameof(directoryPath));
            }

            this.directoryPath = Path.GetFullPath(directoryPath);
            this.realtimeSecondsProvider = realtimeSecondsProvider ??
                throw new ArgumentNullException(nameof(realtimeSecondsProvider));
            this.utcNowProvider = utcNowProvider ?? throw new ArgumentNullException(nameof(utcNowProvider));
        }

        public IReadOnlyList<SaveSlotId> Slots => AvailableSlots;

        public static LocalSaveSlotStore CreateDefault()
        {
            return new LocalSaveSlotStore(UnityEngine.Application.persistentDataPath);
        }

        public ISaveService CreateSaveService(SaveSlotId slotId)
        {
            SaveSlotMetadata existingMetadata = GetMetadata(slotId);
            double previousPlayTimeSeconds = existingMetadata.Status == SaveSlotStatus.Ready
                ? existingMetadata.PlayTimeSeconds
                : 0d;

            return new SaveSlotFileSaveService(
                new JsonFileSaveService(GetSaveFilePath(slotId)),
                GetMetadataFilePath(slotId),
                previousPlayTimeSeconds,
                realtimeSecondsProvider,
                utcNowProvider);
        }

        public SaveSlotMetadata GetMetadata(SaveSlotId slotId)
        {
            string saveFilePath = GetSaveFilePath(slotId);
            if (!File.Exists(saveFilePath))
            {
                return CreateEmptyMetadata(slotId);
            }

            DateTime fallbackSavedAtUtc = File.GetLastWriteTimeUtc(saveFilePath);
            try
            {
                var saveService = new JsonFileSaveService(saveFilePath);
                if (!saveService.TryLoad(out GameSaveData rawSaveData))
                {
                    return CreateEmptyMetadata(slotId);
                }

                int sourceVersion = rawSaveData.version;
                GameSaveData migratedSaveData;
                try
                {
                    migratedSaveData = GameSaveDataMigrator.MigrateToCurrent(rawSaveData);
                }
                catch (NotSupportedException)
                {
                    return new SaveSlotMetadata(
                        slotId,
                        SaveSlotStatus.UnsupportedVersion,
                        fallbackSavedAtUtc,
                        0d,
                        rawSaveData.player?.level ?? 0,
                        rawSaveData.world?.currentFieldId,
                        sourceVersion);
                }

                SaveSlotMetadataFileData metadataFileData = TryReadMetadataFile(
                    GetMetadataFilePath(slotId));
                DateTime lastSavedUtc = ResolveLastSavedUtc(
                    metadataFileData,
                    fallbackSavedAtUtc);
                double playTimeSeconds = metadataFileData == null
                    ? 0d
                    : Math.Max(0d, metadataFileData.playTimeSeconds);

                return new SaveSlotMetadata(
                    slotId,
                    SaveSlotStatus.Ready,
                    lastSavedUtc,
                    playTimeSeconds,
                    migratedSaveData.player?.level ?? 0,
                    migratedSaveData.world?.currentFieldId,
                    sourceVersion);
            }
            catch (Exception)
            {
                return new SaveSlotMetadata(
                    slotId,
                    SaveSlotStatus.Corrupted,
                    fallbackSavedAtUtc,
                    0d,
                    0,
                    string.Empty,
                    0);
            }
        }

        public string GetSaveFilePath(SaveSlotId slotId)
        {
            return Path.Combine(directoryPath, GetSlotFileStem(slotId) + ".json");
        }

        public string GetMetadataFilePath(SaveSlotId slotId)
        {
            return Path.Combine(directoryPath, GetSlotFileStem(slotId) + ".metadata.json");
        }

        private static string GetSlotFileStem(SaveSlotId slotId)
        {
            if (!slotId.IsValid)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotId),
                    slotId.Value,
                    $"Save Slotは{SaveSlotId.MinimumValue}から{SaveSlotId.MaximumValue}の範囲で指定してください。");
            }

            // Slot 1は既存Prototypeのsave.jsonをそのまま利用し、既存Saveを移動せず互換性を維持します。
            return slotId.Value == SaveSlotId.Slot1.Value
                ? "save"
                : $"save-{slotId.Value}";
        }

        private static SaveSlotMetadata CreateEmptyMetadata(SaveSlotId slotId)
        {
            return new SaveSlotMetadata(
                slotId,
                SaveSlotStatus.Empty,
                null,
                0d,
                0,
                string.Empty,
                0);
        }

        private static DateTime ResolveLastSavedUtc(
            SaveSlotMetadataFileData metadataFileData,
            DateTime fallbackSavedAtUtc)
        {
            if (metadataFileData == null || metadataFileData.savedAtUtcTicks <= 0)
            {
                return fallbackSavedAtUtc;
            }

            try
            {
                return new DateTime(metadataFileData.savedAtUtcTicks, DateTimeKind.Utc);
            }
            catch (ArgumentOutOfRangeException)
            {
                return fallbackSavedAtUtc;
            }
        }

        private static SaveSlotMetadataFileData TryReadMetadataFile(string metadataFilePath)
        {
            if (!File.Exists(metadataFilePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(metadataFilePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }

                SaveSlotMetadataFileData metadata =
                    JsonUtility.FromJson<SaveSlotMetadataFileData>(json);
                return metadata != null &&
                       metadata.version == SaveSlotMetadataFileData.CurrentVersion
                    ? metadata
                    : null;
            }
            catch (Exception)
            {
                // Metadataだけが壊れていてもGame Save本体が有効ならLoad可能です。
                // 表示値はGame Saveとファイル更新日時から再構築します。
                return null;
            }
        }
    }

    internal sealed class SaveSlotFileSaveService : ISaveService
    {
        private readonly JsonFileSaveService saveService;
        private readonly string metadataFilePath;
        private readonly double previousPlayTimeSeconds;
        private readonly Func<double> realtimeSecondsProvider;
        private readonly Func<DateTime> utcNowProvider;
        private readonly double sessionStartRealtimeSeconds;

        public SaveSlotFileSaveService(
            JsonFileSaveService saveService,
            string metadataFilePath,
            double previousPlayTimeSeconds,
            Func<double> realtimeSecondsProvider,
            Func<DateTime> utcNowProvider)
        {
            this.saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            if (string.IsNullOrWhiteSpace(metadataFilePath))
            {
                throw new ArgumentException(
                    "Save Metadataファイルパスは空にできません。",
                    nameof(metadataFilePath));
            }

            this.metadataFilePath = Path.GetFullPath(metadataFilePath);
            this.previousPlayTimeSeconds = Math.Max(0d, previousPlayTimeSeconds);
            this.realtimeSecondsProvider = realtimeSecondsProvider ??
                throw new ArgumentNullException(nameof(realtimeSecondsProvider));
            this.utcNowProvider = utcNowProvider ?? throw new ArgumentNullException(nameof(utcNowProvider));
            sessionStartRealtimeSeconds = this.realtimeSecondsProvider();
        }

        public bool TryLoad(out GameSaveData saveData)
        {
            return saveService.TryLoad(out saveData);
        }

        public void Save(GameSaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            saveService.Save(saveData);
            WriteMetadata(saveData);
        }

        private void WriteMetadata(GameSaveData saveData)
        {
            double elapsedSeconds = Math.Max(
                0d,
                realtimeSecondsProvider() - sessionStartRealtimeSeconds);
            DateTime savedAtUtc = utcNowProvider().ToUniversalTime();
            var metadata = new SaveSlotMetadataFileData
            {
                version = SaveSlotMetadataFileData.CurrentVersion,
                savedAtUtcTicks = savedAtUtc.Ticks,
                playTimeSeconds = previousPlayTimeSeconds + elapsedSeconds,
                level = saveData.player?.level ?? 0,
                currentFieldId = saveData.world?.currentFieldId ?? string.Empty
            };

            string directory = Path.GetDirectoryName(metadataFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(metadata, prettyPrint: true);
            string temporaryPath = metadataFilePath + ".tmp";
            File.WriteAllText(temporaryPath, json, Encoding.UTF8);
            File.Copy(temporaryPath, metadataFilePath, overwrite: true);
            File.Delete(temporaryPath);
        }
    }

    [Serializable]
    internal sealed class SaveSlotMetadataFileData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public long savedAtUtcTicks;
        public double playTimeSeconds;
        public int level;
        public string currentFieldId = string.Empty;
    }
}
