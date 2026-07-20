using DemonKing.Gameplay.Combat;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 近接攻撃の実行通知を受け取り、Prototype用の斬撃表現を再生します。
    /// 攻撃判定とはイベントで分離し、演出の有無がダメージ適用へ影響しないようにします。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeleeAttackExecutor))]
    public sealed class PrototypeMeleeAttackEffect : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float duration = 0.22f;

        private MeleeAttackExecutor meleeAttackExecutor;

        public GameObject LastSpawnedEffect { get; private set; }

        private void Awake()
        {
            meleeAttackExecutor = GetComponent<MeleeAttackExecutor>();
        }

        private void OnEnable()
        {
            if (meleeAttackExecutor == null)
            {
                meleeAttackExecutor = GetComponent<MeleeAttackExecutor>();
            }

            meleeAttackExecutor.AttackPerformed += HandleAttackPerformed;
        }

        private void OnDisable()
        {
            if (meleeAttackExecutor != null)
            {
                meleeAttackExecutor.AttackPerformed -= HandleAttackPerformed;
            }
        }

        private void HandleAttackPerformed(MeleeAttackEvent attackEvent)
        {
            LastSpawnedEffect = PrototypeCombatEffectFactory.CreateMeleeSlash(
                attackEvent,
                duration);
        }
    }
}
