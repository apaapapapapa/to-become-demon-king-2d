using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 校舎Prefab内部の試作ビジュアルを構築します。
    /// 配置責務をArchitectureBuilderからPrefabへ移し、本番アートへの差し替え境界を明確にします。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeCottageVisual : MonoBehaviour
    {
        private void Awake()
        {
            if (GetComponentInChildren<SpriteRenderer>(includeInactive: true) != null)
            {
                return;
            }

            var shapes = new RuntimeShapeFactory();

            shapes.CreateEllipse("校舎の影", new Vector2(0.25f, -0.15f), new Vector2(4.7f, 0.8f),
                new Color(0.06f, 0.14f, 0.13f, 0.62f), -5, transform);
            shapes.CreatePatch("校舎の側壁", new Vector2(1.35f, 0.82f), new Vector2(1.35f, 1.75f),
                new Color(0.67f, 0.54f, 0.39f), -1, transform);
            shapes.CreatePatch("校舎の正面", new Vector2(-0.25f, 0.86f), new Vector2(3.25f, 1.82f),
                PrototypePalette.Wall, 0, transform);
            shapes.CreatePatch("壁の陰", new Vector2(-0.25f, 0.12f), new Vector2(3.25f, 0.28f),
                new Color(0.58f, 0.43f, 0.31f), 1, transform);

            shapes.CreatePatch("横梁", new Vector2(-0.25f, 1.18f), new Vector2(3.25f, 0.12f),
                PrototypePalette.Wood, 2, transform);
            shapes.CreatePatch("左の柱", new Vector2(-1.45f, 0.88f), new Vector2(0.13f, 1.72f),
                PrototypePalette.Wood, 2, transform);
            shapes.CreatePatch("中央の柱", new Vector2(0.05f, 0.88f), new Vector2(0.12f, 1.72f),
                PrototypePalette.Wood, 2, transform);

            shapes.CreateDiamond("大屋根の影", new Vector2(0.18f, 2.00f), new Vector2(4.55f, 1.82f),
                new Color(0.24f, 0.12f, 0.19f), 3, transform);
            shapes.CreateDiamond("大屋根", new Vector2(0f, 2.10f), new Vector2(4.42f, 1.78f),
                PrototypePalette.Roof, 4, transform);
            shapes.CreatePatch("屋根の明るい縁", new Vector2(-0.35f, 2.55f), new Vector2(2.55f, 0.12f),
                PrototypePalette.RoofLight, 5, transform, rotation: 11f);
            shapes.CreatePatch("屋根の暗い縁", new Vector2(0.74f, 1.65f), new Vector2(2.7f, 0.11f),
                new Color(0.28f, 0.13f, 0.20f), 5, transform, rotation: -11f);

            shapes.CreatePatch("煙突", new Vector2(1.15f, 2.75f), new Vector2(0.42f, 1.18f),
                new Color(0.39f, 0.28f, 0.25f), 3, transform);
            shapes.CreatePatch("煙突の笠", new Vector2(1.15f, 3.35f), new Vector2(0.56f, 0.16f),
                new Color(0.22f, 0.16f, 0.17f), 4, transform);

            CreateWindow(shapes, new Vector2(-0.92f, 0.92f), 3);
            CreateWindow(shapes, new Vector2(0.72f, 0.92f), 3);
            shapes.CreatePatch("玄関", new Vector2(1.37f, 0.59f), new Vector2(0.65f, 1.22f),
                new Color(0.25f, 0.20f, 0.23f), 3, transform);
            shapes.CreateEllipse("扉の金具", new Vector2(1.18f, 0.62f), new Vector2(0.09f, 0.09f),
                new Color(0.94f, 0.67f, 0.28f), 4, transform);

            shapes.CreatePatch("花箱", new Vector2(-0.92f, 0.39f), new Vector2(0.93f, 0.20f),
                PrototypePalette.Wood, 4, transform);
            CreateFlowerCluster(shapes, new Vector2(-0.92f, 0.53f), 5);
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        private void CreateWindow(RuntimeShapeFactory shapes, Vector2 position, int order)
        {
            shapes.CreatePatch("窓枠", position, new Vector2(0.78f, 0.72f), PrototypePalette.Wood, order, transform);
            shapes.CreatePatch("暖かな窓", position, new Vector2(0.61f, 0.55f),
                new Color(0.96f, 0.74f, 0.35f), order + 1, transform);
            shapes.CreatePatch("窓の縦桟", position, new Vector2(0.07f, 0.56f), PrototypePalette.Wood, order + 2, transform);
            shapes.CreatePatch("窓の横桟", position, new Vector2(0.62f, 0.07f), PrototypePalette.Wood, order + 2, transform);
        }

        private void CreateFlowerCluster(RuntimeShapeFactory shapes, Vector2 position, int order)
        {
            for (int i = -2; i <= 2; i++)
            {
                Vector2 flower = position + new Vector2(i * 0.16f, Mathf.Abs(i) * -0.025f);
                shapes.CreateEllipse("花箱の花", flower, new Vector2(0.16f, 0.13f),
                    i % 2 == 0 ? new Color(0.96f, 0.55f, 0.66f) : new Color(0.98f, 0.79f, 0.32f),
                    order, transform);
            }
        }
    }
}
