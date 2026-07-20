using DemonKing.Domain.Quests;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.PlayMode
{
    public sealed class QuestProgressionServiceTests
    {
        [Test]
        public void QuestProgressionService_GameplayEventを条件照合してObjectiveとQuestを完了する()
        {
            QuestDefinition quest = QuestDefinition.CreateRuntime(
                "quest.test.defeat",
                "討伐テスト",
                QuestObjectiveDefinition.CreateRuntime(
                    "objective.test.defeat",
                    GameplayEventIds.EnemyDefeated,
                    "character.training_dummy",
                    2));
            var eventHub = new GameplayEventHub();
            var service = new QuestProgressionService(new[] { quest });
            eventHub.Published += service.Handle;
            int completedCount = 0;
            service.QuestCompleted += _ => completedCount++;

            eventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.other"));
            service.TryGetState("quest.test.defeat", out QuestProgressState state);
            state.TryGetObjective("objective.test.defeat", out ObjectiveProgressState objective);
            Assert.That(objective.CurrentCount, Is.EqualTo(0));

            eventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));
            Assert.That(objective.CurrentCount, Is.EqualTo(1));
            Assert.That(state.IsCompleted, Is.False);

            eventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));
            Assert.That(objective.CurrentCount, Is.EqualTo(2));
            Assert.That(state.IsCompleted, Is.True);
            Assert.That(completedCount, Is.EqualTo(1));

            eventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));
            Assert.That(completedCount, Is.EqualTo(1));

            Object.DestroyImmediate(quest);
        }
    }
}
