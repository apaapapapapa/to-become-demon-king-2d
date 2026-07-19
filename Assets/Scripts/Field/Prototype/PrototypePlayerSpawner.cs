using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Combat.Configuration;
using DemonKing.Presentation.Characters;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// ProjectAssetsから受け取ったプレイヤーPrefabを生成して初期位置へ配置します。
    /// キャラクター能力値、攻撃、回避データをScriptableObjectから各Gameplayコンポーネントへ注入します。
    /// </summary>
    internal sealed class PrototypePlayerSpawner
    {
        private readonly Vector3 spawnPosition;
        private readonly GameObject playerPrefab;
        private readonly CharacterStatsDefinition characterStats;
        private readonly MeleeAttackDefinition meleeAttackDefinition;
        private readonly DodgeDefinition dodgeDefinition;

        public PrototypePlayerSpawner(
            Vector3 spawnPosition,
            GameObject playerPrefab,
            CharacterStatsDefinition characterStats,
            MeleeAttackDefinition meleeAttackDefinition,
            DodgeDefinition dodgeDefinition)
        {
            this.spawnPosition = spawnPosition;
            this.playerPrefab = playerPrefab;
            this.characterStats = characterStats;
            this.meleeAttackDefinition = meleeAttackDefinition;
            this.dodgeDefinition = dodgeDefinition;
        }

        public GameObject Spawn(Transform parent)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("プレイヤーPrefab参照が設定されていません。");
                return null;
            }

            GameObject root = Object.Instantiate(playerPrefab, parent, false);
            root.transform.localPosition = spawnPosition;

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
                motor.Configure(characterStats);
                motor.DisableBounds();
            }

            Health health = root.GetComponent<Health>();
            if (health == null)
            {
                health = root.AddComponent<Health>();
            }

            if (characterStats != null)
            {
                health.ConfigureMaxHealth(characterStats.MaxHealth);
            }

            PlayerMeleeAttack meleeAttack = root.GetComponent<PlayerMeleeAttack>();
            meleeAttack?.Configure(meleeAttackDefinition);

            CharacterDodge2D dodge = root.GetComponent<CharacterDodge2D>();
            if (dodge == null)
            {
                dodge = root.AddComponent<CharacterDodge2D>();
            }

            dodge.Configure(dodgeDefinition);
            return root;
        }
    }
}
