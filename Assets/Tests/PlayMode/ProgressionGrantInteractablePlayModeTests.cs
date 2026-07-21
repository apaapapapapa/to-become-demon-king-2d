using System.Collections;
using DemonKing.Domain.Progression;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Progression;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    public sealed class ProgressionGrantInteractablePlayModeTests
    {
        [UnityTest]
        public IEnumerator FieldPickups_ArtとSkillを既存AcquisitionService経由で一度だけ取得できる()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            Assert.That(projectAssets, Is.Not.Null);
            Assert.That(projectAssets.IsConfigured, Is.True);
            Assert.That(projectAssets.ProgressionPickups, Has.Count.EqualTo(2));

            GameObject player = new("Progression Pickup Test Player");
            player.AddComponent<MeleeAttackExecutor>();
            player.AddComponent<ProjectileAttackExecutor>();

            AbilityController abilityController = player.AddComponent<AbilityController>();
            abilityController.Configure(projectAssets.PlayerCharacter.AbilityDefinitions);

            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                projectAssets.PlayerCharacter.CharacterId);

            ArtProgressionController artController = player.AddComponent<ArtProgressionController>();
            artController.Initialize(state, projectAssets.PlayerCharacter.ArtDefinitions);

            SkillProgressionController skillController = player.AddComponent<SkillProgressionController>();
            skillController.Initialize(state, projectAssets.PlayerCharacter.SkillDefinitions);

            var acquisitionService = new ProgressionAcquisitionService(
                artController,
                skillController);

            foreach (var pickupDefinition in projectAssets.ProgressionPickups)
            {
                GameObject pickupObject = new(pickupDefinition.DisplayName);
                ProgressionGrantInteractable interactable =
                    pickupObject.AddComponent<ProgressionGrantInteractable>();
                interactable.Initialize(
                    pickupDefinition.GrantDefinition,
                    acquisitionService);

                Assert.That(interactable.CanInteract(player), Is.True);
                interactable.Interact(player);

                Assert.That(interactable.IsConsumed, Is.True);
                Assert.That(pickupObject.activeSelf, Is.False);
                Assert.That(interactable.CanInteract(player), Is.False);

                Object.Destroy(pickupObject);
            }

            Assert.That(
                state.TryGetArtProgress("art.magic.arcane_bolt", out _),
                Is.True);
            Assert.That(state.IsSkillUnlocked("skill.magic.mana_flow"), Is.True);

            Object.Destroy(player);
            yield return null;
        }
    }
}
