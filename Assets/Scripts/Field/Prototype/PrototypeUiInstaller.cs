using DemonKing.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプシーンへCanvas（uGUI）ベースのUIルートを構築します。
    /// UIはプレイヤーPrefabから独立したシーンライフサイクルで管理します。
    /// </summary>
    internal static class PrototypeUiInstaller
    {
        public static GameObject Create()
        {
            GameObject uiRoot = new("UI Root", typeof(RectTransform));

            Canvas canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            uiRoot.AddComponent<GraphicRaycaster>();
            uiRoot.AddComponent<GameHudView>();
            return uiRoot;
        }
    }
}
