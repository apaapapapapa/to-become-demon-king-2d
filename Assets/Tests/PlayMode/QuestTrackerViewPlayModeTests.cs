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
        public IEnumerator QuestTrackerView_受注進捗報告可能完了を表示する()
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
            QuestTrackerView view = uiRoot.AddComponent<QuestTrackerView>();
            Font font = Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets").UiFont;
            view.Initialize(font, service);

            Assert.That(view.IsVisible, Is.False);
            Assert.That(view.IsNotificationVisible, Is.False);

            service.AcceptQuest(quest.QuestId);

            Assert.That(view.IsVisible, Is.True);
            Assert.That(view.DisplayedStatusText, Is.EqualTo("受注中"));
            Assert.That(view.DisplayedQuestTitle, Is.EqualTo("表示テストQuest"));
            Assert.That(view.DisplayedObjectiveText, Does.Contain("訓練用スライムを倒す"));
            Assert.That(view.DisplayedObjectiveText, Does.Contain("0/2"));
            Assert.That(view.IsNotificationVisible, Is.True);
            Assert.That(view.DisplayedNotificationText, Does.Contain("クエスト受注"));

            service.Handle(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));

            Assert.That(view.DisplayedStatusText, Is.EqualTo("受注中"));
            Assert.That(view.DisplayedObjectiveText, Does.Contain("1/2"));
            Assert.That(view.DisplayedNotificationText, Does.Contain("クエスト進捗"));

            service.Handle(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                "character.training_dummy"));

            Assert.That(view.DisplayedStatusText, Is.EqualTo("報告可能"));
            Assert.That(view.DisplayedObjectiveText, Does.Contain("2/2"));
            Assert.That(view.DisplayedObjectiveText, Does.Contain("✓"));
            Assert.That(view.DisplayedNotificationText, Does.Contain("見習い魔術師に報告"));

            service.CompleteQuest(quest.QuestId);

            Assert.That(view.DisplayedStatusText, Is.EqualTo("完了"));
            Assert.That(view.DisplayedNotificationText, Does.Contain("クエスト完了"));
            Assert.That(
                uiRoot.GetComponentsInChildren<Text>(includeInactive: true).Length,
                Is.GreaterThanOrEqualTo(4));

            Object.Destroy(uiRoot);
            Object.Destroy(quest);
            yield return null;
        }
    }
}
