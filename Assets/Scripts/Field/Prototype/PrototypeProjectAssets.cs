using System;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.AI.Configuration;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Content;
using DemonKing.Gameplay.Dialogue.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Quests.Configuration;
using DemonKing.Gameplay.Rewards.Configuration;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプ実行時に必要な主要アセット参照を一か所へ集約します。
    /// コンテンツ、UI、ゲームバランス、起動設定を利用側の文字列パスやPrefab内の重複値から分離します。
    /// </summary>
    [CreateAssetMenu(fileName = "PrototypeProjectAssets", menuName = "Demon King/Prototype Project Assets")]
    public sealed class PrototypeProjectAssets : ScriptableObject
    {
        [Header("Application")]
        [SerializeField] private PrototypeApplicationSettings applicationSettings;

        [Header("Characters")]
        [SerializeField] private CharacterDefinition playerCharacter;

        [Header("Scenarios")]
        [SerializeField] private TrainingScenarioDefinition trainingScenario;

        [Header("UI")]
        [SerializeField] private Font uiFont;

        [Header("World Prefabs")]
        [SerializeField] private GameObject cottagePrefab;
        [SerializeField] private GameObject treePrefab;
        [SerializeField] private GameObject lamppostPrefab;

        [Header("World Art")]
        [SerializeField] private Sprite cottageSprite;
        [SerializeField] private Sprite treeSprite;
        [SerializeField] private Sprite lamppostSprite;

        [Header("Terrain Sprites")]
        [SerializeField] private Sprite grassTileSprite;
        [SerializeField] private Sprite pathTileSprite;

        public PrototypeApplicationSettings ApplicationSettings => applicationSettings;
        public CharacterDefinition PlayerCharacter => playerCharacter;
        public TrainingScenarioDefinition TrainingScenario => trainingScenario;

        // Scenario内部参照のSource of TruthはTrainingScenarioDefinitionです。
        // 既存利用側の段階移行用に読み取り専用の派生アクセサだけを公開します。
        public QuestDefinition TrainingQuestDefinition => trainingScenario?.QuestDefinition;
        public QuestDefinition[] QuestDefinitions => trainingScenario?.QuestDefinition == null
            ? Array.Empty<QuestDefinition>()
            : new[] { trainingScenario.QuestDefinition };
        public EnemyAiDefinition TrainingSlimeAi => trainingScenario?.EnemyAiDefinition;
        public RewardDefinition TrainingDummyReward => trainingScenario?.DefeatReward;
        public ProgressionGrantDefinition FireMagicTrainingGrant => trainingScenario?.CompletionGrant;
        public DialogueDefinition ApprenticeMageDialogue => trainingScenario?.OfferDialogue;
        public DialogueDefinition ApprenticeMageActiveDialogue => trainingScenario?.ActiveDialogue;
        public DialogueDefinition ApprenticeMageTurnInDialogue => trainingScenario?.TurnInDialogue;
        public DialogueDefinition ApprenticeMageCompletedDialogue => trainingScenario?.CompletedDialogue;

        public Font UiFont => uiFont;
        public bool HasUiFont => uiFont != null;
        public GameObject CottagePrefab => cottagePrefab;
        public GameObject TreePrefab => treePrefab;
        public GameObject LamppostPrefab => lamppostPrefab;
        public Sprite CottageSprite => cottageSprite;
        public Sprite TreeSprite => treeSprite;
        public Sprite LamppostSprite => lamppostSprite;
        public Sprite GrassTileSprite => grassTileSprite;
        public Sprite PathTileSprite => pathTileSprite;

        public GameContentCatalog CreateGameContentCatalog()
        {
            if (playerCharacter == null)
            {
                throw new InvalidOperationException(
                    "GameContentCatalogを作成するためのPlayerCharacterが設定されていません。");
            }

            return new GameContentCatalog(new IGameContentDefinition[] { playerCharacter });
        }

        /// <summary>
        /// ゲーム進行に必須の参照が揃っているかを返します。
        /// UIフォントはEditorで自動導入を試みますが、取得失敗時も組み込みフォントで起動できるため必須判定から除外します。
        /// </summary>
        public bool IsConfigured =>
            applicationSettings != null &&
            playerCharacter != null &&
            playerCharacter.IsConfigured &&
            trainingScenario != null &&
            trainingScenario.IsConfigured &&
            cottagePrefab != null &&
            treePrefab != null &&
            lamppostPrefab != null &&
            cottageSprite != null &&
            treeSprite != null &&
            lamppostSprite != null &&
            grassTileSprite != null &&
            pathTileSprite != null;
    }
}
