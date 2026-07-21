using System.Collections;
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
        public IEnumerator QuestTrackerとNotification_uGUI生成購読通知寿命をRuntimeで統合する()
        {
            QuestDefinition quest = QuestDefinition.CreateRuntime(
                "quest.test.ui",
                "表示テストQuest",
                QuestObjectiveDefinition.CreateRuntime(
                    "objective.test.ui",
                    "gameplay.test.progress",
                    count: 1,
                    name: "訓練用スライムを倒す"));
            var service = new QuestProgressionService(new[] { quest });

            GameObject uiRoot = new("Quest UI Test", typeof(RectTransform), typeof(Canvas));
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            QuestNotificationView notification = uiRoot.AddComponent<QuestNotificationView>();
            notification.Initialize(font, displayDurationSeconds: 0.05f);
            QuestTrackerView tracker = uiRoot.AddComponent<QuestTrackerView>();
            tracker.Initialize(font, service, notification);

            Assert.That(uiRoot.transform.Find("Quest Tracker"), Is.Not.Null);
            Assert.That(uiRoot.transform.Find("Quest Notification"), Is.Not.Null);
            Assert.That(
                uiRoot.GetComponentsInChildren<Text>(includeInactive: true).Length,
                Is.EqualTo(4));
            Assert.That(tracker.IsVisible, Is.False);
            Assert.That(notification.IsVisible, Is.False);

            Assert.That(service.AcceptQuest(quest.QuestId), Is.True);

            Assert.That(tracker.IsVisible, Is.True);
            Assert.That(tracker.DisplayedQuestId, Is.EqualTo(quest.QuestId));
            Assert.That(notification.IsVisible, Is.True);
            Assert.That(notification.DisplayedText, Is.Not.Empty);

            yield return new WaitForSecondsRealtime(0.08f);

            Assert.That(notification.IsVisible, Is.False);
            Assert.That(tracker.IsVisible, Is.True);
            Assert.That(tracker.DisplayedQuestId, Is.EqualTo(quest.QuestId));

            Object.Destroy(uiRoot);
            Object.Destroy(quest);
            yield return null;
        }
    }
}
