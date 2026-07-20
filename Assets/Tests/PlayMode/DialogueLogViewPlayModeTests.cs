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
    public sealed class DialogueLogViewPlayModeTests
    {
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
