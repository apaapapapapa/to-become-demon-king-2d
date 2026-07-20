using System;
using System.Collections.Generic;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Modifiers;
using UnityEngine;

namespace DemonKing.Gameplay.Progression
{
    /// <summary>
    /// Unity上のキャラクターをArtProgressionServiceとAbility効果通知へ接続します。
    /// 習得元や入力方式には依存しません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AbilityController))]
    public sealed class ArtProgressionController : MonoBehaviour
    {
        private AbilityController abilityController;

        public ArtProgressionService Service { get; private set; }
        public bool IsInitialized => Service != null;

        public void Initialize(
            CharacterProgressionState progressionState,
            IEnumerable<ArtDefinition> artDefinitions)
        {
            if (abilityController != null)
            {
                abilityController.EffectResolved -= HandleEffectResolved;
            }

            abilityController = GetComponent<AbilityController>();
            if (abilityController == null)
            {
                throw new InvalidOperationException(
                    "Art進捗の初期化にはAbilityControllerが必要です。");
            }

            Service = new ArtProgressionService(
                gameObject,
                progressionState,
                abilityController,
                artDefinitions,
                FindMasteryModifierSources());
            abilityController.EffectResolved += HandleEffectResolved;
        }

        public ArtLearnResult Learn(string artId)
        {
            EnsureInitialized();
            return Service.Learn(artId);
        }

        public ArtMasteryAwardResult AwardMastery(string artId, long amount)
        {
            EnsureInitialized();
            return Service.AwardMastery(artId, amount);
        }

        private void OnDestroy()
        {
            if (abilityController != null)
            {
                abilityController.EffectResolved -= HandleEffectResolved;
            }
        }

        private void HandleEffectResolved(AbilityEffectResolved effect)
        {
            Service?.AwardMastery(effect);
        }

        private IReadOnlyList<IArtMasteryModifierSource> FindMasteryModifierSources()
        {
            var sources = new List<IArtMasteryModifierSource>();
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IArtMasteryModifierSource source)
                {
                    sources.Add(source);
                }
            }

            return sources;
        }

        private void EnsureInitialized()
        {
            if (Service == null)
            {
                throw new InvalidOperationException("Art進捗が初期化されていません。");
            }
        }
    }
}
