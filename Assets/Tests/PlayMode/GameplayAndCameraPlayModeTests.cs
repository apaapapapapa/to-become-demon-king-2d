using System;
using System.Collections;
using System.Reflection;
using DemonKing.Domain.Progression;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Combat.Configuration;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Modifiers;
using DemonKing.Gameplay.Modifiers.Configuration;
using DemonKing.Gameplay.Rewards;
using DemonKing.Presentation.CameraSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    /// <summary>
    /// 実行時ライフサイクルが必要なGameplayとPresentationの最小回帰テストです。
    /// </summary>
    public sealed class GameplayAndCameraPlayModeTests
    {
        [UnityTest]
        public IEnumerator Health_致死ダメージでHPが0になり死亡イベントを1回通知する()
        {
            GameObject target = new("Health Test Target");
            Health health = target.AddComponent<Health>();
            health.ConfigureCombatIdentity("character.test.target", "reward.test.target");
            int diedCount = 0;
            DefeatContext diedContext = null;
            health.Died += context =>
            {
                diedCount++;
                diedContext = context;
            };

            yield return null;

            var request = new DamageRequest(
                99,
                sourceActorId: "character.test.attacker",
                abilityId: "ability.test",
                tags: DamageTags.Art);
            DamageResult lethalResult = health.ApplyDamage(request);
            DamageResult ignoredResult = health.ApplyDamage(new DamageRequest(1));

            Assert.That(health.CurrentHealth, Is.EqualTo(0));
            Assert.That(health.IsAlive, Is.False);
            Assert.That(diedCount, Is.EqualTo(1));
            Assert.That(lethalResult.AppliedAmount, Is.EqualTo(3));
            Assert.That(lethalResult.Target, Is.SameAs(target));
            Assert.That(lethalResult.WasDefeated, Is.True);
            Assert.That(ignoredResult.WasApplied, Is.False);
            Assert.That(diedContext, Is.Not.Null);
            Assert.That(diedContext.DefeatId, Is.Not.EqualTo(System.Guid.Empty));
            Assert.That(diedContext.AttackerActorId, Is.EqualTo("character.test.attacker"));
            Assert.That(diedContext.AbilityId, Is.EqualTo("ability.test"));
            Assert.That(diedContext.RewardDefinitionId, Is.EqualTo("reward.test.target"));

            Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator AbilityController_入力へ依存せず近接攻撃を実行してクールダウンを管理する()
        {
            GameObject attacker = new("Attack Effect Test Player");
            MeleeAttackExecutor meleeAttackExecutor = attacker.AddComponent<MeleeAttackExecutor>();
            AbilityController abilityController = attacker.AddComponent<AbilityController>();
            PrototypeMeleeAttackEffect effect = attacker.AddComponent<PrototypeMeleeAttackEffect>();
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            var definition = (MeleeAttackDefinition)projectAssets.PlayerCharacter.AbilityDefinitions[0];
            abilityController.Configure(projectAssets.PlayerCharacter.AbilityDefinitions);

            GameObject target = new("Attack Effect Test Target");
            target.transform.position = Vector2.down * definition.AttackDistance;
            target.AddComponent<CircleCollider2D>().radius = 0.2f;
            Health targetHealth = target.AddComponent<Health>();
            DamageResult receivedDamage = default;
            targetHealth.Damaged += result => receivedDamage = result;

            int performedCount = 0;
            MeleeAttackEvent performedEvent = default;
            meleeAttackExecutor.AttackPerformed += attackEvent =>
            {
                performedCount++;
                performedEvent = attackEvent;
            };

            yield return null;
            Physics2D.SyncTransforms();

            AbilityUseResult useResult = abilityController.TryUse(
                definition.AbilityId,
                attacker,
                new AbilityExecutionInput(Vector2.down));
            AbilityUseResult cooldownResult = abilityController.TryUse(
                definition.AbilityId,
                attacker,
                new AbilityExecutionInput(Vector2.down));
            GameObject spawnedEffect = effect.LastSpawnedEffect;

            Assert.That(useResult.Succeeded, Is.True);
            Assert.That(useResult.UseCount, Is.EqualTo(1));
            Assert.That(useResult.CooldownRemaining, Is.GreaterThan(0f));
            Assert.That(cooldownResult.Status, Is.EqualTo(AbilityUseStatus.CooldownActive));
            Assert.That(performedCount, Is.EqualTo(1));
            Assert.That(performedEvent.FacingDirection, Is.EqualTo(Vector2.down));
            Assert.That(performedEvent.HitCount, Is.EqualTo(1));
            Assert.That(receivedDamage.Request.Source, Is.SameAs(attacker));
            Assert.That(receivedDamage.Request.AbilityId, Is.EqualTo("ability.basic_melee"));
            Assert.That(spawnedEffect, Is.Not.Null);
            Assert.That(
                spawnedEffect.GetComponentsInChildren<SpriteRenderer>().Length,
                Is.GreaterThanOrEqualTo(8));

            yield return new WaitForSecondsRealtime(0.3f);

            Assert.That(spawnedEffect == null, Is.True);
            Object.Destroy(attacker);
            Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator MonsterDefeatEffect_撃破時に破裂エフェクトを生成して対象と独立して再生する()
        {
            GameObject monster = new("Defeat Effect Test Monster");
            Health health = monster.AddComponent<Health>();
            PrototypeMonsterDefeatEffect effect = monster.AddComponent<PrototypeMonsterDefeatEffect>();

            yield return null;

            health.ApplyDamage(new DamageRequest(99));
            GameObject spawnedEffect = effect.LastSpawnedEffect;
            Object.Destroy(monster);

            Assert.That(spawnedEffect, Is.Not.Null);
            Assert.That(spawnedEffect.transform.parent, Is.Null);
            Assert.That(
                spawnedEffect.GetComponentsInChildren<SpriteRenderer>().Length,
                Is.GreaterThanOrEqualTo(13));

            yield return new WaitForSecondsRealtime(0.65f);

            Assert.That(spawnedEffect == null, Is.True);
        }

        [UnityTest]
        public IEnumerator 訓練用ダミー撃破_RewardServiceがプレイヤーへ経験値を一度だけ付与する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            CharacterProgressionState progressionState = CharacterProgressionState.CreateInitial(
                projectAssets.PlayerCharacter.CharacterId);
            var runtimeContext = new CharacterRuntimeContext(
                projectAssets.PlayerCharacter,
                progressionState);
            var rewardService = new RewardService(runtimeContext);

            GameObject dummyObject = new("Reward Test Dummy");
            PrototypeCombatDummy dummy = dummyObject.AddComponent<PrototypeCombatDummy>();
            dummy.ConfigureReward(projectAssets.TrainingDummyReward);

            RewardGrantResult grantedResult = default;
            dummy.Defeated += context =>
            {
                grantedResult = rewardService.GrantDefeatReward(
                    context,
                    projectAssets.TrainingDummyReward);
            };

            Health health = dummyObject.GetComponent<Health>();
            DamageResult damageResult = health.ApplyDamage(new DamageRequest(
                99,
                sourceActorId: projectAssets.PlayerCharacter.CharacterId,
                abilityId: "ability.basic_melee",
                tags: DamageTags.BasicAttack));

            Assert.That(damageResult.WasDefeated, Is.True);
            Assert.That(grantedResult.WasGranted, Is.True);
            Assert.That(grantedResult.GrantedExperience, Is.EqualTo(5));
            Assert.That(grantedResult.LevelUpResult.DidLevelUp, Is.True);
            Assert.That(progressionState.CurrentExperience, Is.EqualTo(5));
            Assert.That(progressionState.Level, Is.EqualTo(2));

            RewardGrantResult duplicateResult = rewardService.GrantDefeatReward(
                damageResult.DefeatContext,
                projectAssets.TrainingDummyReward);

            Assert.That(duplicateResult.WasGranted, Is.False);
            Assert.That(
                duplicateResult.FailureReason,
                Is.EqualTo(RewardGrantFailureReason.AlreadyGranted));
            Assert.That(progressionState.CurrentExperience, Is.EqualTo(5));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Art習得と効果成立_実行単位で熟練しランクAbilityを解放する()
        {
            MeleeAttackDefinition initialAbility = CreateMeleeAbility(
                "ability.art.test.slash",
                "試し斬り");
            MeleeAttackDefinition derivedAbility = CreateMeleeAbility(
                "ability.art.test.cross_slash",
                "十字斬り");
            ArtDefinition artDefinition = CreateArtDefinition(
                initialAbility,
                derivedAbility);

            GameObject attacker = new("Art Progression Test User");
            attacker.transform.position = new Vector3(100f, 100f, 0f);
            attacker.AddComponent<MeleeAttackExecutor>();
            AbilityController abilityController = attacker.AddComponent<AbilityController>();
            abilityController.Configure(Array.Empty<AbilityDefinition>());

            CharacterProgressionState progressionState =
                CharacterProgressionState.CreateInitial("character.test.art_user");
            ArtProgressionController artController =
                attacker.AddComponent<ArtProgressionController>();
            artController.Initialize(progressionState, new[] { artDefinition });

            int effectResolvedCount = 0;
            int masteryAwardedCount = 0;
            abilityController.EffectResolved += _ => effectResolvedCount++;
            artController.Service.MasteryAwarded += _ => masteryAwardedCount++;

            ArtLearnResult learnResult = artController.Learn(artDefinition.ArtId);

            Assert.That(learnResult.Succeeded, Is.True);
            Assert.That(learnResult.CurrentRank, Is.EqualTo(1));
            Assert.That(abilityController.HasAbility(initialAbility.AbilityId), Is.True);
            Assert.That(abilityController.HasAbility(derivedAbility.AbilityId), Is.False);
            ArtLearnResult duplicateLearnResult = artController.Learn(artDefinition.ArtId);
            Assert.That(duplicateLearnResult.Status, Is.EqualTo(ArtLearnStatus.AlreadyLearned));
            Assert.That(duplicateLearnResult.NewlyGrantedAbilityIds, Is.Empty);
            Assert.That(
                progressionState.TryGetArtProgress(
                    artDefinition.ArtId,
                    out ArtProgressState progressState),
                Is.True);

            AbilityUseResult missResult = abilityController.TryUse(
                initialAbility.AbilityId,
                attacker,
                new AbilityExecutionInput(Vector2.down));

            Assert.That(missResult.Succeeded, Is.True);
            Assert.That(missResult.ExecutionId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(progressState.MasteryPoints, Is.EqualTo(0));
            Assert.That(effectResolvedCount, Is.EqualTo(0));

            Vector2 attackCenter = (Vector2)attacker.transform.position +
                                   Vector2.down * initialAbility.AttackDistance;
            GameObject firstTarget = CreateDamageTarget(
                "Art Progression Target A",
                attackCenter + Vector2.left * 0.1f);
            GameObject secondTarget = CreateDamageTarget(
                "Art Progression Target B",
                attackCenter + Vector2.right * 0.1f);

            yield return null;
            Physics2D.SyncTransforms();

            AbilityUseResult firstHitResult = abilityController.TryUse(
                initialAbility.AbilityId,
                attacker,
                new AbilityExecutionInput(Vector2.down));

            Assert.That(firstHitResult.Succeeded, Is.True);
            Assert.That(progressState.MasteryPoints, Is.EqualTo(1));
            Assert.That(effectResolvedCount, Is.EqualTo(2));
            Assert.That(masteryAwardedCount, Is.EqualTo(1));
            Assert.That(abilityController.HasAbility(derivedAbility.AbilityId), Is.False);

            AbilityUseResult secondHitResult = abilityController.TryUse(
                initialAbility.AbilityId,
                attacker,
                new AbilityExecutionInput(Vector2.down));

            Assert.That(secondHitResult.Succeeded, Is.True);
            Assert.That(secondHitResult.ExecutionId, Is.Not.EqualTo(firstHitResult.ExecutionId));
            Assert.That(progressState.MasteryPoints, Is.EqualTo(2));
            Assert.That(effectResolvedCount, Is.EqualTo(4));
            Assert.That(masteryAwardedCount, Is.EqualTo(2));
            Assert.That(abilityController.HasAbility(derivedAbility.AbilityId), Is.True);
            Assert.That(
                artController.Service.TryGetCurrentRank(artDefinition.ArtId, out int currentRank),
                Is.True);
            Assert.That(currentRank, Is.EqualTo(2));

            GameObject restoredUser = new("Restored Art Progression Test User");
            AbilityController restoredAbilityController =
                restoredUser.AddComponent<AbilityController>();
            restoredAbilityController.Configure(Array.Empty<AbilityDefinition>());
            CharacterProgressionState restoredProgression = CharacterProgressionState.Restore(
                "character.test.restored_art_user",
                level: 1,
                currentExperience: 0,
                unlockedSkillIds: null,
                unlockedEvolutionNodeIds: null,
                artProgressStates: new[]
                {
                    ArtProgressState.Restore(artDefinition.ArtId, 2)
                });
            ArtProgressionController restoredArtController =
                restoredUser.AddComponent<ArtProgressionController>();
            restoredArtController.Initialize(restoredProgression, new[] { artDefinition });

            Assert.That(restoredAbilityController.HasAbility(initialAbility.AbilityId), Is.True);
            Assert.That(restoredAbilityController.HasAbility(derivedAbility.AbilityId), Is.True);

            Object.Destroy(attacker);
            Object.Destroy(restoredUser);
            Object.Destroy(firstTarget);
            Object.Destroy(secondTarget);
            Object.Destroy(initialAbility);
            Object.Destroy(derivedAbility);
            Object.Destroy(artDefinition);
        }

        [UnityTest]
        public IEnumerator 受動Skill_与ダメージとクールダウンとArt熟練度を補正する()
        {
            MeleeAttackDefinition ability = CreateMeleeAbility(
                "ability.art.test.skill_slash",
                "Skill補正斬り");
            SetPrivateField(ability, "cooldownSeconds", 2f);
            ArtDefinition artDefinition = CreateSingleAbilityArtDefinition(ability);
            SkillDefinition skillDefinition = CreateSkillDefinition(
                ability.AbilityId,
                artDefinition.ArtId);

            GameObject attacker = new("Passive Skill Test User");
            attacker.transform.position = new Vector3(200f, 200f, 0f);
            attacker.AddComponent<MeleeAttackExecutor>();
            AbilityController abilityController = attacker.AddComponent<AbilityController>();
            abilityController.Configure(Array.Empty<AbilityDefinition>());

            CharacterProgressionState progressionState =
                CharacterProgressionState.CreateInitial("character.test.skill_user");
            SkillProgressionController skillController =
                attacker.AddComponent<SkillProgressionController>();
            skillController.Initialize(progressionState, new[] { skillDefinition });
            Assert.That(skillController.Unlock(skillDefinition.SkillId).Succeeded, Is.True);
            Assert.That(
                skillController.Unlock(skillDefinition.SkillId).Status,
                Is.EqualTo(SkillUnlockStatus.AlreadyUnlocked));

            ArtProgressionController artController =
                attacker.AddComponent<ArtProgressionController>();
            artController.Initialize(progressionState, new[] { artDefinition });
            Assert.That(artController.Learn(artDefinition.ArtId).Succeeded, Is.True);

            Vector2 attackCenter = (Vector2)attacker.transform.position +
                                   Vector2.down * ability.AttackDistance;
            GameObject target = CreateDamageTarget("Passive Skill Target", attackCenter);
            Health health = target.GetComponent<Health>();
            health.ConfigureMaxHealth(10);

            yield return null;
            Physics2D.SyncTransforms();

            AbilityUseResult result = abilityController.TryUse(
                ability.AbilityId,
                attacker,
                new AbilityExecutionInput(Vector2.down));

            Assert.That(result.Succeeded, Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(8));
            Assert.That(
                abilityController.TryGetRuntimeState(ability.AbilityId, out AbilityRuntimeState state),
                Is.True);
            Assert.That(state.CooldownRemaining, Is.EqualTo(1f).Within(0.001f));
            Assert.That(
                progressionState.TryGetArtProgress(
                    artDefinition.ArtId,
                    out ArtProgressState artProgress),
                Is.True);
            Assert.That(artProgress.MasteryPoints, Is.EqualTo(2));

            Object.Destroy(attacker);
            Object.Destroy(target);
            Object.Destroy(ability);
            Object.Destroy(artDefinition);
            Object.Destroy(skillDefinition);
        }

        [UnityTest]
        public IEnumerator Evolution_適用通知と永続補正がAbility実行へ反映される()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            MeleeAttackDefinition ability = CreateMeleeAbility(
                "ability.test.evolution_strike",
                "Evolution補正攻撃");
            GameObject attacker = new("Evolution Test User");
            attacker.transform.position = new Vector3(300f, 300f, 0f);
            attacker.AddComponent<MeleeAttackExecutor>();
            AbilityController abilityController = attacker.AddComponent<AbilityController>();
            abilityController.Configure(new[] { ability });

            CharacterProgressionState progressionState = CharacterProgressionState.Restore(
                "character.player.slime",
                level: 2,
                currentExperience: 5,
                unlockedSkillIds: new[] { "skill.combat.predatory_instinct" },
                unlockedEvolutionNodeIds: null);
            EvolutionProgressionController evolutionController =
                attacker.AddComponent<EvolutionProgressionController>();
            evolutionController.Initialize(
                progressionState,
                projectAssets.PlayerCharacter.EvolutionDefinitions);

            int appliedCount = 0;
            evolutionController.Service.EvolutionApplied += _ => appliedCount++;
            EvolutionApplyResult evolutionResult = evolutionController.Evolve(
                "evolution.slime.predator");

            Assert.That(evolutionResult.Succeeded, Is.True);
            Assert.That(appliedCount, Is.EqualTo(1));
            Assert.That(
                evolutionController.Evolve("evolution.slime.predator").Succeeded,
                Is.False);
            Assert.That(appliedCount, Is.EqualTo(1));

            Vector2 attackCenter = (Vector2)attacker.transform.position +
                                   Vector2.down * ability.AttackDistance;
            GameObject target = CreateDamageTarget("Evolution Target", attackCenter);
            Health health = target.GetComponent<Health>();
            health.ConfigureMaxHealth(10);

            yield return null;
            Physics2D.SyncTransforms();

            AbilityUseResult useResult = abilityController.TryUse(
                ability.AbilityId,
                attacker,
                new AbilityExecutionInput(Vector2.down));

            Assert.That(useResult.Succeeded, Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(8));

            Object.Destroy(attacker);
            Object.Destroy(target);
            Object.Destroy(ability);
        }

        [UnityTest]
        public IEnumerator CameraFollow2D_ターゲットへ追従しZ座標を維持する()
        {
            GameObject cameraObject = new("Camera Follow Test");
            cameraObject.transform.position = new Vector3(5f, 5f, -10f);
            CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();

            GameObject target = new("Camera Target");
            target.transform.position = new Vector3(1f, 2f, 0f);

            follow.SetTarget(target.transform, snapImmediately: true);

            Assert.That(cameraObject.transform.position.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(cameraObject.transform.position.y, Is.EqualTo(3.7f).Within(0.001f));
            Assert.That(cameraObject.transform.position.z, Is.EqualTo(-10f).Within(0.001f));

            target.transform.position = new Vector3(3f, 2f, 0f);
            float beforeX = cameraObject.transform.position.x;
            yield return null;

            Assert.That(cameraObject.transform.position.x, Is.GreaterThan(beforeX));
            Assert.That(cameraObject.transform.position.z, Is.EqualTo(-10f).Within(0.001f));

            Object.Destroy(cameraObject);
            Object.Destroy(target);
        }

        private static MeleeAttackDefinition CreateMeleeAbility(
            string abilityId,
            string displayName)
        {
            MeleeAttackDefinition definition =
                ScriptableObject.CreateInstance<MeleeAttackDefinition>();
            SetPrivateField(definition, "abilityId", abilityId);
            SetPrivateField(definition, "displayName", displayName);
            SetPrivateField(definition, "cooldownSeconds", 0f);
            SetPrivateField(definition, "damageTags", DamageTags.Art);
            Assert.That(definition.IsConfigured, Is.True);
            return definition;
        }

        private static ArtDefinition CreateArtDefinition(
            AbilityDefinition initialAbility,
            AbilityDefinition derivedAbility)
        {
            var initialUnlock = new ArtAbilityUnlockEntry();
            SetPrivateField(initialUnlock, "abilityDefinition", initialAbility);
            SetPrivateField(initialUnlock, "requiredRank", 1);
            SetPrivateField(initialUnlock, "masteryPointsPerEffectiveUse", 1L);

            var derivedUnlock = new ArtAbilityUnlockEntry();
            SetPrivateField(derivedUnlock, "abilityDefinition", derivedAbility);
            SetPrivateField(derivedUnlock, "requiredRank", 2);
            SetPrivateField(derivedUnlock, "masteryPointsPerEffectiveUse", 1L);

            ArtDefinition definition = ScriptableObject.CreateInstance<ArtDefinition>();
            SetPrivateField(definition, "artId", "art.sword.test_training");
            SetPrivateField(definition, "displayName", "試験剣術");
            SetPrivateField(definition, "cumulativeMasteryPointsByRank", new long[] { 0, 2 });
            SetPrivateField(
                definition,
                "abilityUnlocks",
                new[] { initialUnlock, derivedUnlock });
            Assert.That(definition.IsConfigured, Is.True);
            return definition;
        }

        private static ArtDefinition CreateSingleAbilityArtDefinition(
            AbilityDefinition ability)
        {
            var unlock = new ArtAbilityUnlockEntry();
            SetPrivateField(unlock, "abilityDefinition", ability);
            SetPrivateField(unlock, "requiredRank", 1);
            SetPrivateField(unlock, "masteryPointsPerEffectiveUse", 1L);

            ArtDefinition definition = ScriptableObject.CreateInstance<ArtDefinition>();
            SetPrivateField(definition, "artId", "art.sword.skill_test");
            SetPrivateField(definition, "displayName", "Skill補正試験Art");
            SetPrivateField(definition, "cumulativeMasteryPointsByRank", new long[] { 0 });
            SetPrivateField(definition, "abilityUnlocks", new[] { unlock });
            Assert.That(definition.IsConfigured, Is.True);
            return definition;
        }

        private static SkillDefinition CreateSkillDefinition(
            string abilityId,
            string artId)
        {
            var damage = new GameplayModifierEntry();
            SetPrivateField(damage, "target", GameplayModifierTarget.OutgoingDamage);
            SetPrivateField(damage, "operation", NumericModifierOperation.AddFlat);
            SetPrivateField(damage, "value", 1f);
            SetPrivateField(damage, "targetContentId", abilityId);

            var cooldown = new GameplayModifierEntry();
            SetPrivateField(cooldown, "target", GameplayModifierTarget.AbilityCooldown);
            SetPrivateField(cooldown, "operation", NumericModifierOperation.AddRate);
            SetPrivateField(cooldown, "value", -0.5f);
            SetPrivateField(cooldown, "targetContentId", abilityId);

            var mastery = new GameplayModifierEntry();
            SetPrivateField(mastery, "target", GameplayModifierTarget.ArtMasteryGain);
            SetPrivateField(mastery, "operation", NumericModifierOperation.AddFlat);
            SetPrivateField(mastery, "value", 1f);
            SetPrivateField(mastery, "targetContentId", artId);

            SkillDefinition definition = ScriptableObject.CreateInstance<SkillDefinition>();
            SetPrivateField(definition, "skillId", "skill.test.passive_modifiers");
            SetPrivateField(definition, "displayName", "受動補正試験");
            SetPrivateField(
                definition,
                "modifiers",
                new[] { damage, cooldown, mastery });
            Assert.That(definition.IsConfigured, Is.True);
            return definition;
        }

        private static GameObject CreateDamageTarget(string name, Vector2 position)
        {
            GameObject target = new(name);
            target.transform.position = position;
            target.AddComponent<CircleCollider2D>().radius = 0.2f;
            target.AddComponent<Health>();
            return target;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            Type type = target.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }

                type = type.BaseType;
            }

            throw new MissingFieldException(target.GetType().FullName, fieldName);
        }
    }
}
