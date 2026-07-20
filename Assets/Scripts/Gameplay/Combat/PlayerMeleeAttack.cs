using System;
using System.Collections.Generic;
using DemonKing.Core.Input;
using DemonKing.Gameplay.Combat.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Combat
{
    /// <summary>
    /// プレイヤーのAttack入力を受け取り、向いている方向の近距離対象へダメージを与えます。
    /// ダメージ量と攻撃範囲はMeleeAttackDefinitionから取得し、敵種別や死亡演出には依存しません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerMeleeAttack : MonoBehaviour
    {
        private const int DefaultDamage = 1;
        private const float DefaultAttackRadius = 0.65f;
        private const float DefaultAttackDistance = 0.65f;

        [SerializeField] private MeleeAttackDefinition attackDefinition;
        [SerializeField] private LayerMask attackLayers = ~0;

        private readonly HashSet<IDamageable> damagedTargets = new();
        private PlayerInputReader inputReader;
        private Vector2 facingDirection = Vector2.down;

        public event Action<MeleeAttackEvent> AttackPerformed;

        private int Damage => attackDefinition == null ? DefaultDamage : attackDefinition.Damage;
        private float AttackRadius => attackDefinition == null ? DefaultAttackRadius : attackDefinition.AttackRadius;
        private float AttackDistance => attackDefinition == null ? DefaultAttackDistance : attackDefinition.AttackDistance;
        private string AbilityId => attackDefinition == null ? "ability.basic_melee" : attackDefinition.AbilityId;
        private DamageType AttackDamageType =>
            attackDefinition == null ? DamageType.Physical : attackDefinition.DamageType;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
        }

        private void OnEnable()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (inputReader != null)
            {
                inputReader.AttackPressed += HandleAttackPressed;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.AttackPressed -= HandleAttackPressed;
            }
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            Vector2 move = inputReader.Move;
            if (move.sqrMagnitude > 0.0001f)
            {
                facingDirection = move.normalized;
            }
        }

        public void Configure(MeleeAttackDefinition definition)
        {
            attackDefinition = definition;
        }

        public void PerformAttack()
        {
            Vector2 center = (Vector2)transform.position + facingDirection * AttackDistance;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, AttackRadius, attackLayers);
            Health sourceHealth = GetComponent<Health>();
            var request = new DamageRequest(
                Damage,
                gameObject,
                sourceHealth == null ? string.Empty : sourceHealth.ActorId,
                AbilityId,
                AttackDamageType,
                DamageTags.BasicAttack);

            damagedTargets.Clear();
            int hitCount = 0;

            foreach (Collider2D collider in colliders)
            {
                if (collider == null || collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>(false);
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    IDamageable damageable = behaviour as IDamageable;
                    if (damageable == null || !damageable.IsAlive || !damagedTargets.Add(damageable))
                    {
                        continue;
                    }

                    DamageResult result = damageable.ApplyDamage(request);
                    if (result.WasApplied)
                    {
                        hitCount++;
                    }
                }
            }

            AttackPerformed?.Invoke(new MeleeAttackEvent(
                transform.position,
                center,
                facingDirection,
                AttackRadius,
                hitCount));
        }

        private void HandleAttackPressed() => PerformAttack();

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector2 center = (Vector2)transform.position + facingDirection * AttackDistance;
            Gizmos.DrawWireSphere(center, AttackRadius);
        }
#endif
    }
}
