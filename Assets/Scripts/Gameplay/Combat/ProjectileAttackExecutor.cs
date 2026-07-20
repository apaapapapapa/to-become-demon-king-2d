using System;
using System.Collections.Generic;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Combat.Configuration;
using DemonKing.Gameplay.Modifiers;
using UnityEngine;

namespace DemonKing.Gameplay.Combat
{
    public readonly struct ProjectileAttackEvent
    {
        internal ProjectileAttackEvent(
            GameObject projectile,
            ProjectileAttackDefinition definition,
            Vector2 direction)
        {
            Projectile = projectile;
            Definition = definition;
            Direction = direction;
        }

        public GameObject Projectile { get; }
        public ProjectileAttackDefinition Definition { get; }
        public Vector2 Direction { get; }
    }

    /// <summary>
    /// ProjectileAttackDefinitionを移動する攻撃インスタンスへ変換します。
    /// Art習得状態や入力元を知らず、命中時は共通効果成立通知だけを返します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileAttackExecutor : MonoBehaviour, IAbilityExecutor
    {
        [SerializeField] private LayerMask attackLayers = ~0;

        public event Action<ProjectileAttackEvent> ProjectileSpawned;

        public bool Supports(AbilityDefinition definition)
        {
            return definition is ProjectileAttackDefinition;
        }

        public bool CanExecute(AbilityExecutionRequest request)
        {
            return request.User != null &&
                   request.Definition is ProjectileAttackDefinition definition &&
                   definition.IsConfigured;
        }

        public AbilityExecutionResult Execute(AbilityExecutionRequest request)
        {
            if (!CanExecute(request))
            {
                throw new ArgumentException(
                    "Projectile攻撃として実行できないAbility要求です。",
                    nameof(request));
            }

            var definition = (ProjectileAttackDefinition)request.Definition;
            Vector2 direction = request.Input.Direction;
            Vector2 origin = (Vector2)request.User.transform.position + direction * 0.42f;
            GameObject projectile = new($"Projectile: {definition.DisplayName}");
            projectile.transform.position = origin;
            ProjectileAttackInstance instance =
                projectile.AddComponent<ProjectileAttackInstance>();
            instance.Initialize(
                request,
                definition,
                attackLayers,
                ResolveDamage(request.User, definition),
                request.User.GetComponent<AbilityController>());
            ProjectileSpawned?.Invoke(new ProjectileAttackEvent(
                projectile,
                definition,
                direction));
            return AbilityExecutionResult.Running;
        }

        private static int ResolveDamage(
            GameObject user,
            ProjectileAttackDefinition definition)
        {
            NumericModifier modifier = NumericModifier.Identity;
            foreach (MonoBehaviour behaviour in user.GetComponents<MonoBehaviour>())
            {
                if (behaviour is IOutgoingDamageModifierSource source)
                {
                    modifier = modifier.Combine(
                        source.GetOutgoingDamageModifier(definition));
                }
            }

            return modifier.Apply(definition.Damage);
        }
    }

    /// <summary>
    /// 1回のProjectile実行を移動・命中・終了まで追跡します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileAttackInstance : MonoBehaviour
    {
        private readonly HashSet<IDamageable> checkedTargets = new();
        private AbilityExecutionRequest request;
        private ProjectileAttackDefinition definition;
        private LayerMask attackLayers;
        private AbilityController abilityController;
        private Vector2 direction;
        private Vector2 origin;
        private int damage;
        private bool initialized;
        private bool resolved;

        public float TravelledDistance { get; private set; }

        internal void Initialize(
            AbilityExecutionRequest executionRequest,
            ProjectileAttackDefinition attackDefinition,
            LayerMask layers,
            int resolvedDamage,
            AbilityController controller)
        {
            request = executionRequest;
            definition = attackDefinition;
            attackLayers = layers;
            damage = resolvedDamage;
            abilityController = controller;
            direction = request.Input.Direction;
            origin = transform.position;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized || resolved)
            {
                return;
            }

            float step = definition.Speed * Time.deltaTime;
            Vector2 nextPosition = (Vector2)transform.position + direction * step;
            if (TryHit(nextPosition))
            {
                return;
            }

            transform.position = nextPosition;
            TravelledDistance = Vector2.Distance(origin, nextPosition);
            if (TravelledDistance >= definition.MaxDistance)
            {
                Resolve(wasApplied: false);
            }
        }

        private bool TryHit(Vector2 center)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                center,
                definition.CollisionRadius,
                attackLayers);
            foreach (Collider2D collider in colliders)
            {
                if (collider == null ||
                    collider.transform.IsChildOf(request.User.transform))
                {
                    continue;
                }

                foreach (MonoBehaviour behaviour in
                         collider.GetComponentsInParent<MonoBehaviour>(false))
                {
                    if (behaviour is not IDamageable damageable ||
                        !damageable.IsAlive ||
                        !checkedTargets.Add(damageable))
                    {
                        continue;
                    }

                    Health sourceHealth = request.User.GetComponent<Health>();
                    var damageRequest = new DamageRequest(
                        damage,
                        request.User,
                        sourceHealth == null ? string.Empty : sourceHealth.ActorId,
                        definition.AbilityId,
                        definition.DamageType,
                        definition.DamageTags,
                        request.ExecutionId);
                    DamageResult result = damageable.ApplyDamage(damageRequest);
                    Resolve(result.WasApplied);
                    return true;
                }
            }

            return false;
        }

        private void Resolve(bool wasApplied)
        {
            if (resolved)
            {
                return;
            }

            resolved = true;
            request.ReportEffect(AbilityEffectKind.Damage, wasApplied);
            if (abilityController != null)
            {
                abilityController.CompleteExecution(
                    definition.AbilityId,
                    request.User);
            }

            Destroy(gameObject);
        }
    }
}
