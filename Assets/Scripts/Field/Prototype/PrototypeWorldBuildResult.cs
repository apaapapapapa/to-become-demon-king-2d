using DemonKing.Gameplay.Content;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Rewards;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// ワールド構築後にアプリケーション層へ公開する最小の実行時参照です。
    /// </summary>
    internal readonly struct PrototypeWorldBuildResult
    {
        public PrototypeWorldBuildResult(
            Transform worldRoot,
            GameObject player,
            RewardService rewardService,
            GameContentCatalog gameContentCatalog,
            QuestProgressionService questProgressionService)
        {
            WorldRoot = worldRoot;
            Player = player;
            RewardService = rewardService;
            GameContentCatalog = gameContentCatalog;
            QuestProgressionService = questProgressionService;
        }

        public Transform WorldRoot { get; }
        public GameObject Player { get; }
        public RewardService RewardService { get; }
        public GameContentCatalog GameContentCatalog { get; }
        public QuestProgressionService QuestProgressionService { get; }
    }
}
