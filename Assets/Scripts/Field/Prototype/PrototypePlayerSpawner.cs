using DemonKing.Gameplay.Characters;
using DemonKing.Presentation.Characters;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// ProjectAssetsから受け取ったプレイヤーPrefabを生成して初期位置へ配置します。
    /// Prefabの場所を文字列パスとして保持しません。
    /// </summary>
    internal sealed class PrototypePlayerSpawner
    {
        private readonly Vector3 spawnPosition;
        private readonly GameObject playerPrefab;

        public PrototypePlayerSpawner(Vector3 spawnPosition, GameObject playerPrefab)
        {
            this.spawnPosition = spawnPosition;
            this.playerPrefab = playerPrefab;
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
            motor?.DisableBounds();
            return root;
        }
    }
}
