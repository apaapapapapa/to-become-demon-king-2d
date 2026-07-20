using DemonKing.Gameplay.Combat;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Projectile生成通知へPrototype用の火炎表現を接続します。
    /// Projectileの移動・命中判定には影響しません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProjectileAttackExecutor))]
    public sealed class PrototypeProjectileAttackEffect : MonoBehaviour
    {
        private ProjectileAttackExecutor projectileExecutor;

        public GameObject LastSpawnedEffect { get; private set; }

        private void Awake()
        {
            projectileExecutor = GetComponent<ProjectileAttackExecutor>();
        }

        private void OnEnable()
        {
            projectileExecutor ??= GetComponent<ProjectileAttackExecutor>();
            projectileExecutor.ProjectileSpawned += HandleProjectileSpawned;
        }

        private void OnDisable()
        {
            if (projectileExecutor != null)
            {
                projectileExecutor.ProjectileSpawned -= HandleProjectileSpawned;
            }
        }

        private void HandleProjectileSpawned(ProjectileAttackEvent attackEvent)
        {
            LastSpawnedEffect = PrototypeCombatEffectFactory.CreateFireProjectile(
                attackEvent.Projectile,
                attackEvent.Direction);
        }
    }
}
