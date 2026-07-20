using System.Text;
using DemonKing.Gameplay.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// Evolution選択状態をuGUIへ反映します。
    /// 条件判定と進化実行はEvolutionSelectionControllerへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class EvolutionMenuView : MonoBehaviour
    {
        private static readonly Color OverlayColor = new(0.01f, 0.02f, 0.04f, 0.82f);
        private static readonly Color PanelColor = new(0.045f, 0.09f, 0.12f, 0.98f);
        private static readonly Color AccentColor = new(0.50f, 0.88f, 0.72f, 1f);
        private static readonly Color TitleColor = new(1f, 0.89f, 0.66f, 1f);
        private static readonly Color TextColor = new(0.91f, 0.94f, 0.84f, 1f);
        private static readonly Color MutedColor = new(0.66f, 0.75f, 0.70f, 1f);

        private EvolutionSelectionController selectionController;
        private Font uiFont;
        private GameObject menuRoot;
        private Text choicesText;
        private Text titleText;
        private Text descriptionText;
        private Text requirementsText;

        public void Initialize(Font font, EvolutionSelectionController controller)
        {
            Unbind();
            uiFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            selectionController = controller;
            BuildHierarchy();

            if (selectionController != null)
            {
                selectionController.StateChanged += Refresh;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (selectionController != null)
            {
                selectionController.StateChanged -= Refresh;
                selectionController = null;
            }
        }

        private void Refresh()
        {
            if (menuRoot == null)
            {
                return;
            }

            bool visible = selectionController != null && selectionController.IsOpen;
            menuRoot.SetActive(visible);
            if (!visible)
            {
                return;
            }

            var choices = new StringBuilder();
            for (int index = 0; index < selectionController.Entries.Count; index++)
            {
                EvolutionSelectionEntry entry = selectionController.Entries[index];
                string marker = index == selectionController.SelectedIndex ? "▶" : "  ";
                choices.Append(marker)
                    .Append(' ')
                    .Append(GetStatusLabel(entry.Evaluation.Status))
                    .Append(' ')
                    .Append(entry.Definition.DisplayName)
                    .AppendLine();
            }

            choicesText.text = choices.ToString().TrimEnd();
            EvolutionSelectionEntry? selected = selectionController.SelectedEntry;
            if (!selected.HasValue)
            {
                titleText.text = "進化先がありません";
                descriptionText.text = string.Empty;
                requirementsText.text = string.Empty;
                return;
            }

            EvolutionSelectionEntry value = selected.Value;
            titleText.text = value.Definition.DisplayName;
            descriptionText.text = value.Definition.Description;
            requirementsText.text = BuildRequirements(value.Evaluation);
        }

        private static string BuildRequirements(EvolutionEvaluationResult evaluation)
        {
            switch (evaluation.Status)
            {
                case EvolutionEvaluationStatus.Available:
                    return "進化可能\nすべての条件を満たしています。";
                case EvolutionEvaluationStatus.AlreadyUnlocked:
                    return "取得済み\nこの進化はすでに適用されています。";
                case EvolutionEvaluationStatus.DefinitionNotFound:
                case EvolutionEvaluationStatus.InvalidEvolutionId:
                    return "この進化Definitionは利用できません。";
            }

            var builder = new StringBuilder("不足している条件");
            foreach (EvolutionRequirementFailure failure in evaluation.Failures)
            {
                builder.Append("\n・").Append(FormatFailure(failure));
            }

            return builder.ToString();
        }

        private static string FormatFailure(EvolutionRequirementFailure failure)
        {
            return failure.Kind switch
            {
                EvolutionRequirementKind.Character =>
                    $"対象キャラクター: {failure.ContentId}",
                EvolutionRequirementKind.Level =>
                    $"レベル {failure.RequiredValue}（現在 {failure.CurrentValue}）",
                EvolutionRequirementKind.Skill =>
                    $"Skill取得: {failure.ContentId}",
                EvolutionRequirementKind.ArtRank =>
                    $"{failure.ContentId} ランク {failure.RequiredValue}（現在 {failure.CurrentValue}）",
                EvolutionRequirementKind.EvolutionNode =>
                    $"前提Evolution: {failure.ContentId}",
                EvolutionRequirementKind.ExclusiveChoice =>
                    $"排他Evolution選択済み: {failure.ContentId}",
                _ => "不明な条件"
            };
        }

        private static string GetStatusLabel(EvolutionEvaluationStatus status)
        {
            return status switch
            {
                EvolutionEvaluationStatus.Available => "[進化可能]",
                EvolutionEvaluationStatus.AlreadyUnlocked => "[取得済み]",
                EvolutionEvaluationStatus.RequirementsNotMet => "[条件不足]",
                _ => "[利用不可]"
            };
        }

        private void BuildHierarchy()
        {
            Transform existing = transform.Find("Evolution Menu");
            if (existing != null)
            {
                menuRoot = existing.gameObject;
                choicesText = existing.Find("Evolution Panel/Choices")?.GetComponent<Text>();
                titleText = existing.Find("Evolution Panel/Details/Title")?.GetComponent<Text>();
                descriptionText = existing.Find("Evolution Panel/Details/Description")?.GetComponent<Text>();
                requirementsText = existing.Find("Evolution Panel/Details/Requirements")?.GetComponent<Text>();
                return;
            }

            RectTransform overlay = CreateRect("Evolution Menu", transform);
            StretchToParent(overlay);
            menuRoot = overlay.gameObject;
            Image overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.color = OverlayColor;
            overlayImage.raycastTarget = true;

            RectTransform panel = CreateRect("Evolution Panel", overlay);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(1040f, 650f);
            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = PanelColor;
            panelImage.raycastTarget = false;

            CreateText(
                "Heading",
                panel,
                "EVOLUTION － 進化経路の選択",
                32,
                FontStyle.Bold,
                TitleColor,
                TextAnchor.MiddleLeft,
                new Vector2(42f, -42f),
                new Vector2(956f, 56f),
                new Vector2(0f, 1f));

            choicesText = CreateText(
                "Choices",
                panel,
                string.Empty,
                22,
                FontStyle.Bold,
                TextColor,
                TextAnchor.UpperLeft,
                new Vector2(42f, -128f),
                new Vector2(390f, 410f),
                new Vector2(0f, 1f));

            RectTransform divider = CreateRect("Divider", panel);
            divider.anchorMin = new Vector2(0f, 1f);
            divider.anchorMax = new Vector2(0f, 1f);
            divider.pivot = new Vector2(0f, 1f);
            divider.anchoredPosition = new Vector2(452f, -126f);
            divider.sizeDelta = new Vector2(3f, 420f);
            Image dividerImage = divider.gameObject.AddComponent<Image>();
            dividerImage.color = AccentColor;
            dividerImage.raycastTarget = false;

            RectTransform details = CreateRect("Details", panel);
            details.anchorMin = new Vector2(0f, 1f);
            details.anchorMax = new Vector2(0f, 1f);
            details.pivot = new Vector2(0f, 1f);
            details.anchoredPosition = new Vector2(486f, -126f);
            details.sizeDelta = new Vector2(512f, 420f);

            titleText = CreateText(
                "Title",
                details,
                string.Empty,
                28,
                FontStyle.Bold,
                AccentColor,
                TextAnchor.UpperLeft,
                Vector2.zero,
                new Vector2(512f, 50f),
                new Vector2(0f, 1f));
            descriptionText = CreateText(
                "Description",
                details,
                string.Empty,
                18,
                FontStyle.Normal,
                TextColor,
                TextAnchor.UpperLeft,
                new Vector2(0f, -64f),
                new Vector2(512f, 92f),
                new Vector2(0f, 1f));
            requirementsText = CreateText(
                "Requirements",
                details,
                string.Empty,
                18,
                FontStyle.Normal,
                MutedColor,
                TextAnchor.UpperLeft,
                new Vector2(0f, -174f),
                new Vector2(512f, 226f),
                new Vector2(0f, 1f));

            CreateText(
                "Footer",
                panel,
                "↑↓／WASD 選択    Enter／決定 進化    Esc／キャンセル 戻る",
                18,
                FontStyle.Normal,
                TextColor,
                TextAnchor.MiddleCenter,
                new Vector2(42f, 34f),
                new Vector2(956f, 44f),
                new Vector2(0f, 0f));
        }

        private Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            FontStyle fontStyle,
            Color color,
            TextAnchor alignment,
            Vector2 anchoredPosition,
            Vector2 size,
            Vector2 pivot)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = pivot;
            rect.anchorMax = pivot;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = uiFont;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.supportRichText = false;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }
    }
}
