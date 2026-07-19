using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Combat;
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

            if (root.GetComponent<PrototypeSlimeSpriteAnimator>() == null)
            {
                Debug.LogWarning("PrototypeSlime Prefabにスプライトアニメーションがないため、実行時に補完します。", root);
                root.AddComponent<PrototypeSlimeSpriteAnimator>();
            }

            CharacterMotor2D motor = root.GetComponent<CharacterMotor2D>();
            if (motor != null)
            {
                motor.Configure(characterDefinition.StatsDefinition);
                motor.DisableBounds();
            }

            Health health = root.GetComponent<Health>();
            if (health == null)
            {
                health = root.AddComponent<Health>();
            }

            health.ConfigureMaxHealth(characterDefinition.StatsDefinition.MaxHealth);
            health.ConfigureCombatIdentity(characterDefinition.CharacterId);

            PlayerMeleeAttack meleeAttack = root.GetComponent<PlayerMeleeAttack>();
            meleeAttack?.Configure(characterDefinition.BasicMeleeAttackDefinition);

            CharacterDodge2D dodge = root.GetComponent<CharacterDodge2D>();
            if (dodge == null)
            {
                dodge = root.AddComponent<CharacterDodge2D>();
            }

            dodge.Configure(characterDefinition.DodgeDefinition);
            return root;
        }
    }
}
