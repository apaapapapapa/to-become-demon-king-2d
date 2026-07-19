using UnityEngine;
using UnityEngine.InputSystem;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプ実行時に必要な主要アセット参照を一か所へ集約します。
    /// Resources.Loadの文字列パスではなく、Unityのシリアライズ参照を正としてアセットのリネームや移動へ追従できるようにします。
    /// </summary>
    [CreateAssetMenu(fileName = "PrototypeProjectAssets", menuName = "Demon King/Prototype Project Assets")]
    public sealed class PrototypeProjectAssets : ScriptableObject
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset playerInputActions;

        [Header("Characters")]
        [SerializeField] private GameObject playerPrefab;

        [Header("World Prefabs")]
        [SerializeField] private GameObject cottagePrefab;
        [SerializeField] private GameObject treePrefab;
        [SerializeField] private GameObject lamppostPrefab;

        [Header("Terrain Sprites")]
        [SerializeField] private Sprite grassTileSprite;
        [SerializeField] private Sprite pathTileSprite;

        public InputActionAsset PlayerInputActions => playerInputActions;
        public GameObject PlayerPrefab => playerPrefab;
        public GameObject CottagePrefab => cottagePrefab;
        public GameObject TreePrefab => treePrefab;
        public GameObject LamppostPrefab => lamppostPrefab;
        public Sprite GrassTileSprite => grassTileSprite;
        public Sprite PathTileSprite => pathTileSprite;

        public bool IsConfigured =>
            playerInputActions != null &&
            playerPrefab != null &&
            cottagePrefab != null &&
            treePrefab != null &&
            lamppostPrefab != null &&
            grassTileSprite != null &&
            pathTileSprite != null;
    }
}
