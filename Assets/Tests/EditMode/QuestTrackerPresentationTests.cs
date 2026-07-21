using System.Collections.Generic;
using DemonKing.Domain.Quests;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using DemonKing.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    public sealed class QuestTrackerPresentationTests
    {
        [Test]
        public void Projection_Quest状態を表示Modelへ変換する()
        {
            QuestDefinition definition = CreateQuest("quest.test.projection", 2);
            var objective = new ObjectiveProgressState("objective.test.projection", 2);
            var state = new QuestProgressState(
                definition.QuestId,
                new[] { objective });

            Assert.That(
                QuestTrackerProjection.TryCreate(definition, state, out _),
                Is.False);

            state.Accept();
            Assert.That(
                QuestTrackerProjection.TryCreate(definition, state, out QuestTrackerDisplayModel active),
                Is.True);
            Assert.That(active.Status, Is.EqualTo(QuestTrackerDisplayStatus.Active));
            Assert.That(active.StatusText, Is.EqualTo("受注中"));
            Assert.That(active.ObjectiveText, Does.Contain("□ 訓練対象を倒す  0/2"));

            objective.AddProgress(2);
            state.TryMarkReadyToTurnIn();
            QuestTrackerProjection.TryCreate(definition, state, out QuestTrackerDisplayModel ready);
            Assert.That(ready.Status, Is.EqualTo(QuestTrackerDisplayStatus.ReadyToTurnIn));
            Assert.That(ready.StatusText, Is.EqualTo("報告可能"));
            Assert.That(ready.ObjectiveText, Does.Contain("✓ 訓練対象を倒す  2/2"));

            state.Complete();
            QuestTrackerProjection.TryCreate(definition, state, out QuestTrackerDisplayModel completed);
            Assert.That(completed.Status, Is.EqualTo(QuestTrackerDisplayStatus.Completed));
            Assert.That(completed.StatusText, Is.EqualTo("完了"));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Selector_状態優先度とQuestId順で初期表示対象を決める()
        {
            QuestProgressState completed = CreateState("quest.z.completed", completed: true);
            QuestProgressState ready = CreateState("quest.m.ready", ready: true);
            QuestProgressState activeZ = CreateState("quest.z.active");
            QuestProgressState activeA = CreateState("quest.a.active");

            string selected = QuestTrackerSelector.SelectInitialQuestId(new[]
            {
                completed,
                ready,
                activeZ,
                activeA,
            });

            Assert.That(selected, Is.EqualTo("quest.a.active"));
            Assert.That(
                QuestTrackerSelector.SelectInitialQuestId(new[] { completed, ready }),
                Is.EqualTo("quest.m.ready"));
            Assert.That(
                QuestTrackerSelector.SelectInitialQuestId(new QuestProgressState[0]),
                Is.Empty);
        }

        [Test]
        public void NotificationFormatter_定義の表示名を使用する()
        {
            QuestDefinition definition = CreateQuest("quest.test.notification", 2);
            var update = new QuestProgressUpdate(
                definition.QuestId,
                "objective.test.projection",
                0,
                1,
                questReadyToTurnIn: false);

            Assert.That(
                QuestNotificationFormatter.Accepted(definition, definition.QuestId),
                Is.EqualTo("クエスト受注\n表示テストQuest"));
            Assert.That(
                QuestNotificationFormatter.Progress(definition, update),
                Is.EqualTo("クエスト進捗\n訓練対象を倒す  1"));
            Assert.That(
                QuestNotificationFormatter.ReadyToTurnIn(definition, definition.QuestId),
                Does.Contain("見習い魔術師に報告しよう"));
            Assert.That(
                QuestNotificationFormatter.Completed(definition, definition.QuestId),
                Is.EqualTo("クエスト完了\n表示テストQuest"));

            Object.DestroyImmediate(definition);
        }

        private static QuestDefinition CreateQuest(string questId, int requiredCount)
        {
            return QuestDefinition.CreateRuntime(
                questId,
                "表示テストQuest",
                QuestObjectiveDefinition.CreateRuntime(
                    "objective.test.projection",
                    "gameplay.test.progress",
                    count: requiredCount,
                    name: "訓練対象を倒す"));
        }

        private static QuestProgressState CreateState(
            string questId,
            bool ready = false,
            bool completed = false)
        {
            var objective = new ObjectiveProgressState($"objective.{questId}", 1);
            var state = new QuestProgressState(
                questId,
                new List<ObjectiveProgressState> { objective });
            state.Accept();

            if (ready || completed)
            {
                objective.AddProgress(1);
                state.TryMarkReadyToTurnIn();
            }

            if (completed)
            {
                state.Complete();
            }

            return state;
        }
    }
}
