using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Gameplay.Modifiers.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Progression.Configuration
{
    /// <summary>
    /// ArtランクをEvolution条件として参照する静的要件です。
    /// </summary>
    [Serializable]
    public sealed class EvolutionArtRankRequirement
    {
        [SerializeField] private string artId = string.Empty;
        [SerializeField, Min(1)] private int requiredRank = 1;

        public string ArtId => artId;
        public int RequiredRank => requiredRank;

        internal bool IsConfigured =>
            StableContentId.IsValid(artId) &&
            artId.StartsWith("art.", StringComparison.Ordinal) &&
            requiredRank >= 1;

        internal void Normalize()
        {
            artId = StableContentId.Normalize(artId);
            requiredRank = Mathf.Max(1, requiredRank);
        }
    }

    /// <summary>
    /// 不可逆または排他的な成長経路を表すEvolution Nodeの静的定義です。
    /// 選択状態と条件の現在値はCharacterProgressionStateへ分離します。
    /// </summary>
    [CreateAssetMenu(fileName = "Evolution", menuName = "Demon King/Gameplay/Progression/Evolution")]
    public sealed class EvolutionDefinition : ScriptableObject
    {
        private const string EvolutionIdPrefix = "evolution.";
        private const string ExclusiveGroupIdPrefix = "evolution-group.";

        [Header("Identity")]
        [SerializeField] private string evolutionNodeId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private Sprite icon;
        [SerializeField] private string characterDefinitionId = string.Empty;
        [SerializeField] private string exclusiveGroupId = string.Empty;

        [Header("Requirements")]
        [SerializeField, Min(1)] private int requiredLevel = 1;
        [SerializeField] private string[] requiredSkillIds = Array.Empty<string>();
        [SerializeField] private EvolutionArtRankRequirement[] requiredArtRanks =
            Array.Empty<EvolutionArtRankRequirement>();
        [SerializeField] private string[] requiredEvolutionNodeIds = Array.Empty<string>();

        [Header("Permanent Modifiers")]
        [SerializeField] private GameplayModifierEntry[] modifiers =
            Array.Empty<GameplayModifierEntry>();

        public string EvolutionNodeId => evolutionNodeId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public string CharacterDefinitionId => characterDefinitionId;
        public string ExclusiveGroupId => exclusiveGroupId;
        public int RequiredLevel => requiredLevel;
        public IReadOnlyList<string> RequiredSkillIds =>
            requiredSkillIds ?? Array.Empty<string>();
        public IReadOnlyList<EvolutionArtRankRequirement> RequiredArtRanks =>
            requiredArtRanks ?? Array.Empty<EvolutionArtRankRequirement>();
        public IReadOnlyList<string> RequiredEvolutionNodeIds =>
            requiredEvolutionNodeIds ?? Array.Empty<string>();
        public IReadOnlyList<GameplayModifierEntry> Modifiers =>
            modifiers ?? Array.Empty<GameplayModifierEntry>();

        public bool IsConfigured =>
            StableContentId.IsValid(evolutionNodeId) &&
            evolutionNodeId.StartsWith(EvolutionIdPrefix, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(displayName) &&
            StableContentId.IsValid(characterDefinitionId) &&
            characterDefinitionId.StartsWith("character.", StringComparison.Ordinal) &&
            StableContentId.IsValid(exclusiveGroupId) &&
            exclusiveGroupId.StartsWith(ExclusiveGroupIdPrefix, StringComparison.Ordinal) &&
            requiredLevel >= 1 &&
            HasValidIdRequirements(requiredSkillIds, "skill.", allowSelf: true) &&
            HasValidArtRequirements() &&
            HasValidIdRequirements(
                requiredEvolutionNodeIds,
                EvolutionIdPrefix,
                allowSelf: false) &&
            HasValidModifiers();

        private bool HasValidArtRequirements()
        {
            if (requiredArtRanks == null)
            {
                return false;
            }

            var knownIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (EvolutionArtRankRequirement requirement in requiredArtRanks)
            {
                if (requirement == null ||
                    !requirement.IsConfigured ||
                    !knownIds.Add(requirement.ArtId))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasValidIdRequirements(
            IEnumerable<string> ids,
            string prefix,
            bool allowSelf)
        {
            if (ids == null)
            {
                return false;
            }

            var knownIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in ids)
            {
                if (!StableContentId.IsValid(id) ||
                    !id.StartsWith(prefix, StringComparison.Ordinal) ||
                    (!allowSelf && string.Equals(id, evolutionNodeId, StringComparison.Ordinal)) ||
                    !knownIds.Add(id))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasValidModifiers()
        {
            if (modifiers == null)
            {
                return false;
            }

            foreach (GameplayModifierEntry modifier in modifiers)
            {
                if (modifier == null || !modifier.IsConfigured)
                {
                    return false;
                }
            }

            return true;
        }

        private void OnValidate()
        {
            evolutionNodeId = StableContentId.Normalize(evolutionNodeId);
            displayName = displayName?.Trim() ?? string.Empty;
            description = description?.Trim() ?? string.Empty;
            characterDefinitionId = StableContentId.Normalize(characterDefinitionId);
            exclusiveGroupId = StableContentId.Normalize(exclusiveGroupId);
            requiredLevel = Mathf.Max(1, requiredLevel);
            requiredSkillIds = NormalizeIds(requiredSkillIds);
            requiredEvolutionNodeIds = NormalizeIds(requiredEvolutionNodeIds);
            requiredArtRanks ??= Array.Empty<EvolutionArtRankRequirement>();
            foreach (EvolutionArtRankRequirement requirement in requiredArtRanks)
            {
                requirement?.Normalize();
            }

            modifiers ??= Array.Empty<GameplayModifierEntry>();
            foreach (GameplayModifierEntry modifier in modifiers)
            {
                modifier?.Normalize();
            }
        }

        private static string[] NormalizeIds(string[] ids)
        {
            if (ids == null)
            {
                return Array.Empty<string>();
            }

            for (int index = 0; index < ids.Length; index++)
            {
                ids[index] = StableContentId.Normalize(ids[index]);
            }

            return ids;
        }
    }
}
