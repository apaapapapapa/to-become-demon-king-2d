using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 蛍や夕霧など、空気感を作る演出オブジェクトを構築します。
    /// 動きそのものはAmbientEffectControllerへ登録して更新を委譲します。
    /// </summary>
    internal sealed class AtmosphereBuilder
    {
        private readonly RuntimeShapeFactory shapes;
        private readonly AmbientEffectController ambientEffects;

        public AtmosphereBuilder(RuntimeShapeFactory shapes, AmbientEffectController ambientEffects)
        {
            this.shapes = shapes;
            this.ambientEffects = ambientEffects;
        }

        public void Build(Transform parent)
        {
            var random = new System.Random(420);
            for (int i = 0; i < 16; i++)
            {
                Vector2 position = new(
                    PrototypeWorldMath.Next(random, -6.7f, 6.7f),
                    PrototypeWorldMath.Next(random, -2.8f, 3.0f));
                float size = PrototypeWorldMath.Next(random, 0.07f, 0.13f);
                GameObject firefly = shapes.CreateEllipse("漂う光", position, new Vector2(size, size),
                    new Color(1f, 0.87f, 0.39f, 0.84f), 1450 + i, parent);
                Vector2 direction = new Vector2(
                    PrototypeWorldMath.Next(random, -0.45f, 0.45f), 1f).normalized;
                ambientEffects.Register(
                    firefly,
                    direction,
                    PrototypeWorldMath.Next(random, 0.08f, 0.22f),
                    PrototypeWorldMath.Next(random, 0.55f, 1.15f),
                    PrototypeWorldMath.Next(random, 0f, Mathf.PI * 2f));
            }

            shapes.CreatePatch("薄い夕霧", new Vector2(0f, 3.65f), new Vector2(18f, 0.32f),
                new Color(0.73f, 0.64f, 0.68f, 0.08f), 1400, parent);
        }
    }
}
