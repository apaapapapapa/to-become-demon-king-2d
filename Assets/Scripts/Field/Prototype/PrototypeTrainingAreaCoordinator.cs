using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Rewards;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Prototype訓練エリア内のFeature間イベントを調停します。
    /// NPC、Combat Dummy、Progression、Rewardの各実装は互いを直接参照せず、
    /// Composition層のこのCoordinatorだけがイベント配線と解除を担当します。
    /// </summary>
    internal sealed class PrototypeTrainingAreaCoordinator : MonoBehaviour
    {
        private PrototypeNpcInteractable npc;
        private PrototypeCombatDummyRespawner dummyRespawner;
        private ProgressionAcquisitionService acquisitionService;
        private ProgressionGrantDefinition trainingGrant;
        private DialogueLog dialogueLog;
        private RewardService rewardService;
        private bool initialized;

        public void Initialize(
            PrototypeNpcInteractable npc,
            PrototypeCombatDummyRespawner dummyRespawner,
            ProgressionAcquisitionService acquisitionService,
            ProgressionGrantDefinition trainingGrant,
            DialogueLog dialogueLog,
            RewardService rewardService)
        {
            if (initialized)
            {
                Debug.LogWarning("PrototypeTrainingAreaCoordinatorは既に初期化されています。", this);
                return;
            }

            this.npc = npc;
            this.dummyRespawner = dummyRespawner;
            this.acquisitionService = acquisitionService;
            this.trainingGrant = trainingGrant;
            this.dialogueLog = dialogueLog;
            this.rewardService = rewardService;

            npc.Interacted += HandleNpcInteracted;
            npc.DialogueCompleted += HandleDialogueCompleted;
            rewardService.RewardGranted += HandleRewardGranted;
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

            if (rewardService != null)
            {
                rewardService.RewardGranted -= HandleRewardGranted;
            }
        }

        private void HandleNpcInteracted()
        {
            dummyRespawner.SpawnOrRestore();
        }

        private void HandleDialogueCompleted(GameObject interactor)
        {
            ProgressionGrantResult result = acquisitionService.Grant(trainingGrant);
            dialogueLog.ShowLine(
                "見習い魔術師",
                result.WasGranted
                    ? "火炎魔法を習得した！ Kキー／ゲームパッドYで火炎弾を放てる。"
                    : "火炎魔法はもう身についている。実戦で熟練を重ねよう。");
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
    }
}
