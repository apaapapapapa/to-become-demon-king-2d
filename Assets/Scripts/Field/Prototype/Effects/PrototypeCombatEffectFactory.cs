using DemonKing.Gameplay.Combat;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Prototypeの戦闘演出を、Prefabや本番VFXへ移行するまでの軽量なピクセル図形で生成します。
    /// </summary>
    internal static class PrototypeCombatEffectFactory
    {
        private const int EffectSortingOrder = 500;
        private static readonly RuntimeShapeFactory Shapes = new();

        public static GameObject CreateMeleeSlash(MeleeAttackEvent attackEvent, float duration)
        {
            GameObject root = new("攻撃エフェクト");
            root.transform.position = new Vector3(attackEvent.Center.x, attackEvent.Center.y, 0f);

            float facingAngle = Mathf.Atan2(
                attackEvent.FacingDirection.y,
                attackEvent.FacingDirection.x) * Mathf.Rad2Deg;
            root.transform.rotation = Quaternion.Euler(0f, 0f, facingAngle - 90f);

            float radius = Mathf.Max(0.25f, attackEvent.Radius);
            Color[] colors =
            {
                new Color32(255, 255, 236, 255),
                new Color32(159, 244, 255, 245),
                new Color32(73, 183, 255, 225)
            };

            const int slashPartCount = 7;
            for (int index = 0; index < slashPartCount; index++)
            {
                float normalized = index / (slashPartCount - 1f);
                float side = normalized * 2f - 1f;
                float arch = 1f - side * side;
                Vector2 position = new(
                    side * radius * 0.78f,
                    arch * radius * 0.18f);
                Vector2 size = new(
                    Mathf.Lerp(0.12f, 0.22f, arch),
                    Mathf.Lerp(0.06f, 0.10f, arch));

                Shapes.CreateDiamond(
                    $"斬撃{index + 1}",
                    position,
                    size,
                    colors[index % colors.Length],
                    EffectSortingOrder + index,
                    root.transform,
                    SortingLayerNames.Foreground);
            }

            Shapes.CreateEllipse(
                "命中光",
                new Vector2(0f, radius * 0.12f),
                new Vector2(radius * 0.48f, radius * 0.16f),
                new Color32(255, 255, 255, attackEvent.DidHit ? (byte)230 : (byte)155),
                EffectSortingOrder + slashPartCount,
                root.transform,
                SortingLayerNames.Foreground);

            PrototypeSlashEffectAnimator animator = root.AddComponent<PrototypeSlashEffectAnimator>();
            animator.Initialize(duration);
            return root;
        }

        public static GameObject CreateMonsterDefeatBurst(Vector3 center, float duration)
        {
            GameObject root = new("撃破エフェクト");
            root.transform.position = center;

            Shapes.CreateEllipse(
                "消滅光",
                Vector2.zero,
                new Vector2(0.64f, 0.48f),
                new Color32(255, 238, 180, 235),
                EffectSortingOrder + 20,
                root.transform,
                SortingLayerNames.Foreground);

            Color[] shardColors =
            {
                new Color32(255, 112, 98, 255),
                new Color32(255, 184, 92, 255),
                new Color32(255, 244, 196, 255),
                new Color32(177, 96, 255, 235)
            };

            const int shardCount = 12;
            for (int index = 0; index < shardCount; index++)
            {
                float angle = index * (360f / shardCount) + (index % 2) * 9f;
                float radians = angle * Mathf.Deg2Rad;
                Vector2 direction = new(Mathf.Cos(radians), Mathf.Sin(radians));
                float startDistance = index % 2 == 0 ? 0.10f : 0.16f;
                Vector2 size = index % 3 == 0
                    ? new Vector2(0.18f, 0.13f)
                    : new Vector2(0.12f, 0.09f);

                GameObject shard = Shapes.CreateDiamond(
                    $"消滅片{index + 1}",
                    direction * startDistance,
                    size,
                    shardColors[index % shardColors.Length],
                    EffectSortingOrder + index,
                    root.transform,
                    SortingLayerNames.Foreground);
                shard.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            }

            PrototypeDefeatEffectAnimator animator = root.AddComponent<PrototypeDefeatEffectAnimator>();
            animator.Initialize(duration, 0.78f);
            return root;
        }
    }

    internal sealed class PrototypeSlashEffectAnimator : MonoBehaviour
    {
        private SpriteRenderer[] renderers;
        private Color[] initialColors;
        private float duration;
        private float elapsed;

        public void Initialize(float effectDuration)
        {
            duration = Mathf.Max(0.05f, effectDuration);
            renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            initialColors = new Color[renderers.Length];
            for (int index = 0; index < renderers.Length; index++)
            {
                initialColors[index] = renderers[index].color;
            }

            transform.localScale = Vector3.one * 0.68f;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - progress) * (1f - progress);
            transform.localScale = Vector3.one * Mathf.Lerp(0.68f, 1.18f, eased);
            transform.Rotate(0f, 0f, 55f * Time.unscaledDeltaTime);

            float alpha = 1f - Mathf.InverseLerp(0.28f, 1f, progress);
            ApplyAlpha(alpha);

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void ApplyAlpha(float alpha)
        {
            for (int index = 0; index < renderers.Length; index++)
            {
                Color color = initialColors[index];
                color.a *= alpha;
                renderers[index].color = color;
            }
        }
    }

    internal sealed class PrototypeDefeatEffectAnimator : MonoBehaviour
    {
        private SpriteRenderer[] renderers;
        private Vector3[] initialPositions;
        private Vector3[] initialScales;
        private Color[] initialColors;
        private float duration;
        private float outwardDistance;
        private float elapsed;

        public void Initialize(float effectDuration, float distance)
        {
            duration = Mathf.Max(0.1f, effectDuration);
            outwardDistance = Mathf.Max(0f, distance);
            renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            initialPositions = new Vector3[renderers.Length];
            initialScales = new Vector3[renderers.Length];
            initialColors = new Color[renderers.Length];

            for (int index = 0; index < renderers.Length; index++)
            {
                Transform part = renderers[index].transform;
                initialPositions[index] = part.localPosition;
                initialScales[index] = part.localScale;
                initialColors[index] = renderers[index].color;
            }
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - progress) * (1f - progress);

            for (int index = 0; index < renderers.Length; index++)
            {
                Transform part = renderers[index].transform;
                Vector3 origin = initialPositions[index];
                if (origin.sqrMagnitude < 0.001f)
                {
                    part.localScale = initialScales[index] * Mathf.Lerp(0.55f, 1.9f, eased);
                }
                else
                {
                    part.localPosition = origin + origin.normalized * (outwardDistance * eased);
                    part.localScale = initialScales[index] * Mathf.Lerp(1f, 0.28f, progress);
                    part.Rotate(0f, 0f, (index % 2 == 0 ? 190f : -190f) * Time.unscaledDeltaTime);
                }

                Color color = initialColors[index];
                color.a *= 1f - Mathf.InverseLerp(0.38f, 1f, progress);
                renderers[index].color = color;
            }

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
