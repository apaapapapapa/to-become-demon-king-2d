using DemonKing.Gameplay.Abilities.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Combat.Configuration
{
    /// <summary>
    /// 遠距離Projectile Ability固有の静的な戦闘値です。
    /// Projectileの移動状態と命中状態は実行時インスタンスへ分離します。
    /// </summary>
    [CreateAssetMenu(
        fileName = "ProjectileAttack",
        menuName = "Demon King/Gameplay/Abilities/Projectile Attack")]
    public sealed class ProjectileAttackDefinition : AbilityDefinition
    {
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.1f)] private float speed = 4f;
        [SerializeField, Min(0.1f)] private float maxDistance = 6f;
        [SerializeField, Min(0.05f)] private float collisionRadius = 0.18f;
        [SerializeField] private DamageType damageType = DamageType.Magical;
        [SerializeField] private DamageTags damageTags = DamageTags.Art;

        public int Damage => damage;
        public float Speed => speed;
        public float MaxDistance => maxDistance;
        public float CollisionRadius => collisionRadius;
        public DamageType DamageType => damageType;
        public DamageTags DamageTags => damageTags;

        protected override bool IsAbilitySpecificConfigurationValid =>
            damage > 0 &&
            speed > 0f &&
            maxDistance > 0f &&
            collisionRadius > 0f;
    }
}
