using System;
using DemonKing.Domain;
using UnityEngine;

namespace DemonKing.Gameplay.Abilities.Configuration
{
    /// <summary>
    /// Abilityの実行時間上の性質を表します。具体的な効果の選択はIAbilityExecutorが担当します。
    /// </summary>
    public enum AbilityExecutionMode
    {
        Instant = 0,
        Duration = 1,
        Channeled = 2
    }

    /// <summary>
    /// Abilityが消費する汎用リソースを表します。Amountが0ならコストなしです。
    /// </summary>
    [Serializable]
    public sealed class AbilityCost
    {
        [SerializeField] private string resourceId = string.Empty;
        [SerializeField, Min(0)] private int amount;

        public string ResourceId => resourceId;
        public int Amount => amount;
        public bool IsFree => amount == 0;
        public bool IsConfigured => IsFree || StableContentId.IsValid(resourceId);

        internal void Normalize()
        {
            resourceId = StableContentId.Normalize(resourceId);
            amount = Mathf.Max(0, amount);
        }
    }

    /// <summary>
    /// すべてのAbilityに共通する不変なコンテンツ定義です。
    /// 実行時に変化するクールダウンや使用回数はAbilityRuntimeStateへ分離します。
    /// </summary>
    public abstract class AbilityDefinition : ScriptableObject
    {
        private const string AbilityIdPrefix = "ability.";

        [Header("Identity")]
        [SerializeField] private string abilityId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private Sprite icon;

        [Header("Execution")]
        [SerializeField] private AbilityExecutionMode executionMode = AbilityExecutionMode.Instant;
        [SerializeField, Min(0f)] private float cooldownSeconds;
        [SerializeField] private AbilityCost cost = new();

        public string AbilityId => abilityId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public AbilityExecutionMode ExecutionMode => executionMode;
        public float CooldownSeconds => cooldownSeconds;
        public AbilityCost Cost => cost;

        public bool IsConfigured =>
            StableContentId.IsValid(abilityId) &&
            abilityId.StartsWith(AbilityIdPrefix, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(displayName) &&
            cooldownSeconds >= 0f &&
            cost != null &&
            cost.IsConfigured &&
            IsAbilitySpecificConfigurationValid;

        protected abstract bool IsAbilitySpecificConfigurationValid { get; }

        protected virtual void OnValidate()
        {
            abilityId = StableContentId.Normalize(abilityId);
            displayName = displayName?.Trim() ?? string.Empty;
            description = description?.Trim() ?? string.Empty;
            cooldownSeconds = Mathf.Max(0f, cooldownSeconds);
            cost ??= new AbilityCost();
            cost.Normalize();
        }
    }
}
