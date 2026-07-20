using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Characters.Configuration
{
    /// <summary>
    /// キャラクターを生成するための不変なコンテンツ定義です。
    /// Prefabと基礎Gameplay Definitionを集約し、Spawnerの引数増加を防ぎます。
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Demon King/Gameplay/Character Definition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [SerializeField] private string characterId = string.Empty;
        [SerializeField] private GameObject prefab;
        [SerializeField] private CharacterStatsDefinition statsDefinition;
        [SerializeField] private AbilityDefinition[] abilityDefinitions = Array.Empty<AbilityDefinition>();
        [SerializeField] private DodgeDefinition dodgeDefinition;
        [SerializeField] private ExperienceTableDefinition experienceTableDefinition;

        public string CharacterId => characterId;
        public GameObject Prefab => prefab;
        public CharacterStatsDefinition StatsDefinition => statsDefinition;
        public IReadOnlyList<AbilityDefinition> AbilityDefinitions => abilityDefinitions;
        public DodgeDefinition DodgeDefinition => dodgeDefinition;
        public ExperienceTableDefinition ExperienceTableDefinition => experienceTableDefinition;

        public bool IsConfigured
        {
            get
            {
                if (!StableContentId.IsValid(characterId) ||
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

                return true;
            }
        }

        private void OnValidate()
        {
            characterId = StableContentId.Normalize(characterId);
        }
    }
}
