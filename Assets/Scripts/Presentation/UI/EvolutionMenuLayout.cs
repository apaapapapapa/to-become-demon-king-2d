using System;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// Evolution Menu Prefab内の表示参照を保持します。Hierarchy構築はPrefabの責務です。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EvolutionMenuLayout : MonoBehaviour
    {
        [SerializeField] private Text choicesText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text requirementsText;
        [SerializeField] private Text[] textElements = Array.Empty<Text>();

        public GameObject Root => gameObject;
        public Text ChoicesText => choicesText;
        public Text TitleText => titleText;
        public Text DescriptionText => descriptionText;
        public Text RequirementsText => requirementsText;

        public bool IsConfigured =>
            choicesText != null &&
            titleText != null &&
            descriptionText != null &&
            requirementsText != null;

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
