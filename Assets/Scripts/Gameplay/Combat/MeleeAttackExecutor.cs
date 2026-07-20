using System;
using System.Collections.Generic;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Combat.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Combat
{
    /// <summary>
    /// MeleeAttackDefinitionの効果だけを発生させるExecutorです。
    /// 入力元、Skill取得状態、Evolution条件、クールダウン管理には依存しません。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MeleeAttackExecutor : MonoBehaviour, IAbilityExecutor
    {
        [SerializeField] private LayerMask attackLayers = ~0;

        private readonly HashSet<IDamageable> damagedTargets = new();

        public event Action<MeleeAttackEvent> AttackPerformed;

        public bool Supports(AbilityDefinition definition)
        {
            return definition is MeleeAttackDefinition;
        }

        public bool CanExecute(AbilityExecutionRequest request)
        {
            return request.User != null &&
                   request.Definition is MeleeAttackDefinition definition &&
                   definition.IsConfigured;
        }

        public AbilityExecutionResult Execute(AbilityExecutionRequest request)
        {
            if (!CanExecute(request))
            {
                throw new ArgumentException("近接攻撃として実行できないAbility要求です。", nameof(request));
            }

            var definition = (MeleeAttackDefinition)request.Definition;
            GameObject user = request.User;
            Vector2 facingDirection = request.Input.Direction;
            Vector2 origin = user.transform.position;
            Vector2 center = origin + facingDirection * definition.AttackDistance;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                center,
                definition.AttackRadius,
                attackLayers);
            Health sourceHealth = user.GetComponent<Health>();
            var damageRequest = new DamageRequest(
                definition.Damage,
                user,
                sourceHealth == null ? string.Empty : sourceHealth.ActorId,
                definition.AbilityId,
                definition.DamageType,
                DamageTags.BasicAttack);

            damagedTargets.Clear();
            int hitCount = 0;

            foreach (Collider2D collider in colliders)
            {
                if (collider == null || collider.transform.IsChildOf(user.transform))
                {
                    continue;
                }

                MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>(false);
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour is not IDamageable damageable ||
                        !damageable.IsAlive ||
                        !damagedTargets.Add(damageable))
                    {
                        continue;
                    }

                    DamageResult result = damageable.ApplyDamage(damageRequest);
                    if (result.WasApplied)
                    {
                        hitCount++;
                    }
                }
            }

            AttackPerformed?.Invoke(new MeleeAttackEvent(
                origin,
                center,
                facingDirection,
                definition.AttackRadius,
                hitCount));

            return AbilityExecutionResult.Completed;
        }
    }
}
