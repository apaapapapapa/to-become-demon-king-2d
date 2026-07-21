using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Domain.Quests;
using DemonKing.Domain.Save;
using DemonKing.Gameplay.Quests.Configuration;

namespace DemonKing.Gameplay.Quests
{
    /// <summary>
    /// Quest Definitionを参照しながらSave DTOとQuest Runtime Stateを相互変換します。
    /// RequiredCountはDefinitionを正とし、Saveには現在進捗だけを保持します。
    /// </summary>
    public static class QuestProgressSaveMapper
    {
        public static List<QuestProgressSaveData> ToSaveData(IEnumerable<QuestProgressState> states)
        {
            var result = new List<QuestProgressSaveData>();
            if (states == null)
            {
                return result;
            }

            foreach (QuestProgressState state in states.OrderBy(state => state.QuestId, StringComparer.Ordinal))
            {
                var objectives = new List<ObjectiveProgressSaveData>();
                foreach (ObjectiveProgressState objective in state.Objectives.OrderBy(
                             objective => objective.ObjectiveId,
                             StringComparer.Ordinal))
                {
                    objectives.Add(new ObjectiveProgressSaveData
                    {
                        objectiveId = objective.ObjectiveId,
                        currentCount = objective.CurrentCount
                    });
                }

                result.Add(new QuestProgressSaveData
                {
                    questId = state.QuestId,
                    status = (int)state.Status,
                    objectives = objectives
                });
            }

            return result;
        }

        public static IReadOnlyList<QuestProgressState> FromSaveData(
            IEnumerable<QuestDefinition> definitions,
            IEnumerable<QuestProgressSaveData> saveData)
        {
            Dictionary<string, QuestDefinition> definitionsById = definitions?
                .Where(definition => definition != null && definition.IsConfigured)
                .ToDictionary(definition => definition.QuestId, StringComparer.Ordinal) ??
                new Dictionary<string, QuestDefinition>(StringComparer.Ordinal);

            var restoredStates = new List<QuestProgressState>();
            if (saveData == null)
            {
                return restoredStates;
            }

            foreach (QuestProgressSaveData questSave in saveData)
            {
                if (questSave == null ||
                    !definitionsById.TryGetValue(
                        questSave.questId ?? string.Empty,
                        out QuestDefinition definition))
                {
                    continue;
                }

                if (!Enum.IsDefined(typeof(QuestProgressStatus), questSave.status))
                {
                    throw new ArgumentException(
                        $"保存されたQuest Statusが不正です: {questSave.status}",
                        nameof(saveData));
                }

                var savedObjectives = (questSave.objectives ?? new List<ObjectiveProgressSaveData>())
                    .Where(objective => objective != null)
                    .GroupBy(objective => objective.objectiveId ?? string.Empty, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

                QuestProgressStatus status = (QuestProgressStatus)questSave.status;
                var objectiveStates = new List<ObjectiveProgressState>();
                foreach (QuestObjectiveDefinition objectiveDefinition in definition.Objectives)
                {
                    int currentCount = 0;
                    if (savedObjectives.TryGetValue(
                            objectiveDefinition.ObjectiveId,
                            out ObjectiveProgressSaveData savedObjective))
                    {
                        currentCount = Math.Max(
                            0,
                            Math.Min(savedObjective.currentCount, objectiveDefinition.RequiredCount));
                    }

                    if (status == QuestProgressStatus.ReadyToTurnIn ||
                        status == QuestProgressStatus.Completed)
                    {
                        currentCount = objectiveDefinition.RequiredCount;
                    }

                    objectiveStates.Add(ObjectiveProgressState.Restore(
                        objectiveDefinition.ObjectiveId,
                        objectiveDefinition.RequiredCount,
                        currentCount));
                }

                if (status == QuestProgressStatus.Active &&
                    objectiveStates.All(objective => objective.IsCompleted))
                {
                    status = QuestProgressStatus.ReadyToTurnIn;
                }

                restoredStates.Add(QuestProgressState.Restore(
                    definition.QuestId,
                    objectiveStates,
                    status));
            }

            return restoredStates;
        }
    }
}
