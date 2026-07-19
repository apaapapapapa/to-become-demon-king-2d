using System;
using DemonKing.Domain;
using UnityEngine;

namespace DemonKing.Gameplay.Combat
{
    /// <summary>
    /// HP、ダメージ、死亡状態だけを管理する汎用コンポーネントです。
    /// 最大HPは外側のキャラクター設定から注入でき、敵AIや演出、報酬処理には依存しません。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHealth = 3;

        private int currentHealth;
        private string actorId = string.Empty;
        private string defeatRewardDefinitionId = string.Empty;

        public event Action<int, int> HealthChanged;
        public event Action<DamageResult> Damaged;
        public event Action<DefeatContext> Died;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
        public string ActorId => actorId;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void ConfigureMaxHealth(int value, bool restoreToFull = true)
        {
            maxHealth = Mathf.Max(1, value);
            currentHealth = restoreToFull
                ? maxHealth
                : Mathf.Clamp(currentHealth, 0, maxHealth);

            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ConfigureCombatIdentity(string stableActorId, string rewardDefinitionId = "")
        {
            actorId = StableContentId.Normalize(stableActorId);
            defeatRewardDefinitionId = StableContentId.Normalize(rewardDefinitionId);
        }

        public DamageResult ApplyDamage(DamageRequest request)
        {
            if (!IsAlive || !request.IsValid)
            {
                return new DamageResult(request, 0, currentHealth, gameObject, null);
            }

            int appliedDamage = Mathf.Min(request.Amount, currentHealth);
            currentHealth -= appliedDamage;

            DefeatContext defeatContext = currentHealth == 0
                ? new DefeatContext(request, gameObject, actorId, defeatRewardDefinitionId)
                : null;
            var result = new DamageResult(request, appliedDamage, currentHealth, gameObject, defeatContext);

            Damaged?.Invoke(result);
            HealthChanged?.Invoke(currentHealth, maxHealth);

            if (defeatContext != null)
            {
                Died?.Invoke(defeatContext);
            }

            return result;
        }

        public void RestoreToFull()
        {
            currentHealth = maxHealth;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
