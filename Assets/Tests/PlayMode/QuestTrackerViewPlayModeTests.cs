using System.Collections;
using DemonKing.Domain.Quests;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using DemonKing.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    public sealed class QuestTrackerViewPlayModeTests
    {
        [UnityTest]
        public IEnumerator QuestTrackerとNotificationが独立して受注進捗報告可能完了を表示する()
        {
            QuestDefinition quest = QuestDefinition.CreateRuntime(
                "quest.test.ui",
                "表示テストQuest",
                QuestObjectiveDefinition.CreateRuntime(
                    "objective.test.ui",
                    GameplayEventIds.EnemyDefeated,
                    "character.training_dummy",
                    2,
                    "訓練用スライムを倒す"));
            var service = new QuestProgressionService(new[] { quest });

            GameObject uiRoot = new("Quest UI Test", typeof(RectTransform));
            uiRoot.AddComponent<Canvas>();
            Font font = Resources.Load<PrototypeProjectAssets>(
                "Settings/PrototypeProjectAssets").UiFont;

            QuestNotificationView notification =
                uiRoot.AddComponent<QuestNotificationView>();
            notification.Initialize(font, displayDurationSeconds: 0.05f);
            QuestTrackerView tracker = uiRoot.AddComponent<QuestTrackerView>();
            tracker.Initialize(font, service, notification);

            Assert.That(tracker.IsVisible, Is.False);
            Assert.That(notification.IsVisible, Is.False);

            service.AcceptQuest(quest.QuestId);

            Assert.That(tracker.IsVisible, Is.True);
            Assert.That(tracker.DisplayedStatusText, Is.EqualTo("受注中"));
            Assert.That(tracker.DisplayedQuestTitle, Is.EqualTo("表示テストQuest"));
            Assert.That(tracker.DisplayedObjectiveText, Does.Contain("訓練用スライムを倒す"));
            Assert.That(tracker.DisplayedObjectiveText, Does.Contain("0/2"));
            Assert.That(notification.IsVisible, Is.True);
            Assert.That(notification.DisplayedText, Does.Contain("クエスト受注"));

            service.Handle(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));

            Assert.That(tracker.DisplayedStatusText, Is.EqualTo("受注中"));
            Assert.That(tracker.DisplayedObjectiveText, Does.Contain("1/2"));
            Assert.That(notification.DisplayedText, Does.Contain("クエスト進捗"));

            yield return new WaitForSecondsRealtime(0.08f);

            Assert.That(notification.IsVisible, Is.False);
            Assert.That(tracker.IsVisible, Is.True);
            Assert.That(tracker.DisplayedObjectiveText, Does.Contain("1/2"));

            service.Handle(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));

            Assert.That(tracker.DisplayedStatusText, Is.EqualTo("報告可能"));
            Assert.That(tracker.DisplayedObjectiveText, Does.Contain("2/2"));
            Assert.That(tracker.DisplayedObjectiveText, Does.Contain("✓"));
            Assert.That(notification.DisplayedText, Does.Contain("見習い魔術師に報告"));

            service.CompleteQuest(quest.QuestId);

            Assert.That(tracker.DisplayedStatusText, Is.EqualTo("完了"));
            Assert.That(notification.DisplayedText, Does.Contain("クエスト完了"));
            Assert.That(
                uiRoot.GetComponentsInChildren<Text>(includeInactive: true).Length,
                Is.GreaterThanOrEqualTo(4));

            Object.Destroy(uiRoot);
            Object.Destroy(quest);
            yield return null;
        }
    }
}
