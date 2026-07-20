using System;

namespace DemonKing.Domain.Progression
{
    /// <summary>
    /// キャラクターが習得したArtの永続的な進捗です。
    /// ランクや解放Abilityは静的Definitionから導出し、ここへ重複保持しません。
    /// </summary>
    public sealed class ArtProgressState
    {
        private const string ArtIdPrefix = "art.";

        private ArtProgressState(string artId, long masteryPoints)
        {
            ArtId = RequireArtId(artId, nameof(artId));

            if (masteryPoints < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(masteryPoints),
                    "Art熟練ポイントは0以上である必要があります。");
            }

            MasteryPoints = masteryPoints;
        }

        public string ArtId { get; }
        public long MasteryPoints { get; private set; }

        public static ArtProgressState CreateLearned(string artId)
        {
            return new ArtProgressState(artId, 0);
        }

        public static ArtProgressState Restore(string artId, long masteryPoints)
        {
            return new ArtProgressState(artId, masteryPoints);
        }

        public long GainMastery(long amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "加算するArt熟練ポイントは0以上である必要があります。");
            }

            long previousPoints = MasteryPoints;
            MasteryPoints = previousPoints > long.MaxValue - amount
                ? long.MaxValue
                : previousPoints + amount;
            return MasteryPoints - previousPoints;
        }

        private static string RequireArtId(string value, string parameterName)
        {
            string artId = StableContentId.Require(value, parameterName);
            if (!artId.StartsWith(ArtIdPrefix, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Art IDは'{ArtIdPrefix}'で始まる必要があります。",
                    parameterName);
            }

            return artId;
        }
    }
}
