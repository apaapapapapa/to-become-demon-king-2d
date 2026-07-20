using System;
using DemonKing.Core.Math;
using DemonKing.Gameplay.AI.Configuration;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Combat;
using UnityEngine;

namespace DemonKing.Gameplay.AI
{
    public enum EnemyAiState
    {
        Idle = 0,
        Chasing = 1,
        Attacking = 2
    }

    /// <summary>
    /// 敵個体の索敵、追跡、攻撃状態を管理します。
    /// 移動はCharacterPhysicsBody3D、攻撃はAbilityControllerへ委譲し、AI自身は物理・ダメージ処理を実装しません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterPhysicsBody3D))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AbilityController))]
    [RequireComponent(typeof(MeleeAttackExecutor))]
    public sealed class EnemyAiController : MonoBehaviour
    {
        [SerializeField] private EnemyAiDefinition definition;

        private CharacterPhysicsBody3D physicsBody;
        private AbilityController abilityController;
        private Health health;
        private GameObject target;
        private Health targetHealth;
        private bool engaged;
        private Vector2 facingDirection = Vector2.down;

        public event Action<EnemyAiState> StateChanged;

        public EnemyAiState State { get; private set; } = EnemyAiState.Idle;
        public GameObject Target => target;
        public bool IsConfigured => definition != null && definition.IsConfigured && target != null;

        private void Awake()
        {
            physicsBody = GetComponent<CharacterPhysicsBody3D>();
            physicsBody.EnsureConfigured();
            abilityController = GetComponent<AbilityController>();
            health = GetComponent<Health>();
        }

        private void FixedUpdate()
        {
            if (!CanAct())
            {
                engaged = false;
                SetState(EnemyAiState.Idle);
                return;
            }

            Vector2 selfPlanar = FieldSpace3D.ToPlanar(transform.position);
            Vector2 targetPlanar = FieldSpace3D.ToPlanar(target.transform.position);
            Vector2 offset = targetPlanar - selfPlanar;
            float planarDistance = offset.magnitude;
            float elevationDifference = Mathf.Abs(
                ResolveElevation(target) - ResolveElevation(gameObject));

            float trackingRange = engaged
                ? definition.DisengageRange
                : definition.DetectionRange;
            if (planarDistance > trackingRange ||
                elevationDifference > definition.MaxChaseElevationDifference)
            {
                engaged = false;
                SetState(EnemyAiState.Idle);
                return;
            }

            engaged = true;
            if (offset.sqrMagnitude > 0.0001f)
            {
                facingDirection = offset.normalized;
            }

            if (planarDistance <= definition.AttackRange &&
                elevationDifference <= definition.MaxAttackElevationDifference)
            {
                SetState(EnemyAiState.Attacking);
                TryAttack();
                return;
            }

            SetState(EnemyAiState.Chasing);
            if (offset.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            physicsBody.QueuePlanarDelta(
                facingDirection * definition.MoveSpeed * Time.fixedDeltaTime);
        }

        public void Configure(EnemyAiDefinition aiDefinition, GameObject newTarget)
        {
            if (aiDefinition == null || !aiDefinition.IsConfigured)
            {
                throw new ArgumentException(
                    "敵AI Definitionが正しく設定されていません。",
                    nameof(aiDefinition));
            }

            if (newTarget == null)
            {
                throw new ArgumentNullException(nameof(newTarget));
            }

            definition = aiDefinition;
            target = newTarget;
            targetHealth = target.GetComponent<Health>();
            engaged = false;
            SetState(EnemyAiState.Idle);

            abilityController.Configure(new[] { definition.PrimaryAttack });
            abilityController.RefreshExecutors();
        }

        public void SetTarget(GameObject newTarget)
        {
            target = newTarget;
            targetHealth = target == null ? null : target.GetComponent<Health>();
            engaged = false;
            SetState(EnemyAiState.Idle);
        }

        private bool CanAct()
        {
            if (!IsConfigured || health == null || !health.IsAlive)
            {
                return false;
            }

            return targetHealth == null || targetHealth.IsAlive;
        }

        private void TryAttack()
        {
            abilityController.TryUse(
                definition.PrimaryAttack.AbilityId,
                gameObject,
                new AbilityExecutionInput(facingDirection));
        }

        private static float ResolveElevation(GameObject actor)
        {
            CharacterElevationMotor elevationMotor = actor.GetComponent<CharacterElevationMotor>();
            return elevationMotor == null
                ? actor.transform.position.z
                : elevationMotor.Elevation;
        }

        private void SetState(EnemyAiState next)
        {
            if (State == next)
            {
                return;
            }

            State = next;
            StateChanged?.Invoke(State);
        }
    }
}
