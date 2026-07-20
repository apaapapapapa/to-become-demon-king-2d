using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Modifiers;
using DemonKing.Gameplay.Progression.Configuration;

namespace DemonKing.Gameplay.Progression
{
    public enum SkillUnlockStatus
    {
        Succeeded = 0,
        InvalidSkillId = 1,
        DefinitionNotFound = 2,
        AlreadyUnlocked = 3
    }

    public readonly struct SkillUnlockResult
    {
        internal SkillUnlockResult(SkillUnlockStatus status, string skillId)
        {
            Status = status;
            SkillId = skillId ?? string.Empty;
        }

        public SkillUnlockStatus Status { get; }
        public string SkillId { get; }
        public bool Succeeded => Status == SkillUnlockStatus.Succeeded;
    }

    /// <summary>
    /// Skill取得と、取得済みDefinitionからの受動補正集約を担当します。
    /// 補正の適用先は汎用契約だけを参照し、Skill取得状態を知りません。
    /// </summary>
    public sealed class SkillProgressionService
    {
        private const string SkillIdPrefix = "skill.";

        private readonly CharacterProgressionState progressionState;
        private readonly Dictionary<string, SkillDefinition> definitions =
            new(StringComparer.Ordinal);

        public SkillProgressionService(
            CharacterProgressionState progressionState,
            IEnumerable<SkillDefinition> skillDefinitions)
        {
            this.progressionState = progressionState ??
                throw new ArgumentNullException(nameof(progressionState));
            RegisterDefinitions(skillDefinitions);
        }

        public event Action<SkillUnlockResult> SkillUnlocked;

        public SkillUnlockResult Unlock(string skillId)
        {
            string normalizedId = StableContentId.Normalize(skillId);
            if (!IsValidSkillId(normalizedId))
            {
                return new SkillUnlockResult(SkillUnlockStatus.InvalidSkillId, normalizedId);
            }

            if (!definitions.ContainsKey(normalizedId))
            {
                return new SkillUnlockResult(
                    SkillUnlockStatus.DefinitionNotFound,
                    normalizedId);
            }

            if (!progressionState.TryUnlockSkill(normalizedId))
            {
                return new SkillUnlockResult(
                    SkillUnlockStatus.AlreadyUnlocked,
                    normalizedId);
            }

            var result = new SkillUnlockResult(SkillUnlockStatus.Succeeded, normalizedId);
            SkillUnlocked?.Invoke(result);
            return result;
        }

        public NumericModifier GetModifier(SkillModifierTarget target, string contentId)
        {
            string normalizedContentId = StableContentId.Normalize(contentId);
            NumericModifier result = NumericModifier.Identity;

            foreach (string skillId in progressionState.UnlockedSkillIds)
            {
                if (!definitions.TryGetValue(skillId, out SkillDefinition definition))
                {
                    continue;
                }

                foreach (SkillModifierEntry entry in definition.Modifiers)
                {
                    if (entry.AppliesTo(target, normalizedContentId))
                    {
                        result = result.Combine(entry.CreateModifier());
                    }
                }
            }

            return result;
        }

        private void RegisterDefinitions(IEnumerable<SkillDefinition> skillDefinitions)
        {
            if (skillDefinitions == null)
            {
                throw new ArgumentNullException(nameof(skillDefinitions));
            }

            foreach (SkillDefinition definition in skillDefinitions)
            {
                if (definition == null || !definition.IsConfigured)
                {
                    throw new ArgumentException(
                        "正しく設定されたSkillDefinitionだけを登録できます。",
                        nameof(skillDefinitions));
                }

                if (!definitions.TryAdd(definition.SkillId, definition))
                {
                    throw new ArgumentException(
                        $"Skill IDが重複しています: {definition.SkillId}",
                        nameof(skillDefinitions));
                }
            }
        }

        private static bool IsValidSkillId(string skillId)
        {
            return StableContentId.IsValid(skillId) &&
                   skillId.StartsWith(SkillIdPrefix, StringComparison.Ordinal);
        }
    }
}
