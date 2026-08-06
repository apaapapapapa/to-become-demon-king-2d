using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using DemonKing.Domain.Events;
using DemonKing.Domain.Story;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    public sealed class PrototypePrologueFlowPlayModeTests
    {
        [UnityTest]
        public IEnumerator PrologueFlow_会話後に探索と戦闘が解放され帰還会話でPart1を完了する()
        {
            GameObject world = new("Prologue Flow Test");
            GameObject player = new("Prologue Player");
            player.transform.SetParent(world.transform, false);

            var dialogueLog = new DialogueLog();
            GameObject guardianObject = new("Guardian");
            guardianObject.transform.SetParent(world.transform, false);
            PrototypeNpcInteractable guardian =
                guardianObject.AddComponent<PrototypeNpcInteractable>();
            guardian.ConfigureDialogueLog(dialogueLog);
            guardian.ConfigureDialogue(PrototypePrologueContent.GuardianIntroDialogue);

            StoryProgressState storyState =
                StoryProgressState.CreateInitial(PrototypeStoryDefinitions.PrologueChapterId);
            var storyService = new StoryProgressionService(
                storyState,
                PrototypePrologueContent.CreateStoryEvents());
            var eventHub = new GameplayEventHub();
            eventHub.Published += gameplayEvent => storyService.Handle(gameplayEvent);

            MonoBehaviour flow = AddInternalComponent(
                world,
                "DemonKing.Field.Prototype.PrototypePrologueFlowController");
            InvokeInitialize(
                flow,
                world.transform,
                player,
                guardian,
                dialogueLog,
                eventHub,
                storyService,
                null);

            Assert.That(world.transform.Find("赤い木の実"), Is.Null);
            Assert.That(world.transform.Find("森の幼獣"), Is.Null);

            GameObject interactor = new("Interactor");
            for (int index = 0; index < 8 && !storyState.HasFlag(PrototypeStoryDefinitions.MetGuardianFlagId); index++)
            {
                guardian.Interact(interactor);
                yield return null;
            }

            Assert.That(storyState.HasFlag(PrototypeStoryDefinitions.MetGuardianFlagId), Is.True);
            Transform forageTransform = world.transform.Find("赤い木の実");
            Transform creatureTransform = world.transform.Find("森の幼獣");
            Assert.That(forageTransform, Is.Not.Null);
            Assert.That(creatureTransform, Is.Not.Null);

            IInteractable forage = forageTransform
                .GetComponents<MonoBehaviour>()
                .OfType<IInteractable>()
                .Single();
            forage.Interact(interactor);
            yield return null;

            Health creatureHealth = creatureTransform.GetComponent<Health>();
            creatureHealth.ApplyDamage(new DamageRequest(999));
            yield return null;

            Assert.That(storyState.HasFlag(PrototypeStoryDefinitions.FoundFoodFlagId), Is.True);
            Assert.That(storyState.HasFlag(PrototypeStoryDefinitions.FirstHuntFlagId), Is.True);
            Assert.That(
                guardian.DialogueId,
                Is.EqualTo(PrototypePrologueContent.GuardianCompleteDialogueId));

            for (int index = 0; index < 8 && !storyState.HasFlag(PrototypeStoryDefinitions.ProloguePart1CompletedFlagId); index++)
            {
                guardian.Interact(interactor);
                yield return null;
            }

            Assert.That(
                storyState.HasFlag(PrototypeStoryDefinitions.ProloguePart1CompletedFlagId),
                Is.True);
            Assert.That(
                guardian.DialogueId,
                Is.EqualTo(PrototypePrologueContent.GuardianAfterDialogueId));

            Object.Destroy(interactor);
            Object.Destroy(world);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PrologueFlow_Save復元済みFlagから未達目標だけ再生成する()
        {
            GameObject world = new("Prologue Restore Test");
            GameObject player = new("Prologue Restore Player");
            player.transform.SetParent(world.transform, false);

            var dialogueLog = new DialogueLog();
            GameObject guardianObject = new("Guardian");
            guardianObject.transform.SetParent(world.transform, false);
            PrototypeNpcInteractable guardian =
                guardianObject.AddComponent<PrototypeNpcInteractable>();
            guardian.ConfigureDialogueLog(dialogueLog);

            StoryProgressState storyState = StoryProgressState.Restore(
                PrototypeStoryDefinitions.PrologueChapterId,
                new[]
                {
                    PrototypeStoryDefinitions.BornFlagId,
                    PrototypeStoryDefinitions.MetGuardianFlagId,
                    PrototypeStoryDefinitions.FoundFoodFlagId
                },
                Array.Empty<string>());
            var storyService = new StoryProgressionService(
                storyState,
                PrototypePrologueContent.CreateStoryEvents());
            var eventHub = new GameplayEventHub();
            eventHub.Published += gameplayEvent => storyService.Handle(gameplayEvent);

            MonoBehaviour flow = AddInternalComponent(
                world,
                "DemonKing.Field.Prototype.PrototypePrologueFlowController");
            InvokeInitialize(
                flow,
                world.transform,
                player,
                guardian,
                dialogueLog,
                eventHub,
                storyService,
                null);

            yield return null;

            Assert.That(world.transform.Find("赤い木の実"), Is.Null);
            Assert.That(world.transform.Find("森の幼獣"), Is.Not.Null);
            Assert.That(
                guardian.DialogueId,
                Is.EqualTo(PrototypePrologueContent.GuardianObjectiveDialogueId));

            Object.Destroy(world);
            yield return null;
        }

        private static MonoBehaviour AddInternalComponent(GameObject target, string typeName)
        {
            Type type = typeof(PrototypeNpcInteractable).Assembly.GetType(typeName, throwOnError: true);
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
