using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Gameplay.Modifiers;
using UnityEngine;

namespace DemonKing.Gameplay.Progression.Configuration
{
    public enum SkillModifierTarget
    {
        OutgoingDamage = 0,
        AbilityCooldown = 1,
        ArtMasteryGain = 2
    }

    public enum SkillModifierOperation
    {
        AddFlat = 0,
        AddRate = 1
    }

    /// <summary>
    /// Skillが提供する数値補正です。対象IDが空なら同種の全コンテンツへ作用します。
    /// </summary>
    [Serializable]
    public sealed class SkillModifierEntry
    {
        [SerializeField] private SkillModifierTarget target;
        [SerializeField] private SkillModifierOperation operation;
        [SerializeField] private float value;
        [SerializeField] private string targetContentId = string.Empty;

        public SkillModifierTarget Target => target;
        public SkillModifierOperation Operation => operation;
        public float Value => value;
        public string TargetContentId => targetContentId;

        public bool AppliesTo(SkillModifierTarget requestedTarget, string contentId)
        {
            return target == requestedTarget &&
                   (string.IsNullOrEmpty(targetContentId) ||
                    string.Equals(targetContentId, contentId, StringComparison.Ordinal));
        }

        public NumericModifier CreateModifier()
        {
            return operation == SkillModifierOperation.AddFlat
                ? new NumericModifier(value, 0d)
                : new NumericModifier(0d, value);
        }

        internal bool IsConfigured
        {
            get
            {
                if (!Enum.IsDefined(typeof(SkillModifierTarget), target) ||
                    !Enum.IsDefined(typeof(SkillModifierOperation), operation) ||
                    float.IsNaN(value) ||
                    float.IsInfinity(value) ||
                    Mathf.Approximately(value, 0f) ||
                    (operation == SkillModifierOperation.AddRate && value <= -1f))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(targetContentId))
                {
                    return true;
                }

                string requiredPrefix = target == SkillModifierTarget.ArtMasteryGain
                    ? "art."
                    : "ability.";
                return StableContentId.IsValid(targetContentId) &&
                       targetContentId.StartsWith(requiredPrefix, StringComparison.Ordinal);
            }
        }

        internal void Normalize()
        {
            targetContentId = StableContentId.Normalize(targetContentId);
        }
    }

    /// <summary>
    /// 能動行動を所有せず、取得中に常時作用する受動Skillの静的定義です。
    /// </summary>
    [CreateAssetMenu(fileName = "Skill", menuName = "Demon King/Gameplay/Progression/Skill")]
    public sealed class SkillDefinition : ScriptableObject
    {
        private const string SkillIdPrefix = "skill.";

        [Header("Identity")]
        [SerializeField] private string skillId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private Sprite icon;
        [SerializeField] private string category = string.Empty;

        [Header("Passive Modifiers")]
        [SerializeField] private SkillModifierEntry[] modifiers =
            Array.Empty<SkillModifierEntry>();

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public string Category => category;
        public IReadOnlyList<SkillModifierEntry> Modifiers =>
            modifiers ?? Array.Empty<SkillModifierEntry>();

        public bool IsConfigured
        {
            get
            {
                if (!StableContentId.IsValid(skillId) ||
                    !skillId.StartsWith(SkillIdPrefix, StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(displayName) ||
                    modifiers == null ||
                    modifiers.Length == 0)
                {
                    return false;
                }

                foreach (SkillModifierEntry modifier in modifiers)
                {
                    if (modifier == null || !modifier.IsConfigured)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void OnValidate()
        {
            skillId = StableContentId.Normalize(skillId);
            displayName = displayName?.Trim() ?? string.Empty;
            description = description?.Trim() ?? string.Empty;
            category = category?.Trim() ?? string.Empty;
            modifiers ??= Array.Empty<SkillModifierEntry>();
            foreach (SkillModifierEntry modifier in modifiers)
            {
                modifier?.Normalize();
            }
        }
    }
}
