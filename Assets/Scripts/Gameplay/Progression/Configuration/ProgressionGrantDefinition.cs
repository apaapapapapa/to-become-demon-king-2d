using System;
using System.Collections.Generic;
using DemonKing.Domain;
using UnityEngine;

namespace DemonKing.Gameplay.Progression.Configuration
{
    /// <summary>
    /// 訓練、報酬、アイテムなどの取得元が共通境界へ渡す成長内容です。
    /// 取得条件や消費は保持せず、付与対象Definitionだけを参照します。
    /// </summary>
    [CreateAssetMenu(
        fileName = "ProgressionGrant",
        menuName = "Demon King/Gameplay/Progression/Grant")]
    public sealed class ProgressionGrantDefinition : ScriptableObject
    {
        [SerializeField] private string grantId = string.Empty;
        [SerializeField] private ArtDefinition[] learnedArts = Array.Empty<ArtDefinition>();
        [SerializeField] private SkillDefinition[] unlockedSkills = Array.Empty<SkillDefinition>();

        public string GrantId => grantId;
        public IReadOnlyList<ArtDefinition> LearnedArts =>
            learnedArts ?? Array.Empty<ArtDefinition>();
        public IReadOnlyList<SkillDefinition> UnlockedSkills =>
            unlockedSkills ?? Array.Empty<SkillDefinition>();

        public bool IsConfigured =>
            StableContentId.IsValid(grantId) &&
            grantId.StartsWith("grant.", StringComparison.Ordinal) &&
            HasValidDefinitions();

        private bool HasValidDefinitions()
        {
            if (learnedArts == null || unlockedSkills == null ||
                learnedArts.Length + unlockedSkills.Length == 0)
            {
                return false;
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (ArtDefinition definition in learnedArts)
            {
                if (definition == null ||
                    !definition.IsConfigured ||
                    !ids.Add(definition.ArtId))
                {
                    return false;
                }
            }

            foreach (SkillDefinition definition in unlockedSkills)
            {
                if (definition == null ||
                    !definition.IsConfigured ||
                    !ids.Add(definition.SkillId))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnValidate()
        {
            grantId = StableContentId.Normalize(grantId);
            learnedArts ??= Array.Empty<ArtDefinition>();
            unlockedSkills ??= Array.Empty<SkillDefinition>();
        }
    }
}
