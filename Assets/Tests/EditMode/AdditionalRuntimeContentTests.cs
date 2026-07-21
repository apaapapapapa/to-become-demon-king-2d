using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Content;
using DemonKing.Gameplay.Modifiers.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    public sealed class AdditionalRuntimeContentTests
    {
        [Test]
        public void ProjectAssets_追加ArtSkillと取得GrantをCatalogへ登録できる()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");

            Assert.That(projectAssets, Is.Not.Null);
            Assert.That(projectAssets.IsConfigured, Is.True);
            Assert.That(projectAssets.PlayerCharacter.ArtDefinitions, Has.Count.EqualTo(2));
            Assert.That(projectAssets.PlayerCharacter.SkillDefinitions, Has.Count.EqualTo(2));
            Assert.That(projectAssets.ProgressionPickups, Has.Count.EqualTo(2));

            GameContentCatalog catalog = projectAssets.CreateGameContentCatalog();
            Assert.That(catalog.TryGet("art.magic.arcane_bolt", out var artContent), Is.True);
            Assert.That(artContent, Is.TypeOf<ArtDefinition>());
            Assert.That(catalog.TryGet("ability.magic.arcane_bolt", out _), Is.True);
            Assert.That(catalog.TryGet("skill.magic.mana_flow", out var skillContent), Is.True);

            var manaFlow = (SkillDefinition)skillContent;
            Assert.That(manaFlow.Modifiers, Has.Count.EqualTo(1));
            Assert.That(
                manaFlow.Modifiers[0].Target,
                Is.EqualTo(GameplayModifierTarget.AbilityCooldown));
            Assert.That(
                manaFlow.Modifiers[0].Operation,
                Is.EqualTo(NumericModifierOperation.AddRate));
            Assert.That(manaFlow.Modifiers[0].Value, Is.EqualTo(-0.15f).Within(0.0001f));

            Assert.That(
                projectAssets.ProgressionPickups[0].GrantDefinition.GrantId,
                Is.EqualTo("grant.field.arcane_grimoire"));
            Assert.That(
                projectAssets.ProgressionPickups[1].GrantDefinition.GrantId,
                Is.EqualTo("grant.field.mana_crystal"));
        }
    }
}
