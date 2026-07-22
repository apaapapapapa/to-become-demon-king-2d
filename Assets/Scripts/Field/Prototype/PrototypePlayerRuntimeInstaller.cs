using System;
using DemonKing.Core.Input;
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
    internal interface IPrototypePlayerFeatureInstaller
    {
        void Install(
            GameObject root,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState);
    }

    /// <summary>
    /// Player Prefab生成後のFeature初期化を責務単位で調停します。
    /// Prefabが所有すべきComponentは不足時に補完せず構成エラーとし、
    /// Runtime Stateへ依存するControllerだけを明示的に追加します。
    /// </summary>
    internal sealed class PrototypePlayerRuntimeInstaller
    {
        private readonly IPrototypePlayerFeatureInstaller[] featureInstallers;

        public PrototypePlayerRuntimeInstaller()
            : this(new IPrototypePlayerFeatureInstaller[]
            {
                new PlayerPhysicsMovementInstaller(),
                new PlayerCombatAbilityInstaller(),
                new PlayerProgressionEvolutionInstaller(),
                new PlayerInputLoadoutInstaller(),
                new PlayerPresentationInstaller()
            })
        {
        }

        internal PrototypePlayerRuntimeInstaller(
            IPrototypePlayerFeatureInstaller[] featureInstallers)
        {
            this.featureInstallers = featureInstallers ??
                throw new ArgumentNullException(nameof(featureInstallers));
        }

        public void Install(
            GameObject root,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (characterDefinition == null)
            {
                throw new ArgumentNullException(nameof(characterDefinition));
            }

            if (progressionState == null)
            {
                throw new ArgumentNullException(nameof(progressionState));
            }

            PlayerRuntimeComponentAccess.RequirePrefabComponent<SlimeController>(root);
            foreach (IPrototypePlayerFeatureInstaller installer in featureInstallers)
            {
                installer.Install(root, characterDefinition, progressionState);
            }
        }
    }

    internal static class PlayerRuntimeComponentAccess
    {
        public static T RequirePrefabComponent<T>(GameObject root)
            where T : Component
        {
            T component = root.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            throw new InvalidOperationException(
                $"Player Prefab '{root.name}' に必須Component {typeof(T).Name} がありません。Prefab構成を修正してください。");
        }

        public static T GetOrAddRuntimeComponent<T>(GameObject root)
            where T : Component
        {
            T component = root.GetComponent<T>();
            return component != null ? component : root.AddComponent<T>();
        }
    }

    internal sealed class PlayerPhysicsMovementInstaller : IPrototypePlayerFeatureInstaller
    {
        public void Install(
            GameObject root,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            CharacterPhysicsBody3D physicsBody =
                PlayerRuntimeComponentAccess.RequirePrefabComponent<CharacterPhysicsBody3D>(root);
            physicsBody.EnsureConfigured();

            CharacterPlanarMotor motor =
                PlayerRuntimeComponentAccess.RequirePrefabComponent<CharacterPlanarMotor>(root);
            motor.Configure(characterDefinition.StatsDefinition);
            motor.DisableBounds();

            PlayerRuntimeComponentAccess.RequirePrefabComponent<CharacterElevationMotor>(root);
            PlayerRuntimeComponentAccess.RequirePrefabComponent<PlayerElevationInput>(root);

            CharacterDodge dodge =
                PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<CharacterDodge>(root);
            dodge.Configure(characterDefinition.DodgeDefinition);
        }
    }

    internal sealed class PlayerCombatAbilityInstaller : IPrototypePlayerFeatureInstaller
    {
        public void Install(
            GameObject root,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            Health health = PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<Health>(root);
            health.ConfigureMaxHealth(characterDefinition.StatsDefinition.MaxHealth);
            health.ConfigureCombatIdentity(characterDefinition.CharacterId);

            PlayerRuntimeComponentAccess.RequirePrefabComponent<MeleeAttackExecutor>(root);
            PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<ProjectileAttackExecutor>(root);

            AbilityController abilityController =
                PlayerRuntimeComponentAccess.RequirePrefabComponent<AbilityController>(root);
            abilityController.Configure(characterDefinition.AbilityDefinitions);
        }
    }

    internal sealed class PlayerProgressionEvolutionInstaller : IPrototypePlayerFeatureInstaller
    {
        public void Install(
            GameObject root,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            SkillProgressionController skillController =
                PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<SkillProgressionController>(root);
            skillController.Initialize(progressionState, characterDefinition.SkillDefinitions);

            EvolutionProgressionController evolutionController =
                PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<EvolutionProgressionController>(root);
            evolutionController.Initialize(
                progressionState,
                characterDefinition.EvolutionDefinitions);

            ArtProgressionController artController =
                PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<ArtProgressionController>(root);
            artController.Initialize(progressionState, characterDefinition.ArtDefinitions);
        }
    }

    internal sealed class PlayerInputLoadoutInstaller : IPrototypePlayerFeatureInstaller
    {
        public void Install(
            GameObject root,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            PlayerInputReader inputReader =
                PlayerRuntimeComponentAccess.RequirePrefabComponent<PlayerInputReader>(root);
            AbilityLoadoutController loadoutController =
                PlayerRuntimeComponentAccess.RequirePrefabComponent<AbilityLoadoutController>(root);
            PlayerRuntimeComponentAccess.RequirePrefabComponent<PlayerAbilityInput>(root);

            loadoutController.Initialize(characterDefinition, progressionState);

            AbilityLoadoutSelectionController selectionController =
                PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<AbilityLoadoutSelectionController>(root);
            selectionController.Initialize(
                inputReader,
                loadoutController,
                characterDefinition,
                progressionState);
        }
    }

    internal sealed class PlayerPresentationInstaller : IPrototypePlayerFeatureInstaller
    {
        public void Install(
            GameObject root,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState)
        {
            PlayerRuntimeComponentAccess.RequirePrefabComponent<PrototypeSlimeSpriteAnimator>(root);
            PlayerRuntimeComponentAccess.RequirePrefabComponent<CharacterElevationPresenter>(root);

            EvolutionProgressionController evolutionController =
                PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<EvolutionProgressionController>(root);
            PrototypeSlimeEvolutionPresenter evolutionPresenter =
                PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<PrototypeSlimeEvolutionPresenter>(root);
            evolutionPresenter.Initialize(evolutionController);

            PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<PrototypeMeleeAttackEffect>(root);
            PlayerRuntimeComponentAccess.GetOrAddRuntimeComponent<PrototypeProjectileAttackEffect>(root);
        }
    }
}
