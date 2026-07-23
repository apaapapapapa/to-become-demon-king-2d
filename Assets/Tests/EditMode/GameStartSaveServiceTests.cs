using DemonKing.Core.Application;
using DemonKing.Domain.Save;
using NUnit.Framework;

namespace DemonKing.Tests.EditMode
{
    public sealed class GameStartSaveServiceTests
    {
        [Test]
        public void FreshGameSaveService_既存Saveを読まず保存だけ選択Slotへ委譲する()
        {
            var inner = new RecordingSaveService
            {
                LoadData = new GameSaveData
                {
                    player = new PlayerSaveData { level = 99 }
                }
            };
            var service = new FreshGameSaveService(inner);

            bool loaded = service.TryLoad(out GameSaveData loadedData);
            var newSave = new GameSaveData
            {
                player = new PlayerSaveData { level = 1 }
            };
            service.Save(newSave);

            Assert.That(loaded, Is.False);
            Assert.That(loadedData, Is.Null);
            Assert.That(inner.TryLoadCount, Is.Zero);
            Assert.That(inner.LastSavedData, Is.SameAs(newSave));
        }

        private sealed class RecordingSaveService : ISaveService
        {
            public GameSaveData LoadData { get; set; }
            public GameSaveData LastSavedData { get; private set; }
            public int TryLoadCount { get; private set; }

            public bool TryLoad(out GameSaveData saveData)
            {
                TryLoadCount++;
                saveData = LoadData;
                return saveData != null;
            }

            public void Save(GameSaveData saveData)
            {
                LastSavedData = saveData;
            }
        }
    }
}
