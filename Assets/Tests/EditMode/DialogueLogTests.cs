using DemonKing.Gameplay.Dialogue;
using NUnit.Framework;

namespace DemonKing.Tests.EditMode
{
    public sealed class DialogueLogTests
    {
        [Test]
        public void DialogueLog_容量を超えると古い発言から削除する()
        {
            var dialogueLog = new DialogueLog(capacity: 2);
            int notifiedCount = 0;
            dialogueLog.LineAdded += line => notifiedCount++;

            dialogueLog.AddLine("NPC A", "最初の発言");
            dialogueLog.AddLine("NPC B", "二番目の発言");
            DialogueLine latest = dialogueLog.AddLine("NPC C", "最新の発言");

            Assert.That(dialogueLog.Lines.Count, Is.EqualTo(2));
            Assert.That(dialogueLog.Lines[0].Speaker, Is.EqualTo("NPC B"));
            Assert.That(dialogueLog.Lines[1].Text, Is.EqualTo("最新の発言"));
            Assert.That(latest.Speaker, Is.EqualTo("NPC C"));
            Assert.That(notifiedCount, Is.EqualTo(3));
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
