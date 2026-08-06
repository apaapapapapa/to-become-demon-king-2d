using System.Collections.Generic;
using DemonKing.Domain.Story;
using DemonKing.Gameplay.Dialogue.Configuration;
using DemonKing.Gameplay.Events;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Playable Prologue Part 1で使うStable IDと会話コンテンツをまとめます。
    /// Runtime進行はStoryProgressionServiceへ委譲し、この型はコンテンツ定義だけを所有します。
    /// </summary>
    internal static class PrototypePrologueContent
    {
        public const string GuardianIntroDialogueId = "dialogue.prologue.guardian.intro";
        public const string GuardianObjectiveDialogueId = "dialogue.prologue.guardian.objective";
        public const string GuardianCompleteDialogueId = "dialogue.prologue.guardian.complete";
        public const string GuardianAfterDialogueId = "dialogue.prologue.guardian.after";

        public const string ForageInteractionId = "interaction.prologue.forage";
        public const string ForestCreatureActorId = "character.prologue.forest_whelp";

        private static DialogueDefinition guardianIntroDialogue;
        private static DialogueDefinition guardianObjectiveDialogue;
        private static DialogueDefinition guardianCompleteDialogue;
        private static DialogueDefinition guardianAfterDialogue;

        public static DialogueDefinition GuardianIntroDialogue =>
            guardianIntroDialogue ??= DialogueDefinition.CreateRuntime(
                GuardianIntroDialogueId,
                "育ての親",
                "……目が覚めたか。動けるなら、それで十分だ。",
                "ここは深森の奥だ。おまえはまだ弱い。遠くへ行かず、まず生きるためのことを覚えろ。",
                "腹が減ったら森をよく見ろ。食べられるものと、近づくべきでないものを見分けるんだ。");

        public static DialogueDefinition GuardianObjectiveDialogue =>
            guardianObjectiveDialogue ??= DialogueDefinition.CreateRuntime(
                GuardianObjectiveDialogueId,
                "育ての親",
                "赤い木の実を一つ探してこい。匂いを覚えれば、次からは自分で見つけられる。",
                "それから近くの小さな魔物と一度だけ戦ってみろ。危なくなったら逃げていい。",
                "強さを見せる必要はない。生きて戻ることが一番大事だ。");

        public static DialogueDefinition GuardianCompleteDialogue =>
            guardianCompleteDialogue ??= DialogueDefinition.CreateRuntime(
                GuardianCompleteDialogueId,
                "育ての親",
                "木の実も見つけた。自分の身も、自分で少し守れるようになったな。",
                "強くなくていい。生き延びる術を一つずつ覚えればいい。",
                "戻ってきたら、ここがおまえの居場所だ。忘れるな。");

        public static DialogueDefinition GuardianAfterDialogue =>
            guardianAfterDialogue ??= DialogueDefinition.CreateRuntime(
                GuardianAfterDialogueId,
                "育ての親",
                "今日はもう十分だ。傷があるなら休め。",
                "森は明日もここにある。焦らず、生き延びろ。");

        public static DialogueDefinition SelectGuardianDialogue(StoryProgressState state)
        {
            if (state == null || !state.HasFlag(PrototypeStoryDefinitions.MetGuardianFlagId))
            {
                return GuardianIntroDialogue;
            }

            if (state.HasFlag(PrototypeStoryDefinitions.ProloguePart1CompletedFlagId))
            {
                return GuardianAfterDialogue;
            }

            if (state.HasFlag(PrototypeStoryDefinitions.FoundFoodFlagId) &&
                state.HasFlag(PrototypeStoryDefinitions.FirstHuntFlagId))
            {
                return GuardianCompleteDialogue;
            }

            return GuardianObjectiveDialogue;
        }

        public static IReadOnlyList<StoryEventDefinition> CreateStoryEvents()
        {
            return new[]
            {
                new StoryEventDefinition(
                    "story.event.prologue.born",
                    GameplayEventIds.FieldEntered,
                    PrototypeFieldDefinition.PrologueFieldId,
                    setFlags: new[] { PrototypeStoryDefinitions.BornFlagId }),
                new StoryEventDefinition(
                    "story.event.prologue.met_guardian",
                    GameplayEventIds.DialogueCompleted,
                    GuardianIntroDialogueId,
                    setFlags: new[] { PrototypeStoryDefinitions.MetGuardianFlagId }),
                new StoryEventDefinition(
                    "story.event.prologue.found_food",
                    GameplayEventIds.InteractionCompleted,
                    ForageInteractionId,
                    requiredFlags: new[] { PrototypeStoryDefinitions.MetGuardianFlagId },
                    setFlags: new[] { PrototypeStoryDefinitions.FoundFoodFlagId }),
                new StoryEventDefinition(
                    "story.event.prologue.first_hunt",
                    GameplayEventIds.EnemyDefeated,
                    ForestCreatureActorId,
                    requiredFlags: new[] { PrototypeStoryDefinitions.MetGuardianFlagId },
                    setFlags: new[] { PrototypeStoryDefinitions.FirstHuntFlagId }),
                new StoryEventDefinition(
                    "story.event.prologue.part1_completed",
                    GameplayEventIds.DialogueCompleted,
                    GuardianCompleteDialogueId,
                    requiredFlags: new[]
                    {
                        PrototypeStoryDefinitions.FoundFoodFlagId,
                        PrototypeStoryDefinitions.FirstHuntFlagId
                    },
                    setFlags: new[] { PrototypeStoryDefinitions.ProloguePart1CompletedFlagId })
            };
        }
    }
}
