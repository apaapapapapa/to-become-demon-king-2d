using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 街灯などの光輪を軽く脈動させる試作演出です。
    /// ワールド全体のAmbientEffectControllerへ依存せず、Prefab内部で完結します。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeGlowPulse : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float scaleAmplitude = 0.08f;
        [SerializeField, Min(0.01f)] private float frequency = 2.4f;
        [SerializeField] private float phase;

        private SpriteRenderer targetRenderer;
        private Vector3 baseScale;
        private Color baseColor;

        private void Awake()
        {
            targetRenderer = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
            baseColor = targetRenderer == null ? Color.white : targetRenderer.color;
        }

        private void Update()
        {
            float wave = (Mathf.Sin(Time.time * frequency + phase) + 1f) * 0.5f;
            float scale = 1f + wave * scaleAmplitude;
            transform.localScale = baseScale * scale;

            if (targetRenderer != null)
            {
                Color color = baseColor;
                color.a = baseColor.a * Mathf.Lerp(0.75f, 1f, wave);
                targetRenderer.color = color;
            }
        }
    }
}
