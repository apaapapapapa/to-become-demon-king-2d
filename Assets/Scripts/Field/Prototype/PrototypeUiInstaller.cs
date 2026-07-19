using DemonKing.Presentation.UI;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプシーンのUIルートを構築します。
    /// プレイヤーPrefabとは独立したシーンライフサイクルでHUDを保持します。
    /// </summary>
    internal static class PrototypeUiInstaller
    {
        public static GameObject Create()
        {
            GameObject uiRoot = new("UI Root");
            uiRoot.AddComponent<PrototypeHud>();
            return uiRoot;
        }
    }
}
