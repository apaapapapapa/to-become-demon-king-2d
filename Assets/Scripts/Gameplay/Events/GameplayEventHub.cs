using System;
using DemonKing.Domain.Quests;

namespace DemonKing.Gameplay.Events
{
    /// <summary>
    /// Feature間で具体実装を参照せずGameplay上の出来事を通知する最小Event Hubです。
    /// 永続化や履歴保持は行わず、現在のRuntime内での同期通知だけを担当します。
    /// </summary>
    public sealed class GameplayEventHub
    {
        public event Action<GameplayEvent> Published;

        public void Publish(GameplayEvent gameplayEvent)
        {
            Published?.Invoke(gameplayEvent);
        }
    }

    public static class GameplayEventIds
    {
        public const string EnemyDefeated = "gameplay.enemy_defeated";
        public const string DialogueCompleted = "gameplay.dialogue_completed";
    }
}
