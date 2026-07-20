using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Domain.Quests;
using DemonKing.Gameplay.Quests.Configuration;

namespace DemonKing.Gameplay.Quests
{
    public readonly struct QuestProgressUpdate
    {
        public QuestProgressUpdate(
            string questId,
            string objectiveId,
            int previousCount,
            int currentCount,
            bool questCompleted)
        {
            QuestId = questId;
            ObjectiveId = objectiveId;
            PreviousCount = previousCount;
            CurrentCount = currentCount;
            QuestCompleted = questCompleted;
        }

        public string QuestId { get; }
        public string ObjectiveId { get; }
        public int PreviousCount { get; }
        public int CurrentCount { get; }
        public bool QuestCompleted { get; }
    }

    /// <summary>
    /// GameplayEventをQuest DefinitionのObjective条件へ照合し、Domain Stateだけを更新します。
    /// Combat、Dialogue、NPCなどイベント発生元の実装には依存しません。
    /// </summary>
    public sealed class QuestProgressionService
    {
        private readonly Dictionary<string, QuestDefinition> definitions;
        private readonly Dictionary<string, QuestProgressState> states;

        public QuestProgressionService(IEnumerable<QuestDefinition> questDefinitions)
        {
            QuestDefinition[] validDefinitions = questDefinitions?
                .Where(definition => definition != null && definition.IsConfigured)
                .ToArray() ?? Array.Empty<QuestDefinition>();

            definitions = validDefinitions.ToDictionary(
                definition => definition.QuestId,
                StringComparer.Ordinal);
            states = validDefinitions.ToDictionary(
                definition => definition.QuestId,
                CreateState,
                StringComparer.Ordinal);
        }

        public event Action<QuestProgressUpdate> ProgressChanged;
        public event Action<QuestProgressState> QuestCompleted;

        public IReadOnlyCollection<QuestProgressState> States => states.Values;

        public bool TryGetState(string questId, out QuestProgressState state)
        {
            return states.TryGetValue(questId ?? string.Empty, out state);
        }

        public void Handle(GameplayEvent gameplayEvent)
        {
            foreach (QuestDefinition definition in definitions.Values)
            {
                QuestProgressState state = states[definition.QuestId];
                bool wasQuestCompleted = state.IsCompleted;

                foreach (QuestObjectiveDefinition objectiveDefinition in definition.Objectives)
                {
                    if (!objectiveDefinition.Matches(gameplayEvent.EventId, gameplayEvent.SubjectId) ||
                        !state.TryGetObjective(objectiveDefinition.ObjectiveId, out ObjectiveProgressState objectiveState))
                    {
                        continue;
                    }

                    int previousCount = objectiveState.CurrentCount;
                    if (!objectiveState.AddProgress(gameplayEvent.Amount))
                    {
                        continue;
                    }

                    bool questCompletedNow = !wasQuestCompleted && state.IsCompleted;
                    ProgressChanged?.Invoke(new QuestProgressUpdate(
                        definition.QuestId,
                        objectiveDefinition.ObjectiveId,
                        previousCount,
                        objectiveState.CurrentCount,
                        questCompletedNow));

                    if (questCompletedNow)
                    {
                        QuestCompleted?.Invoke(state);
                        wasQuestCompleted = true;
                    }
                }
            }
        }

        private static QuestProgressState CreateState(QuestDefinition definition)
        {
            return new QuestProgressState(
                definition.QuestId,
                definition.Objectives.Select(objective =>
                    new ObjectiveProgressState(objective.ObjectiveId, objective.RequiredCount)));
        }
    }
}
