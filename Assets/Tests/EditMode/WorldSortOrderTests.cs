using DemonKing.Core.Math;
using NUnit.Framework;

namespace DemonKing.Tests.EditMode
{
    /// <summary>
    /// アイソメトリック描画順の共通計算ルールを検証します。
    /// </summary>
    public sealed class WorldSortOrderTests
    {
        [Test]
        public void FromWorldY_画面下側ほど大きい描画順になる()
        {
            int upper = WorldSortOrder.FromWorldY(2f, 100);
            int lower = WorldSortOrder.FromWorldY(-2f, 100);

            Assert.That(lower, Is.GreaterThan(upper));
        }

        [Test]
        public void FromWorldY_Offsetを最終描画順へ加算する()
        {
            int withoutOffset = WorldSortOrder.FromWorldY(1.25f, 100);
            int withOffset = WorldSortOrder.FromWorldY(1.25f, 100, 7);

            Assert.That(withOffset, Is.EqualTo(withoutOffset + 7));
        }
    }
}
