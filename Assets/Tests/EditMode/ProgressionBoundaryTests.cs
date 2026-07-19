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
        public void ExperienceTable_累積経験値から境界上のレベルを解決する()
        {
            var table = new ExperienceTable(
                new long[] { 0, 5, 15, 30 },
                keepOverflowAtMaxLevel: true);

            Assert.That(table.GetLevelForTotalExperience(0), Is.EqualTo(1));
            Assert.That(table.GetLevelForTotalExperience(4), Is.EqualTo(1));
            Assert.That(table.GetLevelForTotalExperience(5), Is.EqualTo(2));
            Assert.That(table.GetLevelForTotalExperience(29), Is.EqualTo(3));
            Assert.That(table.GetLevelForTotalExperience(30), Is.EqualTo(4));
            Assert.That(table.GetLevelForTotalExperience(999), Is.EqualTo(4));
        }

        [Test]
        public void CharacterProgressionState_一度の経験値加算で複数レベル上昇する()
        {
            var table = new ExperienceTable(
                new long[] { 0, 5, 15, 30 },
                keepOverflowAtMaxLevel: true);
            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                "character.player.slime");

            LevelUpResult result = state.GainExperience(16, table);

            Assert.That(result.AppliedExperience, Is.EqualTo(16));
            Assert.That(result.PreviousLevel, Is.EqualTo(1));
            Assert.That(result.CurrentLevel, Is.EqualTo(3));
            Assert.That(result.LevelsGained, Is.EqualTo(2));
            Assert.That(result.DidLevelUp, Is.True);
            Assert.That(state.CurrentExperience, Is.EqualTo(16));
            Assert.That(state.Level, Is.EqualTo(3));
        }

        [Test]
        public void CharacterProgressionState_最大レベルで余剰を切り捨てる()
        {
            var table = new ExperienceTable(
                new long[] { 0, 5, 15 },
                keepOverflowAtMaxLevel: false);
            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                "character.player.slime");

            LevelUpResult result = state.GainExperience(99, table);

            Assert.That(result.RequestedExperience, Is.EqualTo(99));
            Assert.That(result.AppliedExperience, Is.EqualTo(15));
            Assert.That(result.ReachedMaxLevel, Is.True);
            Assert.That(state.CurrentExperience, Is.EqualTo(15));
            Assert.That(state.Level, Is.EqualTo(3));
        }

        [Test]
        public void CharacterProgressionState_設定時は最大レベル後の余剰を保持する()
        {
            var table = new ExperienceTable(
                new long[] { 0, 5, 15 },
                keepOverflowAtMaxLevel: true);
            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                "character.player.slime");

            LevelUpResult result = state.GainExperience(99, table);

            Assert.That(result.AppliedExperience, Is.EqualTo(99));
            Assert.That(result.ReachedMaxLevel, Is.True);
            Assert.That(state.CurrentExperience, Is.EqualTo(99));
            Assert.That(state.Level, Is.EqualTo(3));
        }

        [Test]
        public void ExperienceTable_単調増加でない定義を拒否する()
        {
            Assert.That(
                () => new ExperienceTable(
                    new long[] { 0, 5, 5 },
                    keepOverflowAtMaxLevel: true),
                Throws.TypeOf<System.ArgumentException>());
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
            Assert.That(projectAssets.PlayerCharacter.ExperienceTableDefinition, Is.Not.Null);
            Assert.That(projectAssets.PlayerCharacter.ExperienceTableDefinition.IsConfigured, Is.True);
            Assert.That(projectAssets.TrainingDummyReward, Is.Not.Null);
            Assert.That(projectAssets.TrainingDummyReward.IsConfigured, Is.True);
            Assert.That(projectAssets.TrainingDummyReward.RewardId, Is.EqualTo("reward.training_dummy"));
            Assert.That(projectAssets.TrainingDummyReward.Experience, Is.EqualTo(5));
        }
    }
}
