using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Content;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Characters.Configuration
{
    /// <summary>
    /// キャラクターを生成するための不変なコンテンツ定義です。
    /// Prefabと基礎Gameplay Definitionを集約し、Spawnerの引数増加を防ぎます。
    /// 図鑑で表示する静的説明もこのDefinitionをSource of Truthとします。
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Demon King/Gameplay/Character Definition")]
    public sealed class CharacterDefinition : ScriptableObject, IGameContentDefinition
    {
        [Header("Identity")]
        [SerializeField] private string characterId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField, TextArea] private string encyclopediaDescription = string.Empty;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool visibleInEncyclopedia = true;

        [Header("Gameplay")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private CharacterStatsDefinition statsDefinition;
        [SerializeField] private AbilityDefinition[] abilityDefinitions = Array.Empty<AbilityDefinition>();
        [SerializeField] private ArtDefinition[] artDefinitions = Array.Empty<ArtDefinition>();
        [SerializeField] private SkillDefinition[] skillDefinitions = Array.Empty<SkillDefinition>();
        [SerializeField] private EvolutionDefinition[] evolutionDefinitions =
            Array.Empty<EvolutionDefinition>();
        [SerializeField] private DodgeDefinition dodgeDefinition;
        [SerializeField] private ExperienceTableDefinition experienceTableDefinition;

        public string ContentId => characterId;
        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public string Description => description;
        public string EncyclopediaDescription => encyclopediaDescription;
        public Sprite Icon => icon;
        public bool VisibleInEncyclopedia => visibleInEncyclopedia;
        public GameObject Prefab => prefab;
        public CharacterStatsDefinition StatsDefinition => statsDefinition;
        public IReadOnlyList<AbilityDefinition> AbilityDefinitions => abilityDefinitions;
        public IReadOnlyList<ArtDefinition> ArtDefinitions =>
            artDefinitions ?? Array.Empty<ArtDefinition>();
        public IReadOnlyList<SkillDefinition> SkillDefinitions =>
            skillDefinitions ?? Array.Empty<SkillDefinition>();
        public IReadOnlyList<EvolutionDefinition> EvolutionDefinitions =>
            evolutionDefinitions ?? Array.Empty<EvolutionDefinition>();
        public DodgeDefinition DodgeDefinition => dodgeDefinition;
        public ExperienceTableDefinition ExperienceTableDefinition => experienceTableDefinition;

        public bool IsConfigured
        {
            get
            {
                if (!StableContentId.IsValid(characterId) ||
                    (visibleInEncyclopedia && string.IsNullOrWhiteSpace(displayName)) ||
                    prefab == null ||
                    statsDefinition == null ||
                    abilityDefinitions == null ||
                    dodgeDefinition == null ||
                    experienceTableDefinition == null ||
                    !experienceTableDefinition.IsConfigured)
                {
                    return false;
                }

                var abilityIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (AbilityDefinition definition in abilityDefinitions)
                {
                    if (definition == null ||
                        !definition.IsConfigured ||
                        !abilityIds.Add(definition.AbilityId))
                    {
                        return false;
                    }
                }

                var artIds = new HashSet<string>(StringComparer.Ordinal);
                var artAbilityIds = new HashSet<string>(StringComparer.Ordinal);
                if (artDefinitions != null)
                {
                    foreach (ArtDefinition definition in artDefinitions)
                    {
                        if (definition == null ||
                            !definition.IsConfigured ||
                            !artIds.Add(definition.ArtId))
                        {
                            return false;
                        }

                        foreach (ArtAbilityUnlockEntry entry in definition.AbilityUnlocks)
                        {
                            if (!artAbilityIds.Add(entry.AbilityDefinition.AbilityId))
                            {
                                return false;
                            }
                        }
                    }
                }

                var skillIds = new HashSet<string>(StringComparer.Ordinal);
                if (skillDefinitions != null)
                {
                    foreach (SkillDefinition definition in skillDefinitions)
                    {
                        if (definition == null ||
                            !definition.IsConfigured ||
                            !skillIds.Add(definition.SkillId))
                        {
                            return false;
                        }
                    }
                }

                var evolutionIds = new HashSet<string>(StringComparer.Ordinal);
                if (evolutionDefinitions != null)
                {
                    foreach (EvolutionDefinition definition in evolutionDefinitions)
                    {
                        if (definition == null ||
                            !definition.IsConfigured ||
                            !string.Equals(
                                definition.CharacterDefinitionId,
                                characterId,
                                StringComparison.Ordinal) ||
                            !evolutionIds.Add(definition.EvolutionNodeId))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        private void OnValidate()
        {
            characterId = StableContentId.Normalize(characterId);
            displayName = displayName?.Trim() ?? string.Empty;
            description = description?.Trim() ?? string.Empty;
            encyclopediaDescription = encyclopediaDescription?.Trim() ?? string.Empty;
        }
    }
}
