using System;
using System.Collections.Generic;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Modifiers;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Progression
{
    /// <summary>
    /// キャラクターのSkill進捗を汎用補正契約へ公開するUnity接続点です。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SkillProgressionController : MonoBehaviour,
        IAbilityCooldownModifierSource,
        IOutgoingDamageModifierSource,
        IArtMasteryModifierSource
    {
        public SkillProgressionService Service { get; private set; }
        public bool IsInitialized => Service != null;

        public void Initialize(
            CharacterProgressionState progressionState,
            IEnumerable<SkillDefinition> skillDefinitions)
        {
            Service = new SkillProgressionService(progressionState, skillDefinitions);
        }

        public SkillUnlockResult Unlock(string skillId)
        {
            EnsureInitialized();
            return Service.Unlock(skillId);
        }

        public NumericModifier GetAbilityCooldownModifier(AbilityDefinition definition)
        {
            return definition == null || Service == null
                ? NumericModifier.Identity
                : Service.GetModifier(
                    SkillModifierTarget.AbilityCooldown,
                    definition.AbilityId);
        }

        public NumericModifier GetOutgoingDamageModifier(AbilityDefinition definition)
        {
            return definition == null || Service == null
                ? NumericModifier.Identity
                : Service.GetModifier(
                    SkillModifierTarget.OutgoingDamage,
                    definition.AbilityId);
        }

        public NumericModifier GetArtMasteryModifier(ArtDefinition definition)
        {
            return definition == null || Service == null
                ? NumericModifier.Identity
                : Service.GetModifier(
                    SkillModifierTarget.ArtMasteryGain,
                    definition.ArtId);
        }

        private void EnsureInitialized()
        {
            if (Service == null)
            {
                throw new InvalidOperationException("Skill進捗が初期化されていません。");
            }
        }
    }
}
