using DemonKing.Domain.Quests;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    public sealed class QuestProgressionServiceTests
    {
        [Test]
        public void QuestProgressionService_受注後にObjective達成し報告完了でQuestを完了する()
        {
            QuestDefinition quest = QuestDefinition.CreateRuntime(
                "quest.test.defeat",
                "討伐テスト",
                QuestObjectiveDefinition.CreateRuntime(
                    "objective.test.defeat",
                    GameplayEventIds.EnemyDefeated,
                    "character.training_dummy",
                    2,
                    "訓練対象を倒す"));
            var eventHub = new GameplayEventHub();
            var service = new QuestProgressionService(new[] { quest });
            eventHub.Published += service.Handle;
            int acceptedCount = 0;
            int readyCount = 0;
            int completedCount = 0;
            service.QuestAccepted += _ => acceptedCount++;
            service.QuestReadyToTurnIn += _ => readyCount++;
            service.QuestCompleted += _ => completedCount++;

            service.TryGetState("quest.test.defeat", out QuestProgressState state);
            state.TryGetObjective("objective.test.defeat", out ObjectiveProgressState objective);

            eventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));
            Assert.That(objective.CurrentCount, Is.EqualTo(0));
            Assert.That(state.Status, Is.EqualTo(QuestProgressStatus.Available));

            Assert.That(service.AcceptQuest("quest.test.defeat"), Is.True);
            Assert.That(service.AcceptQuest("quest.test.defeat"), Is.False);
            Assert.That(acceptedCount, Is.EqualTo(1));
            Assert.That(state.Status, Is.EqualTo(QuestProgressStatus.Active));

            eventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.other"));
            Assert.That(objective.CurrentCount, Is.EqualTo(0));

            eventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));
            Assert.That(objective.CurrentCount, Is.EqualTo(1));
            Assert.That(state.IsActive, Is.True);

            eventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));
            Assert.That(objective.CurrentCount, Is.EqualTo(2));
            Assert.That(state.IsReadyToTurnIn, Is.True);
            Assert.That(state.IsCompleted, Is.False);
            Assert.That(readyCount, Is.EqualTo(1));
            Assert.That(completedCount, Is.EqualTo(0));

            eventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));
            Assert.That(readyCount, Is.EqualTo(1));
            Assert.That(objective.CurrentCount, Is.EqualTo(2));

            Assert.That(service.CompleteQuest("quest.test.defeat"), Is.True);
            Assert.That(service.CompleteQuest("quest.test.defeat"), Is.False);
            Assert.That(state.Status, Is.EqualTo(QuestProgressStatus.Completed));
            Assert.That(completedCount, Is.EqualTo(1));

            Object.DestroyImmediate(quest);
        }
    }
}
