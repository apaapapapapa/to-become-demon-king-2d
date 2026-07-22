using System.Collections.Generic;
using System.Text.RegularExpressions;
using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Domain.Quests;
using DemonKing.Domain.Save;
using DemonKing.Field.Composition;
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
            var defaultLocation = new FieldLocation(
                PrototypeFieldDefinition.DefaultFieldId,
                PrototypeFieldDefinition.DefaultEntryPointId);

            PrototypeSaveSession saveSession = PrototypeSaveSession.Load(
                saveService,
                projectAssets.PlayerCharacter.CharacterId,
                defaultLocation);

            Assert.That(saveSession.WasLoaded, Is.True);
            Assert.That(saveSession.SavingEnabled, Is.True);
            Assert.That(saveSession.SaveData.version, Is.EqualTo(GameSaveData.CurrentVersion));
            Assert.That(saveSession.ProgressionState.Level, Is.EqualTo(3));
            Assert.That(saveSession.ProgressionState.CurrentExperience, Is.EqualTo(42));
            Assert.That(saveSession.CurrentFieldLocation, Is.EqualTo(defaultLocation));

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
                    saveSession.GrantConsumptionState,
                    saveSession.CurrentFieldLocation);

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
                Assert.That(saveService.LastSaved.world.currentFieldId, Is.EqualTo(defaultLocation.FieldId));
                Assert.That(saveService.LastSaved.world.entryPointId, Is.EqualTo(defaultLocation.EntryPointId));
            }
            finally
            {
                Object.DestroyImmediate(application);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void SaveSession_Version4のFieldLocationをStableIdで復元する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            var savedLocation = new FieldLocation(
                PrototypeFieldDefinition.DefaultFieldId,
                PrototypeFieldDefinition.DefaultEntryPointId);
            var saveService = new MemorySaveService(new GameSaveData
            {
                version = GameSaveData.CurrentVersion,
                player = new PlayerSaveData
                {
                    characterDefinitionId = projectAssets.PlayerCharacter.CharacterId
                },
                world = new WorldSaveData
                {
                    currentFieldId = savedLocation.FieldId,
                    entryPointId = savedLocation.EntryPointId
                }
            });

            PrototypeSaveSession saveSession = PrototypeSaveSession.Load(
                saveService,
                projectAssets.PlayerCharacter.CharacterId,
                new FieldLocation("field.fallback", "entry.fallback"));

            Assert.That(saveSession.CurrentFieldLocation, Is.EqualTo(savedLocation));
        }

        [Test]
        public void GameSaveRestorer_LoadoutとQuestを構築済みRuntimeへ適用する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            string questId = projectAssets.QuestDefinitions[0].QuestId;
            var saveService = new MemorySaveService(new GameSaveData
            {
                version = GameSaveData.CurrentVersion,
                player = new PlayerSaveData
                {
                    characterDefinitionId = projectAssets.PlayerCharacter.CharacterId,
                    artProgress = new List<ArtProgressSaveData>
                    {
                        new()
                        {
                            artId = "art.magic.fire",
                            masteryPoints = 0
                        }
                    },
                    abilityLoadout = new AbilityLoadoutSaveData
                    {
                        slots = new List<AbilitySlotSaveData>
                        {
                            new()
                            {
                                slot = (int)AbilitySlot.Action3,
                                abilityId = "ability.magic.fire_bolt"
                            }
                        }
                    }
                },
                quests = new List<QuestProgressSaveData>
                {
                    new()
                    {
                        questId = questId,
                        status = (int)QuestProgressStatus.Active
                    }
                },
                world = new WorldSaveData()
            });
            PrototypeSaveSession saveSession = PrototypeSaveSession.Load(
                saveService,
                projectAssets.PlayerCharacter.CharacterId);

            GameObject player = new("Save Restore Player");
            try
            {
                AbilityLoadoutController loadout = player.AddComponent<AbilityLoadoutController>();
                loadout.Initialize(projectAssets.PlayerCharacter, saveSession.ProgressionState);
                var questService = new QuestProgressionService(projectAssets.QuestDefinitions);
                var worldResult = new PrototypeWorldBuildResult(
                    worldRoot: null,
                    player,
                    rewardService: null,
                    gameContentCatalog: null,
                    questService);

                new PrototypeGameSaveRestorer(projectAssets).Restore(saveSession, worldResult);

                Assert.That(
                    loadout.TryResolve(AbilitySlot.Action3, out string abilityId),
                    Is.True);
                Assert.That(abilityId, Is.EqualTo("ability.magic.fire_bolt"));
                Assert.That(questService.TryGetState(questId, out var questState), Is.True);
                Assert.That(questState.Status, Is.EqualTo(QuestProgressStatus.Active));
            }
            finally
            {
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
                    saveSession.GrantConsumptionState,
                    saveSession.CurrentFieldLocation);
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
