using DemonKing.Field.Prototype;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// プロトタイプフィールド起動時の構成ルートです。
    /// シーン初期設定、描画順、UI、Tilemap地形、衝突、ワールドPrefabの構築開始を担当します。
    /// </summary>
    public sealed class FieldBootstrap : MonoBehaviour
    {
        [Header("プレイヤー配置")]
        [SerializeField] private Vector3 playerSpawnPosition = new(0f, -1.35f, -1f);

        [Header("フィールド")]
        [SerializeField, Min(4)] private int playableTileRadius = 15;

        private void Awake()
        {
            PrototypeSceneConfigurator.Configure(Camera.main);
            PrototypeSortingConfigurator.Configure();
            PrototypeUiInstaller.Create();
            new PrototypeWorldBuilder(playerSpawnPosition, playableTileRadius).Build();
        }
    }
}
