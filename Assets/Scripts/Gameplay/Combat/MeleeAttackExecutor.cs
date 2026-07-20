using System;
using System.Collections.Generic;
using DemonKing.Core.Math;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Combat.Configuration;
using DemonKing.Gameplay.Modifiers;
using UnityEngine;

namespace DemonKing.Gameplay.Combat
{
    /// <summary>
    /// MeleeAttackDefinitionの効果だけを発生させるExecutorです。
    /// 入力元、Art進捗、Skill取得状態、Evolution条件、クールダウン管理には依存しません。
    /// 命中判定は3D Physicsを使用し、Elevationが離れた対象には命中しません。
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
            Vector2 origin = FieldSpace3D.ToPlanar(user.transform.position);
            Vector2 center = origin + facingDirection * definition.AttackDistance;
            Vector3 physicsCenter = FieldSpace3D.Planar(center, user.transform.position.z);
            Health sourceHealth = user.GetComponent<Health>();
            var damageRequest = new DamageRequest(
                ResolveDamage(user, definition),
                user,
                sourceHealth == null ? string.Empty : sourceHealth.ActorId,
                definition.AbilityId,
                definition.DamageType,
                definition.DamageTags,
                request.ExecutionId);

            damagedTargets.Clear();
            int hitCount = Apply3DHits(
                physicsCenter,
                definition.AttackRadius,
                user,
                damageRequest,
                request);

            AttackPerformed?.Invoke(new MeleeAttackEvent(
                origin,
                center,
                facingDirection,
                definition.AttackRadius,
                hitCount));

            return AbilityExecutionResult.Completed;
        }

        private int Apply3DHits(
            Vector3 center,
            float radius,
            GameObject user,
            DamageRequest damageRequest,
            AbilityExecutionRequest executionRequest)
        {
            Collider[] colliders = Physics.OverlapSphere(
                center,
                radius,
                attackLayers,
                QueryTriggerInteraction.Collide);
            int hitCount = 0;

            foreach (Collider collider in colliders)
            {
                if (collider == null || collider.transform.IsChildOf(user.transform))
                {
                    continue;
                }

                hitCount += ApplyDamageFromBehaviours(
                    collider.GetComponentsInParent<MonoBehaviour>(false),
                    damageRequest,
                    executionRequest);
            }

            return hitCount;
        }

        private int ApplyDamageFromBehaviours(
            MonoBehaviour[] behaviours,
            DamageRequest damageRequest,
            AbilityExecutionRequest executionRequest)
        {
            int hitCount = 0;
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is not IDamageable damageable ||
                    !damageable.IsAlive ||
                    !damagedTargets.Add(damageable))
                {
                    continue;
                }

                DamageResult result = damageable.ApplyDamage(damageRequest);
                executionRequest.ReportEffect(AbilityEffectKind.Damage, result.WasApplied);
                if (result.WasApplied)
                {
                    hitCount++;
                }
            }

            return hitCount;
        }

        private static int ResolveDamage(GameObject user, AbilityDefinition definition)
        {
            NumericModifier modifier = NumericModifier.Identity;
            MonoBehaviour[] behaviours = user.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IOutgoingDamageModifierSource source)
                {
                    modifier = modifier.Combine(
                        source.GetOutgoingDamageModifier(definition));
                }
            }

            return modifier.Apply(((MeleeAttackDefinition)definition).Damage);
        }
    }
}
