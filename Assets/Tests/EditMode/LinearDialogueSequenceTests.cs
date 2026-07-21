using DemonKing.Gameplay.Dialogue;
using NUnit.Framework;

namespace DemonKing.Tests.EditMode
{
    public sealed class LinearDialogueSequenceTests
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
    }
}
