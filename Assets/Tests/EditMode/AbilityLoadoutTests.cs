using DemonKing.Core.Input;
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
