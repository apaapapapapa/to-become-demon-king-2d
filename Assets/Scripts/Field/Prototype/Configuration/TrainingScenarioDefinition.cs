using DemonKing.Gameplay.AI.Configuration;
using DemonKing.Gameplay.Dialogue.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Quests.Configuration;
using DemonKing.Gameplay.Rewards.Configuration;
using UnityEngine;

namespace DemonKing.Field.Prototype.Configuration
{
    [CreateAssetMenu(fileName = "TrainingScenario", menuName = "Demon King/Prototype/Training Scenario")]
    public sealed class TrainingScenarioDefinition : ScriptableObject
    {
        [SerializeField] private QuestDefinition questDefinition;
        [SerializeField] private DialogueDefinition offerDialogue;
        [SerializeField] private DialogueDefinition activeDialogue;
        [SerializeField] private DialogueDefinition turnInDialogue;
        [SerializeField] private DialogueDefinition completedDialogue;
        [SerializeField] private EnemyAiDefinition enemyAiDefinition;
        [SerializeField] private RewardDefinition defeatReward;
        [SerializeField] private ProgressionGrantDefinition completionGrant;

        public QuestDefinition QuestDefinition => questDefinition;
        public DialogueDefinition OfferDialogue => offerDialogue;
        public DialogueDefinition ActiveDialogue => activeDialogue;
        public DialogueDefinition TurnInDialogue => turnInDialogue;
        public DialogueDefinition CompletedDialogue => completedDialogue;
        public EnemyAiDefinition EnemyAiDefinition => enemyAiDefinition;
        public RewardDefinition DefeatReward => defeatReward;
        public ProgressionGrantDefinition CompletionGrant => completionGrant;

        public bool IsConfigured =>
            questDefinition != null && questDefinition.IsConfigured &&
            offerDialogue != null && offerDialogue.IsConfigured &&
            activeDialogue != null && activeDialogue.IsConfigured &&
            turnInDialogue != null && turnInDialogue.IsConfigured &&
            completedDialogue != null && completedDialogue.IsConfigured &&
            enemyAiDefinition != null && enemyAiDefinition.IsConfigured &&
            defeatReward != null && defeatReward.IsConfigured &&
            completionGrant != null && completionGrant.IsConfigured;
    }
}
