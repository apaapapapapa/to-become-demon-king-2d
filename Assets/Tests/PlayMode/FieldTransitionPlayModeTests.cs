using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Domain.Quests;
using DemonKing.Domain.Save;
using DemonKing.Field.Composition;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    public sealed class FieldTransitionPlayModeTests
    {
        [UnityTest]
        public IEnumerator FieldTransition_FieldAからBを往復してSession状態とSaveを保持する()
        {
            PrototypeProjectAssets projectAssets = LoadProjectAssets();
            GameSaveData initialSave = CreateProgressedSave(
                projectAssets,
                new FieldLocation(
                    PrototypeFieldDefinition.DefaultFieldId,
                    PrototypeFieldDefinition.DefaultEntryPointId));
            var saveService = new MemorySaveService(initialSave);
            Scene testRunnerScene = SceneManager.GetActiveScene();
            Scene bootstrapScene = CreateBootstrapScene();
            GameObject applicationRoot = null;

            try
            {
                applicationRoot = new PrototypeApplicationInstaller(projectAssets, saveService).Install();
                Assert.That(applicationRoot, Is.Not.Null);
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(PrototypeFieldDefinition.DefaultSceneName));

                PrototypeFieldTransitionService transitionService =
                    applicationRoot.GetComponent<PrototypeFieldTransitionService>();
                Assert.That(transitionService, Is.Not.Null);

                var secondaryLocation = new FieldLocation(
                    PrototypeFieldDefinition.SecondaryFieldId,
                    PrototypeFieldDefinition.SecondaryEntryPointId);
                Assert.That(transitionService.TryTransition(secondaryLocation), Is.True);
                while (transitionService.IsTransitioning)
                {
                    yield return null;
                }

                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(PrototypeFieldDefinition.SecondarySceneName));
                AssertStatePreserved(projectAssets, saveService.LastSaved, secondaryLocation);
                AssertPlayerLoadoutPreserved(projectAssets);

                var returnLocation = new FieldLocation(
                    PrototypeFieldDefinition.DefaultFieldId,
                    PrototypeFieldDefinition.ReturnFromSecondaryEntryPointId);
                Assert.That(transitionService.TryTransition(returnLocation), Is.True);
                while (transitionService.IsTransitioning)
                {
                    yield return null;
                }

                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(PrototypeFieldDefinition.DefaultSceneName));
                AssertStatePreserved(projectAssets, saveService.LastSaved, returnLocation);
                AssertPlayerLoadoutPreserved(projectAssets);
            }
            finally
            {
                if (applicationRoot != null)
                {
                    Object.Destroy(applicationRoot);
                }
            }

            yield return CleanupFieldScenes(testRunnerScene, bootstrapScene);
        }

        [UnityTest]
        public IEnumerator Continue_SaveされたFieldBのEntryPointからRuntimeを復元する()
        {
            PrototypeProjectAssets projectAssets = LoadProjectAssets();
            var secondaryLocation = new FieldLocation(
                PrototypeFieldDefinition.SecondaryFieldId,
                PrototypeFieldDefinition.SecondaryEntryPointId);
            var saveService = new MemorySaveService(
                CreateProgressedSave(projectAssets, secondaryLocation));
            Scene testRunnerScene = SceneManager.GetActiveScene();
            Scene bootstrapScene = CreateBootstrapScene();
            GameObject applicationRoot = null;

            try
            {
                applicationRoot = new PrototypeApplicationInstaller(projectAssets, saveService).Install();
                Assert.That(applicationRoot, Is.Not.Null);
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(PrototypeFieldDefinition.SecondarySceneName));
                AssertStatePreserved(projectAssets, saveService.LastSaved, secondaryLocation);

                PlayerInputReader inputReader =
                    PrototypeFieldSceneRuntime.FindInActiveScene<PlayerInputReader>();
                Assert.That(inputReader, Is.Not.Null);

                PrototypeFieldCatalog catalog = PrototypeFieldCatalog.CreateInitial(
                    projectAssets.ApplicationSettings,
                    projectAssets);
                Assert.That(
                    catalog.TryResolve(
                        secondaryLocation,
                        out _,
                        out FieldEntryPoint entryPoint),
                    Is.True);
                Assert.That(
                    Vector3.Distance(inputReader.transform.position, entryPoint.Position),
                    Is.LessThan(0.001f));
            }
            finally
            {
                if (applicationRoot != null)
                {
                    Object.Destroy(applicationRoot);
                }
            }

            yield return CleanupFieldScenes(testRunnerScene, bootstrapScene);
        }

        private static Scene CreateBootstrapScene()
        {
            Scene bootstrapScene = SceneManager.CreateScene(
                "FieldTransitionBootstrap-" + Guid.NewGuid().ToString("N"));
            Assert.That(SceneManager.SetActiveScene(bootstrapScene), Is.True);
            return bootstrapScene;
        }

        private static PrototypeProjectAssets LoadProjectAssets()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            Assert.That(projectAssets, Is.Not.Null);
            Assert.That(projectAssets.PlayerCharacter.ArtDefinitions, Is.Not.Empty);
            Assert.That(projectAssets.PlayerCharacter.SkillDefinitions, Is.Not.Empty);
            Assert.That(projectAssets.PlayerCharacter.EvolutionDefinitions, Is.Not.Empty);
            Assert.That(projectAssets.QuestDefinitions, Is.Not.Empty);
            return projectAssets;
        }

        private static GameSaveData CreateProgressedSave(
            PrototypeProjectAssets projectAssets,
            FieldLocation location)
        {
            var art = projectAssets.PlayerCharacter.ArtDefinitions[0];
            string abilityId = art.AbilityUnlocks
                .First(entry => entry.RequiredRank == 1)
                .AbilityDefinition.AbilityId;
            string skillId = projectAssets.PlayerCharacter.SkillDefinitions[0].SkillId;
            string evolutionId = projectAssets.PlayerCharacter.EvolutionDefinitions[0].EvolutionNodeId;
            string questId = projectAssets.QuestDefinitions[0].QuestId;

            return new GameSaveData
            {
                version = GameSaveData.CurrentVersion,
                player = new PlayerSaveData
                {
                    characterDefinitionId = projectAssets.PlayerCharacter.CharacterId,
                    level = 5,
                    currentExperience = 7,
                    artProgress = new List<ArtProgressSaveData>
                    {
                        new()
                        {
                            artId = art.ArtId,
                            masteryPoints = 0
                        }
                    },
                    unlockedSkillIds = new List<string> { skillId },
                    unlockedEvolutionNodeIds = new List<string> { evolutionId },
                    abilityLoadout = new AbilityLoadoutSaveData
                    {
                        slots = new List<AbilitySlotSaveData>
                        {
                            new()
                            {
                                slot = (int)AbilitySlot.Action3,
                                abilityId = abilityId
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
                world = new WorldSaveData
                {
                    currentFieldId = location.FieldId,
                    entryPointId = location.EntryPointId
                }
            };
        }

        private static void AssertStatePreserved(
            PrototypeProjectAssets projectAssets,
            GameSaveData saveData,
            FieldLocation expectedLocation)
        {
            Assert.That(saveData, Is.Not.Null);
            Assert.That(saveData.player.level, Is.EqualTo(5));
            Assert.That(saveData.player.currentExperience, Is.EqualTo(7));
            Assert.That(saveData.player.artProgress, Has.Count.EqualTo(1));
            Assert.That(
                saveData.player.artProgress[0].artId,
                Is.EqualTo(projectAssets.PlayerCharacter.ArtDefinitions[0].ArtId));
            Assert.That(
                saveData.player.unlockedSkillIds,
                Does.Contain(projectAssets.PlayerCharacter.SkillDefinitions[0].SkillId));
            Assert.That(
                saveData.player.unlockedEvolutionNodeIds,
                Does.Contain(projectAssets.PlayerCharacter.EvolutionDefinitions[0].EvolutionNodeId));
            Assert.That(saveData.player.abilityLoadout.slots, Has.Count.EqualTo(1));
            Assert.That(saveData.quests, Has.Count.EqualTo(1));
            Assert.That(saveData.quests[0].status, Is.EqualTo((int)QuestProgressStatus.Active));
            Assert.That(saveData.world.currentFieldId, Is.EqualTo(expectedLocation.FieldId));
            Assert.That(saveData.world.entryPointId, Is.EqualTo(expectedLocation.EntryPointId));
        }

        private static void AssertPlayerLoadoutPreserved(PrototypeProjectAssets projectAssets)
        {
            PlayerInputReader inputReader =
                PrototypeFieldSceneRuntime.FindInActiveScene<PlayerInputReader>();
            Assert.That(inputReader, Is.Not.Null);
            AbilityLoadoutController loadout = inputReader.GetComponent<AbilityLoadoutController>();
            Assert.That(loadout, Is.Not.Null);
            Assert.That(loadout.TryResolve(AbilitySlot.Action3, out string abilityId), Is.True);

            string expectedAbilityId = projectAssets.PlayerCharacter.ArtDefinitions[0]
                .AbilityUnlocks.First(entry => entry.RequiredRank == 1)
                .AbilityDefinition.AbilityId;
            Assert.That(abilityId, Is.EqualTo(expectedAbilityId));
        }

        private static IEnumerator CleanupFieldScenes(Scene testRunnerScene, Scene bootstrapScene)
        {
            yield return null;

            if (testRunnerScene.IsValid() && testRunnerScene.isLoaded)
            {
                SceneManager.SetActiveScene(testRunnerScene);
            }

            yield return UnloadIfLoaded(PrototypeFieldDefinition.DefaultSceneName, testRunnerScene);
            yield return UnloadIfLoaded(PrototypeFieldDefinition.SecondarySceneName, testRunnerScene);

            if (bootstrapScene.IsValid() && bootstrapScene.isLoaded)
            {
                AsyncOperation bootstrapUnload = SceneManager.UnloadSceneAsync(bootstrapScene);
                if (bootstrapUnload != null)
                {
                    yield return bootstrapUnload;
                }
            }
        }

        private static IEnumerator UnloadIfLoaded(string sceneName, Scene preservedScene)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded || scene.handle == preservedScene.handle)
            {
                yield break;
            }

            AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);
            if (operation != null)
            {
                yield return operation;
            }
        }

        private sealed class MemorySaveService : ISaveService
        {
            private GameSaveData source;

            public MemorySaveService(GameSaveData source)
            {
                this.source = source;
            }

            public GameSaveData LastSaved { get; private set; }

            public bool TryLoad(out GameSaveData saveData)
            {
                saveData = source;
                return source != null;
            }

            public void Save(GameSaveData saveData)
            {
                LastSaved = saveData;
                source = saveData;
            }
        }
    }
}
