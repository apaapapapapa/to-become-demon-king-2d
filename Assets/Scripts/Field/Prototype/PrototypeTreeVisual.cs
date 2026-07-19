using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 木Prefab内部の試作ビジュアルを構築します。
    /// 配置はPrefab利用側、見た目はPrefab側という境界を作ります。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeTreeVisual : MonoBehaviour
    {
        private void Awake()
        {
            if (GetComponentInChildren<SpriteRenderer>(includeInactive: true) != null)
            {
                return;
            }

            var shapes = new RuntimeShapeFactory();
            shapes.CreateEllipse("木の影", new Vector2(0.22f, -0.14f), new Vector2(1.85f, 0.48f),
                new Color(0.05f, 0.15f, 0.14f, 0.62f), -3, transform);
            shapes.CreatePatch("木の幹", new Vector2(0f, 0.72f), new Vector2(0.38f, 1.55f),
                new Color(0.31f, 0.20f, 0.14f), 0, transform);
            shapes.CreatePatch("幹の光", new Vector2(-0.09f, 0.82f), new Vector2(0.10f, 1.20f),
                new Color(0.55f, 0.37f, 0.20f), 1, transform);
            shapes.CreateEllipse("樹冠の影", new Vector2(0.12f, 1.72f), new Vector2(2.10f, 1.72f),
                new Color(0.08f, 0.29f, 0.22f), 2, transform);
            shapes.CreateEllipse("左の樹冠", new Vector2(-0.52f, 1.82f), new Vector2(1.45f, 1.35f),
                new Color(0.12f, 0.42f, 0.25f), 3, transform);
            shapes.CreateEllipse("右の樹冠", new Vector2(0.48f, 1.98f), new Vector2(1.58f, 1.43f),
                new Color(0.16f, 0.49f, 0.27f), 3, transform);
            shapes.CreateEllipse("樹冠の光", new Vector2(-0.32f, 2.28f), new Vector2(0.92f, 0.68f),
                new Color(0.39f, 0.68f, 0.34f), 4, transform);
            shapes.CreateEllipse("葉のきらめき", new Vector2(-0.52f, 2.43f), new Vector2(0.31f, 0.20f),
                new Color(0.65f, 0.82f, 0.45f, 0.72f), 5, transform);
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }
    }
}
