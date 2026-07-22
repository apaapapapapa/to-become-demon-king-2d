using System;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// Ability Loadout Menu Prefab内の表示参照を保持します。Hierarchy構築はPrefabの責務です。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AbilityLoadoutMenuLayout : MonoBehaviour
    {
        [SerializeField] private Text entriesText;
        [SerializeField] private Text slotsText;
        [SerializeField] private Text detailTitleText;
        [SerializeField] private Text detailDescriptionText;
        [SerializeField] private Text detailStatusText;
        [SerializeField] private Text[] textElements = Array.Empty<Text>();

        public GameObject Root => gameObject;
        public Text EntriesText => entriesText;
        public Text SlotsText => slotsText;
        public Text DetailTitleText => detailTitleText;
        public Text DetailDescriptionText => detailDescriptionText;
        public Text DetailStatusText => detailStatusText;

        public bool IsConfigured =>
            entriesText != null &&
            slotsText != null &&
            detailTitleText != null &&
            detailDescriptionText != null &&
            detailStatusText != null;

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
