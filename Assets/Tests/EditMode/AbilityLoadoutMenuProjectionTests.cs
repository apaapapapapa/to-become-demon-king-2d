using System.Linq;
using DemonKing.Core.Input;
using DemonKing.Domain.Progression;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    public sealed class AbilityLoadoutMenuProjectionTests
    {
        [Test]
        public void Build_未取得状態ではArtとSkillを候補に含めない()
        {
            PrototypeProjectAssets projectAssets = LoadProjectAssets();
            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                projectAssets.PlayerCharacter.CharacterId);

            var entries = AbilityLoadoutMenuProjection.Build(
                projectAssets.PlayerCharacter,
                state);

            Assert.That(entries, Is.Empty);
        }

        [Test]
        public void Build_取得済みArtAbilityは割当可能で受動Skillは表示専用になる()
        {
            PrototypeProjectAssets projectAssets = LoadProjectAssets();
            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                projectAssets.PlayerCharacter.CharacterId);
            state.TryLearnArt("art.magic.fire", out _);
            state.TryUnlockSkill("skill.combat.predatory_instinct");

            var entries = AbilityLoadoutMenuProjection.Build(
                projectAssets.PlayerCharacter,
                state);

            AbilityLoadoutMenuEntry artAbility = entries.Single(
                entry => entry.AbilityId == "ability.magic.fire_bolt");
            AbilityLoadoutMenuEntry passiveSkill = entries.Single(
                entry => entry.SourceContentId == "skill.combat.predatory_instinct");

            Assert.That(artAbility.Kind, Is.EqualTo(AbilityLoadoutMenuEntryKind.ArtAbility));
            Assert.That(artAbility.CanAssign, Is.True);
            Assert.That(passiveSkill.Kind, Is.EqualTo(AbilityLoadoutMenuEntryKind.PassiveSkill));
            Assert.That(passiveSkill.CanAssign, Is.False);
            Assert.That(passiveSkill.AbilityId, Is.Empty);
        }

        [Test]
        public void AbilityLoadoutController_Runtime進捗に応じてArtAbilityの初期割当を切り替える()
        {
            PrototypeProjectAssets projectAssets = LoadProjectAssets();
            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                projectAssets.PlayerCharacter.CharacterId);
            GameObject actor = new("Loadout Progression Test");

            try
            {
                AbilityLoadoutController controller = actor.AddComponent<AbilityLoadoutController>();
                controller.Initialize(projectAssets.PlayerCharacter, state);

                Assert.That(
                    controller.TryResolve(AbilitySlot.Primary, out string primaryAbilityId),
                    Is.True);
                Assert.That(primaryAbilityId, Is.EqualTo("ability.basic_melee"));
                Assert.That(controller.TryResolve(AbilitySlot.Action1, out _), Is.False);

                state.TryLearnArt("art.magic.fire", out _);
                controller.Initialize(projectAssets.PlayerCharacter, state);

                Assert.That(
                    controller.TryResolve(AbilitySlot.Action1, out string actionAbilityId),
                    Is.True);
                Assert.That(actionAbilityId, Is.EqualTo("ability.magic.fire_bolt"));
            }
            finally
            {
                Object.DestroyImmediate(actor);
            }
        }

        private static PrototypeProjectAssets LoadProjectAssets()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            Assert.That(projectAssets, Is.Not.Null);
            Assert.That(projectAssets.PlayerCharacter, Is.Not.Null);
            return projectAssets;
        }
    }
}
