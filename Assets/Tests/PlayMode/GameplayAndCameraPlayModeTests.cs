using System.Collections;
using DemonKing.Core.Input;
using DemonKing.Domain.Progression;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Rewards;
using DemonKing.Presentation.CameraSystem;
using DemonKing.Presentation.Characters;
using DemonKing.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    /// <summary>
    /// Combat計算以外のRuntime統合とPresentationの最小回帰テストです。
    /// 3D Combat / Progressionの回帰はCombatAndProgressionPlayModeTestsが担当します。
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
            GameObject recipient = new("Reward Test Recipient");
            AbilityController abilityController = recipient.AddComponent<AbilityController>();
            abilityController.Configure(projectAssets.PlayerCharacter.AbilityDefinitions);
            SkillProgressionController skillController =
                recipient.AddComponent<SkillProgressionController>();
            skillController.Initialize(
                progressionState,
                projectAssets.PlayerCharacter.SkillDefinitions);
            ArtProgressionController artController =
                recipient.AddComponent<ArtProgressionController>();
            artController.Initialize(
                progressionState,
                projectAssets.PlayerCharacter.ArtDefinitions);
            var acquisitionService = new ProgressionAcquisitionService(
                artController,
                skillController);
            var rewardService = new RewardService(runtimeContext, acquisitionService);

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
            Assert.That(
                progressionState.IsSkillUnlocked("skill.combat.predatory_instinct"),
                Is.True);

            RewardGrantResult duplicateResult = rewardService.GrantDefeatReward(
                damageResult.DefeatContext,
                projectAssets.TrainingDummyReward);
            Assert.That(duplicateResult.WasGranted, Is.False);
            Assert.That(
                duplicateResult.FailureReason,
                Is.EqualTo(RewardGrantFailureReason.AlreadyGranted));

            Object.Destroy(recipient);
            Object.Destroy(dummyObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Evolution選択UI_進化確定で外見を変更しGameplayへ戻る()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            CharacterProgressionState progressionState = CharacterProgressionState.Restore(
                "character.player.slime",
                level: 2,
                currentExperience: 5,
                unlockedSkillIds: new[] { "skill.combat.predatory_instinct" },
                unlockedEvolutionNodeIds: null);

            GameObject player = new("Evolution Presentation Player");
            PlayerInputReader inputReader = player.AddComponent<PlayerInputReader>();
            PrototypeSlimeSpriteAnimator spriteAnimator =
                player.AddComponent<PrototypeSlimeSpriteAnimator>();
            EvolutionProgressionController evolutionController =
                player.AddComponent<EvolutionProgressionController>();
            evolutionController.Initialize(
                progressionState,
                projectAssets.PlayerCharacter.EvolutionDefinitions);
            PrototypeSlimeEvolutionPresenter presenter =
                player.AddComponent<PrototypeSlimeEvolutionPresenter>();
            presenter.Initialize(evolutionController);

            GameObject application = new("Evolution Selection Test");
            EvolutionSelectionController selectionController =
                application.AddComponent<EvolutionSelectionController>();
            selectionController.Initialize(inputReader, evolutionController);

            GameObject uiRoot = new("Evolution UI Test");
            uiRoot.AddComponent<Canvas>();
            GameObject layoutObject = Object.Instantiate(
                projectAssets.EvolutionMenuPrefab,
                uiRoot.transform,
                false);
            EvolutionMenuLayout layout = layoutObject.GetComponent<EvolutionMenuLayout>();
            EvolutionMenuView menuView = uiRoot.AddComponent<EvolutionMenuView>();
            menuView.Initialize(
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                selectionController,
                layout);

            Assert.That(layout, Is.Not.Null);
            Assert.That(layout.IsConfigured, Is.True);
            Assert.That(selectionController.OpenMenu(), Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.UI));
            Assert.That(
                selectionController.SelectedEntry.Value.Evaluation.Status,
                Is.EqualTo(EvolutionEvaluationStatus.Available));
            Assert.That(layout.ChoicesText.text, Does.Contain("捕食スライム"));

            Assert.That(selectionController.ConfirmSelection(), Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.Gameplay));
            Assert.That(
                presenter.CurrentEvolutionNodeId,
                Is.EqualTo("evolution.slime.predator"));
            Assert.That(presenter.EffectRendererCount, Is.EqualTo(2));
            Assert.That(
                spriteAnimator.Appearance,
                Is.SameAs(projectAssets.PlayerCharacter.EvolutionDefinitions[0].Appearance));

            Object.Destroy(uiRoot);
            Object.Destroy(application);
            Object.Destroy(player);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ModalUiPrefabs_主要画面がPrefabLayout参照を保持する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            GameObject root = new("Modal Prefab Test", typeof(RectTransform), typeof(Canvas));

            GameObject pause = Object.Instantiate(projectAssets.PauseMenuPrefab, root.transform, false);
            GameObject evolution = Object.Instantiate(projectAssets.EvolutionMenuPrefab, root.transform, false);
            GameObject loadout = Object.Instantiate(projectAssets.AbilityLoadoutMenuPrefab, root.transform, false);

            Assert.That(pause.GetComponent<PauseMenuLayout>().IsConfigured, Is.True);
            Assert.That(evolution.GetComponent<EvolutionMenuLayout>().IsConfigured, Is.True);
            Assert.That(loadout.GetComponent<AbilityLoadoutMenuLayout>().IsConfigured, Is.True);

            RectTransform pauseRect = pause.GetComponent<RectTransform>();
            Assert.That(pauseRect.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(pauseRect.anchorMax, Is.EqualTo(Vector2.one));

            Object.Destroy(root);
            yield return null;
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
    }
}
