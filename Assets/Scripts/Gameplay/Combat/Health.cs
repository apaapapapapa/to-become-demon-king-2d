using System;
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

        public event Action<int, int> HealthChanged;
        public event Action<int, GameObject> Damaged;
        public event Action<GameObject> Died;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;

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

        public void TakeDamage(int amount, GameObject source)
        {
            if (!IsAlive || amount <= 0)
            {
                return;
            }

            int appliedDamage = Mathf.Min(amount, currentHealth);
            currentHealth -= appliedDamage;

            Damaged?.Invoke(appliedDamage, source);
            HealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth == 0)
            {
                Died?.Invoke(source);
            }
        }

        public void RestoreToFull()
        {
            currentHealth = maxHealth;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
