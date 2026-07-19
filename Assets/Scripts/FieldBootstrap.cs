using DemonKing.Field.Prototype;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// プロトタイプフィールド起動時の構成ルートです。
    /// シーン初期設定、描画順設定、UI初期化、ワールド構築の開始を担当します。
    /// フィールド固有の配置値はここで所有し、生成側へ明示的に渡します。
    /// </summary>
    public sealed class FieldBootstrap : MonoBehaviour
    {
        [Header("プレイヤー配置")]
        [SerializeField] private Vector3 playerSpawnPosition = new(0f, -1.35f, -1f);
        [SerializeField] private Vector2 playableHalfExtents = new(7.15f, 3.45f);

        private void Awake()
        {
            PrototypeSceneConfigurator.Configure(Camera.main);
            PrototypeSortingConfigurator.Configure();
            PrototypeUiInstaller.Create();
            new PrototypeWorldBuilder(playerSpawnPosition, playableHalfExtents).Build();
        }
    }
}
