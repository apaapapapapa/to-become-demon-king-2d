using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Domain.Quests;
using DemonKing.Domain.Story;

namespace DemonKing.Gameplay.Dialogue
{
    /// <summary>
    /// 同一NPCのDialogue候補をStory Flagと任意のQuest Statusで選択するComposition用Ruleです。
    /// Dialogue本体の型はジェネリックとし、Story RuntimeがUnityのDialogueDefinitionへ依存しないようにします。
    /// </summary>
    public sealed class StoryDialogueVariant<TDialogue>
    {
        private readonly string[] requiredStoryFlags;

        public StoryDialogueVariant(
            TDialogue dialogue,
            IEnumerable<string> requiredFlags = null,
            string questId = "",
            QuestProgressStatus? requiredQuestStatus = null)
        {
            Dialogue = dialogue;
            requiredStoryFlags = requiredFlags?
                .Where(flagId => !string.IsNullOrWhiteSpace(flagId))
                .Distinct(StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();
            QuestId = questId ?? string.Empty;
            RequiredQuestStatus = requiredQuestStatus;

            if (RequiredQuestStatus.HasValue && string.IsNullOrWhiteSpace(QuestId))
            {
                throw new ArgumentException(
                    "Quest Status条件を指定する場合はQuest IDが必要です。",
                    nameof(questId));
            }
        }

        public TDialogue Dialogue { get; }
        public IReadOnlyList<string> RequiredStoryFlags => requiredStoryFlags;
        public string QuestId { get; }
        public QuestProgressStatus? RequiredQuestStatus { get; }

        public bool Matches(
            StoryProgressState storyState,
            Func<string, QuestProgressStatus?> questStatusResolver)
        {
            if (storyState == null || requiredStoryFlags.Any(flagId => !storyState.HasFlag(flagId)))
            {
                return false;
            }

            if (!RequiredQuestStatus.HasValue)
            {
                return true;
            }

            QuestProgressStatus? currentStatus = questStatusResolver?.Invoke(QuestId);
            return currentStatus.HasValue && currentStatus.Value == RequiredQuestStatus.Value;
        }
    }

    public static class StoryDialogueSelector
    {
        /// <summary>
        /// Variantは優先度順に渡し、最初に条件を満たしたDialogueを返します。
        /// 一致しなければfallbackを返します。
        /// </summary>
        public static TDialogue Select<TDialogue>(
            TDialogue fallback,
            StoryProgressState storyState,
            IEnumerable<StoryDialogueVariant<TDialogue>> variants,
            Func<string, QuestProgressStatus?> questStatusResolver = null)
        {
            if (storyState == null || variants == null)
            {
                return fallback;
            }

            foreach (StoryDialogueVariant<TDialogue> variant in variants)
            {
                if (variant != null && variant.Matches(storyState, questStatusResolver))
                {
                    return variant.Dialogue;
                }
            }

            return fallback;
        }
    }
}
