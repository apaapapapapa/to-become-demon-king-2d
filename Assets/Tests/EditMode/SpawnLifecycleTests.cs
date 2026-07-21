using DemonKing.Gameplay.Spawning;
using NUnit.Framework;

namespace DemonKing.Tests.EditMode
{
    public sealed class SpawnLifecycleTests
    {
        [Test]
        public void SpawnLifecycle_生存中は復元し利用不可なら新規生成する()
        {
            int spawnCount = 0;
            int restoreCount = 0;
            FakeSpawnTarget currentFactoryTarget = null;
            var lifecycle = new SpawnLifecycle<FakeSpawnTarget>(
                () => currentFactoryTarget = new FakeSpawnTarget(++spawnCount),
                target => target.IsReusable,
                target =>
                {
                    restoreCount++;
                    target.Restore();
                });

            FakeSpawnTarget first = lifecycle.SpawnOrRestore();
            first.Damage();
            FakeSpawnTarget restored = lifecycle.SpawnOrRestore();

            Assert.That(restored, Is.SameAs(first));
            Assert.That(spawnCount, Is.EqualTo(1));
            Assert.That(restoreCount, Is.EqualTo(1));
            Assert.That(first.WasRestored, Is.True);

            first.IsReusable = false;
            FakeSpawnTarget second = lifecycle.SpawnOrRestore();

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(second.Sequence, Is.EqualTo(2));
            Assert.That(spawnCount, Is.EqualTo(2));
        }

        private sealed class FakeSpawnTarget
        {
            public FakeSpawnTarget(int sequence)
            {
                Sequence = sequence;
            }

            public int Sequence { get; }
            public bool IsReusable { get; set; } = true;
            public bool WasRestored { get; private set; }

            public void Damage()
            {
                WasRestored = false;
            }

            public void Restore()
            {
                WasRestored = true;
            }
        }
    }
}
