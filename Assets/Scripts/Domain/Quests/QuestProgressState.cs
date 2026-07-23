using System;
using System.Collections.Generic;
using System.Linq;

namespace DemonKing.Domain.Quests
{
    /// <summary>
    /// 1つのObjectiveの可変進捗を保持します。DefinitionやUnity Objectは参照しません。
    /// </summary>
    public sealed class ObjectiveProgressState
    {
        public ObjectiveProgressState(string objectiveId, int requiredCount)
            : this(objectiveId, requiredCount, currentCount: 0)
        {
        }

        private ObjectiveProgressState(string objectiveId, int requiredCount, int currentCount)
        {
            ObjectiveId = StableContentId.Require(objectiveId, nameof(objectiveId));
            RequiredCount = Math.Max(1, requiredCount);
            if (currentCount < 0 || currentCount > RequiredCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentCount),
                    "Objective進捗は0以上かつ必要数以下である必要があります。");
            }

            CurrentCount = currentCount;
        }

        public string ObjectiveId { get; }
        public int RequiredCount { get; }
        public int CurrentCount { get; private set; }
        public bool IsCompleted => CurrentCount >= RequiredCount;

        public static ObjectiveProgressState Restore(
            string objectiveId,
            int requiredCount,
            int currentCount)
        {
            return new ObjectiveProgressState(objectiveId, requiredCount, currentCount);
        }

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
        ReadyToTurnIn = 2,
        Completed = 3,
    }

    /// <summary>
    /// Quest単位のRuntime Stateです。
    /// Objective達成とQuest報告完了を分離し、Available -> Active -> ReadyToTurnIn -> Completedを管理します。
    /// </summary>
    public sealed class QuestProgressState
    {
        private readonly Dictionary<string, ObjectiveProgressState> objectives;

        public QuestProgressState(string questId, IEnumerable<ObjectiveProgressState> objectiveStates)
            : this(questId, objectiveStates, QuestProgressStatus.Available)
        {
        }

        private QuestProgressState(
            string questId,
            IEnumerable<ObjectiveProgressState> objectiveStates,
            QuestProgressStatus status)
        {
            QuestId = StableContentId.Require(questId, nameof(questId));
            if (objectiveStates == null)
            {
                throw new ArgumentNullException(nameof(objectiveStates));
            }

            if (!Enum.IsDefined(typeof(QuestProgressStatus), status))
            {
                throw new ArgumentOutOfRangeException(nameof(status));
            }

            objectives = objectiveStates.ToDictionary(state => state.ObjectiveId, StringComparer.Ordinal);
            if (objectives.Count == 0)
            {
                throw new ArgumentException("Questには1つ以上のObjectiveが必要です。", nameof(objectiveStates));
            }

            if ((status == QuestProgressStatus.ReadyToTurnIn ||
                 status == QuestProgressStatus.Completed) &&
                objectives.Values.Any(objective => !objective.IsCompleted))
            {
                throw new ArgumentException(
                    "報告可能または完了済みQuestは全Objectiveが完了している必要があります。",
                    nameof(objectiveStates));
            }

            Status = status;
        }

        public string QuestId { get; }
        public IReadOnlyCollection<ObjectiveProgressState> Objectives => objectives.Values;
        public QuestProgressStatus Status { get; private set; }
        public bool IsAccepted => Status != QuestProgressStatus.Available;
        public bool IsActive => Status == QuestProgressStatus.Active;
        public bool IsReadyToTurnIn => Status == QuestProgressStatus.ReadyToTurnIn;
        public bool IsCompleted => Status == QuestProgressStatus.Completed;
        public bool AreObjectivesCompleted => objectives.Values.All(objective => objective.IsCompleted);

        public static QuestProgressState Restore(
            string questId,
            IEnumerable<ObjectiveProgressState> objectiveStates,
            QuestProgressStatus status)
        {
            return new QuestProgressState(questId, objectiveStates, status);
        }

        public bool Accept()
        {
            if (Status != QuestProgressStatus.Available)
            {
                return false;
            }

            Status = QuestProgressStatus.Active;
            return true;
        }

        public bool TryMarkReadyToTurnIn()
        {
            if (Status != QuestProgressStatus.Active || !AreObjectivesCompleted)
            {
                return false;
            }

            Status = QuestProgressStatus.ReadyToTurnIn;
            return true;
        }

        public bool Complete()
        {
            if (Status != QuestProgressStatus.ReadyToTurnIn)
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
