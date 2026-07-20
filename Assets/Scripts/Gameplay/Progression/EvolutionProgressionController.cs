using System;
using System.Collections.Generic;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Modifiers;
using DemonKing.Gameplay.Modifiers.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Progression
{
    /// <summary>
    /// Unity上のキャラクターへEvolution評価・実行と永続補正を接続します。
    /// 見た目の形態変更はEvolutionApplied通知を購読するPresentation側の責務です。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EvolutionProgressionController : MonoBehaviour,
        IAbilityCooldownModifierSource,
        IOutgoingDamageModifierSource,
        IArtMasteryModifierSource
    {
        public EvolutionProgressionService Service { get; private set; }
        public bool IsInitialized => Service != null;

        public void Initialize(
            CharacterProgressionState progressionState,
            IEnumerable<EvolutionDefinition> evolutionDefinitions)
        {
            Service = new EvolutionProgressionService(
                progressionState,
                evolutionDefinitions,
                TryResolveArtRank);
        }

        public EvolutionEvaluationResult Evaluate(string evolutionNodeId)
        {
            EnsureInitialized();
            return Service.Evaluate(evolutionNodeId);
        }

        public EvolutionApplyResult Evolve(string evolutionNodeId)
        {
            EnsureInitialized();
            return Service.Evolve(evolutionNodeId);
        }

        public NumericModifier GetAbilityCooldownModifier(AbilityDefinition definition)
        {
            return definition == null || Service == null
                ? NumericModifier.Identity
                : Service.GetModifier(
                    GameplayModifierTarget.AbilityCooldown,
                    definition.AbilityId);
        }

        public NumericModifier GetOutgoingDamageModifier(AbilityDefinition definition)
        {
            return definition == null || Service == null
                ? NumericModifier.Identity
                : Service.GetModifier(
                    GameplayModifierTarget.OutgoingDamage,
                    definition.AbilityId);
        }

        public NumericModifier GetArtMasteryModifier(ArtDefinition definition)
        {
            return definition == null || Service == null
                ? NumericModifier.Identity
                : Service.GetModifier(
                    GameplayModifierTarget.ArtMasteryGain,
                    definition.ArtId);
        }

        private bool TryResolveArtRank(string artId, out int rank)
        {
            ArtProgressionController artController = GetComponent<ArtProgressionController>();
            if (artController != null && artController.IsInitialized)
            {
                return artController.Service.TryGetCurrentRank(artId, out rank);
            }

            rank = 0;
            return false;
        }

        private void EnsureInitialized()
        {
            if (Service == null)
            {
                throw new InvalidOperationException("Evolution進捗が初期化されていません。");
            }
        }
    }
}
