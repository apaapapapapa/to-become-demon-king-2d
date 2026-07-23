using DemonKing.Domain.Events;
using DemonKing.Domain.Quests;
using DemonKing.Domain.Story;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Dialogue.Configuration;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Spawning;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    internal sealed class TrainingQuestFlowController : MonoBehaviour
    {
        private PrototypeNpcInteractable npc;
        private SpawnLifecycle<PrototypeCombatDummy> dummyLifecycle;
        private ProgressionAcquisitionService acquisitionService;
        private DialogueLog dialogueLog;
        private GameplayEventHub gameplayEventHub;
        private QuestProgressionService questProgressionService;
        private StoryProgressionService storyProgressionService;
        private TrainingScenarioDefinition scenario;
        private bool initialized;

        public void Initialize(
            PrototypeNpcInteractable npc,
            SpawnLifecycle<PrototypeCombatDummy> dummyLifecycle,
            ProgressionAcquisitionService acquisitionService,
            DialogueLog dialogueLog,
            GameplayEventHub gameplayEventHub,
            QuestProgressionService questProgressionService,
            TrainingScenarioDefinition scenario,
            StoryProgressionService storyProgressionService = null)
        {
            if (initialized)
            {
                Debug.LogWarning("TrainingQuestFlowControllerは既に初期化されています。", this);
                return;
            }

            this.npc = npc;
            this.dummyLifecycle = dummyLifecycle;
            this.acquisitionService = acquisitionService;
            this.dialogueLog = dialogueLog;
            this.gameplayEventHub = gameplayEventHub;
            this.questProgressionService = questProgressionService;
            this.scenario = scenario;
            this.storyProgressionService = storyProgressionService;

            npc.Interacted += HandleNpcInteracted;
            npc.ConversationStarted += HandleConversationStarted;
            npc.DialogueCompleted += HandleDialogueCompleted;
            initialized = true;
        }

        private void OnDestroy()
        {
            if (!initialized || npc == null)
            {
                return;
            }

            npc.Interacted -= HandleNpcInteracted;
            npc.ConversationStarted -= HandleConversationStarted;
            npc.DialogueCompleted -= HandleDialogueCompleted;
        }

        private void HandleNpcInteracted()
        {
            gameplayEventHub.Publish(new GameplayEvent(
                GameplayEventIds.InteractionCompleted,
                npc.DialogueId));
            dummyLifecycle.SpawnOrRestore();
        }

        private void HandleConversationStarted()
        {
            if (!TryGetQuestState(out QuestProgressState state))
            {
                return;
            }

            DialogueDefinition questDialogue = state.Status switch
            {
                QuestProgressStatus.Available => scenario.OfferDialogue,
                QuestProgressStatus.Active => scenario.ActiveDialogue,
                QuestProgressStatus.ReadyToTurnIn => scenario.TurnInDialogue,
                QuestProgressStatus.Completed => scenario.CompletedDialogue,
                _ => scenario.OfferDialogue,
            };

            if (storyProgressionService == null)
            {
                npc.ConfigureDialogue(questDialogue);
                return;
            }

            // P0では既存Dialogue Assetを再利用し、Story/Quest条件による同一NPCの候補選択境界を検証します。
            DialogueDefinition selectedDialogue = StoryDialogueSelector.Select(
                questDialogue,
                storyProgressionService.State,
                new[]
                {
                    new StoryDialogueVariant<DialogueDefinition>(
                        scenario.CompletedDialogue,
                        requiredFlags: new[] { PrototypeStoryDefinitions.LeftForestFlagId },
                        questId: scenario.QuestDefinition.QuestId,
                        requiredQuestStatus: QuestProgressStatus.Available)
                },
                ResolveQuestStatus);
            npc.ConfigureDialogue(selectedDialogue);
        }

        private void HandleDialogueCompleted(GameObject interactor)
        {
            gameplayEventHub.Publish(new GameplayEvent(
                GameplayEventIds.DialogueCompleted,
                npc.DialogueId));

            if (!TryGetQuestState(out QuestProgressState state))
            {
                return;
            }

            if (state.Status == QuestProgressStatus.Available)
            {
                questProgressionService.AcceptQuest(scenario.QuestDefinition.QuestId);
                return;
            }

            if (!state.IsReadyToTurnIn ||
                !questProgressionService.CompleteQuest(scenario.QuestDefinition.QuestId))
            {
                return;
            }

            ProgressionGrantResult result = acquisitionService.Grant(scenario.CompletionGrant);
            dialogueLog.ShowLine(
                "見習い魔術師",
                result.WasGranted
                    ? "火炎魔法を習得した！ Kキー／ゲームパッドYで火炎弾を放てる。"
                    : "火炎魔法はもう身についている。実戦で熟練を重ねよう。");
        }

        private QuestProgressStatus? ResolveQuestStatus(string questId)
        {
            return questProgressionService != null &&
                   questProgressionService.TryGetState(questId, out QuestProgressState state)
                ? state.Status
                : null;
        }

        private bool TryGetQuestState(out QuestProgressState state)
        {
            state = null;
            return scenario != null &&
                   scenario.QuestDefinition != null &&
                   questProgressionService != null &&
                   questProgressionService.TryGetState(scenario.QuestDefinition.QuestId, out state);
        }
    }
}
