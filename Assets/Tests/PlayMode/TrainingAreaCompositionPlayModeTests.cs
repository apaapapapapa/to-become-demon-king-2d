using System;
using System.Collections;
using System.Reflection;
using DemonKing.Field.Prototype;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    public sealed class TrainingAreaCompositionPlayModeTests
    {
        [UnityTest]
        public IEnumerator TrainingDummyEventBridge_DefeatをGameplayEventへ変換する()
        {
            GameObject world = new("Training Dummy Bridge Test");
            var lifecycle = new SpawnLifecycle<PrototypeCombatDummy>(
                () =>
                {
                    GameObject dummyObject = new("Training Dummy");
                    dummyObject.transform.SetParent(world.transform, false);
                    return dummyObject.AddComponent<PrototypeCombatDummy>();
                },
                dummy => dummy != null && dummy.IsAlive,
                dummy => dummy.RestoreToFull());
            var eventHub = new GameplayEventHub();
            int publishedCount = 0;
            eventHub.Published += _ => publishedCount++;

            MonoBehaviour bridge = AddInternalComponent(world, "DemonKing.Field.Prototype.TrainingDummyEventBridge");
            InvokeInitialize(bridge, lifecycle, eventHub);
            PrototypeCombatDummy dummy = lifecycle.SpawnOrRestore();

            yield return null;

            Health health = dummy.GetComponent<Health>();
            health.ApplyDamage(new DamageRequest(999));
            yield return null;

            Assert.That(publishedCount, Is.EqualTo(1));
            Assert.That(lifecycle.Current, Is.Null);

            Object.Destroy(world);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TrainingQuestFlowController_依頼会話完了で受注し次回は進行中会話を選ぶ()
        {
            TrainingScenarioDefinition scenario =
                Resources.Load<TrainingScenarioDefinition>("Settings/Gameplay/TrainingScenario");
            Assert.That(scenario, Is.Not.Null);
            Assert.That(scenario.IsConfigured, Is.True);

            GameObject world = new("Training Quest Flow Test");
            GameObject npcObject = new("Training Quest NPC");
            npcObject.transform.SetParent(world.transform, false);
            PrototypeNpcInteractable npc = npcObject.AddComponent<PrototypeNpcInteractable>();
            var dialogueLog = new DialogueLog();
            npc.ConfigureDialogueLog(dialogueLog);
            npc.ConfigureDialogue(scenario.OfferDialogue);

            var lifecycle = new SpawnLifecycle<PrototypeCombatDummy>(
                () =>
                {
                    GameObject dummyObject = new("Training Quest Dummy");
                    dummyObject.transform.SetParent(world.transform, false);
                    return dummyObject.AddComponent<PrototypeCombatDummy>();
                },
                dummy => dummy != null && dummy.IsAlive,
                dummy => dummy.RestoreToFull());
            var eventHub = new GameplayEventHub();
            var questService = new QuestProgressionService(new[] { scenario.QuestDefinition });

            MonoBehaviour flow = AddInternalComponent(world, "DemonKing.Field.Prototype.TrainingQuestFlowController");
            InvokeInitialize(
                flow,
                npc,
                lifecycle,
                null,
                dialogueLog,
                eventHub,
                questService,
                scenario);

            GameObject interactor = new("Training Quest Interactor");
            for (int index = 0; index < 12; index++)
            {
                npc.Interact(interactor);
                yield return null;

                questService.TryGetState(scenario.QuestDefinition.QuestId, out var state);
                if (state.IsActive)
                {
                    break;
                }
            }

            questService.TryGetState(scenario.QuestDefinition.QuestId, out var activeState);
            Assert.That(activeState.IsActive, Is.True);

            npc.Interact(interactor);
            yield return null;

            Assert.That(npc.DialogueId, Is.EqualTo(scenario.ActiveDialogue.DialogueId));

            Object.Destroy(interactor);
            Object.Destroy(world);
            yield return null;
        }

        private static MonoBehaviour AddInternalComponent(GameObject target, string typeName)
        {
            Type type = typeof(PrototypeCombatDummy).Assembly.GetType(typeName, throwOnError: true);
            return (MonoBehaviour)target.AddComponent(type);
        }

        private static void InvokeInitialize(MonoBehaviour component, params object[] arguments)
        {
            MethodInfo initialize = component.GetType().GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(initialize, Is.Not.Null);
            initialize.Invoke(component, arguments);
        }
    }
}
