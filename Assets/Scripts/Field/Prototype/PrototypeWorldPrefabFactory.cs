using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// ProjectAssetsで管理されたワールドPrefabの配置を集約します。
    /// Builder側はPrefabの保管場所やResourcesパスを知りません。
    /// </summary>
    internal sealed class PrototypeWorldPrefabFactory
    {
        private readonly PrototypeProjectAssets projectAssets;

        public PrototypeWorldPrefabFactory(PrototypeProjectAssets projectAssets)
        {
            this.projectAssets = projectAssets;
        }

        public GameObject CreateCottage(Vector2 position, Transform parent)
        {
            return Instantiate(projectAssets.CottagePrefab, "校舎", position, parent);
        }

        public GameObject CreateTree(Vector2 position, Transform parent)
        {
            return Instantiate(projectAssets.TreePrefab, "木", position, parent);
        }

        public GameObject CreateLamppost(Vector2 position, Transform parent)
        {
            return Instantiate(projectAssets.LamppostPrefab, "街灯", position, parent);
        }

        private static GameObject Instantiate(
            GameObject prefab,
            string fallbackName,
            Vector2 position,
            Transform parent)
        {
            if (prefab == null)
            {
                Debug.LogError($"{fallbackName}Prefabの参照が設定されていません。");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parent, false);
            instance.transform.localPosition = new Vector3(position.x, position.y, 0f);
            return instance;
        }
    }
}
