using UnityEngine;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// 試作段階の案内表示だけを担当します。
    /// 本番UIへ移行するときは、このコンポーネントをCanvasベースのUI実装へ置き換えます。
    /// </summary>
    public sealed class PrototypeHud : MonoBehaviour
    {
        private Texture2D panelTexture;
        private Texture2D accentTexture;

        private void Awake()
        {
            panelTexture = CreateSolidTexture(new Color(0.045f, 0.10f, 0.12f, 0.86f));
            accentTexture = CreateSolidTexture(new Color(0.93f, 0.57f, 0.31f, 0.95f));
        }

        private void OnDestroy()
        {
            if (panelTexture != null)
            {
                Destroy(panelTexture);
            }

            if (accentTexture != null)
            {
                Destroy(accentTexture);
            }
        }

        private void OnGUI()
        {
            float scale = Mathf.Clamp(Screen.height / 720f, 0.82f, 1.22f);

            GUIStyle title = new(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(20f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.89f, 0.66f) }
            };
            GUIStyle subtitle = new(title)
            {
                fontSize = Mathf.RoundToInt(12f * scale),
                fontStyle = FontStyle.Normal,
                normal = { textColor = new Color(0.72f, 0.86f, 0.72f) }
            };
            GUIStyle hint = new(subtitle)
            {
                fontSize = Mathf.RoundToInt(13f * scale),
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.91f, 0.94f, 0.84f) }
            };

            Rect locationPanel = new(22f * scale, 22f * scale, 286f * scale, 76f * scale);
            GUI.DrawTexture(locationPanel, panelTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(
                new Rect(locationPanel.x, locationPanel.y, 5f * scale, locationPanel.height),
                accentTexture,
                ScaleMode.StretchToFill);
            GUI.Label(
                new Rect(locationPanel.x + 18f * scale, locationPanel.y + 8f * scale,
                    locationPanel.width - 24f * scale, 34f * scale),
                "夕映えの学園草原",
                title);
            GUI.Label(
                new Rect(locationPanel.x + 18f * scale, locationPanel.y + 40f * scale,
                    locationPanel.width - 24f * scale, 24f * scale),
                "魔法学園・西の庭",
                subtitle);

            float controlsWidth = 510f * scale;
            Rect controlsPanel = new(
                (Screen.width - controlsWidth) * 0.5f,
                Screen.height - 54f * scale,
                controlsWidth,
                34f * scale);
            GUI.DrawTexture(controlsPanel, panelTexture, ScaleMode.StretchToFill);
            GUI.Label(controlsPanel, "移動　WASD／矢印キー／ゲームパッド左スティック", hint);
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
