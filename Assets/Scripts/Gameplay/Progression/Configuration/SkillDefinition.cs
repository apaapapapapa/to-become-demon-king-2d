using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Gameplay.Modifiers.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Progression.Configuration
{
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
        [SerializeField] private GameplayModifierEntry[] modifiers =
            Array.Empty<GameplayModifierEntry>();

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public string Category => category;
        public IReadOnlyList<GameplayModifierEntry> Modifiers =>
            modifiers ?? Array.Empty<GameplayModifierEntry>();

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

                foreach (GameplayModifierEntry modifier in modifiers)
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
            modifiers ??= Array.Empty<GameplayModifierEntry>();
            foreach (GameplayModifierEntry modifier in modifiers)
            {
                modifier?.Normalize();
            }
        }
    }
}
