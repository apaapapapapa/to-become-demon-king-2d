using System.Collections;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Dialogue.Configuration;
using DemonKing.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DemonKing.Tests.PlayMode
{
    public sealed class PrototypeNpcInteractablePlayModeTests
    {
        [UnityTest]
        public IEnumerator PrototypeNpcInteractable_DialogueDefinitionの内容で会話を進め終了後に閉じる()
        {
            DialogueDefinition definition = DialogueDefinition.CreateRuntime(
                "dialogue.test.apprentice",
                "見習い魔術師",
                "魔王を目指しているの？",
                "まずは訓練用スライムで腕試ししてみて。",
                "攻撃は何度でも試せるよ。");
            var dialogueLog = new DialogueLog();
            GameObject npcObject = new("Dialogue Test NPC");
            PrototypeNpcInteractable npc = npcObject.AddComponent<PrototypeNpcInteractable>();
            npc.ConfigureDialogueLog(dialogueLog);
            npc.ConfigureDialogue(definition);
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
            Object.Destroy(definition);
        }

        [UnityTest]
        public IEnumerator PrototypeNpcInteractable_汎用SpawnLifecycle経由で撃破済みスライムを復活させる()
        {
            GameObject world = new("Respawn Test World");
            Vector3 spawnPosition = new(1.45f, -0.45f, 0f);
            var lifecycle = new SpawnLifecycle<PrototypeCombatDummy>(
                () =>
                {
                    GameObject dummyObject = new("訓練用スライム");
                    dummyObject.transform.SetParent(world.transform, false);
                    dummyObject.transform.localPosition = spawnPosition;
                    return dummyObject.AddComponent<PrototypeCombatDummy>();
                },
                dummy => dummy != null && dummy.IsAlive,
                dummy => dummy.RestoreToFull());
            PrototypeCombatDummy defeatedDummy = lifecycle.SpawnOrRestore();

            DialogueDefinition definition = DialogueDefinition.CreateRuntime(
                "dialogue.test.respawn",
                "見習い魔術師",
                "訓練しよう。");
            var dialogueLog = new DialogueLog();
            GameObject npcObject = new("Respawn Test NPC");
            PrototypeNpcInteractable npc = npcObject.AddComponent<PrototypeNpcInteractable>();
            npc.ConfigureDialogueLog(dialogueLog);
            npc.ConfigureDialogue(definition);
            npc.Interacted += () => lifecycle.SpawnOrRestore();
            GameObject interactor = new("Respawn Test Interactor");

            yield return null;

            Health defeatedHealth = defeatedDummy.GetComponent<Health>();
            defeatedHealth.ApplyDamage(new DamageRequest(99));
            yield return null;

            Assert.That(defeatedDummy == null, Is.True);

            npc.Interact(interactor);

            PrototypeCombatDummy respawnedDummy = lifecycle.Current;
            Assert.That(respawnedDummy, Is.Not.Null);
            Assert.That(respawnedDummy, Is.Not.SameAs(defeatedDummy));
            Assert.That(respawnedDummy.IsAlive, Is.True);
            Assert.That(respawnedDummy.transform.localPosition, Is.EqualTo(spawnPosition));

            Health respawnedHealth = respawnedDummy.GetComponent<Health>();
            respawnedHealth.ApplyDamage(new DamageRequest(1));
            npc.Interact(interactor);

            Assert.That(lifecycle.Current, Is.SameAs(respawnedDummy));
            Assert.That(respawnedHealth.CurrentHealth, Is.EqualTo(respawnedHealth.MaxHealth));

            Object.Destroy(world);
            Object.Destroy(npcObject);
            Object.Destroy(interactor);
            Object.Destroy(definition);
            yield return null;
        }
    }
}
