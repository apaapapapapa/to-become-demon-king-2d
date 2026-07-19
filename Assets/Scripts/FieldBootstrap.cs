using DemonKing.Field.Prototype;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// プロトタイプ起動時の最小エントリーポイントです。
    /// 具体的な設定値や初期化順序は保持せず、ProjectAssetsを解決してApplicationInstallerへ委譲します。
    /// </summary>
    public sealed class FieldBootstrap : MonoBehaviour
    {
        private const string ProjectAssetsResourcePath = "Settings/PrototypeProjectAssets";

        private void Awake()
        {
            PrototypeProjectAssets projectAssets = Resources.Load<PrototypeProjectAssets>(ProjectAssetsResourcePath);
            if (projectAssets == null || !projectAssets.IsConfigured)
            {
                Debug.LogError(
                    $"プロトタイプ用アセット設定が不足しています。Resources/{ProjectAssetsResourcePath}.asset を確認してください。",
                    this);
                return;
            }

            new PrototypeApplicationInstaller(projectAssets).Install();
        }
    }
}
