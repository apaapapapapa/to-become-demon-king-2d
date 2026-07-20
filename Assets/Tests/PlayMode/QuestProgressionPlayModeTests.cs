using DemonKing.Domain.Quests;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using DemonKing.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.PlayMode
{
    public sealed class QuestProgressionPlayModeTests
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

        [Test]
        public void SpawnLifecycle_生存中は復元し利用不可なら新規生成する()
        {
            int spawnCount = 0;
            int restoreCount = 0;
            FakeSpawnTarget currentFactoryTarget = null;
            var lifecycle = new SpawnLifecycle<FakeSpawnTarget>(
                () => currentFactoryTarget = new FakeSpawnTarget(++spawnCount),
                target => target.IsReusable,
                target =>
                {
                    restoreCount++;
                    target.Restore();
                });

            FakeSpawnTarget first = lifecycle.SpawnOrRestore();
            first.Damage();
            FakeSpawnTarget restored = lifecycle.SpawnOrRestore();

            Assert.That(restored, Is.SameAs(first));
            Assert.That(spawnCount, Is.EqualTo(1));
            Assert.That(restoreCount, Is.EqualTo(1));
            Assert.That(first.WasRestored, Is.True);

            first.IsReusable = false;
            FakeSpawnTarget second = lifecycle.SpawnOrRestore();

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(second.Sequence, Is.EqualTo(2));
            Assert.That(spawnCount, Is.EqualTo(2));
        }

        private sealed class FakeSpawnTarget
        {
            public FakeSpawnTarget(int sequence)
            {
                Sequence = sequence;
            }

            public int Sequence { get; }
            public bool IsReusable { get; set; } = true;
            public bool WasRestored { get; private set; }

            public void Damage()
            {
                WasRestored = false;
            }

            public void Restore()
            {
                WasRestored = true;
            }
        }
    }
}
