using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemonKing.Domain.Quests;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;

namespace DemonKing.Presentation.UI
{
    public enum QuestTrackerDisplayStatus
    {
        Active = 0,
        ReadyToTurnIn = 1,
        Completed = 2,
    }

    /// <summary>
    /// Quest Runtime StateをuGUIへ依存しない表示値へ変換した不変Modelです。
    /// </summary>
    public readonly struct QuestTrackerDisplayModel
    {
        public QuestTrackerDisplayModel(
            string questId,
            QuestTrackerDisplayStatus status,
            string statusText,
            string title,
            string objectiveText)
        {
            QuestId = questId ?? string.Empty;
            Status = status;
            StatusText = statusText ?? string.Empty;
            Title = title ?? string.Empty;
            ObjectiveText = objectiveText ?? string.Empty;
        }

        public string QuestId { get; }
        public QuestTrackerDisplayStatus Status { get; }
        public string StatusText { get; }
        public string Title { get; }
        public string ObjectiveText { get; }
    }

    /// <summary>
    /// Quest Definition / Runtime Stateから常設Tracker用Modelを構築します。
    /// Unity Lifecycleや表示対象の保持は担当しません。
    /// </summary>
    public static class QuestTrackerProjection
    {
        public static bool TryCreate(
            QuestDefinition definition,
            QuestProgressState state,
            out QuestTrackerDisplayModel model)
        {
            model = default;
            if (definition == null || state == null || !state.IsAccepted ||
                !string.Equals(definition.QuestId, state.QuestId, StringComparison.Ordinal))
            {
                return false;
            }

            QuestTrackerDisplayStatus displayStatus;
            string statusText;
            if (state.IsCompleted)
            {
                displayStatus = QuestTrackerDisplayStatus.Completed;
                statusText = "完了";
            }
            else if (state.IsReadyToTurnIn)
            {
                displayStatus = QuestTrackerDisplayStatus.ReadyToTurnIn;
                statusText = "報告可能";
            }
            else
            {
                displayStatus = QuestTrackerDisplayStatus.Active;
                statusText = "受注中";
            }

            model = new QuestTrackerDisplayModel(
                state.QuestId,
                displayStatus,
                statusText,
                definition.DisplayName,
                BuildObjectiveText(definition, state));
            return true;
        }

        private static string BuildObjectiveText(
            QuestDefinition definition,
            QuestProgressState state)
        {
            var builder = new StringBuilder();
            foreach (QuestObjectiveDefinition objectiveDefinition in definition.Objectives)
            {
                if (!state.TryGetObjective(
                        objectiveDefinition.ObjectiveId,
                        out ObjectiveProgressState objectiveState))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(objectiveState.IsCompleted ? "✓ " : "□ ");
                builder.Append(objectiveDefinition.DisplayName);
                builder.Append("  ");
                builder.Append(objectiveState.CurrentCount);
                builder.Append('/');
                builder.Append(objectiveState.RequiredCount);
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// 手動追跡状態を持たず、現在のQuest状態だけから初期表示対象を決定します。
    /// Active、ReadyToTurnIn、Completedの順で優先し、同一状態ではQuest ID順に決定します。
    /// </summary>
    public static class QuestTrackerSelector
    {
        public static string SelectInitialQuestId(IEnumerable<QuestProgressState> states)
        {
            if (states == null)
            {
                return string.Empty;
            }

            QuestProgressState selected = states
                .Where(state => state != null && state.IsAccepted)
                .OrderBy(GetPriority)
                .ThenBy(state => state.QuestId, StringComparer.Ordinal)
                .FirstOrDefault();
            return selected?.QuestId ?? string.Empty;
        }

        private static int GetPriority(QuestProgressState state)
        {
            if (state.IsActive)
            {
                return 0;
            }

            if (state.IsReadyToTurnIn)
            {
                return 1;
            }

            return 2;
        }
    }

    /// <summary>
    /// Questイベントを非モーダル通知文へ変換します。
    /// 通知表示時間やCoroutineは保持しません。
    /// </summary>
    public static class QuestNotificationFormatter
    {
        public static string Accepted(QuestDefinition definition, string fallbackQuestId)
        {
            return $"クエスト受注\n{ResolveQuestName(definition, fallbackQuestId)}";
        }

        public static string Progress(
            QuestDefinition definition,
            QuestProgressUpdate update)
        {
            string objectiveName = ResolveObjectiveName(definition, update.ObjectiveId);
            return $"クエスト進捗\n{objectiveName}  {update.CurrentCount}";
        }

        public static string ReadyToTurnIn(QuestDefinition definition, string fallbackQuestId)
        {
            return $"目標達成\n{ResolveQuestName(definition, fallbackQuestId)}\n見習い魔術師に報告しよう";
        }

        public static string Completed(QuestDefinition definition, string fallbackQuestId)
        {
            return $"クエスト完了\n{ResolveQuestName(definition, fallbackQuestId)}";
        }

        private static string ResolveQuestName(
            QuestDefinition definition,
            string fallbackQuestId)
        {
            return definition == null ? fallbackQuestId ?? string.Empty : definition.DisplayName;
        }

        private static string ResolveObjectiveName(
            QuestDefinition definition,
            string objectiveId)
        {
            if (definition != null)
            {
                QuestObjectiveDefinition objective = definition.Objectives
                    .FirstOrDefault(candidate => string.Equals(
                        candidate.ObjectiveId,
                        objectiveId,
                        StringComparison.Ordinal));
                if (objective != null)
                {
                    return objective.DisplayName;
                }
            }

            return objectiveId ?? string.Empty;
        }
    }
}
