using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプ用ワールドPrefabの読み込みと配置を集約します。
    /// Builder側はPrefabの具体的なResourcesパスやフォールバック生成を知りません。
    /// </summary>
    internal sealed class PrototypeWorldPrefabFactory
    {
        private const string CottagePath = "Prefabs/World/PrototypeCottage";
        private const string TreePath = "Prefabs/World/PrototypeTree";
        private const string LamppostPath = "Prefabs/World/PrototypeLamppost";

        public GameObject CreateCottage(Vector2 position, Transform parent)
        {
            return InstantiateOrFallback(CottagePath, "校舎", position, parent, typeof(PrototypeCottageVisual));
        }

        public GameObject CreateTree(Vector2 position, Transform parent)
        {
            return InstantiateOrFallback(TreePath, "木", position, parent, typeof(PrototypeTreeVisual));
        }

        public GameObject CreateLamppost(Vector2 position, Transform parent)
        {
            return InstantiateOrFallback(LamppostPath, "街灯", position, parent, typeof(PrototypeLamppostVisual));
        }

        private static GameObject InstantiateOrFallback(
            string resourcesPath,
            string fallbackName,
            Vector2 position,
            Transform parent,
            System.Type visualComponentType)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcesPath);
            GameObject instance;

            if (prefab != null)
            {
                instance = Object.Instantiate(prefab, parent, false);
            }
            else
            {
                Debug.LogWarning($"ワールドPrefabが見つかりません。Resources/{resourcesPath}.prefab を確認してください。");
                instance = new GameObject(fallbackName);
                instance.transform.SetParent(parent, false);
                instance.AddComponent(visualComponentType);
                instance.AddComponent<DemonKing.Presentation.Rendering.GroupYSorter>();
            }

            instance.transform.localPosition = new Vector3(position.x, position.y, 0f);
            return instance;
        }
    }
}
