using DemonKing.Domain;
using DemonKing.Gameplay.Combat.Configuration;
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
        [SerializeField] private MeleeAttackDefinition basicMeleeAttackDefinition;
        [SerializeField] private DodgeDefinition dodgeDefinition;
        [SerializeField] private ExperienceTableDefinition experienceTableDefinition;

        public string CharacterId => characterId;
        public GameObject Prefab => prefab;
        public CharacterStatsDefinition StatsDefinition => statsDefinition;
        public MeleeAttackDefinition BasicMeleeAttackDefinition => basicMeleeAttackDefinition;
        public DodgeDefinition DodgeDefinition => dodgeDefinition;
        public ExperienceTableDefinition ExperienceTableDefinition => experienceTableDefinition;

        public bool IsConfigured =>
            StableContentId.IsValid(characterId) &&
            prefab != null &&
            statsDefinition != null &&
            basicMeleeAttackDefinition != null &&
            basicMeleeAttackDefinition.IsConfigured &&
            dodgeDefinition != null &&
            experienceTableDefinition != null &&
            experienceTableDefinition.IsConfigured;

        private void OnValidate()
        {
            characterId = StableContentId.Normalize(characterId);
        }
    }
}
