using DemonKing.Gameplay.Dialogue;
using NUnit.Framework;

namespace DemonKing.Tests.EditMode
{
    public sealed class DialogueLogTests
    {
        [Test]
        public void DialogueLog_新しい発言で表示中の1件を置き換える()
        {
            var dialogueLog = new DialogueLog();
            int notifiedCount = 0;
            dialogueLog.Changed += () => notifiedCount++;

            dialogueLog.ShowLine("NPC A", "最初の発言");
            DialogueLine latest = dialogueLog.ShowLine("NPC B", "最新の発言");

            Assert.That(dialogueLog.HasCurrentLine, Is.True);
            Assert.That(dialogueLog.CurrentLine?.Speaker, Is.EqualTo("NPC B"));
            Assert.That(dialogueLog.CurrentLine?.Text, Is.EqualTo("最新の発言"));
            Assert.That(latest.Speaker, Is.EqualTo("NPC B"));
            Assert.That(notifiedCount, Is.EqualTo(2));
        }

        [Test]
        public void DialogueLog_Clearで表示中の発言を削除する()
        {
            var dialogueLog = new DialogueLog();
            int notifiedCount = 0;
            dialogueLog.Changed += () => notifiedCount++;

            dialogueLog.ShowLine("NPC", "表示中の発言");
            dialogueLog.Clear();

            Assert.That(dialogueLog.HasCurrentLine, Is.False);
            Assert.That(dialogueLog.CurrentLine, Is.Null);
            Assert.That(notifiedCount, Is.EqualTo(2));
        }

        [Test]
        public void DialogueLine_空の話者名を代替表示へ変換し空の本文を拒否する()
        {
            var line = new DialogueLine(" ", " こんにちは ");

            Assert.That(line.Speaker, Is.EqualTo("？？？"));
            Assert.That(line.Text, Is.EqualTo("こんにちは"));
            Assert.That(
                () => new DialogueLine("NPC", " "),
                Throws.TypeOf<System.ArgumentException>());
        }
    }
}
