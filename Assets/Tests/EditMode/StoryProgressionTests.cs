using System.Collections.Generic;
using DemonKing.Core.Application;
using DemonKing.Domain.Events;
using DemonKing.Domain.Quests;
using DemonKing.Domain.Save;
using DemonKing.Domain.Story;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    public sealed class StoryProgressionTests
    {
        [Test]
        public void StoryProgressionService_Flag条件を満たすEventを一度だけ実行する()
        {
            StoryProgressState state = StoryProgressState.CreateInitial("story.chapter.prologue");
            state.SetFlag("prologue.met_human");
            var definition = new StoryEventDefinition(
                "story.event.leave_forest",
                GameplayEventIds.FieldEntered,
                "field.town",
                requiredFlags: new[] { "prologue.met_human" },
                setFlags: new[] { "prologue.left_forest" },
                nextChapterId: "story.chapter.journey");
            var service = new StoryProgressionService(state, new[] { definition });
            int executedCount = 0;
            service.EventExecuted += _ => executedCount++;

            bool first = service.Handle(new GameplayEvent(
                GameplayEventIds.FieldEntered,
                "field.town"));
            bool second = service.Handle(new GameplayEvent(
                GameplayEventIds.FieldEntered,
                "field.town"));

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(state.HasFlag("prologue.left_forest"), Is.True);
            Assert.That(state.CurrentChapterId, Is.EqualTo("story.chapter.journey"));
            Assert.That(state.WasEventExecuted("story.event.leave_forest"), Is.True);
            Assert.That(executedCount, Is.EqualTo(1));
        }

        [Test]
        public void StoryProgressionService_必須FlagがないEventを実行しない()
        {
            StoryProgressState state = StoryProgressState.CreateInitial("story.chapter.prologue");
            var service = new StoryProgressionService(
                state,
                new[]
                {
                    new StoryEventDefinition(
                        "story.event.ruins",
                        GameplayEventIds.FieldEntered,
                        "field.ruins",
                        requiredFlags: new[] { "prologue.left_forest" },
                        setFlags: new[] { "prologue.found_ruins" })
                });

            Assert.That(
                service.Handle(new GameplayEvent(GameplayEventIds.FieldEntered, "field.ruins")),
                Is.False);
            Assert.That(state.HasFlag("prologue.found_ruins"), Is.False);
            Assert.That(state.WasEventExecuted("story.event.ruins"), Is.False);
        }

        [Test]
        public void StorySaveMapper_ChapterFlag実行済みEventをRoundTripする()
        {
            StoryProgressState state = StoryProgressState.CreateInitial("story.chapter.prologue");
            var service = new StoryProgressionService(
                state,
                new[]
                {
                    new StoryEventDefinition(
                        "story.event.test",
                        GameplayEventIds.DialogueCompleted,
                        setFlags: new[] { "story.flag.test" },
                        nextChapterId: "story.chapter.next")
                });
            service.Handle(new GameplayEvent(GameplayEventIds.DialogueCompleted, "dialogue.test"));

            StorySaveData saveData = StoryProgressionSaveMapper.ToSaveData(state);
            StoryProgressState restored = StoryProgressionSaveMapper.FromSaveData(
                saveData,
                "story.chapter.fallback");

            Assert.That(restored.CurrentChapterId, Is.EqualTo("story.chapter.next"));
            Assert.That(restored.HasFlag("story.flag.test"), Is.True);
            Assert.That(restored.WasEventExecuted("story.event.test"), Is.True);
        }

        [Test]
        public void SaveVersion4_StoryなしSaveをVersion5へMigrationできる()
        {
            var saveData = new GameSaveData
            {
                version = 4,
                story = null
            };

            GameSaveData migrated = GameSaveDataMigrator.MigrateToCurrent(saveData);

            Assert.That(migrated.version, Is.EqualTo(5));
            Assert.That(migrated.story, Is.Not.Null);
            Assert.That(migrated.story.flags, Is.Empty);
            Assert.That(migrated.story.executedEventIds, Is.Empty);
        }

        [Test]
        public void StoryDialogueSelector_StoryFlagとQuestStatusで同一NPCのDialogue候補を切り替える()
        {
            StoryProgressState state = StoryProgressState.CreateInitial("story.chapter.prologue");
            state.SetFlag("prologue.left_forest");
            var variants = new[]
            {
                new StoryDialogueVariant<string>(
                    "after-return-active-quest",
                    requiredFlags: new[] { "prologue.left_forest" },
                    questId: "quest.training",
                    requiredQuestStatus: QuestProgressStatus.Active),
                new StoryDialogueVariant<string>(
                    "after-return",
                    requiredFlags: new[] { "prologue.left_forest" })
            };

            string active = StoryDialogueSelector.Select(
                "default",
                state,
                variants,
                questId => questId == "quest.training"
                    ? QuestProgressStatus.Active
                    : null);
            string completed = StoryDialogueSelector.Select(
                "default",
                state,
                variants,
                questId => questId == "quest.training"
                    ? QuestProgressStatus.Completed
                    : null);

            Assert.That(active, Is.EqualTo("after-return-active-quest"));
            Assert.That(completed, Is.EqualTo("after-return"));
        }

        [Test]
        public void GameplayEventHub_同じEventをQuestとStoryへ配信できる()
        {
            QuestDefinition quest = QuestDefinition.CreateRuntime(
                "quest.story.shared",
                "共有Eventテスト",
                QuestObjectiveDefinition.CreateRuntime(
                    "objective.story.shared",
                    GameplayEventIds.EnemyDefeated,
                    "enemy.story.test",
                    1,
                    "対象を倒す"));
            var questService = new QuestProgressionService(new[] { quest });
            questService.AcceptQuest(quest.QuestId);

            StoryProgressState storyState = StoryProgressState.CreateInitial("story.chapter.prologue");
            var storyService = new StoryProgressionService(
                storyState,
                new[]
                {
                    new StoryEventDefinition(
                        "story.event.shared",
                        GameplayEventIds.EnemyDefeated,
                        "enemy.story.test",
                        setFlags: new[] { "story.flag.shared" })
                });
            var hub = new GameplayEventHub();
            hub.Published += questService.Handle;
            hub.Published += gameplayEvent => storyService.Handle(gameplayEvent);

            hub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "enemy.story.test"));

            Assert.That(
                questService.TryGetState(quest.QuestId, out QuestProgressState questState),
                Is.True);
            Assert.That(questState.IsReadyToTurnIn, Is.True);
            Assert.That(storyState.HasFlag("story.flag.shared"), Is.True);

            Object.DestroyImmediate(quest);
        }
    }
}
