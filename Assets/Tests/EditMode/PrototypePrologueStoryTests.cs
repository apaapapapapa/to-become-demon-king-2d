using DemonKing.Domain.Events;
using DemonKing.Domain.Story;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Events;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    public sealed class PrototypePrologueStoryTests
    {
        [Test]
        public void PrologueStoryEvents_誕生からPart1完了までFlagを順番に進行する()
        {
            StoryProgressState state =
                StoryProgressState.CreateInitial(PrototypeStoryDefinitions.PrologueChapterId);
            var service = new StoryProgressionService(
                state,
                PrototypePrologueContent.CreateStoryEvents());

            service.Handle(new GameplayEvent(
                GameplayEventIds.FieldEntered,
                PrototypeFieldDefinition.PrologueFieldId));

            Assert.That(state.HasFlag(PrototypeStoryDefinitions.BornFlagId), Is.True);

            service.Handle(new GameplayEvent(
                GameplayEventIds.InteractionCompleted,
                PrototypePrologueContent.ForageInteractionId));
            Assert.That(state.HasFlag(PrototypeStoryDefinitions.FoundFoodFlagId), Is.False);

            service.Handle(new GameplayEvent(
                GameplayEventIds.DialogueCompleted,
                PrototypePrologueContent.GuardianIntroDialogueId));
            Assert.That(state.HasFlag(PrototypeStoryDefinitions.MetGuardianFlagId), Is.True);

            service.Handle(new GameplayEvent(
                GameplayEventIds.InteractionCompleted,
                PrototypePrologueContent.ForageInteractionId));
            service.Handle(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                PrototypePrologueContent.ForestCreatureActorId));

            Assert.That(state.HasFlag(PrototypeStoryDefinitions.FoundFoodFlagId), Is.True);
            Assert.That(state.HasFlag(PrototypeStoryDefinitions.FirstHuntFlagId), Is.True);

            service.Handle(new GameplayEvent(
                GameplayEventIds.DialogueCompleted,
                PrototypePrologueContent.GuardianCompleteDialogueId));

            Assert.That(
                state.HasFlag(PrototypeStoryDefinitions.ProloguePart1CompletedFlagId),
                Is.True);
            Assert.That(
                state.CurrentChapterId,
                Is.EqualTo(PrototypeStoryDefinitions.PrologueChapterId));
        }

        [Test]
        public void GuardianDialogueSelector_Save復元済みFlagから会話を再開する()
        {
            StoryProgressState objectiveState = StoryProgressState.Restore(
                PrototypeStoryDefinitions.PrologueChapterId,
                new[] { PrototypeStoryDefinitions.MetGuardianFlagId },
                System.Array.Empty<string>());
            StoryProgressState completionState = StoryProgressState.Restore(
                PrototypeStoryDefinitions.PrologueChapterId,
                new[]
                {
                    PrototypeStoryDefinitions.MetGuardianFlagId,
                    PrototypeStoryDefinitions.FoundFoodFlagId,
                    PrototypeStoryDefinitions.FirstHuntFlagId
                },
                System.Array.Empty<string>());
            StoryProgressState completedState = StoryProgressState.Restore(
                PrototypeStoryDefinitions.PrologueChapterId,
                new[]
                {
                    PrototypeStoryDefinitions.MetGuardianFlagId,
                    PrototypeStoryDefinitions.FoundFoodFlagId,
                    PrototypeStoryDefinitions.FirstHuntFlagId,
                    PrototypeStoryDefinitions.ProloguePart1CompletedFlagId
                },
                System.Array.Empty<string>());

            Assert.That(
                PrototypePrologueContent.SelectGuardianDialogue(objectiveState).DialogueId,
                Is.EqualTo(PrototypePrologueContent.GuardianObjectiveDialogueId));
            Assert.That(
                PrototypePrologueContent.SelectGuardianDialogue(completionState).DialogueId,
                Is.EqualTo(PrototypePrologueContent.GuardianCompleteDialogueId));
            Assert.That(
                PrototypePrologueContent.SelectGuardianDialogue(completedState).DialogueId,
                Is.EqualTo(PrototypePrologueContent.GuardianAfterDialogueId));
        }

        [Test]
        public void MetHumanStoryEvent_育ての親Interactionでは発火せず人間NPCで発火する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            StoryProgressState state =
                StoryProgressState.CreateInitial(PrototypeStoryDefinitions.PrologueChapterId);
            var service = new StoryProgressionService(
                state,
                PrototypeStoryDefinitions.Create(projectAssets));

            service.Handle(new GameplayEvent(
                GameplayEventIds.InteractionCompleted,
                PrototypePrologueContent.GuardianIntroDialogueId));

            Assert.That(state.HasFlag(PrototypeStoryDefinitions.MetHumanFlagId), Is.False);

            service.Handle(new GameplayEvent(
                GameplayEventIds.InteractionCompleted,
                projectAssets.ApprenticeMageDialogue.DialogueId));

            Assert.That(state.HasFlag(PrototypeStoryDefinitions.MetHumanFlagId), Is.True);
        }
    }
}
