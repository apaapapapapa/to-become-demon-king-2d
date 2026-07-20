using DemonKing.Gameplay.Content;
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
            GameContentCatalog gameContentCatalog)
        {
            WorldRoot = worldRoot;
            Player = player;
            RewardService = rewardService;
            GameContentCatalog = gameContentCatalog;
        }

        public Transform WorldRoot { get; }
        public GameObject Player { get; }
        public RewardService RewardService { get; }
        public GameContentCatalog GameContentCatalog { get; }
    }
}
