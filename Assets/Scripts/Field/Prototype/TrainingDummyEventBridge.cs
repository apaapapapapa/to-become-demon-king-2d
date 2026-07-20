using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Spawning;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 現在の訓練DummyのDefeatをGameplay Eventへ変換します。
    /// Quest状態や会話フローは参照しません。
    /// </summary>
    internal sealed class TrainingDummyEventBridge : MonoBehaviour
    {
        private SpawnLifecycle<PrototypeCombatDummy> dummyLifecycle;
        private GameplayEventHub gameplayEventHub;
        private PrototypeCombatDummy subscribedDummy;
        private bool initialized;

        public void Initialize(
            SpawnLifecycle<PrototypeCombatDummy> dummyLifecycle,
            GameplayEventHub gameplayEventHub)
        {
            if (initialized)
            {
                Debug.LogWarning("TrainingDummyEventBridgeは既に初期化されています。", this);
                return;
            }

            this.dummyLifecycle = dummyLifecycle;
            this.gameplayEventHub = gameplayEventHub;
            dummyLifecycle.Spawned += HandleDummySpawned;

            if (dummyLifecycle.Current != null)
            {
                HandleDummySpawned(dummyLifecycle.Current);
            }

            initialized = true;
        }

        private void OnDestroy()
        {
            if (dummyLifecycle != null)
            {
                dummyLifecycle.Spawned -= HandleDummySpawned;
            }

            UnsubscribeCurrentDummy();
        }

        private void HandleDummySpawned(PrototypeCombatDummy dummy)
        {
            UnsubscribeCurrentDummy();
            subscribedDummy = dummy;
            if (subscribedDummy != null)
            {
                subscribedDummy.Defeated += HandleDummyDefeated;
            }
        }

        private void HandleDummyDefeated(DefeatContext context)
        {
            PrototypeCombatDummy defeatedDummy = subscribedDummy;
            if (defeatedDummy == null)
            {
                return;
            }

            UnsubscribeCurrentDummy();
            gameplayEventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                defeatedDummy.ActorId));
            dummyLifecycle.Forget(defeatedDummy);
        }

        private void UnsubscribeCurrentDummy()
        {
            if (subscribedDummy != null)
            {
                subscribedDummy.Defeated -= HandleDummyDefeated;
            }

            subscribedDummy = null;
        }
    }
}
