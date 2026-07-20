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
        public AbilityExecutionRequest(
            GameObject user,
            AbilityDefinition definition,
            AbilityExecutionInput input)
        {
            User = user;
            Definition = definition;
            Input = input;
        }

        public GameObject User { get; }
        public AbilityDefinition Definition { get; }
        public AbilityExecutionInput Input { get; }
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
            AbilityRuntimeState runtimeState)
        {
            Status = status;
            AbilityId = abilityId ?? string.Empty;
            CooldownRemaining = runtimeState?.CooldownRemaining ?? 0f;
            UseCount = runtimeState?.UseCount ?? 0;
            IsExecuting = runtimeState?.IsExecuting ?? false;
        }

        public AbilityUseStatus Status { get; }
        public string AbilityId { get; }
        public float CooldownRemaining { get; }
        public int UseCount { get; }
        public bool IsExecuting { get; }
        public bool Succeeded => Status == AbilityUseStatus.Succeeded;
    }
}
