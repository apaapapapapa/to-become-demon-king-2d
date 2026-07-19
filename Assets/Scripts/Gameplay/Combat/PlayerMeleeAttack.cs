using System.Collections.Generic;
using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Gameplay.Combat
{
    /// <summary>
    /// プレイヤーのAttack入力を受け取り、向いている方向の近距離対象へダメージを与えます。
    /// 敵種別や死亡演出には依存せず、IDamageableだけを対象にします。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerMeleeAttack : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.1f)] private float attackRadius = 0.65f;
        [SerializeField, Min(0f)] private float attackDistance = 0.65f;
        [SerializeField] private LayerMask attackLayers = ~0;

        private readonly HashSet<IDamageable> damagedTargets = new();
        private PlayerInputReader inputReader;
        private Vector2 facingDirection = Vector2.down;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
        }

        private void OnEnable()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (inputReader != null)
            {
                inputReader.AttackPressed += HandleAttackPressed;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.AttackPressed -= HandleAttackPressed;
            }
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            Vector2 move = inputReader.Move;
            if (move.sqrMagnitude > 0.0001f)
            {
                facingDirection = move.normalized;
            }
        }

        private void HandleAttackPressed()
        {
            Vector2 center = (Vector2)transform.position + facingDirection * attackDistance;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, attackRadius, attackLayers);

            damagedTargets.Clear();

            foreach (Collider2D collider in colliders)
            {
                if (collider == null || collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>(false);
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    IDamageable damageable = behaviour as IDamageable;
                    if (damageable == null || !damageable.IsAlive || !damagedTargets.Add(damageable))
                    {
                        continue;
                    }

                    damageable.TakeDamage(damage, gameObject);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector2 center = (Vector2)transform.position + facingDirection * attackDistance;
            Gizmos.DrawWireSphere(center, attackRadius);
        }
#endif
    }
}
