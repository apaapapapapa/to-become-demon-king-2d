using System.Text;
using DemonKing.Gameplay.Progression;
using UnityEngine;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// Evolution選択状態をPrefabベースのuGUIへ反映します。
    /// 条件判定、進化実行、Hierarchy構築は行いません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class EvolutionMenuView : MonoBehaviour
    {
        private EvolutionSelectionController selectionController;
        private EvolutionMenuLayout layout;

        public void Initialize(
            Font font,
            EvolutionSelectionController controller,
            EvolutionMenuLayout menuLayout)
        {
            Unbind();
            selectionController = controller;
            layout = menuLayout;
            if (layout == null || !layout.IsConfigured)
            {
                Debug.LogError("EvolutionMenuLayoutが正しく設定されていません。", this);
                return;
            }

            Font resolvedFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            layout.ApplyFont(resolvedFont);

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
            if (layout == null)
            {
                return;
            }

            bool visible = selectionController != null && selectionController.IsOpen;
            layout.Root.SetActive(visible);
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

            layout.ChoicesText.text = choices.ToString().TrimEnd();
            EvolutionSelectionEntry? selected = selectionController.SelectedEntry;
            if (!selected.HasValue)
            {
                layout.TitleText.text = "進化先がありません";
                layout.DescriptionText.text = string.Empty;
                layout.RequirementsText.text = string.Empty;
                return;
            }

            EvolutionSelectionEntry value = selected.Value;
            layout.TitleText.text = value.Definition.DisplayName;
            layout.DescriptionText.text = value.Definition.Description;
            layout.RequirementsText.text = BuildRequirements(value.Evaluation);
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
    }
}
