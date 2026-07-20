using System;
using DemonKing.Gameplay.Abilities.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// 入力やAIがAbilityへ渡す、デバイス非依存の実行パラメーターです。
    /// </summary>
    public readonly struct AbilityExecutionInput
    {
        public AbilityExecutionInput(Vector2 direction)
        {
            Direction = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector2.down;
        }

        public Vector2 Direction { get; }
    }

    /// <summary>
    /// Executorへ渡す実行要求です。使用者を暗黙に推測せず、必ず明示します。
    /// </summary>
    public readonly struct AbilityExecutionRequest
    {
        private readonly Action<AbilityEffectResolved> effectReporter;

        internal AbilityExecutionRequest(
            Guid executionId,
            GameObject user,
            AbilityDefinition definition,
            AbilityExecutionInput input,
            Action<AbilityEffectResolved> effectReporter)
        {
            ExecutionId = executionId;
            User = user;
            Definition = definition;
            Input = input;
            this.effectReporter = effectReporter;
        }

        public Guid ExecutionId { get; }
        public GameObject User { get; }
        public AbilityDefinition Definition { get; }
        public AbilityExecutionInput Input { get; }

        public void ReportEffect(AbilityEffectKind effectKind, bool wasApplied)
        {
            if (ExecutionId == Guid.Empty || Definition == null || effectReporter == null)
            {
                return;
            }

            effectReporter.Invoke(new AbilityEffectResolved(
                ExecutionId,
                User,
                Definition.AbilityId,
                effectKind,
                wasApplied));
        }
    }

    public enum AbilityEffectKind
    {
        Damage = 0,
        Healing = 1,
        Buff = 2,
        Debuff = 3,
        Other = 4
    }

    /// <summary>
    /// Abilityの実効果が成立したかを、成長や演出へ通知する共通結果です。
    /// </summary>
    public readonly struct AbilityEffectResolved
    {
        internal AbilityEffectResolved(
            Guid executionId,
            GameObject user,
            string abilityId,
            AbilityEffectKind effectKind,
            bool wasApplied)
        {
            ExecutionId = executionId;
            User = user;
            AbilityId = abilityId ?? string.Empty;
            EffectKind = effectKind;
            WasApplied = wasApplied;
        }

        public Guid ExecutionId { get; }
        public GameObject User { get; }
        public string AbilityId { get; }
        public AbilityEffectKind EffectKind { get; }
        public bool WasApplied { get; }
    }

    /// <summary>
    /// Executorが開始した処理をControllerへ返します。
    /// </summary>
    public readonly struct AbilityExecutionResult
    {
        public AbilityExecutionResult(bool isComplete)
        {
            IsComplete = isComplete;
        }

        public bool IsComplete { get; }

        public static AbilityExecutionResult Completed => new(true);
        public static AbilityExecutionResult Running => new(false);
    }

    public enum AbilityUseStatus
    {
        Succeeded = 0,
        InvalidUser = 1,
        UserMismatch = 2,
        InvalidAbilityId = 3,
        AbilityNotGranted = 4,
        CooldownActive = 5,
        AlreadyExecuting = 6,
        CostUnavailable = 7,
        ExecutorUnavailable = 8,
        ExecutorRejected = 9
    }

    /// <summary>
    /// Abilityの使用可否または実行結果を、入力・AI・演出へ返す値です。
    /// </summary>
    public readonly struct AbilityUseResult
    {
        internal AbilityUseResult(
            AbilityUseStatus status,
            string abilityId,
            AbilityRuntimeState runtimeState,
            Guid executionId = default)
        {
            Status = status;
            AbilityId = abilityId ?? string.Empty;
            ExecutionId = executionId;
            CooldownRemaining = runtimeState?.CooldownRemaining ?? 0f;
            UseCount = runtimeState?.UseCount ?? 0;
            IsExecuting = runtimeState?.IsExecuting ?? false;
        }

        public AbilityUseStatus Status { get; }
        public string AbilityId { get; }
        public Guid ExecutionId { get; }
        public float CooldownRemaining { get; }
        public int UseCount { get; }
        public bool IsExecuting { get; }
        public bool Succeeded => Status == AbilityUseStatus.Succeeded;
    }
}
