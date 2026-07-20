using DemonKing.Gameplay.Abilities.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.AI.Configuration
{
    /// <summary>
    /// 地上敵AIの索敵・追跡・攻撃に使う静的な調整値を保持します。
    /// 実行時状態やTarget参照は保持せず、個体ごとの状態はEnemyAiControllerが所有します。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyAi", menuName = "Demon King/AI/Enemy AI Definition")]
    public sealed class EnemyAiDefinition : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float detectionRange = 1.4f;
        [SerializeField, Min(0.1f)] private float disengageRange = 4.5f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.05f;
        [SerializeField, Min(0.1f)] private float moveSpeed = 1.8f;
        [SerializeField, Min(0f)] private float maxChaseElevationDifference = 1.25f;
        [SerializeField, Min(0f)] private float maxAttackElevationDifference = 0.8f;
        [SerializeField] private AbilityDefinition primaryAttack;

        public float DetectionRange => detectionRange;
        public float DisengageRange => disengageRange;
        public float AttackRange => attackRange;
        public float MoveSpeed => moveSpeed;
        public float MaxChaseElevationDifference => maxChaseElevationDifference;
        public float MaxAttackElevationDifference => maxAttackElevationDifference;
        public AbilityDefinition PrimaryAttack => primaryAttack;

        public bool IsConfigured =>
            detectionRange > 0f &&
            disengageRange >= detectionRange &&
            attackRange > 0f &&
            attackRange <= detectionRange &&
            moveSpeed > 0f &&
            maxAttackElevationDifference <= maxChaseElevationDifference &&
            primaryAttack != null &&
            primaryAttack.IsConfigured;

        private void OnValidate()
        {
            detectionRange = Mathf.Max(0.1f, detectionRange);
            disengageRange = Mathf.Max(detectionRange, disengageRange);
            attackRange = Mathf.Clamp(attackRange, 0.1f, detectionRange);
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            maxChaseElevationDifference = Mathf.Max(0f, maxChaseElevationDifference);
            maxAttackElevationDifference = Mathf.Clamp(
                maxAttackElevationDifference,
                0f,
                maxChaseElevationDifference);
        }
    }
}
