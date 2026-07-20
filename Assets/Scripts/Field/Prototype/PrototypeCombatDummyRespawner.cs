using System;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Prototypeの訓練用スライムを初期配置し、撃破後は同じ位置へ再生成します。
    /// NPCや報酬の詳細には依存せず、新しく生成した個体の構成は外側へ委譲します。
    /// </summary>
    public sealed class PrototypeCombatDummyRespawner
    {
        private readonly Transform parent;
        private readonly Vector3 spawnPosition;
        private readonly Action<PrototypeCombatDummy> configureSpawnedDummy;

        public PrototypeCombatDummyRespawner(
            Transform parent,
            Vector3 spawnPosition,
            Action<PrototypeCombatDummy> configureSpawnedDummy)
        {
            this.parent = parent != null
                ? parent
                : throw new ArgumentNullException(nameof(parent));
            this.spawnPosition = spawnPosition;
            this.configureSpawnedDummy = configureSpawnedDummy;
        }

        public PrototypeCombatDummy CurrentDummy { get; private set; }

        public PrototypeCombatDummy SpawnOrRestore()
        {
            if (CurrentDummy != null && CurrentDummy.IsAlive)
            {
                CurrentDummy.RestoreToFull();
                return CurrentDummy;
            }

            GameObject dummyObject = new("訓練用スライム");
            dummyObject.transform.SetParent(parent, false);
            dummyObject.transform.localPosition = spawnPosition;
            CurrentDummy = dummyObject.AddComponent<PrototypeCombatDummy>();
            configureSpawnedDummy?.Invoke(CurrentDummy);
            return CurrentDummy;
        }
    }
}
