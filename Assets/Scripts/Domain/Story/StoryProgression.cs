using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Domain.Events;

namespace DemonKing.Domain.Story
{
    /// <summary>
    /// 本編Storyの現在章、成立済みFlag、一度きり実行済みEventを保持するUnity非依存Runtime Stateです。
    /// </summary>
    public sealed class StoryProgressState
    {
        private readonly HashSet<string> flags;
        private readonly HashSet<string> executedEventIds;

        private StoryProgressState(
            string currentChapterId,
            IEnumerable<string> storyFlags,
            IEnumerable<string> executedEvents)
        {
            CurrentChapterId = StableContentId.Require(currentChapterId, nameof(currentChapterId));
            flags = new HashSet<string>(
                NormalizeIds(storyFlags),
                StringComparer.Ordinal);
            executedEventIds = new HashSet<string>(
                NormalizeIds(executedEvents),
                StringComparer.Ordinal);
        }

        public string CurrentChapterId { get; private set; }
        public IReadOnlyCollection<string> Flags => flags;
        public IReadOnlyCollection<string> ExecutedEventIds => executedEventIds;

        public static StoryProgressState CreateInitial(string initialChapterId)
        {
            return new StoryProgressState(
                initialChapterId,
                Array.Empty<string>(),
                Array.Empty<string>());
        }

        public static StoryProgressState Restore(
            string currentChapterId,
            IEnumerable<string> storyFlags,
            IEnumerable<string> executedEventIds)
        {
            return new StoryProgressState(
                currentChapterId,
                storyFlags,
                executedEventIds);
        }

        public bool HasFlag(string flagId)
        {
            return flags.Contains(StableContentId.Normalize(flagId));
        }

        public bool SetFlag(string flagId)
        {
            return flags.Add(StableContentId.Require(flagId, nameof(flagId)));
        }

        public bool SetChapter(string chapterId)
        {
            string normalized = StableContentId.Require(chapterId, nameof(chapterId));
            if (string.Equals(CurrentChapterId, normalized, StringComparison.Ordinal))
            {
                return false;
            }

            CurrentChapterId = normalized;
            return true;
        }

        public bool WasEventExecuted(string storyEventId)
        {
            return executedEventIds.Contains(StableContentId.Normalize(storyEventId));
        }

        internal bool MarkEventExecuted(string storyEventId)
        {
            return executedEventIds.Add(
                StableContentId.Require(storyEventId, nameof(storyEventId)));
        }

        private static IEnumerable<string> NormalizeIds(IEnumerable<string> values)
        {
            if (values == null)
            {
                return Array.Empty<string>();
            }

            return values
                .Select(StableContentId.Normalize)
                .Where(value => value.Length > 0);
        }
    }

    /// <summary>
    /// 1つのStory Eventを発火させるデータ定義です。
    /// Triggerは汎用GameplayEvent、条件はStory Flag、結果はFlag追加と任意の章遷移に限定します。
    /// </summary>
    public sealed class StoryEventDefinition
    {
        private readonly string[] requiredFlagIds;
        private readonly string[] setFlagIds;

        public StoryEventDefinition(
            string storyEventId,
            string triggerEventId,
            string triggerSubjectId = "",
            IEnumerable<string> requiredFlags = null,
            IEnumerable<string> setFlags = null,
            string nextChapterId = "")
        {
            StoryEventId = StableContentId.Require(storyEventId, nameof(storyEventId));
            TriggerEventId = StableContentId.Require(triggerEventId, nameof(triggerEventId));
            TriggerSubjectId = StableContentId.Normalize(triggerSubjectId);
            requiredFlagIds = NormalizeUnique(requiredFlags);
            setFlagIds = NormalizeUnique(setFlags);
            NextChapterId = StableContentId.Normalize(nextChapterId);
        }

        public string StoryEventId { get; }
        public string TriggerEventId { get; }
        public string TriggerSubjectId { get; }
        public IReadOnlyList<string> RequiredFlagIds => requiredFlagIds;
        public IReadOnlyList<string> SetFlagIds => setFlagIds;
        public string NextChapterId { get; }

        public bool Matches(GameplayEvent gameplayEvent, StoryProgressState state)
        {
            if (state == null ||
                !string.Equals(TriggerEventId, gameplayEvent.EventId, StringComparison.Ordinal) ||
                (TriggerSubjectId.Length > 0 &&
                 !string.Equals(TriggerSubjectId, gameplayEvent.SubjectId, StringComparison.Ordinal)))
            {
                return false;
            }

            return requiredFlagIds.All(state.HasFlag);
        }

        private static string[] NormalizeUnique(IEnumerable<string> values)
        {
            return values?
                .Select(StableContentId.Normalize)
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();
        }
    }

    public readonly struct StoryEventExecution
    {
        public StoryEventExecution(string storyEventId, string currentChapterId)
        {
            StoryEventId = storyEventId;
            CurrentChapterId = currentChapterId;
        }

        public string StoryEventId { get; }
        public string CurrentChapterId { get; }
    }

    /// <summary>
    /// Gameplay EventをStory Event定義へ照合し、一度きりのFlag / Chapter更新を実行します。
    /// Event発生元FeatureはStory Stateを直接参照しません。
    /// </summary>
    public sealed class StoryProgressionService
    {
        private readonly StoryEventDefinition[] definitions;

        public StoryProgressionService(
            StoryProgressState state,
            IEnumerable<StoryEventDefinition> storyEventDefinitions)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            definitions = storyEventDefinitions?
                .Where(definition => definition != null)
                .ToArray() ?? Array.Empty<StoryEventDefinition>();

            if (definitions
                .GroupBy(definition => definition.StoryEventId, StringComparer.Ordinal)
                .Any(group => group.Count() > 1))
            {
                throw new ArgumentException(
                    "Story Event IDが重複しています。",
                    nameof(storyEventDefinitions));
            }
        }

        public event Action<StoryEventExecution> EventExecuted;

        public StoryProgressState State { get; }

        public bool Handle(GameplayEvent gameplayEvent)
        {
            bool executedAny = false;
            foreach (StoryEventDefinition definition in definitions)
            {
                if (State.WasEventExecuted(definition.StoryEventId) ||
                    !definition.Matches(gameplayEvent, State))
                {
                    continue;
                }

                foreach (string flagId in definition.SetFlagIds)
                {
                    State.SetFlag(flagId);
                }

                if (definition.NextChapterId.Length > 0)
                {
                    State.SetChapter(definition.NextChapterId);
                }

                State.MarkEventExecuted(definition.StoryEventId);
                EventExecuted?.Invoke(new StoryEventExecution(
                    definition.StoryEventId,
                    State.CurrentChapterId));
                executedAny = true;
            }

            return executedAny;
        }
    }
}
