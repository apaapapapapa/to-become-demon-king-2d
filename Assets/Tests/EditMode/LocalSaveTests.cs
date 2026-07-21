using System;
using System.Collections.Generic;
using System.IO;
using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Quests;
using DemonKing.Domain.Save;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Quests;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.EditMode
{
    public sealed class LocalSaveTests
    {
        [Test]
        public void GameSaveData_Version2をVersion3へ移行する()
        {
            var saveData = new GameSaveData
            {
                version = 2,
                player = new PlayerSaveData
                {
                    characterDefinitionId = "character.player.slime",
                    abilityLoadout = null
                },
                quests = null,
                world = null
            };

            GameSaveData migrated = GameSaveDataMigrator.MigrateToCurrent(saveData);

            Assert.That(migrated.version, Is.EqualTo(GameSaveData.CurrentVersion));
            Assert.That(migrated.player.abilityLoadout, Is.Not.Null);
            Assert.That(migrated.player.abilityLoadout.slots, Is.Empty);
            Assert.That(migrated.quests, Is.Not.Null.And.Empty);
            Assert.That(migrated.world, Is.Not.Null);
            Assert.That(migrated.world.consumedProgressionGrantIds, Is.Empty);
        }

        [Test]
        public void JsonFileSaveService_GameSaveDataを実ファイルへ保存して復元できる()
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "demon-king-save-tests",
                Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "save.json");

            try
            {
                var service = new JsonFileSaveService(path);
                var saveData = new GameSaveData
                {
                    player = new PlayerSaveData
                    {
                        characterDefinitionId = "character.player.slime",
                        level = 3,
                        currentExperience = 42,
                        abilityLoadout = new AbilityLoadoutSaveData
                        {
                            slots = new List<AbilitySlotSaveData>
                            {
                                new AbilitySlotSaveData
                                {
                                    slot = (int)AbilitySlot.Action3,
                                    abilityId = "ability.magic.fire_bolt"
                                }
                            }
                        }
                    },
                    quests = new List<QuestProgressSaveData>
                    {
                        new QuestProgressSaveData
                        {
                            questId = "quest.training.first",
                            status = (int)QuestProgressStatus.Active
                        }
                    },
                    world = new WorldSaveData
                    {
                        consumedProgressionGrantIds = new List<string>
                        {
                            "grant.field.arcane_grimoire"
                        }
                    }
                };

                service.Save(saveData);

                Assert.That(File.Exists(path), Is.True);
                Assert.That(service.TryLoad(out GameSaveData restored), Is.True);
                Assert.That(restored.version, Is.EqualTo(GameSaveData.CurrentVersion));
                Assert.That(restored.player.level, Is.EqualTo(3));
                Assert.That(restored.player.currentExperience, Is.EqualTo(42));
                Assert.That(restored.player.abilityLoadout.slots.Count, Is.EqualTo(1));
                Assert.That(restored.quests.Count, Is.EqualTo(1));
                Assert.That(
                    restored.world.consumedProgressionGrantIds,
                    Is.EqualTo(new[] { "grant.field.arcane_grimoire" }));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        [Test]
        public void ProgressionGrantConsumptionState_消費済みGrantを復元して冪等に扱う()
        {
            ProgressionGrantConsumptionState state = ProgressionGrantConsumptionState.Restore(
                new[] { "grant.field.arcane_grimoire" });

            Assert.That(state.IsConsumed("grant.field.arcane_grimoire"), Is.True);
            Assert.That(state.TryConsume("grant.field.arcane_grimoire"), Is.False);
            Assert.That(state.TryConsume("grant.field.mana_crystal"), Is.True);
            Assert.That(state.ConsumedGrantIds.Count, Is.EqualTo(2));
        }

        [Test]
        public void AbilityLoadoutSaveMapper_取得済みAbilityの編集枠を復元できる()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                projectAssets.PlayerCharacter.CharacterId);
            Assert.That(state.TryLearnArt("art.magic.fire", out _), Is.True);

            GameObject sourceObject = new("Loadout Save Source");
            GameObject restoredObject = new("Loadout Save Restored");
            try
            {
                AbilityLoadoutController source = sourceObject.AddComponent<AbilityLoadoutController>();
                source.Initialize(projectAssets.PlayerCharacter, state);
                source.Clear(AbilitySlot.Action1);
                source.Assign(AbilitySlot.Action3, "ability.magic.fire_bolt");

                AbilityLoadoutSaveData saveData = AbilityLoadoutSaveMapper.ToSaveData(source.Loadout);

                AbilityLoadoutController restored =
                    restoredObject.AddComponent<AbilityLoadoutController>();
                restored.Initialize(projectAssets.PlayerCharacter, state);
                AbilityLoadoutSaveMapper.ApplySavedAssignments(
                    restored,
                    saveData,
                    projectAssets.PlayerCharacter,
                    state);

                Assert.That(restored.TryResolve(AbilitySlot.Action1, out _), Is.False);
                Assert.That(
                    restored.TryResolve(AbilitySlot.Action3, out string restoredAbilityId),
                    Is.True);
                Assert.That(restoredAbilityId, Is.EqualTo("ability.magic.fire_bolt"));
            }
            finally
            {
                Object.DestroyImmediate(sourceObject);
                Object.DestroyImmediate(restoredObject);
            }
        }

        [Test]
        public void QuestProgressSaveMapper_Quest状態とObjective進捗を復元できる()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            var service = new QuestProgressionService(projectAssets.QuestDefinitions);
            string questId = projectAssets.QuestDefinitions[0].QuestId;

            Assert.That(service.AcceptQuest(questId), Is.True);
            Assert.That(service.TryGetState(questId, out QuestProgressState sourceState), Is.True);

            ObjectiveProgressState sourceObjective = null;
            foreach (ObjectiveProgressState objective in sourceState.Objectives)
            {
                sourceObjective = objective;
                break;
            }

            Assert.That(sourceObjective, Is.Not.Null);
            sourceObjective.AddProgress(1);

            List<QuestProgressSaveData> saveData =
                QuestProgressSaveMapper.ToSaveData(service.States);
            IReadOnlyList<QuestProgressState> restoredStates =
                QuestProgressSaveMapper.FromSaveData(
                    projectAssets.QuestDefinitions,
                    saveData);
            var restoredService = new QuestProgressionService(projectAssets.QuestDefinitions);
            restoredService.Restore(restoredStates);

            Assert.That(restoredService.TryGetState(questId, out QuestProgressState restored), Is.True);
            Assert.That(restored.Status, Is.EqualTo(sourceState.Status));
            Assert.That(
                restored.TryGetObjective(
                    sourceObjective.ObjectiveId,
                    out ObjectiveProgressState restoredObjective),
                Is.True);
            Assert.That(restoredObjective.CurrentCount, Is.EqualTo(sourceObjective.CurrentCount));
        }
    }
}
