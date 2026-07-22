using DemonKing.Core.Input;
using DemonKing.Domain.Progression;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    public sealed class AbilityLoadoutTests
    {
        [Test]
        public void Assign_ReassigningSameSlot_ChangesResolvedAbility()
        {
            var loadout = new AbilityLoadout();
            loadout.Assign(AbilitySlot.Action1, "ability.test.first");

            bool changed = loadout.Assign(AbilitySlot.Action1, "ability.test.second");

            Assert.That(changed, Is.True);
            Assert.That(loadout.TryResolve(AbilitySlot.Action1, out string abilityId), Is.True);
            Assert.That(abilityId, Is.EqualTo("ability.test.second"));
        }

        [Test]
        public void Assign_SameAbilityToSameSlot_IsIdempotent()
        {
            var loadout = new AbilityLoadout();
            Assert.That(loadout.Assign(AbilitySlot.Action1, "ability.test.same"), Is.True);

            bool changed = loadout.Assign(AbilitySlot.Action1, " ability.test.same ");

            Assert.That(changed, Is.False);
        }

        [Test]
        public void Assign_SameAbilityToDifferentEditableSlot_MovesAssignment()
        {
            var loadout = new AbilityLoadout();
            loadout.Assign(AbilitySlot.Action1, "ability.test.unique");

            bool changed = loadout.Assign(AbilitySlot.Action3, "ability.test.unique");

            Assert.That(changed, Is.True);
            Assert.That(loadout.TryResolve(AbilitySlot.Action1, out _), Is.False);
            Assert.That(loadout.TryResolve(AbilitySlot.Action3, out string abilityId), Is.True);
            Assert.That(abilityId, Is.EqualTo("ability.test.unique"));
        }

        [Test]
        public void AbilityLoadoutPolicy_PrimaryIsReservedAndActionSlotsAreEditable()
        {
            Assert.That(AbilityLoadoutPolicy.IsEditableSlot(AbilitySlot.Primary), Is.False);
            Assert.That(
                AbilityLoadoutPolicy.EditableSlots,
                Is.EqualTo(new[]
                {
                    AbilitySlot.Action1,
                    AbilitySlot.Action2,
                    AbilitySlot.Action3,
                    AbilitySlot.Action4
                }));
        }

        [Test]
        public void AbilityLoadoutEligibility_LearnedArtExposesAssignableAbility()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            var definition = projectAssets.PlayerCharacter;
            CharacterProgressionState state =
                CharacterProgressionState.CreateInitial(definition.CharacterId);

            Assert.That(definition.ArtDefinitions.Count, Is.GreaterThan(0));
            Assert.That(state.TryLearnArt(definition.ArtDefinitions[0].ArtId, out _), Is.True);

            var eligible = AbilityLoadoutEligibility.GetAssignableAbilities(definition, state);

            Assert.That(eligible.Count, Is.GreaterThan(0));
            Assert.That(
                AbilityLoadoutEligibility.CanAssign(
                    definition,
                    state,
                    eligible[0].Ability.AbilityId),
                Is.True);
        }

        [Test]
        public void PlayerAbilityInput_TryUseSlot_UsesRuntimeLoadoutResolution()
        {
            GameObject actor = new("Ability Input Test");
            try
            {
                actor.AddComponent<AbilityController>();
                AbilityLoadoutController loadoutController = actor.AddComponent<AbilityLoadoutController>();
                var loadout = new AbilityLoadout();
                loadout.Assign(AbilitySlot.Action2, "ability.test.runtime_slot");
                loadoutController.Initialize(loadout);

                PlayerAbilityInput input = actor.AddComponent<PlayerAbilityInput>();

                Assert.That(input.TryUseSlot(AbilitySlot.Action2), Is.True);
                Assert.That(input.TryUseSlot(AbilitySlot.Action3), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(actor);
            }
        }
    }
}
