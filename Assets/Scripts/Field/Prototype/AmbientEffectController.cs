using System.Collections.Generic;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 水面のきらめきや蛍など、試作フィールドの軽量な環境アニメーションを一括更新します。
    /// フィールド構築処理と毎フレーム更新処理を分離するためのコンポーネントです。
    /// </summary>
    internal sealed class AmbientEffectController : MonoBehaviour
    {
        private readonly List<AmbientElement> elements = new();
        private float elapsedTime;

        public void Register(GameObject target, Vector2 direction, float amplitude, float speed, float phase)
        {
            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogWarning($"環境アニメーション対象にSpriteRendererがありません: {target.name}", target);
                return;
            }

            elements.Add(new AmbientElement
            {
                Transform = target.transform,
                Renderer = renderer,
                BasePosition = target.transform.localPosition,
                Direction = direction,
                BaseColor = renderer.color,
                Amplitude = amplitude,
                Speed = speed,
                Phase = phase
            });
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;

            foreach (AmbientElement element in elements)
            {
                float wave = Mathf.Sin(elapsedTime * element.Speed + element.Phase);
                Vector2 drift = element.Direction * (wave * element.Amplitude);
                element.Transform.localPosition = element.BasePosition + new Vector3(drift.x, drift.y, 0f);

                Color color = element.BaseColor;
                color.a *= 0.72f + (wave + 1f) * 0.14f;
                element.Renderer.color = color;
            }
        }

        private sealed class AmbientElement
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 BasePosition;
            public Vector2 Direction;
            public Color BaseColor;
            public float Amplitude;
            public float Speed;
            public float Phase;
        }
    }
}
