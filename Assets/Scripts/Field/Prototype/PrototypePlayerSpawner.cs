using DemonKing.Gameplay.Characters;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 試作プレイヤーPrefabを生成し、フィールド側が所有する配置設定へ接続します。
    /// プレイヤーの見た目や移動速度などのキャラクター固有設定はPrefab側で管理します。
    /// </summary>
    internal sealed class PrototypePlayerSpawner
    {
        private const string PrefabResourcesPath = "Prefabs/Characters/PrototypeSlime";

        private readonly Vector3 spawnPosition;
        private readonly Vector2 playableHalfExtents;

        public PrototypePlayerSpawner(Vector3 spawnPosition, Vector2 playableHalfExtents)
        {
            this.spawnPosition = spawnPosition;
            this.playableHalfExtents = new Vector2(
                Mathf.Abs(playableHalfExtents.x),
                Mathf.Abs(playableHalfExtents.y));
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

            CharacterMotor2D motor = root.GetComponent<CharacterMotor2D>();
            if (motor != null)
            {
                // プレイ可能範囲はフィールド側が所有し、キャラクター側には実行時設定として渡します。
                motor.SetBounds(playableHalfExtents);
            }

            return root;
        }

        private static GameObject InstantiatePrefab(Transform parent)
        {
            GameObject prefab = Resources.Load<GameObject>(PrefabResourcesPath);
            if (prefab != null)
            {
                return Object.Instantiate(prefab, parent, false);
            }

            // Prefabの参照不備でもプロトタイプを完全に停止させないための最小フォールバックです。
            Debug.LogError(
                $"試作プレイヤーPrefabが見つかりません。Resources/{PrefabResourcesPath}.prefab を確認してください。");

            GameObject root = new("スライム");
            root.transform.SetParent(parent, false);
            root.AddComponent<PrototypeSlimeView>();
            return root;
        }
    }
}
