using System;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Prototype訓練用スライムの具体的なGameObject生成だけを担当します。
    /// 再生成・復元判断は汎用SpawnLifecycleへ委譲します。
    /// </summary>
    internal sealed class PrototypeCombatDummyFactory
    {
        private readonly Transform parent;
        private readonly Vector3 spawnPosition;
        private readonly Action<PrototypeCombatDummy> configureSpawnedDummy;

        public PrototypeCombatDummyFactory(
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

        public PrototypeCombatDummy Spawn()
        {
            GameObject dummyObject = new("訓練用スライム");
            dummyObject.transform.SetParent(parent, false);
            dummyObject.transform.localPosition = spawnPosition;
            PrototypeCombatDummy dummy = dummyObject.AddComponent<PrototypeCombatDummy>();
            configureSpawnedDummy?.Invoke(dummy);
            return dummy;
        }
    }
}
