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

    public enum QuestProgressStatus
    {
        Available = 0,
        Active = 1,
        Completed = 2,
    }

    /// <summary>
    /// Quest単位のRuntime Stateです。受注状態とObjective進捗を保持し、完了状態を明示的に遷移させます。
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
        public QuestProgressStatus Status { get; private set; } = QuestProgressStatus.Available;
        public bool IsAccepted => Status != QuestProgressStatus.Available;
        public bool IsActive => Status == QuestProgressStatus.Active;
        public bool IsCompleted => Status == QuestProgressStatus.Completed;

        public bool Accept()
        {
            if (Status != QuestProgressStatus.Available)
            {
                return false;
            }

            Status = QuestProgressStatus.Active;
            return true;
        }

        public bool TryComplete()
        {
            if (Status != QuestProgressStatus.Active ||
                !objectives.Values.All(objective => objective.IsCompleted))
            {
                return false;
            }

            Status = QuestProgressStatus.Completed;
            return true;
        }

        public bool TryGetObjective(string objectiveId, out ObjectiveProgressState state)
        {
            return objectives.TryGetValue(StableContentId.Normalize(objectiveId), out state);
        }
    }
}
