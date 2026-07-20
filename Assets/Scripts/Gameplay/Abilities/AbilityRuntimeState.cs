using UnityEngine;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// キャラクター個体ごとに変化するAbilityの状態です。Definitionは一切変更しません。
    /// </summary>
    public sealed class AbilityRuntimeState
    {
        public AbilityRuntimeState(string abilityId)
        {
            AbilityId = abilityId ?? string.Empty;
        }

        public string AbilityId { get; }
        public float CooldownRemaining { get; private set; }
        public int UseCount { get; private set; }
        public bool IsExecuting { get; private set; }
        public bool IsOnCooldown => CooldownRemaining > 0f;

        internal void Advance(float deltaTime)
        {
            if (deltaTime <= 0f || CooldownRemaining <= 0f)
            {
                return;
            }

            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - deltaTime);
        }

        internal void BeginExecution()
        {
            IsExecuting = true;
        }

        internal void CommitUse(float cooldownSeconds, bool executionCompleted)
        {
            UseCount++;
            CooldownRemaining = Mathf.Max(0f, cooldownSeconds);
            IsExecuting = !executionCompleted;
        }

        internal void CancelExecution()
        {
            IsExecuting = false;
        }

        internal void CompleteExecution()
        {
            IsExecuting = false;
        }
    }
}
