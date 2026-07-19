using DemonKing.Core.Application;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Save;
using DemonKing.Field.Prototype;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    /// <summary>
    /// 成長状態と保存DTOの境界を検証します。
    /// </summary>
    public sealed class ProgressionBoundaryTests
    {
        [Test]
        public void CharacterProgressionState_保存DTOを経由して状態を復元できる()
        {
            CharacterProgressionState state = CharacterProgressionState.Restore(
                "character.player.slime",
                level: 3,
                currentExperience: 240,
                unlockedSkillIds: new[] { "skill.fire", "skill.fire", "skill.guard" },
                unlockedEvolutionNodeIds: new[] { "evolution.red" });

            PlayerSaveData saveData = CharacterProgressionSaveMapper.ToSaveData(state);
            CharacterProgressionState restored = CharacterProgressionSaveMapper.FromSaveData(saveData);

            Assert.That(saveData.characterDefinitionId, Is.EqualTo("character.player.slime"));
            Assert.That(restored.Level, Is.EqualTo(3));
            Assert.That(restored.CurrentExperience, Is.EqualTo(240));
            Assert.That(restored.UnlockedSkillIds, Is.EqualTo(new[] { "skill.fire", "skill.guard" }));
            Assert.That(restored.UnlockedEvolutionNodeIds, Is.EqualTo(new[] { "evolution.red" }));
        }

        [Test]
        public void GameSaveData_新規作成時は現在のバージョンを持つ()
        {
            var saveData = new GameSaveData();

            Assert.That(saveData.version, Is.EqualTo(GameSaveData.CurrentVersion));
            Assert.That(saveData.player, Is.Not.Null);
        }

        [Test]
        public void CharacterProgressionState_不正なレベルを拒否する()
        {
            Assert.That(
                () => CharacterProgressionState.Restore(
                    "character.player.slime",
                    level: 0,
                    currentExperience: 0,
                    unlockedSkillIds: null,
                    unlockedEvolutionNodeIds: null),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void PrototypeProjectAssets_プレイヤー定義の必須参照が揃っている()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");

            Assert.That(projectAssets, Is.Not.Null);
            Assert.That(projectAssets.PlayerCharacter, Is.Not.Null);
            Assert.That(projectAssets.PlayerCharacter.IsConfigured, Is.True);
            Assert.That(projectAssets.PlayerCharacter.CharacterId, Is.EqualTo("character.player.slime"));
        }
    }
}
