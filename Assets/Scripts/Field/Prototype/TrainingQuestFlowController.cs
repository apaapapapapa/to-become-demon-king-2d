using DemonKing.Domain.Events;
using DemonKing.Domain.Quests;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Spawning;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 訓練シナリオのNPC会話、Quest受注・報告完了、Progression Grant、再訓練要求を調停します。
    /// Dummy撃破イベントの変換やReward付与は担当しません。
    /// </summary>
    internal sealed class TrainingQuestFlowController : MonoBehaviour
    {
        private PrototypeNpcInteractable npc;
        private SpawnLifecycle<PrototypeCombatDummy> dummyLifecycle;
        private ProgressionAcquisitionService acquisitionService;
        private DialogueLog dialogueLog;
        private GameplayEventHub gameplayEventHub;
        private QuestProgressionService questProgressionService;
        private TrainingScenarioDefinition scenario;
        private bool initialized;

        public void Initialize(
            PrototypeNpcInteractable npc,
            SpawnLifecycle<PrototypeCombatDummy> dummyLifecycle,
            ProgressionAcquisitionService acquisitionService,
            DialogueLog dialogueLog,
            GameplayEventHub gameplayEventHub,
            QuestProgressionService questProgressionService,
            TrainingScenarioDefinition scenario)
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
                npc.ActorId));
            dummyLifecycle.SpawnOrRestore();
        }

        private void HandleConversationStarted()
        {
            if (!TryGetQuestState(out QuestProgressState state))
            {
                return;
            }

            npc.ConfigureDialogue(state.Status switch
            {
                QuestProgressStatus.Available => scenario.OfferDialogue,
                QuestProgressStatus.Active => scenario.ActiveDialogue,
                QuestProgressStatus.ReadyToTurnIn => scenario.TurnInDialogue,
                QuestProgressStatus.Completed => scenario.CompletedDialogue,
                _ => scenario.OfferDialogue,
            });
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
