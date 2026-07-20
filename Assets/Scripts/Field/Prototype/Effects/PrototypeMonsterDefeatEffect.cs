using DemonKing.Gameplay.Combat;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Healthの撃破通知を受け取り、対象の破棄後も残るPrototype用の消滅表現を再生します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class PrototypeMonsterDefeatEffect : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float duration = 0.55f;
        [SerializeField] private Vector2 centerOffset = new(0f, 0.12f);

        private Health health;

        public GameObject LastSpawnedEffect { get; private set; }

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            health.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= HandleDied;
            }
        }

        private void HandleDied(DefeatContext context)
        {
            Vector3 center = transform.position + (Vector3)centerOffset;
            LastSpawnedEffect = PrototypeCombatEffectFactory.CreateMonsterDefeatBurst(
                center,
                duration);
        }
    }
}
