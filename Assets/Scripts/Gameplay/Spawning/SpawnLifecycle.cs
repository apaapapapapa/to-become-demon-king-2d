using System;

namespace DemonKing.Gameplay.Spawning
{
    /// <summary>
    /// Spawn対象の生成・再利用判定・復元を調停する汎用Runtime Lifecycleです。
    /// 生成位置、Prefab、敵種別、報酬などの具体情報はFactory Delegateの外側へ委譲します。
    /// </summary>
    public sealed class SpawnLifecycle<T> where T : class
    {
        private readonly Func<T> spawn;
        private readonly Func<T, bool> canRestore;
        private readonly Action<T> restore;

        public SpawnLifecycle(
            Func<T> spawn,
            Func<T, bool> canRestore,
            Action<T> restore)
        {
            this.spawn = spawn ?? throw new ArgumentNullException(nameof(spawn));
            this.canRestore = canRestore ?? throw new ArgumentNullException(nameof(canRestore));
            this.restore = restore ?? throw new ArgumentNullException(nameof(restore));
        }

        public event Action<T> Spawned;
        public event Action<T> Restored;

        public T Current { get; private set; }

        public T SpawnOrRestore()
        {
            if (Current != null && canRestore(Current))
            {
                restore(Current);
                Restored?.Invoke(Current);
                return Current;
            }

            Current = spawn();
            if (Current == null)
            {
                throw new InvalidOperationException("Spawn Factoryがnullを返しました。");
            }

            Spawned?.Invoke(Current);
            return Current;
        }

        public void Forget(T instance)
        {
            if (ReferenceEquals(Current, instance))
            {
                Current = null;
            }
        }
    }
}
