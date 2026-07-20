using DemonKing.Domain.Quests;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Quests;
using DemonKing.Gameplay.Quests.Configuration;
using DemonKing.Gameplay.Rewards;
using DemonKing.Gameplay.Spawning;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Prototype訓練エリア内のFeature間イベントを調停します。
    /// NPC、Combat Dummy、Progression、Reward、Questの各実装は互いを直接参照せず、
    /// Composition層のこのCoordinatorだけがイベント配線と解除を担当します。
    /// </summary>
    internal sealed class PrototypeTrainingAreaCoordinator : MonoBehaviour
    {
        private PrototypeNpcInteractable npc;
        private SpawnLifecycle<PrototypeCombatDummy> dummyLifecycle;
        private ProgressionAcquisitionService acquisitionService;
        private ProgressionGrantDefinition trainingGrant;
        private DialogueLog dialogueLog;
        private RewardService rewardService;
        private GameplayEventHub gameplayEventHub;
        private QuestProgressionService questProgressionService;
        private QuestDefinition trainingQuestDefinition;
        private bool initialized;

        public void Initialize(
            PrototypeNpcInteractable npc,
            SpawnLifecycle<PrototypeCombatDummy> dummyLifecycle,
            ProgressionAcquisitionService acquisitionService,
            ProgressionGrantDefinition trainingGrant,
            DialogueLog dialogueLog,
            RewardService rewardService,
            GameplayEventHub gameplayEventHub,
            QuestProgressionService questProgressionService,
            QuestDefinition trainingQuestDefinition)
        {
            if (initialized)
            {
                Debug.LogWarning("PrototypeTrainingAreaCoordinatorは既に初期化されています。", this);
                return;
            }

            this.npc = npc;
            this.dummyLifecycle = dummyLifecycle;
            this.acquisitionService = acquisitionService;
            this.trainingGrant = trainingGrant;
            this.dialogueLog = dialogueLog;
            this.rewardService = rewardService;
            this.gameplayEventHub = gameplayEventHub;
            this.questProgressionService = questProgressionService;
            this.trainingQuestDefinition = trainingQuestDefinition;

            npc.Interacted += HandleNpcInteracted;
            npc.DialogueCompleted += HandleDialogueCompleted;
            dummyLifecycle.Spawned += HandleDummySpawned;
            rewardService.RewardGranted += HandleRewardGranted;
            gameplayEventHub.Published += questProgressionService.Handle;
            questProgressionService.QuestCompleted += HandleQuestCompleted;
            initialized = true;
        }

        private void OnDestroy()
        {
            if (!initialized)
            {
                return;
            }

            if (npc != null)
            {
                npc.Interacted -= HandleNpcInteracted;
                npc.DialogueCompleted -= HandleDialogueCompleted;
            }

            if (dummyLifecycle != null)
            {
                dummyLifecycle.Spawned -= HandleDummySpawned;
                PrototypeCombatDummy currentDummy = dummyLifecycle.Current;
                if (currentDummy != null)
                {
                    currentDummy.Defeated -= HandleDummyDefeated;
                }
            }

            if (rewardService != null)
            {
                rewardService.RewardGranted -= HandleRewardGranted;
            }

            if (gameplayEventHub != null && questProgressionService != null)
            {
                gameplayEventHub.Published -= questProgressionService.Handle;
            }

            if (questProgressionService != null)
            {
                questProgressionService.QuestCompleted -= HandleQuestCompleted;
            }
        }

        private void HandleNpcInteracted()
        {
            if (trainingQuestDefinition != null)
            {
                questProgressionService.AcceptQuest(trainingQuestDefinition.QuestId);
            }

            dummyLifecycle.SpawnOrRestore();
        }

        private void HandleDialogueCompleted(GameObject interactor)
        {
            ProgressionGrantResult result = acquisitionService.Grant(trainingGrant);
            dialogueLog.ShowLine(
                "見習い魔術師",
                result.WasGranted
                    ? "火炎魔法を習得した！ Kキー／ゲームパッドYで火炎弾を放てる。"
                    : "火炎魔法はもう身についている。実戦で熟練を重ねよう。");
            gameplayEventHub.Publish(new GameplayEvent(
                GameplayEventIds.DialogueCompleted,
                npc.DialogueId));
        }

        private void HandleDummySpawned(PrototypeCombatDummy dummy)
        {
            dummy.Defeated += HandleDummyDefeated;
        }

        private void HandleDummyDefeated(DefeatContext context)
        {
            PrototypeCombatDummy defeatedDummy = dummyLifecycle.Current;
            if (defeatedDummy == null)
            {
                return;
            }

            defeatedDummy.Defeated -= HandleDummyDefeated;
            gameplayEventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                defeatedDummy.ActorId));
            dummyLifecycle.Forget(defeatedDummy);
        }

        private static void HandleRewardGranted(RewardGrantResult result)
        {
            string progressionSummary = result.ProgressionGrantResult.WasGranted
                ? $" Art {string.Join(", ", result.ProgressionGrantResult.LearnedArtIds)}" +
                  $" Skill {string.Join(", ", result.ProgressionGrantResult.UnlockedSkillIds)}"
                : string.Empty;
            Debug.Log(
                $"経験値を{result.GrantedExperience}獲得。" +
                $" レベル {result.LevelUpResult.PreviousLevel} → {result.LevelUpResult.CurrentLevel}、" +
                $"累積経験値 {result.LevelUpResult.CurrentExperience}{progressionSummary}");
        }

        private static void HandleQuestCompleted(QuestProgressState state)
        {
            Debug.Log($"クエスト達成: {state.QuestId}");
        }
    }
}
