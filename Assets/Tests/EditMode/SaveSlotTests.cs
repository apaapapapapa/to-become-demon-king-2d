using System;
using System.IO;
using DemonKing.Core.Application;
using DemonKing.Domain.Save;
using NUnit.Framework;

namespace DemonKing.Tests.EditMode
{
    public sealed class SaveSlotTests
    {
        [Test]
        public void LocalSaveSlotStore_3Slotを独立したSaveとして扱う()
        {
            string directory = CreateTemporaryDirectory();
            double realtimeSeconds = 10d;
            DateTime utcNow = new DateTime(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);

            try
            {
                var store = new LocalSaveSlotStore(
                    directory,
                    () => realtimeSeconds,
                    () => utcNow);
                ISaveService slot1 = store.CreateSaveService(SaveSlotId.Slot1);
                ISaveService slot2 = store.CreateSaveService(SaveSlotId.Slot2);

                slot1.Save(CreateSaveData(level: 2, fieldId: "field.forest"));
                slot2.Save(CreateSaveData(level: 7, fieldId: "field.town"));

                Assert.That(slot1.TryLoad(out GameSaveData slot1Save), Is.True);
                Assert.That(slot2.TryLoad(out GameSaveData slot2Save), Is.True);
                Assert.That(slot1Save.player.level, Is.EqualTo(2));
                Assert.That(slot2Save.player.level, Is.EqualTo(7));
                Assert.That(File.Exists(store.GetSaveFilePath(SaveSlotId.Slot1)), Is.True);
                Assert.That(File.Exists(store.GetSaveFilePath(SaveSlotId.Slot2)), Is.True);
                Assert.That(store.GetMetadata(SaveSlotId.Slot3).Status, Is.EqualTo(SaveSlotStatus.Empty));
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        [Test]
        public void LocalSaveSlotStore_Save時に表示用Metadataを更新してPlayTimeを累積する()
        {
            string directory = CreateTemporaryDirectory();
            double realtimeSeconds = 100d;
            DateTime utcNow = new DateTime(2026, 7, 23, 12, 30, 0, DateTimeKind.Utc);

            try
            {
                var store = new LocalSaveSlotStore(
                    directory,
                    () => realtimeSeconds,
                    () => utcNow);
                ISaveService firstSession = store.CreateSaveService(SaveSlotId.Slot2);

                realtimeSeconds = 125d;
                firstSession.Save(CreateSaveData(level: 4, fieldId: "field.forest.depths"));

                SaveSlotMetadata firstMetadata = store.GetMetadata(SaveSlotId.Slot2);
                Assert.That(firstMetadata.Status, Is.EqualTo(SaveSlotStatus.Ready));
                Assert.That(firstMetadata.CanLoad, Is.True);
                Assert.That(firstMetadata.LastSavedUtc, Is.EqualTo(utcNow));
                Assert.That(firstMetadata.PlayTimeSeconds, Is.EqualTo(25d).Within(0.001d));
                Assert.That(firstMetadata.Level, Is.EqualTo(4));
                Assert.That(firstMetadata.CurrentFieldId, Is.EqualTo("field.forest.depths"));
                Assert.That(firstMetadata.SaveVersion, Is.EqualTo(GameSaveData.CurrentVersion));

                realtimeSeconds = 200d;
                ISaveService secondSession = store.CreateSaveService(SaveSlotId.Slot2);
                realtimeSeconds = 215d;
                utcNow = utcNow.AddMinutes(10);
                secondSession.Save(CreateSaveData(level: 5, fieldId: "field.human_town"));

                SaveSlotMetadata secondMetadata = store.GetMetadata(SaveSlotId.Slot2);
                Assert.That(secondMetadata.LastSavedUtc, Is.EqualTo(utcNow));
                Assert.That(secondMetadata.PlayTimeSeconds, Is.EqualTo(40d).Within(0.001d));
                Assert.That(secondMetadata.Level, Is.EqualTo(5));
                Assert.That(secondMetadata.CurrentFieldId, Is.EqualTo("field.human_town"));
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        [Test]
        public void LocalSaveSlotStore_Slot1は既存SaveJsonをそのままLoad可能として扱う()
        {
            string directory = CreateTemporaryDirectory();

            try
            {
                string legacyPath = Path.Combine(directory, JsonFileSaveService.DefaultFileName);
                var legacyService = new JsonFileSaveService(legacyPath);
                GameSaveData legacySave = CreateSaveData(level: 3, fieldId: string.Empty);
                legacySave.version = 3;
                legacyService.Save(legacySave);

                var store = new LocalSaveSlotStore(directory, () => 0d, () => DateTime.UtcNow);
                SaveSlotMetadata metadata = store.GetMetadata(SaveSlotId.Slot1);

                Assert.That(store.GetSaveFilePath(SaveSlotId.Slot1), Is.EqualTo(legacyPath));
                Assert.That(metadata.Status, Is.EqualTo(SaveSlotStatus.Ready));
                Assert.That(metadata.Level, Is.EqualTo(3));
                Assert.That(metadata.SaveVersion, Is.EqualTo(3));
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        [Test]
        public void LocalSaveSlotStore_破損Saveと未対応VersionをUI向け状態として区別する()
        {
            string directory = CreateTemporaryDirectory();

            try
            {
                var store = new LocalSaveSlotStore(directory, () => 0d, () => DateTime.UtcNow);
                File.WriteAllText(store.GetSaveFilePath(SaveSlotId.Slot2), string.Empty);

                var unsupportedService = new JsonFileSaveService(
                    store.GetSaveFilePath(SaveSlotId.Slot3));
                GameSaveData unsupportedSave = CreateSaveData(level: 9, fieldId: "field.future");
                unsupportedSave.version = GameSaveData.CurrentVersion + 1;
                unsupportedService.Save(unsupportedSave);

                SaveSlotMetadata corrupted = store.GetMetadata(SaveSlotId.Slot2);
                SaveSlotMetadata unsupported = store.GetMetadata(SaveSlotId.Slot3);

                Assert.That(corrupted.Status, Is.EqualTo(SaveSlotStatus.Corrupted));
                Assert.That(corrupted.CanLoad, Is.False);
                Assert.That(unsupported.Status, Is.EqualTo(SaveSlotStatus.UnsupportedVersion));
                Assert.That(unsupported.CanLoad, Is.False);
                Assert.That(unsupported.SaveVersion, Is.EqualTo(GameSaveData.CurrentVersion + 1));
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static GameSaveData CreateSaveData(int level, string fieldId)
        {
            return new GameSaveData
            {
                version = GameSaveData.CurrentVersion,
                player = new PlayerSaveData
                {
                    characterDefinitionId = "character.player.slime",
                    level = level
                },
                world = new WorldSaveData
                {
                    currentFieldId = fieldId,
                    entryPointId = string.IsNullOrEmpty(fieldId) ? string.Empty : "entry.default"
                }
            };
        }

        private static string CreateTemporaryDirectory()
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "demon-king-save-slot-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            return directory;
        }

        private static void DeleteTemporaryDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
