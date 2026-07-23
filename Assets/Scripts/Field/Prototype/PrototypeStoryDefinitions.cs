using System;
using System.Collections.Generic;
using DemonKing.Domain.Story;
using DemonKing.Gameplay.Events;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// P0 Story Runtimeを検証する最小のStable ID / Event Definition群です。
    /// Story固有のGameplay contentは専用Content境界から追加し、訓練Questへ本編進行を混在させません。
    /// </summary>
    internal static class PrototypeStoryDefinitions
    {
        public const string PrologueChapterId = "story.chapter.prologue";
        public const string JourneyChapterId = "story.chapter.first_journey";

        public const string BornFlagId = "prologue.born";
        public const string MetGuardianFlagId = "prologue.met_guardian";
        public const string FoundFoodFlagId = "prologue.found_food";
        public const string FirstHuntFlagId = "prologue.first_hunt";
        public const string ProloguePart1CompletedFlagId = "prologue.part1_completed";
        public const string MetHumanFlagId = "prologue.met_human";
        public const string LeftForestFlagId = "prologue.left_forest";
        public const string FoundRuinsFlagId = "prologue.found_ruins";
        public const string TrainingCompletedFlagId = "prologue.training_completed";

        public static IReadOnlyList<StoryEventDefinition> Create(PrototypeProjectAssets projectAssets)
        {
            if (projectAssets == null)
            {
                throw new ArgumentNullException(nameof(projectAssets));
            }

            var definitions = new List<StoryEventDefinition>();
            definitions.AddRange(PrototypePrologueContent.CreateStoryEvents());

            if (projectAssets.ApprenticeMageDialogue != null)
            {
                definitions.Add(new StoryEventDefinition(
                    "story.event.met_first_human",
                    GameplayEventIds.InteractionCompleted,
                    projectAssets.ApprenticeMageDialogue.DialogueId,
                    setFlags: new[] { MetHumanFlagId }));
            }

            definitions.Add(new StoryEventDefinition(
                "story.event.left_forest",
                GameplayEventIds.FieldEntered,
                PrototypeFieldDefinition.SecondaryFieldId,
                setFlags: new[] { LeftForestFlagId }));

            if (projectAssets.TrainingQuestDefinition != null)
            {
                definitions.Add(new StoryEventDefinition(
                    "story.event.training_completed",
                    GameplayEventIds.QuestCompleted,
                    projectAssets.TrainingQuestDefinition.QuestId,
                    setFlags: new[] { TrainingCompletedFlagId },
                    nextChapterId: JourneyChapterId));
            }

            return definitions;
        }
    }
}
