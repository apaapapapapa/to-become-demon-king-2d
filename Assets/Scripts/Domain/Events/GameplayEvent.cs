using System;

namespace DemonKing.Domain.Events
{
    /// <summary>
    /// Quest、Story、Achievement等が同じ境界から購読できるUnity非依存のGameplay上の出来事です。
    /// EventIdは出来事の種類、SubjectIdは対象コンテンツ、Amountは進捗量を表します。
    /// </summary>
    public readonly struct GameplayEvent
    {
        public GameplayEvent(string eventId, string subjectId = "", int amount = 1)
        {
            EventId = StableContentId.Require(eventId, nameof(eventId));
            SubjectId = StableContentId.Normalize(subjectId);
            Amount = Math.Max(1, amount);
        }

        public string EventId { get; }
        public string SubjectId { get; }
        public int Amount { get; }
    }
}
