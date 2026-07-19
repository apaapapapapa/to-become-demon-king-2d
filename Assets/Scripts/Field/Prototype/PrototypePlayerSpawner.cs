using DemonKing.Gameplay.Characters;
using DemonKing.Presentation.Characters;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 試作プレイヤーPrefabを生成して初期位置へ配置します。
    /// フィールド境界はCollision Tilemapが担当するため、プレイヤー側の座標Clampは使用しません。
    /// </summary>
    internal sealed class PrototypePlayerSpawner
    {
        private const string PrefabResourcesPath = "Prefabs/Characters/PrototypeSlime";

        private readonly Vector3 spawnPosition;

        public PrototypePlayerSpawner(Vector3 spawnPosition)
        {
            this.spawnPosition = spawnPosition;
        }

        public GameObject Spawn(Transform parent)
        {
            GameObject root = InstantiatePrefab(parent);
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

        private static GameObject InstantiatePrefab(Transform parent)
        {
            GameObject prefab = Resources.Load<GameObject>(PrefabResourcesPath);
            if (prefab != null)
            {
                return Object.Instantiate(prefab, parent, false);
            }

            Debug.LogError(
                $"試作プレイヤーPrefabが見つかりません。Resources/{PrefabResourcesPath}.prefab を確認してください。");

            GameObject root = new("スライム");
            root.transform.SetParent(parent, false);
            return root;
        }
    }
}
