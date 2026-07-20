using System;
using System.Collections;
using System.Reflection;
using DemonKing.Domain.Progression;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Combat.Configuration;
using DemonKing.Gameplay.Modifiers.Configuration;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Progression.Configuration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    public sealed class CombatAndProgressionPlayModeTests
    {
        [UnityTest]
        public IEnumerator AbilityController_3D近接攻撃を実行してクールダウンを管理する()
        {
            GameObject attacker = new("Attack Effect Test Player");
            MeleeAttackExecutor meleeAttackExecutor = attacker.AddComponent<MeleeAttackExecutor>();
            AbilityController abilityController = attacker.AddComponent<AbilityController>();
            PrototypeMeleeAttackEffect effect = attacker.AddComponent<PrototypeMeleeAttackEffect>();
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            var definition = (MeleeAttackDefinition)projectAssets.PlayerCharacter.AbilityDefinitions[0];
            abilityController.Configure(projectAssets.PlayerCharacter.AbilityDefinitions);

            GameObject target = CreateDamageTarget(
                "Attack Effect Test Target",
                Vector2.down * definition.AttackDistance);
            DamageResult receivedDamage = default;
            target.GetComponent<Health>().Damaged += result => receivedDamage = result;

            int performedCount = 0;
            MeleeAttackEvent performedEvent = default;
            meleeAttackExecutor.AttackPerformed += attackEvent =>
            {
                performedCount++;
                performedEvent = attackEvent;
            };

            yield return null;
            Physics.SyncTransforms();

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

            yield return new WaitForSecondsRealtime(0.3f);

            Assert.That(spawnedEffect == null, Is.True);
            Object.Destroy(attacker);
            Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator 火炎魔法Art取得_3D火炎弾が命中して実行単位で熟練する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            CharacterProgressionState progressionState = CharacterProgressionState.CreateInitial(
                projectAssets.PlayerCharacter.CharacterId);

            GameObject attacker = new("Fire Art Test User");
            attacker.transform.position = new Vector3(600f, 600f, 0f);
            attacker.AddComponent<ProjectileAttackExecutor>();
            AbilityController abilityController = attacker.AddComponent<AbilityController>();
            abilityController.Configure(projectAssets.PlayerCharacter.AbilityDefinitions);
            SkillProgressionController skillController =
                attacker.AddComponent<SkillProgressionController>();
            skillController.Initialize(
                progressionState,
                projectAssets.PlayerCharacter.SkillDefinitions);
            ArtProgressionController artController =
                attacker.AddComponent<ArtProgressionController>();
            artController.Initialize(
                progressionState,
                projectAssets.PlayerCharacter.ArtDefinitions);
            var acquisitionService = new ProgressionAcquisitionService(
                artController,
                skillController);

            ProgressionGrantResult acquisitionResult = acquisitionService.Grant(
                projectAssets.FireMagicTrainingGrant);
            Assert.That(acquisitionResult.LearnedArtIds,
                Is.EqualTo(new[] { "art.magic.fire" }));
            Assert.That(abilityController.HasAbility("ability.magic.fire_bolt"), Is.True);

            GameObject target = CreateDamageTarget(
                "Fire Art Target",
                (Vector2)attacker.transform.position + Vector2.right * 2f);
            Health targetHealth = target.GetComponent<Health>();
            targetHealth.ConfigureMaxHealth(10);
            yield return null;
            Physics.SyncTransforms();

            AbilityUseResult useResult = abilityController.TryUse(
                "ability.magic.fire_bolt",
                attacker,
                new AbilityExecutionInput(Vector2.right));
            Assert.That(useResult.Succeeded, Is.True);

            for (int frame = 0; frame < 120 && targetHealth.CurrentHealth == 10; frame++)
            {
                yield return null;
            }

            Assert.That(targetHealth.CurrentHealth, Is.EqualTo(8));
            Assert.That(
                progressionState.TryGetArtProgress(
                    "art.magic.fire",
                    out ArtProgressState fireProgress),
                Is.True);
            Assert.That(fireProgress.MasteryPoints, Is.EqualTo(1));
            Assert.That(
                abilityController.TryGetRuntimeState(
                    "ability.magic.fire_bolt",
                    out AbilityRuntimeState runtimeState),
                Is.True);
            Assert.That(runtimeState.IsExecuting, Is.False);

            Object.Destroy(attacker);
            Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator Art習得と3D攻撃効果成立_実行単位で熟練しランクAbilityを解放する()
        {
            MeleeAttackDefinition initialAbility = CreateMeleeAbility(
                "ability.art.test.slash",
                "試し斬り");
            MeleeAttackDefinition derivedAbility = CreateMeleeAbility(
                "ability.art.test.cross_slash",
                "十字斬り");
            ArtDefinition artDefinition = CreateArtDefinition(initialAbility, derivedAbility);

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
            Assert.That(abilityController.HasAbility(initialAbility.AbilityId), Is.True);
            Assert.That(abilityController.HasAbility(derivedAbility.AbilityId), Is.False);
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
            Assert.That(progressState.MasteryPoints, Is.EqualTo(0));

            Vector2 attackCenter = (Vector2)attacker.transform.position +
                                   Vector2.down * initialAbility.AttackDistance;
            GameObject firstTarget = CreateDamageTarget(
                "Art Progression Target A",
                attackCenter + Vector2.left * 0.1f);
            GameObject secondTarget = CreateDamageTarget(
                "Art Progression Target B",
                attackCenter + Vector2.right * 0.1f);

            yield return null;
            Physics.SyncTransforms();

            AbilityUseResult firstHitResult = abilityController.TryUse(
                initialAbility.AbilityId,
                attacker,
                new AbilityExecutionInput(Vector2.down));
            Assert.That(firstHitResult.Succeeded, Is.True);
            Assert.That(progressState.MasteryPoints, Is.EqualTo(1));
            Assert.That(effectResolvedCount, Is.EqualTo(2));
            Assert.That(masteryAwardedCount, Is.EqualTo(1));

            AbilityUseResult secondHitResult = abilityController.TryUse(
                initialAbility.AbilityId,
                attacker,
                new AbilityExecutionInput(Vector2.down));
            Assert.That(secondHitResult.Succeeded, Is.True);
            Assert.That(progressState.MasteryPoints, Is.EqualTo(2));
            Assert.That(effectResolvedCount, Is.EqualTo(4));
            Assert.That(masteryAwardedCount, Is.EqualTo(2));
            Assert.That(abilityController.HasAbility(derivedAbility.AbilityId), Is.True);

            Object.Destroy(attacker);
            Object.Destroy(firstTarget);
            Object.Destroy(secondTarget);
            Object.Destroy(initialAbility);
            Object.Destroy(derivedAbility);
            Object.Destroy(artDefinition);
        }

        [UnityTest]
        public IEnumerator 受動Skill_3D与ダメージとクールダウンとArt熟練度を補正する()
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
            Physics.SyncTransforms();

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
        public IEnumerator Evolution_永続補正が3D Ability実行へ反映される()
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

            EvolutionApplyResult evolutionResult = evolutionController.Evolve(
                "evolution.slime.predator");
            Assert.That(evolutionResult.Succeeded, Is.True);

            Vector2 attackCenter = (Vector2)attacker.transform.position +
                                   Vector2.down * ability.AttackDistance;
            GameObject target = CreateDamageTarget("Evolution Target", attackCenter);
            Health health = target.GetComponent<Health>();
            health.ConfigureMaxHealth(10);

            yield return null;
            Physics.SyncTransforms();

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

        private static GameObject CreateDamageTarget(string name, Vector2 position)
        {
            GameObject target = new(name);
            target.transform.position = new Vector3(position.x, position.y, 0f);
            target.AddComponent<SphereCollider>().radius = 0.2f;
            target.AddComponent<Health>();
            return target;
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

        private static ArtDefinition CreateSingleAbilityArtDefinition(AbilityDefinition ability)
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

        private static SkillDefinition CreateSkillDefinition(string abilityId, string artId)
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
            SetPrivateField(definition, "modifiers", new[] { damage, cooldown, mastery });
            Assert.That(definition.IsConfigured, Is.True);
            return definition;
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
