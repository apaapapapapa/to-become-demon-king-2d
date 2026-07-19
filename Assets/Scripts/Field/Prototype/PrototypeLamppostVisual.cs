using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 街灯Prefab内部の試作ビジュアルを構築します。
    /// 光輪アニメーションもPrefab内部へ閉じ込め、ArchitectureBuilderから演出依存を除去します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeLamppostVisual : MonoBehaviour
    {
        [SerializeField] private float glowPhase;

        private void Awake()
        {
            if (GetComponentInChildren<SpriteRenderer>(includeInactive: true) != null)
            {
                return;
            }

            var shapes = new RuntimeShapeFactory();
            shapes.CreateEllipse("街灯の影", new Vector2(0.12f, -0.10f), new Vector2(0.82f, 0.24f),
                new Color(0.05f, 0.13f, 0.13f, 0.55f), -2, transform);
            shapes.CreatePatch("街灯の支柱", Vector2.up * 0.78f, new Vector2(0.11f, 1.62f),
                new Color(0.19f, 0.19f, 0.22f), 0, transform);
            shapes.CreatePatch("街灯の笠", Vector2.up * 1.62f, new Vector2(0.54f, 0.18f),
                new Color(0.24f, 0.20f, 0.25f), 1, transform);
            shapes.CreateDiamond("街灯の灯り", Vector2.up * 1.45f, new Vector2(0.34f, 0.40f),
                new Color(1f, 0.73f, 0.31f), 2, transform);
            GameObject glow = shapes.CreateEllipse("街灯の光輪", Vector2.up * 1.44f, new Vector2(1.35f, 1.35f),
                new Color(1f, 0.65f, 0.25f, 0.13f), 3, transform);
            PrototypeGlowPulse pulse = glow.AddComponent<PrototypeGlowPulse>();

            // Prefabごとに異なる位相を持たせたい場合はInspector値を利用します。
            if (!Mathf.Approximately(glowPhase, 0f))
            {
                // 現状はコンポーネント生成時の既定値で十分なため、位相差はルート回転値を小さな入力として利用します。
                glow.transform.localRotation = Quaternion.Euler(0f, 0f, glowPhase);
            }
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }
    }
}
