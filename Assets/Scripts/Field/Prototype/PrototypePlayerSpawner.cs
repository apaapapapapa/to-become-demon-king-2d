using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 現在の試作用プレイヤーを生成し、既存のSlimeControllerへ接続します。
    /// 将来Prefab管理へ移行する際の置き換え境界です。
    /// </summary>
    internal sealed class PrototypePlayerSpawner
    {
        private static readonly Vector3 SpawnPosition = new(0f, -1.35f, -1f);
        private static readonly Vector2 FieldExtents = new(7.15f, 3.45f);

        private readonly RuntimeShapeFactory shapes;

        public PrototypePlayerSpawner(RuntimeShapeFactory shapes)
        {
            this.shapes = shapes;
        }

        public GameObject Spawn(Transform parent)
        {
            GameObject root = CreateSlime(parent);
            root.transform.localPosition = SpawnPosition;
            root.AddComponent<SlimeController>().Configure(FieldExtents);
            return root;
        }

        private GameObject CreateSlime(Transform parent)
        {
            GameObject root = new("スライム");
            root.transform.SetParent(parent, false);
            shapes.CreateEllipse("影", new Vector2(0f, -0.38f), new Vector2(1.18f, 0.34f),
                new Color(0.05f, 0.16f, 0.14f, 0.70f), -2, root.transform);
            shapes.CreateEllipse("輪郭", new Vector2(0f, 0.02f), new Vector2(1.18f, 0.94f),
                new Color(0.08f, 0.31f, 0.25f), 0, root.transform);
            shapes.CreateEllipse("からだ", new Vector2(0f, 0.06f), new Vector2(1.04f, 0.82f),
                new Color(0.31f, 0.86f, 0.53f), 1, root.transform);
            shapes.CreateEllipse("下側の色", new Vector2(0f, -0.20f), new Vector2(0.83f, 0.25f),
                new Color(0.17f, 0.63f, 0.43f), 2, root.transform);
            shapes.CreateEllipse("つや", new Vector2(-0.25f, 0.29f), new Vector2(0.28f, 0.18f),
                new Color(0.76f, 1f, 0.78f), 3, root.transform);
            shapes.CreateEllipse("左目", new Vector2(-0.20f, 0.05f), new Vector2(0.09f, 0.15f),
                new Color(0.04f, 0.11f, 0.10f), 4, root.transform);
            shapes.CreateEllipse("右目", new Vector2(0.20f, 0.05f), new Vector2(0.09f, 0.15f),
                new Color(0.04f, 0.11f, 0.10f), 4, root.transform);
            return root;
        }
    }
}
