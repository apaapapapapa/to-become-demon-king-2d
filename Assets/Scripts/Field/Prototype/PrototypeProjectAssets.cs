using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private EnemyAiDefinition trainingSlimeAi;

        [Header("Content")]
        [SerializeField] private DialogueDefinition apprenticeMageDialogue;
        [SerializeField] private QuestDefinition[] questDefinitions = Array.Empty<QuestDefinition>();

        [Header("Rewards")]
        [SerializeField] private RewardDefinition trainingDummyReward;
        [SerializeField] private ProgressionGrantDefinition fireMagicTrainingGrant;

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
        public EnemyAiDefinition TrainingSlimeAi => trainingSlimeAi;
        public DialogueDefinition ApprenticeMageDialogue => apprenticeMageDialogue;
        public QuestDefinition[] QuestDefinitions => questDefinitions ?? Array.Empty<QuestDefinition>();
        public RewardDefinition TrainingDummyReward => trainingDummyReward;
        public ProgressionGrantDefinition FireMagicTrainingGrant => fireMagicTrainingGrant;
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

            var definitions = new List<IGameContentDefinition> { playerCharacter };

            foreach (var abilityDefinition in playerCharacter.AbilityDefinitions)
            {
                definitions.Add(abilityDefinition);
            }

            foreach (ArtDefinition artDefinition in playerCharacter.ArtDefinitions)
            {
                definitions.Add(artDefinition);
                foreach (ArtAbilityUnlockEntry unlockEntry in artDefinition.AbilityUnlocks)
                {
                    definitions.Add(unlockEntry.AbilityDefinition);
                }
            }

            foreach (SkillDefinition skillDefinition in playerCharacter.SkillDefinitions)
            {
                definitions.Add(skillDefinition);
            }

            foreach (EvolutionDefinition evolutionDefinition in playerCharacter.EvolutionDefinitions)
            {
                definitions.Add(evolutionDefinition);
            }

            return new GameContentCatalog(definitions);
        }

        /// <summary>
        /// ゲーム進行に必須の参照が揃っているかを返します。
        /// UIフォントはEditorで自動導入を試みますが、取得失敗時も組み込みフォントで起動できるため必須判定から除外します。
        /// </summary>
        public bool IsConfigured =>
            applicationSettings != null &&
            playerCharacter != null &&
            playerCharacter.IsConfigured &&
            trainingSlimeAi != null &&
            trainingSlimeAi.IsConfigured &&
            apprenticeMageDialogue != null &&
            apprenticeMageDialogue.IsConfigured &&
            questDefinitions != null &&
            questDefinitions.Length > 0 &&
            questDefinitions.All(definition => definition != null && definition.IsConfigured) &&
            questDefinitions.Select(definition => definition.QuestId).Distinct(StringComparer.Ordinal).Count() == questDefinitions.Length &&
            trainingDummyReward != null &&
            trainingDummyReward.IsConfigured &&
            fireMagicTrainingGrant != null &&
            fireMagicTrainingGrant.IsConfigured &&
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
