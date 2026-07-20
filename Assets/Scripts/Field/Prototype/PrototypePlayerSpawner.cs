using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Progression;
using DemonKing.Presentation.Characters;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// CharacterDefinitionからプレイヤーを生成し、不変な定義と実行時状態を各Gameplayコンポーネントへ接続します。
    /// </summary>
    internal sealed class PrototypePlayerSpawner
    {
        private readonly Vector3 spawnPosition;
        private readonly CharacterDefinition characterDefinition;
        private readonly CharacterProgressionState progressionState;

        public PrototypePlayerSpawner(
            Vector3 spawnPosition,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState = null)
        {
            this.spawnPosition = spawnPosition;
            this.characterDefinition = characterDefinition;
            this.progressionState = progressionState;
        }

        public GameObject Spawn(Transform parent)
        {
            if (characterDefinition == null || !characterDefinition.IsConfigured)
            {
                Debug.LogError("プレイヤーのCharacterDefinitionが正しく設定されていません。");
                return null;
            }

            CharacterProgressionState state = progressionState ??
                CharacterProgressionState.CreateInitial(characterDefinition.CharacterId);
            var runtimeContext = new CharacterRuntimeContext(characterDefinition, state);

            GameObject root = Object.Instantiate(characterDefinition.Prefab, parent, false);
            root.transform.localPosition = spawnPosition;

            CharacterRuntimeContextHost contextHost = root.GetComponent<CharacterRuntimeContextHost>();
            if (contextHost == null)
            {
                contextHost = root.AddComponent<CharacterRuntimeContextHost>();
            }

            contextHost.Initialize(runtimeContext);

            SlimeController controller = root.GetComponent<SlimeController>();
            if (controller == null)
            {
                Debug.LogWarning("PrototypeSlime PrefabにSlimeControllerがないため、実行時に補完します。", root);
                root.AddComponent<SlimeController>();
            }

            CharacterPhysicsBody3D physicsBody = root.GetComponent<CharacterPhysicsBody3D>();
            if (physicsBody == null)
            {
                physicsBody = root.AddComponent<CharacterPhysicsBody3D>();
            }

            physicsBody.EnsureConfigured();

            PrototypeSlimeSpriteAnimator spriteAnimator =
                root.GetComponent<PrototypeSlimeSpriteAnimator>();
            if (spriteAnimator == null)
            {
                Debug.LogWarning("PrototypeSlime Prefabにスプライトアニメーションがないため、実行時に補完します。", root);
                spriteAnimator = root.AddComponent<PrototypeSlimeSpriteAnimator>();
            }

            if (root.GetComponent<CharacterElevationMotor>() == null)
            {
                root.AddComponent<CharacterElevationMotor>();
            }

            if (root.GetComponent<PlayerElevationInput>() == null)
            {
                root.AddComponent<PlayerElevationInput>();
            }

            if (root.GetComponent<CharacterElevationPresenter>() == null)
            {
                root.AddComponent<CharacterElevationPresenter>();
            }

            CharacterPlanarMotor motor = root.GetComponent<CharacterPlanarMotor>();
            if (motor == null)
            {
                motor = root.AddComponent<CharacterPlanarMotor>();
            }

            motor.Configure(characterDefinition.StatsDefinition);
            motor.DisableBounds();

            Health health = root.GetComponent<Health>();
            if (health == null)
            {
                health = root.AddComponent<Health>();
            }

            health.ConfigureMaxHealth(characterDefinition.StatsDefinition.MaxHealth);
            health.ConfigureCombatIdentity(characterDefinition.CharacterId);

            MeleeAttackExecutor meleeAttackExecutor = root.GetComponent<MeleeAttackExecutor>();
            if (meleeAttackExecutor == null)
            {
                meleeAttackExecutor = root.AddComponent<MeleeAttackExecutor>();
            }

            ProjectileAttackExecutor projectileAttackExecutor =
                root.GetComponent<ProjectileAttackExecutor>();
            if (projectileAttackExecutor == null)
            {
                projectileAttackExecutor = root.AddComponent<ProjectileAttackExecutor>();
            }

            AbilityController abilityController = root.GetComponent<AbilityController>();
            if (abilityController == null)
            {
                abilityController = root.AddComponent<AbilityController>();
            }

            abilityController.Configure(characterDefinition.AbilityDefinitions);

            AbilityLoadoutController loadoutController = root.GetComponent<AbilityLoadoutController>();
            if (loadoutController == null)
            {
                loadoutController = root.AddComponent<AbilityLoadoutController>();
            }

            loadoutController.Initialize(characterDefinition);

            if (root.GetComponent<PlayerAbilityInput>() == null)
            {
                root.AddComponent<PlayerAbilityInput>();
            }

            SkillProgressionController skillProgressionController =
                root.GetComponent<SkillProgressionController>();
            if (skillProgressionController == null)
            {
                skillProgressionController = root.AddComponent<SkillProgressionController>();
            }

            skillProgressionController.Initialize(state, characterDefinition.SkillDefinitions);

            EvolutionProgressionController evolutionProgressionController =
                root.GetComponent<EvolutionProgressionController>();
            if (evolutionProgressionController == null)
            {
                evolutionProgressionController =
                    root.AddComponent<EvolutionProgressionController>();
            }

            evolutionProgressionController.Initialize(
                state,
                characterDefinition.EvolutionDefinitions);

            PrototypeSlimeEvolutionPresenter evolutionPresenter =
                root.GetComponent<PrototypeSlimeEvolutionPresenter>();
            if (evolutionPresenter == null)
            {
                evolutionPresenter = root.AddComponent<PrototypeSlimeEvolutionPresenter>();
            }

            evolutionPresenter.Initialize(evolutionProgressionController);

            ArtProgressionController artProgressionController =
                root.GetComponent<ArtProgressionController>();
            if (artProgressionController == null)
            {
                artProgressionController = root.AddComponent<ArtProgressionController>();
            }

            artProgressionController.Initialize(state, characterDefinition.ArtDefinitions);

            if (root.GetComponent<PrototypeMeleeAttackEffect>() == null)
            {
                root.AddComponent<PrototypeMeleeAttackEffect>();
            }

            if (root.GetComponent<PrototypeProjectileAttackEffect>() == null)
            {
                root.AddComponent<PrototypeProjectileAttackEffect>();
            }

            CharacterDodge dodge = root.GetComponent<CharacterDodge>();
            if (dodge == null)
            {
                dodge = root.AddComponent<CharacterDodge>();
            }

            dodge.Configure(characterDefinition.DodgeDefinition);
            return root;
        }
    }
}
