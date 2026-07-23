using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Domain.Events;
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
            bool questReadyToTurnIn)
        {
            QuestId = questId;
            ObjectiveId = objectiveId;
            PreviousCount = previousCount;
            CurrentCount = currentCount;
            QuestReadyToTurnIn = questReadyToTurnIn;
        }

        public string QuestId { get; }
        public string ObjectiveId { get; }
        public int PreviousCount { get; }
        public int CurrentCount { get; }
        public bool QuestReadyToTurnIn { get; }
    }

    /// <summary>
    /// Questの受注、GameplayEventによるObjective進捗、報告完了を管理します。
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

        public event Action<QuestProgressState> QuestAccepted;
        public event Action<QuestProgressUpdate> ProgressChanged;
        public event Action<QuestProgressState> QuestReadyToTurnIn;
        public event Action<QuestProgressState> QuestCompleted;

        public IReadOnlyCollection<QuestDefinition> Definitions => definitions.Values;
        public IReadOnlyCollection<QuestProgressState> States => states.Values;

        public bool TryGetDefinition(string questId, out QuestDefinition definition)
        {
            return definitions.TryGetValue(questId ?? string.Empty, out definition);
        }

        public bool TryGetState(string questId, out QuestProgressState state)
        {
            return states.TryGetValue(questId ?? string.Empty, out state);
        }

        public void Restore(IEnumerable<QuestProgressState> restoredStates)
        {
            if (restoredStates == null)
            {
                return;
            }

            foreach (QuestProgressState restoredState in restoredStates)
            {
                if (restoredState == null ||
                    !definitions.TryGetValue(restoredState.QuestId, out QuestDefinition definition))
                {
                    continue;
                }

                foreach (QuestObjectiveDefinition objectiveDefinition in definition.Objectives)
                {
                    if (!restoredState.TryGetObjective(
                            objectiveDefinition.ObjectiveId,
                            out ObjectiveProgressState objectiveState) ||
                        objectiveState.RequiredCount != objectiveDefinition.RequiredCount)
                    {
                        throw new ArgumentException(
                            $"Quest SaveのObjective構成が現在のDefinitionと一致しません: {definition.QuestId}",
                            nameof(restoredStates));
                    }
                }

                if (restoredState.Objectives.Count != definition.Objectives.Count)
                {
                    throw new ArgumentException(
                        $"Quest SaveのObjective数が現在のDefinitionと一致しません: {definition.QuestId}",
                        nameof(restoredStates));
                }

                states[definition.QuestId] = restoredState;
            }
        }

        public bool AcceptQuest(string questId)
        {
            if (!TryGetState(questId, out QuestProgressState state) || !state.Accept())
            {
                return false;
            }

            QuestAccepted?.Invoke(state);
            return true;
        }

        public bool CompleteQuest(string questId)
        {
            if (!TryGetState(questId, out QuestProgressState state) || !state.Complete())
            {
                return false;
            }

            QuestCompleted?.Invoke(state);
            return true;
        }

        public void Handle(GameplayEvent gameplayEvent)
        {
            foreach (QuestDefinition definition in definitions.Values)
            {
                QuestProgressState state = states[definition.QuestId];
                if (!state.IsActive)
                {
                    continue;
                }

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

                    bool becameReadyToTurnIn = state.TryMarkReadyToTurnIn();
                    ProgressChanged?.Invoke(new QuestProgressUpdate(
                        definition.QuestId,
                        objectiveDefinition.ObjectiveId,
                        previousCount,
                        objectiveState.CurrentCount,
                        becameReadyToTurnIn));
                    if (becameReadyToTurnIn)
                    {
                        QuestReadyToTurnIn?.Invoke(state);
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
