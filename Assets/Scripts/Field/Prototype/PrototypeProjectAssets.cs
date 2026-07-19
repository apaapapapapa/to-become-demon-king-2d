using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Combat.Configuration;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプ実行時に必要な主要アセット参照を一か所へ集約します。
    /// コンテンツ、UI、ゲームバランス設定を利用側の文字列パスやPrefab内の重複値から分離します。
    /// </summary>
    [CreateAssetMenu(fileName = "PrototypeProjectAssets", menuName = "Demon King/Prototype Project Assets")]
    public sealed class PrototypeProjectAssets : ScriptableObject
    {
        [Header("Characters")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private CharacterStatsDefinition playerCharacterStats;
        [SerializeField] private MeleeAttackDefinition playerMeleeAttack;

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

        public GameObject PlayerPrefab => playerPrefab;
        public CharacterStatsDefinition PlayerCharacterStats => playerCharacterStats;
        public MeleeAttackDefinition PlayerMeleeAttack => playerMeleeAttack;
        public Font UiFont => uiFont;
        public GameObject CottagePrefab => cottagePrefab;
        public GameObject TreePrefab => treePrefab;
        public GameObject LamppostPrefab => lamppostPrefab;
        public Sprite CottageSprite => cottageSprite;
        public Sprite TreeSprite => treeSprite;
        public Sprite LamppostSprite => lamppostSprite;
        public Sprite GrassTileSprite => grassTileSprite;
        public Sprite PathTileSprite => pathTileSprite;

        public bool IsConfigured =>
            playerPrefab != null &&
            playerCharacterStats != null &&
            playerMeleeAttack != null &&
            uiFont != null &&
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
