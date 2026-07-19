using UnityEngine;

namespace DemonKing.Gameplay.Combat.Configuration
{
    /// <summary>
    /// 近接攻撃のゲームバランス値を定義するScriptableObjectです。
    /// 攻撃コンポーネントからダメージ量や当たり判定サイズを分離します。
    /// </summary>
    [CreateAssetMenu(fileName = "MeleeAttack", menuName = "Demon King/Gameplay/Melee Attack")]
    public sealed class MeleeAttackDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.1f)] private float attackRadius = 0.65f;
        [SerializeField, Min(0f)] private float attackDistance = 0.65f;

        public int Damage => damage;
        public float AttackRadius => attackRadius;
        public float AttackDistance => attackDistance;
    }
}
