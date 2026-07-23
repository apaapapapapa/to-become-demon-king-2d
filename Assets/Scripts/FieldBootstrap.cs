using DemonKing.Field.Prototype;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// プロトタイプ起動時の最小エントリーポイントです。
    /// ProjectAssetsを解決し、Game Sessionを直接開始せずTitle Screenへ起動制御を委譲します。
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

            PrototypeTitleScreenInstaller.Install(projectAssets);
        }
    }
}
