using DemonKing.Gameplay.Abilities.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Combat.Configuration
{
    /// <summary>
    /// 近接攻撃Ability固有のゲームバランス値を定義します。
    /// Ability共通情報は基底のAbilityDefinitionが保持します。
    /// </summary>
    [CreateAssetMenu(fileName = "MeleeAttack", menuName = "Demon King/Gameplay/Abilities/Melee Attack")]
    public sealed class MeleeAttackDefinition : AbilityDefinition
    {
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.1f)] private float attackRadius = 0.65f;
        [SerializeField, Min(0f)] private float attackDistance = 0.65f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private DamageTags damageTags = DamageTags.BasicAttack;

        public int Damage => damage;
        public float AttackRadius => attackRadius;
        public float AttackDistance => attackDistance;
        public DamageType DamageType => damageType;
        public DamageTags DamageTags => damageTags;
        protected override bool IsAbilitySpecificConfigurationValid =>
            damage > 0 &&
            attackRadius > 0f &&
            attackDistance >= 0f;
    }
}
