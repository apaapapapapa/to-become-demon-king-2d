using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 試作スライムの見た目だけを構築します。
    /// プレイヤー生成処理から見た目の生成を分離し、本番スプライトへ差し替える境界として扱います。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeSlimeView : MonoBehaviour
    {
        private void Awake()
        {
            EnsureVisuals();
        }

        private void Start()
        {
            // 見た目を実行時生成しているため、全コンポーネントのAwake後に描画対象を再収集します。
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        public void EnsureVisuals()
        {
            if (GetComponentInChildren<SpriteRenderer>(includeInactive: true) != null)
            {
                return;
            }

            var shapes = new RuntimeShapeFactory();
            shapes.CreateEllipse("影", new Vector2(0f, -0.38f), new Vector2(1.18f, 0.34f),
                new Color(0.05f, 0.16f, 0.14f, 0.70f), -2, transform);
            shapes.CreateEllipse("輪郭", new Vector2(0f, 0.02f), new Vector2(1.18f, 0.94f),
                new Color(0.08f, 0.31f, 0.25f), 0, transform);
            shapes.CreateEllipse("からだ", new Vector2(0f, 0.06f), new Vector2(1.04f, 0.82f),
                new Color(0.31f, 0.86f, 0.53f), 1, transform);
            shapes.CreateEllipse("下側の色", new Vector2(0f, -0.20f), new Vector2(0.83f, 0.25f),
                new Color(0.17f, 0.63f, 0.43f), 2, transform);
            shapes.CreateEllipse("つや", new Vector2(-0.25f, 0.29f), new Vector2(0.28f, 0.18f),
                new Color(0.76f, 1f, 0.78f), 3, transform);
            shapes.CreateEllipse("左目", new Vector2(-0.20f, 0.05f), new Vector2(0.09f, 0.15f),
                new Color(0.04f, 0.11f, 0.10f), 4, transform);
            shapes.CreateEllipse("右目", new Vector2(0.20f, 0.05f), new Vector2(0.09f, 0.15f),
                new Color(0.04f, 0.11f, 0.10f), 4, transform);
        }
    }
}
