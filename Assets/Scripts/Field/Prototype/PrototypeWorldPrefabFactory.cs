using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// ProjectAssetsで管理されたワールドPrefabの配置と静的アートの接続を集約します。
    /// Builder側はPrefabやSpriteの保管場所、Resourcesパスを知りません。
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
            GameObject instance = Instantiate(projectAssets.CottagePrefab, "校舎", position, parent);
            instance?.GetComponent<PrototypeCottageVisual>()?.SetSprite(projectAssets.CottageSprite);
            return instance;
        }

        public GameObject CreateTree(Vector2 position, Transform parent)
        {
            GameObject instance = Instantiate(projectAssets.TreePrefab, "木", position, parent);
            instance?.GetComponent<PrototypeTreeVisual>()?.SetSprite(projectAssets.TreeSprite);
            return instance;
        }

        public GameObject CreateLamppost(Vector2 position, Transform parent)
        {
            GameObject instance = Instantiate(projectAssets.LamppostPrefab, "街灯", position, parent);
            instance?.GetComponent<PrototypeLamppostVisual>()?.SetSprite(projectAssets.LamppostSprite);
            return instance;
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
