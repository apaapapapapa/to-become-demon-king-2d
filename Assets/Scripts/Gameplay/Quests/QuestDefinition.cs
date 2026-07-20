using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Domain;
using UnityEngine;

namespace DemonKing.Gameplay.Quests.Configuration
{
    [Serializable]
    public sealed class QuestObjectiveDefinition
    {
        [SerializeField] private string objectiveId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string eventId = string.Empty;
        [SerializeField] private string subjectId = string.Empty;
        [SerializeField, Min(1)] private int requiredCount = 1;

        public string ObjectiveId => objectiveId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? objectiveId : displayName.Trim();
        public string EventId => eventId;
        public string SubjectId => subjectId;
        public int RequiredCount => Math.Max(1, requiredCount);
        public bool IsConfigured =>
            StableContentId.IsValid(objectiveId) &&
            StableContentId.IsValid(eventId) &&
            requiredCount > 0;

        public static QuestObjectiveDefinition CreateRuntime(
            string id,
            string requiredEventId,
            string requiredSubjectId = "",
            int count = 1,
            string name = "")
        {
            return new QuestObjectiveDefinition
            {
                objectiveId = StableContentId.Require(id, nameof(id)),
                displayName = string.IsNullOrWhiteSpace(name) ? StableContentId.Require(id, nameof(id)) : name.Trim(),
                eventId = StableContentId.Require(requiredEventId, nameof(requiredEventId)),
                subjectId = StableContentId.Normalize(requiredSubjectId),
                requiredCount = Math.Max(1, count),
            };
        }

        public bool Matches(string incomingEventId, string incomingSubjectId)
        {
            if (!string.Equals(eventId, incomingEventId, StringComparison.Ordinal))
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(subjectId) ||
                   string.Equals(subjectId, incomingSubjectId, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Questの静的な表示情報とObjective条件を定義します。
    /// Runtime進捗はDomainのQuestProgressStateへ分離し、Definition自体は変更しません。
    /// </summary>
    [CreateAssetMenu(
        fileName = "QuestDefinition",
        menuName = "Demon King/Gameplay/Quest Definition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        [SerializeField] private string questId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private QuestObjectiveDefinition[] objectives = Array.Empty<QuestObjectiveDefinition>();

        public string QuestId => questId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? questId : displayName.Trim();
        public IReadOnlyList<QuestObjectiveDefinition> Objectives => objectives ?? Array.Empty<QuestObjectiveDefinition>();
        public bool IsConfigured =>
            StableContentId.IsValid(questId) &&
            objectives != null &&
            objectives.Length > 0 &&
            objectives.All(objective => objective != null && objective.IsConfigured) &&
            objectives.Select(objective => objective.ObjectiveId).Distinct(StringComparer.Ordinal).Count() == objectives.Length;

        public static QuestDefinition CreateRuntime(
            string id,
            string name,
            params QuestObjectiveDefinition[] objectiveDefinitions)
        {
            QuestDefinition definition = CreateInstance<QuestDefinition>();
            definition.questId = StableContentId.Require(id, nameof(id));
            definition.displayName = string.IsNullOrWhiteSpace(name) ? definition.questId : name.Trim();
            definition.objectives = objectiveDefinitions == null
                ? Array.Empty<QuestObjectiveDefinition>()
                : objectiveDefinitions.ToArray();
            return definition;
        }

        private void OnValidate()
        {
            questId = StableContentId.Normalize(questId);
            displayName = string.IsNullOrWhiteSpace(displayName) ? questId : displayName.Trim();
            objectives ??= Array.Empty<QuestObjectiveDefinition>();
        }
    }
}
