using DemonKing.Field.Composition;
using DemonKing.Gameplay.Content;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Rewards;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Field構築後にアプリケーション層へ公開する最小の実行時参照です。
    /// Field位置はScene Build IndexではなくStable IDで保持します。
    /// </summary>
    internal readonly struct PrototypeWorldBuildResult
    {
        public PrototypeWorldBuildResult(
            Transform worldRoot,
            GameObject player,
            RewardService rewardService,
            GameContentCatalog gameContentCatalog,
            QuestProgressionService questProgressionService)
            : this(
                worldRoot,
                player,
                rewardService,
                gameContentCatalog,
                questProgressionService,
                new FieldLocation(
                    PrototypeFieldDefinition.DefaultFieldId,
                    PrototypeFieldDefinition.DefaultEntryPointId))
        {
        }

        public PrototypeWorldBuildResult(
            Transform worldRoot,
            GameObject player,
            RewardService rewardService,
            GameContentCatalog gameContentCatalog,
            QuestProgressionService questProgressionService,
            FieldLocation fieldLocation)
        {
            WorldRoot = worldRoot;
            Player = player;
            RewardService = rewardService;
            GameContentCatalog = gameContentCatalog;
            QuestProgressionService = questProgressionService;
            FieldLocation = fieldLocation;
        }

        public Transform WorldRoot { get; }
        public GameObject Player { get; }
        public RewardService RewardService { get; }
        public GameContentCatalog GameContentCatalog { get; }
        public QuestProgressionService QuestProgressionService { get; }
        public FieldLocation FieldLocation { get; }
    }
}
