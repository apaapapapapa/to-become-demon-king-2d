using System.Collections;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DemonKing.Tests.PlayMode
{
    public sealed class DialogueLogPlayModeTests
    {
        [Test]
        public void LinearDialogueSequence_空行を読み飛ばし終了後Resetで先頭から再開できる()
        {
            var sequence = new LinearDialogueSequence(new[] { " first ", " ", null, "second" });

            Assert.That(sequence.TryAdvance(out string first), Is.True);
            Assert.That(first, Is.EqualTo("first"));
            Assert.That(sequence.TryAdvance(out string second), Is.True);
            Assert.That(second, Is.EqualTo("second"));
            Assert.That(sequence.TryAdvance(out _), Is.False);

            sequence.Reset();

            Assert.That(sequence.TryAdvance(out string restarted), Is.True);
            Assert.That(restarted, Is.EqualTo("first"));
        }

        [UnityTest]
        public IEnumerator PrototypeNpcInteractable_話しかけるたび会話を進め終了後に閉じる()
        {
            var dialogueLog = new DialogueLog();
            GameObject npcObject = new("Dialogue Test NPC");
            PrototypeNpcInteractable npc = npcObject.AddComponent<PrototypeNpcInteractable>();
            npc.ConfigureDialogueLog(dialogueLog);
            GameObject interactor = new("Dialogue Test Interactor");

            yield return null;

            npc.Interact(interactor);

            Assert.That(dialogueLog.CurrentLine?.Speaker, Is.EqualTo("見習い魔術師"));
            Assert.That(dialogueLog.CurrentLine?.Text, Does.Contain("魔王"));

            npc.Interact(interactor);
            Assert.That(dialogueLog.CurrentLine?.Text, Does.Contain("訓練用スライム"));

            npc.Interact(interactor);
            Assert.That(dialogueLog.CurrentLine?.Text, Does.Contain("攻撃"));

            npc.Interact(interactor);
            Assert.That(dialogueLog.HasCurrentLine, Is.False);

            npc.Interact(interactor);
            Assert.That(dialogueLog.CurrentLine?.Text, Does.Contain("魔王"));

            Object.Destroy(npcObject);
            Object.Destroy(interactor);
        }

        [UnityTest]
        public IEnumerator PrototypeNpcInteractable_話しかけると撃破済みスライムを復活させる()
        {
            GameObject world = new("Respawn Test World");
            var respawner = new PrototypeCombatDummyRespawner(
                world.transform,
                new Vector3(1.45f, -0.45f, 0f),
                configureSpawnedDummy: null);
            PrototypeCombatDummy defeatedDummy = respawner.SpawnOrRestore();

            var dialogueLog = new DialogueLog();
            GameObject npcObject = new("Respawn Test NPC");
            PrototypeNpcInteractable npc = npcObject.AddComponent<PrototypeNpcInteractable>();
            npc.ConfigureDialogueLog(dialogueLog);
            npc.Interacted += () => respawner.SpawnOrRestore();
            GameObject interactor = new("Respawn Test Interactor");

            yield return null;

            Health defeatedHealth = defeatedDummy.GetComponent<Health>();
            defeatedHealth.ApplyDamage(new DamageRequest(99));
            yield return null;

            Assert.That(defeatedDummy == null, Is.True);

            npc.Interact(interactor);

            PrototypeCombatDummy respawnedDummy = respawner.CurrentDummy;
            Assert.That(respawnedDummy, Is.Not.Null);
            Assert.That(respawnedDummy, Is.Not.SameAs(defeatedDummy));
            Assert.That(respawnedDummy.IsAlive, Is.True);
            Assert.That(respawnedDummy.transform.localPosition, Is.EqualTo(new Vector3(1.45f, -0.45f, 0f)));

            Health respawnedHealth = respawnedDummy.GetComponent<Health>();
            respawnedHealth.ApplyDamage(new DamageRequest(1));
            npc.Interact(interactor);

            Assert.That(respawner.CurrentDummy, Is.SameAs(respawnedDummy));
            Assert.That(respawnedHealth.CurrentHealth, Is.EqualTo(respawnedHealth.MaxHealth));

            Object.Destroy(world);
            Object.Destroy(npcObject);
            Object.Destroy(interactor);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DialogueLogView_最新の発言のみを表示しClearでパネルを閉じる()
        {
            var dialogueLog = new DialogueLog();
            GameObject uiRoot = new("Dialogue UI Test", typeof(RectTransform));
            uiRoot.AddComponent<Canvas>();
            DialogueLogView view = uiRoot.AddComponent<DialogueLogView>();
            Font font = Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets").UiFont;
            view.Initialize(font, dialogueLog);

            Assert.That(view.IsVisible, Is.False);

            dialogueLog.ShowLine("見習い魔術師", "最初の会話です。");

            Assert.That(view.IsVisible, Is.True);
            Assert.That(view.DisplayedText, Does.Contain("見習い魔術師"));
            Assert.That(view.DisplayedText, Does.Contain("最初の会話です。"));

            dialogueLog.ShowLine("見習い魔術師", "次の会話です。");

            Assert.That(view.DisplayedText, Does.Not.Contain("最初の会話です。"));
            Assert.That(view.DisplayedText, Does.Contain("次の会話です。"));

            dialogueLog.Clear();

            Assert.That(view.IsVisible, Is.False);
            Assert.That(view.DisplayedText, Is.Empty);
            Assert.That(
                uiRoot.GetComponentsInChildren<Text>(includeInactive: true).Length,
                Is.GreaterThanOrEqualTo(2));

            Object.Destroy(uiRoot);
            yield return null;
        }
    }
}
