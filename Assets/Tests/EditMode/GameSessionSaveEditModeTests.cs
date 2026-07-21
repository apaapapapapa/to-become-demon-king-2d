using System.Text.RegularExpressions;
using DemonKing.Core.Application;
using DemonKing.Domain.Save;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Quests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.EditMode
{
    public sealed class GameSessionSaveEditModeTests
    {
        [Test]
        public void SaveSession_Version2をMigrationしてRuntime復元後にCurrentVersionSnapshotを保存できる()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            var saveService = new MemorySaveService(new GameSaveData
            {
                version = 2,
                player = new PlayerSaveData
                {
                    characterDefinitionId = projectAssets.PlayerCharacter.CharacterId,
                    level = 3,
                    currentExperience = 42
                }
            });

            PrototypeSaveSession saveSession = PrototypeSaveSession.Load(
                saveService,
                projectAssets.PlayerCharacter.CharacterId);

            Assert.That(saveSession.WasLoaded, Is.True);
            Assert.That(saveSession.SavingEnabled, Is.True);
            Assert.That(saveSession.SaveData.version, Is.EqualTo(GameSaveData.CurrentVersion));
            Assert.That(saveSession.ProgressionState.Level, Is.EqualTo(3));
            Assert.That(saveSession.ProgressionState.CurrentExperience, Is.EqualTo(42));

            GameObject player = new("Save Round Trip Player");
            GameObject application = new("Save Round Trip Application");
            try
            {
                AbilityLoadoutController loadout = player.AddComponent<AbilityLoadoutController>();
                loadout.Initialize(projectAssets.PlayerCharacter, saveSession.ProgressionState);
                var questService = new QuestProgressionService(projectAssets.QuestDefinitions);
                var snapshotProvider = new PrototypeGameSaveSnapshotProvider(
                    saveSession.ProgressionState,
                    loadout,
                    questService,
                    saveSession.GrantConsumptionState);

                PrototypeLocalSaveCoordinator coordinator =
                    application.AddComponent<PrototypeLocalSaveCoordinator>();
                coordinator.Initialize(saveService, snapshotProvider, enableSaving: true);

                Assert.That(coordinator.SaveNow(), Is.True);
                Assert.That(saveService.SaveCount, Is.EqualTo(1));
                Assert.That(saveService.LastSaved.version, Is.EqualTo(GameSaveData.CurrentVersion));
                Assert.That(saveService.LastSaved.player.level, Is.EqualTo(3));
                Assert.That(saveService.LastSaved.player.currentExperience, Is.EqualTo(42));
                Assert.That(saveService.LastSaved.player.abilityLoadout, Is.Not.Null);
                Assert.That(saveService.LastSaved.quests, Is.Not.Null);
                Assert.That(saveService.LastSaved.world, Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(application);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void SaveSession_未対応Versionでは既存Save保護のため保存を無効化する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            var saveService = new MemorySaveService(new GameSaveData
            {
                version = GameSaveData.CurrentVersion + 1,
                player = new PlayerSaveData
                {
                    characterDefinitionId = projectAssets.PlayerCharacter.CharacterId
                }
            });

            LogAssert.Expect(
                LogType.Error,
                new Regex("ローカルSaveを復元できませんでした", RegexOptions.Singleline));
            PrototypeSaveSession saveSession = PrototypeSaveSession.Load(
                saveService,
                projectAssets.PlayerCharacter.CharacterId);

            Assert.That(saveSession.WasLoaded, Is.False);
            Assert.That(saveSession.SavingEnabled, Is.False);

            GameObject player = new("Protected Save Player");
            GameObject application = new("Protected Save Application");
            try
            {
                AbilityLoadoutController loadout = player.AddComponent<AbilityLoadoutController>();
                loadout.Initialize(projectAssets.PlayerCharacter, saveSession.ProgressionState);
                var snapshotProvider = new PrototypeGameSaveSnapshotProvider(
                    saveSession.ProgressionState,
                    loadout,
                    new QuestProgressionService(projectAssets.QuestDefinitions),
                    saveSession.GrantConsumptionState);
                PrototypeLocalSaveCoordinator coordinator =
                    application.AddComponent<PrototypeLocalSaveCoordinator>();
                coordinator.Initialize(
                    saveService,
                    snapshotProvider,
                    saveSession.SavingEnabled);

                Assert.That(coordinator.SaveNow(), Is.False);
                Assert.That(saveService.SaveCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(application);
                Object.DestroyImmediate(player);
            }
        }

        private sealed class MemorySaveService : ISaveService
        {
            private readonly GameSaveData source;

            public MemorySaveService(GameSaveData source)
            {
                this.source = source;
            }

            public int SaveCount { get; private set; }
            public GameSaveData LastSaved { get; private set; }

            public bool TryLoad(out GameSaveData saveData)
            {
                saveData = source;
                return source != null;
            }

            public void Save(GameSaveData saveData)
            {
                SaveCount++;
                LastSaved = saveData;
            }
        }
    }
}
