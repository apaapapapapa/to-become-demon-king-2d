using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 試作プレイヤーPrefabを生成し、現在のフィールド設定へ接続します。
    /// プレイヤーの見た目やコンポーネント構成はPrefab側で管理し、フィールド側は配置だけを担当します。
    /// </summary>
    internal sealed class PrototypePlayerSpawner
    {
        private const string PrefabResourcesPath = "Prefabs/Characters/PrototypeSlime";

        private static readonly Vector3 SpawnPosition = new(0f, -1.35f, -1f);
        private static readonly Vector2 FieldExtents = new(7.15f, 3.45f);

        public GameObject Spawn(Transform parent)
        {
            GameObject root = InstantiatePrefab(parent);
            root.transform.localPosition = SpawnPosition;

            SlimeController controller = root.GetComponent<SlimeController>();
            if (controller == null)
            {
                Debug.LogWarning("PrototypeSlime PrefabにSlimeControllerがないため、実行時に補完します。", root);
                controller = root.AddComponent<SlimeController>();
            }

            controller.Configure(FieldExtents);
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
