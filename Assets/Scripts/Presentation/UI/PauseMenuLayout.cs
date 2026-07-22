using System;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// Pause Menu Prefab内の表示参照を保持します。Hierarchy構築はPrefabの責務です。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseMenuLayout : MonoBehaviour
    {
        [SerializeField] private Text[] textElements = Array.Empty<Text>();

        public GameObject Root => gameObject;
        public bool IsConfigured
        {
            get
            {
                if (textElements == null || textElements.Length == 0)
                {
                    return false;
                }

                foreach (Text text in textElements)
                {
                    if (text == null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void ApplyFont(Font font)
        {
            if (font == null)
            {
                return;
            }

            foreach (Text text in textElements)
            {
                if (text != null)
                {
                    text.font = font;
                }
            }
        }
    }
}
