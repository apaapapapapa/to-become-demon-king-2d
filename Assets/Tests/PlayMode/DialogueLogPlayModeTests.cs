using System.Collections;
using DemonKing.Field.Prototype;
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
        [UnityTest]
        public IEnumerator PrototypeNpcInteractable_会話ログへ固定発言を追加する()
        {
            var dialogueLog = new DialogueLog();
            GameObject npcObject = new("Dialogue Test NPC");
            PrototypeNpcInteractable npc = npcObject.AddComponent<PrototypeNpcInteractable>();
            npc.ConfigureDialogueLog(dialogueLog);
            GameObject interactor = new("Dialogue Test Interactor");

            yield return null;

            npc.Interact(interactor);

            Assert.That(dialogueLog.Lines.Count, Is.EqualTo(1));
            Assert.That(dialogueLog.Lines[0].Speaker, Is.EqualTo("見習い魔術師"));
            Assert.That(dialogueLog.Lines[0].Text, Does.Contain("訓練用スライム"));

            Object.Destroy(npcObject);
            Object.Destroy(interactor);
        }

        [UnityTest]
        public IEnumerator DialogueLogView_発言追加時に画面内のログパネルを表示する()
        {
            var dialogueLog = new DialogueLog();
            GameObject uiRoot = new("Dialogue UI Test", typeof(RectTransform));
            uiRoot.AddComponent<Canvas>();
            DialogueLogView view = uiRoot.AddComponent<DialogueLogView>();
            Font font = Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets").UiFont;
            view.Initialize(font, dialogueLog);

            Assert.That(view.IsVisible, Is.False);

            dialogueLog.AddLine("見習い魔術師", "画面に表示する会話です。");

            Assert.That(view.IsVisible, Is.True);
            Assert.That(view.DisplayedText, Does.Contain("見習い魔術師"));
            Assert.That(view.DisplayedText, Does.Contain("画面に表示する会話です。"));
            Assert.That(
                uiRoot.GetComponentsInChildren<Text>(includeInactive: true).Length,
                Is.GreaterThanOrEqualTo(2));

            Object.Destroy(uiRoot);
            yield return null;
        }
    }
}
