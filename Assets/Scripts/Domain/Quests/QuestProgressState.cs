using System;
using System.Collections.Generic;
using System.Linq;

namespace DemonKing.Domain.Quests
{
    /// <summary>
    /// QuestやAchievementなどが購読できる、Unity非依存のGameplay上の出来事です。
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

    /// <summary>
    /// 1つのObjectiveの可変進捗を保持します。DefinitionやUnity Objectは参照しません。
    /// </summary>
    public sealed class ObjectiveProgressState
    {
        public ObjectiveProgressState(string objectiveId, int requiredCount)
        {
            ObjectiveId = StableContentId.Require(objectiveId, nameof(objectiveId));
            RequiredCount = Math.Max(1, requiredCount);
        }

        public string ObjectiveId { get; }
        public int RequiredCount { get; }
        public int CurrentCount { get; private set; }
        public bool IsCompleted => CurrentCount >= RequiredCount;

        public bool AddProgress(int amount)
        {
            if (IsCompleted || amount <= 0)
            {
                return false;
            }

            int next = Math.Min(RequiredCount, CurrentCount + amount);
            if (next == CurrentCount)
            {
                return false;
            }

            CurrentCount = next;
            return true;
        }
    }

    /// <summary>
    /// Quest単位のRuntime Stateです。Objective群の完了状態からQuest完了を導出します。
    /// </summary>
    public sealed class QuestProgressState
    {
        private readonly Dictionary<string, ObjectiveProgressState> objectives;

        public QuestProgressState(string questId, IEnumerable<ObjectiveProgressState> objectiveStates)
        {
            QuestId = StableContentId.Require(questId, nameof(questId));
            if (objectiveStates == null)
            {
                throw new ArgumentNullException(nameof(objectiveStates));
            }

            objectives = objectiveStates.ToDictionary(state => state.ObjectiveId, StringComparer.Ordinal);
            if (objectives.Count == 0)
            {
                throw new ArgumentException("Questには1つ以上のObjectiveが必要です。", nameof(objectiveStates));
            }
        }

        public string QuestId { get; }
        public IReadOnlyCollection<ObjectiveProgressState> Objectives => objectives.Values;
        public bool IsCompleted => objectives.Values.All(objective => objective.IsCompleted);

        public bool TryGetObjective(string objectiveId, out ObjectiveProgressState state)
        {
            return objectives.TryGetValue(StableContentId.Normalize(objectiveId), out state);
        }
    }
}
