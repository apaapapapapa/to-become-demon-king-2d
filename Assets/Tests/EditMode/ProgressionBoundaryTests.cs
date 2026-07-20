using DemonKing.Core.Application;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Save;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Combat.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Modifiers;
using DemonKing.Gameplay.Modifiers.Configuration;
using DemonKing.Gameplay.Progression;
using NUnit.Framework;
using UnityEditor;
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
                unlockedEvolutionNodeIds: new[] { "evolution.red" },
                artProgressStates: new[]
                {
                    ArtProgressState.Restore("art.magic.fire", 12)
                });

            PlayerSaveData saveData = CharacterProgressionSaveMapper.ToSaveData(state);
            CharacterProgressionState restored = CharacterProgressionSaveMapper.FromSaveData(saveData);

            Assert.That(saveData.characterDefinitionId, Is.EqualTo("character.player.slime"));
            Assert.That(restored.Level, Is.EqualTo(3));
            Assert.That(restored.CurrentExperience, Is.EqualTo(240));
            Assert.That(saveData.artProgress, Has.Count.EqualTo(1));
            Assert.That(saveData.artProgress[0].artId, Is.EqualTo("art.magic.fire"));
            Assert.That(saveData.artProgress[0].masteryPoints, Is.EqualTo(12));
            Assert.That(restored.ArtProgressStates, Has.Count.EqualTo(1));
            Assert.That(restored.ArtProgressStates[0].ArtId, Is.EqualTo("art.magic.fire"));
            Assert.That(restored.ArtProgressStates[0].MasteryPoints, Is.EqualTo(12));
            Assert.That(restored.UnlockedSkillIds, Is.EqualTo(new[] { "skill.fire", "skill.guard" }));
            Assert.That(restored.UnlockedEvolutionNodeIds, Is.EqualTo(new[] { "evolution.red" }));
        }

        [Test]
        public void GameSaveData_新規作成時は現在のバージョンを持つ()
        {
            var saveData = new GameSaveData();

            Assert.That(saveData.version, Is.EqualTo(GameSaveData.CurrentVersion));
            Assert.That(saveData.player, Is.Not.Null);
            Assert.That(saveData.player.artProgress, Is.Empty);
        }

        [Test]
        public void GameSaveData_Version1を空のArt進捗を持つVersion2へ移行する()
        {
            var saveData = new GameSaveData
            {
                version = 1,
                player = new PlayerSaveData
                {
                    characterDefinitionId = "character.player.slime",
                    artProgress = null
                }
            };

            GameSaveData migrated = GameSaveDataMigrator.MigrateToCurrent(saveData);

            Assert.That(migrated, Is.SameAs(saveData));
            Assert.That(migrated.version, Is.EqualTo(2));
            Assert.That(migrated.player.artProgress, Is.Not.Null.And.Empty);
        }

        [Test]
        public void GameSaveData_未来のVersionを拒否する()
        {
            var saveData = new GameSaveData
            {
                version = GameSaveData.CurrentVersion + 1
            };

            Assert.That(
                () => GameSaveDataMigrator.MigrateToCurrent(saveData),
                Throws.TypeOf<System.NotSupportedException>());
        }

        [Test]
        public void ArtMasteryTable_累積熟練度からランクを解決する()
        {
            var table = new ArtMasteryTable(new long[] { 0, 2, 5 });

            Assert.That(table.GetRankForTotalMasteryPoints(0), Is.EqualTo(1));
            Assert.That(table.GetRankForTotalMasteryPoints(1), Is.EqualTo(1));
            Assert.That(table.GetRankForTotalMasteryPoints(2), Is.EqualTo(2));
            Assert.That(table.GetRankForTotalMasteryPoints(5), Is.EqualTo(3));
            Assert.That(table.GetRankForTotalMasteryPoints(999), Is.EqualTo(3));
        }

        [Test]
        public void ArtProgressState_熟練度を加算してオーバーフローを防ぐ()
        {
            ArtProgressState state = ArtProgressState.Restore(
                "art.sword.test",
                long.MaxValue - 1);

            long applied = state.GainMastery(10);

            Assert.That(applied, Is.EqualTo(1));
            Assert.That(state.MasteryPoints, Is.EqualTo(long.MaxValue));
        }

        [Test]
        public void CharacterProgressionState_Skill取得を冪等に保存できる()
        {
            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                "character.player.slime");

            Assert.That(state.TryUnlockSkill("skill.combat.test"), Is.True);
            Assert.That(state.TryUnlockSkill("skill.combat.test"), Is.False);
            Assert.That(state.IsSkillUnlocked("skill.combat.test"), Is.True);

            CharacterProgressionState restored = CharacterProgressionSaveMapper.FromSaveData(
                CharacterProgressionSaveMapper.ToSaveData(state));

            Assert.That(restored.UnlockedSkillIds, Is.EqualTo(new[] { "skill.combat.test" }));
        }

        [Test]
        public void Evolution_条件成立後に排他的なNodeを不可逆に取得して保存できる()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            CharacterProgressionState state = CharacterProgressionState.CreateInitial(
                projectAssets.PlayerCharacter.CharacterId);
            var service = new EvolutionProgressionService(
                state,
                projectAssets.PlayerCharacter.EvolutionDefinitions,
                (string artId, out int rank) =>
                {
                    rank = artId == "art.magic.fire" ? 2 : 0;
                    return rank > 0;
                });

            EvolutionEvaluationResult initial = service.Evaluate(
                "evolution.slime.predator");

            Assert.That(initial.Status, Is.EqualTo(EvolutionEvaluationStatus.RequirementsNotMet));
            Assert.That(
                HasFailure(initial, EvolutionRequirementKind.Level),
                Is.True);
            Assert.That(
                HasFailure(initial, EvolutionRequirementKind.Skill),
                Is.True);

            state.GainExperience(
                5,
                projectAssets.PlayerCharacter.ExperienceTableDefinition.CreateRuntimeTable());
            state.TryUnlockSkill("skill.combat.predatory_instinct");

            EvolutionApplyResult applied = service.Evolve("evolution.slime.predator");

            Assert.That(applied.Succeeded, Is.True);
            Assert.That(state.IsEvolutionNodeUnlocked("evolution.slime.predator"), Is.True);
            Assert.That(
                service.GetModifier(
                        GameplayModifierTarget.OutgoingDamage,
                        "ability.basic_melee")
                    .Apply(1),
                Is.EqualTo(2));
            Assert.That(
                service.Evaluate("evolution.slime.predator").Status,
                Is.EqualTo(EvolutionEvaluationStatus.AlreadyUnlocked));

            EvolutionEvaluationResult exclusive = service.Evaluate(
                "evolution.slime.arcane");
            Assert.That(exclusive.Status, Is.EqualTo(EvolutionEvaluationStatus.RequirementsNotMet));
            Assert.That(
                HasFailure(exclusive, EvolutionRequirementKind.ExclusiveChoice),
                Is.True);

            CharacterProgressionState restored = CharacterProgressionSaveMapper.FromSaveData(
                CharacterProgressionSaveMapper.ToSaveData(state));
            Assert.That(
                restored.UnlockedEvolutionNodeIds,
                Is.EqualTo(new[] { "evolution.slime.predator" }));
        }

        [Test]
        public void Evolution_Save復元時の排他Node重複を拒否する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            CharacterProgressionState state = CharacterProgressionState.Restore(
                "character.player.slime",
                level: 2,
                currentExperience: 5,
                unlockedSkillIds: null,
                unlockedEvolutionNodeIds: new[]
                {
                    "evolution.slime.predator",
                    "evolution.slime.arcane"
                });

            Assert.That(
                () => new EvolutionProgressionService(
                    state,
                    projectAssets.PlayerCharacter.EvolutionDefinitions),
                Throws.TypeOf<System.InvalidOperationException>());
        }

        [Test]
        public void NumericModifier_加算値と割合を順序に依存せず適用する()
        {
            NumericModifier modifier = new NumericModifier(2d, 0d)
                .Combine(new NumericModifier(0d, 0.5d));

            Assert.That(modifier.Apply(2), Is.EqualTo(6));
            Assert.That(modifier.Apply(2f), Is.EqualTo(6f));
            Assert.That(modifier.Apply(2L), Is.EqualTo(6L));
        }

        [Test]
        public void ArtDefinition_ランク1Abilityと熟練閾値があれば有効になる()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            var basicMelee = (MeleeAttackDefinition)
                projectAssets.PlayerCharacter.AbilityDefinitions[0];
            ArtDefinition definition = ScriptableObject.CreateInstance<ArtDefinition>();
            var serializedObject = new SerializedObject(definition);
            serializedObject.FindProperty("artId").stringValue = "art.test.training";
            serializedObject.FindProperty("displayName").stringValue = "訓練Art";

            SerializedProperty thresholds = serializedObject.FindProperty(
                "cumulativeMasteryPointsByRank");
            thresholds.arraySize = 2;
            thresholds.GetArrayElementAtIndex(0).longValue = 0;
            thresholds.GetArrayElementAtIndex(1).longValue = 2;

            SerializedProperty unlocks = serializedObject.FindProperty("abilityUnlocks");
            unlocks.arraySize = 1;
            SerializedProperty entry = unlocks.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("abilityDefinition").objectReferenceValue = basicMelee;
            entry.FindPropertyRelative("requiredRank").intValue = 1;
            entry.FindPropertyRelative("masteryPointsPerEffectiveUse").longValue = 1;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(definition.IsConfigured, Is.True);
            Assert.That(definition.CreateMasteryTable().MaxRank, Is.EqualTo(2));

            Object.DestroyImmediate(definition);
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
            Assert.That(projectAssets.PlayerCharacter.AbilityDefinitions.Count, Is.EqualTo(1));
            var basicMelee = projectAssets.PlayerCharacter.AbilityDefinitions[0] as MeleeAttackDefinition;
            Assert.That(basicMelee, Is.Not.Null);
            Assert.That(basicMelee.IsConfigured, Is.True);
            Assert.That(basicMelee.AbilityId, Is.EqualTo("ability.basic_melee"));
            Assert.That(basicMelee.DisplayName, Is.Not.Empty);
            Assert.That(basicMelee.CooldownSeconds, Is.GreaterThanOrEqualTo(0f));
            Assert.That(projectAssets.PlayerCharacter.SkillDefinitions.Count, Is.EqualTo(1));
            SkillDefinition skillDefinition = projectAssets.PlayerCharacter.SkillDefinitions[0];
            Assert.That(skillDefinition, Is.Not.Null);
            Assert.That(skillDefinition.IsConfigured, Is.True);
            Assert.That(skillDefinition.SkillId, Is.EqualTo("skill.combat.predatory_instinct"));
            Assert.That(projectAssets.PlayerCharacter.EvolutionDefinitions.Count, Is.EqualTo(2));
            Assert.That(
                projectAssets.PlayerCharacter.EvolutionDefinitions[0].IsConfigured,
                Is.True);
            Assert.That(
                projectAssets.PlayerCharacter.EvolutionDefinitions[1].IsConfigured,
                Is.True);
            Assert.That(
                projectAssets.PlayerCharacter.EvolutionDefinitions[0].Appearance.VisualEffect,
                Is.EqualTo(EvolutionVisualEffect.PredatorSpikes));
            Assert.That(
                projectAssets.PlayerCharacter.EvolutionDefinitions[1].Appearance.VisualEffect,
                Is.EqualTo(EvolutionVisualEffect.ArcaneWisps));
            Assert.That(projectAssets.PlayerCharacter.ExperienceTableDefinition, Is.Not.Null);
            Assert.That(projectAssets.PlayerCharacter.ExperienceTableDefinition.IsConfigured, Is.True);
            Assert.That(projectAssets.TrainingDummyReward, Is.Not.Null);
            Assert.That(projectAssets.TrainingDummyReward.IsConfigured, Is.True);
            Assert.That(projectAssets.TrainingDummyReward.RewardId, Is.EqualTo("reward.training_dummy"));
            Assert.That(projectAssets.TrainingDummyReward.Experience, Is.EqualTo(5));
        }

        private static bool HasFailure(
            EvolutionEvaluationResult result,
            EvolutionRequirementKind kind)
        {
            foreach (EvolutionRequirementFailure failure in result.Failures)
            {
                if (failure.Kind == kind)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
