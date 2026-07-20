using DemonKing.Domain;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Rewards.Configuration
{
    /// <summary>
    /// 撃破などの成果に対して付与する不変の報酬を定義します。
    /// </summary>
    [CreateAssetMenu(
        fileName = "RewardDefinition",
        menuName = "Demon King/Gameplay/Reward Definition")]
    public sealed class RewardDefinition : ScriptableObject
    {
        [SerializeField] private string rewardId = string.Empty;
        [SerializeField, Min(0)] private long experience;
        [SerializeField] private ProgressionGrantDefinition progressionGrant;

        public string RewardId => rewardId;
        public long Experience => experience;
        public ProgressionGrantDefinition ProgressionGrant => progressionGrant;
        public bool IsConfigured =>
            StableContentId.IsValid(rewardId) &&
            experience >= 0 &&
            (progressionGrant == null || progressionGrant.IsConfigured);

        private void OnValidate()
        {
            rewardId = StableContentId.Normalize(rewardId);
            experience = System.Math.Max(0, experience);
        }
    }
}
