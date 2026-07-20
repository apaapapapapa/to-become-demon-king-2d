using System;
using System.Collections.Generic;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Presentation.Characters
{
    /// <summary>
    /// 選択済みEvolution Nodeの外見プロファイルを試作スライムへ反映します。
    /// Node条件や保存状態は変更せず、成功通知と復元済み状態だけを描画へ変換します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PrototypeSlimeSpriteAnimator))]
    public sealed class PrototypeSlimeEvolutionPresenter : MonoBehaviour
    {
        private const float EffectPixelsPerUnit = 16f;

        private readonly List<SpriteRenderer> effectRenderers = new();
        private EvolutionProgressionController progressionController;
        private PrototypeSlimeSpriteAnimator spriteAnimator;
        private Transform effectRoot;
        private Sprite effectSprite;
        private EvolutionVisualEffect activeEffect;
        private Color effectColor;
        private float pulseElapsed;

        public string CurrentEvolutionNodeId { get; private set; } = string.Empty;
        public int EffectRendererCount => effectRenderers.Count;

        public void Initialize(EvolutionProgressionController controller)
        {
            Unbind();
            progressionController = controller != null && controller.IsInitialized
                ? controller
                : throw new ArgumentException(
                    "初期化済みのEvolutionProgressionControllerが必要です。",
                    nameof(controller));
            spriteAnimator = GetComponent<PrototypeSlimeSpriteAnimator>();
            progressionController.Service.EvolutionApplied += HandleEvolutionApplied;
            ApplyRestoredEvolution();
        }

        private void Update()
        {
            if (effectRoot == null)
            {
                return;
            }

            pulseElapsed += Time.deltaTime;
            switch (activeEffect)
            {
                case EvolutionVisualEffect.PredatorSpikes:
                    AnimatePredatorSpikes();
                    break;
                case EvolutionVisualEffect.ArcaneWisps:
                    AnimateArcaneWisps();
                    break;
            }
        }

        private void OnDestroy()
        {
            Unbind();
            if (effectSprite != null)
            {
                Texture2D texture = effectSprite.texture;
                Destroy(effectSprite);
                if (texture != null)
                {
                    Destroy(texture);
                }
            }
        }

        private void Unbind()
        {
            if (progressionController != null && progressionController.Service != null)
            {
                progressionController.Service.EvolutionApplied -= HandleEvolutionApplied;
            }

            progressionController = null;
        }

        private void ApplyRestoredEvolution()
        {
            EvolutionDefinition latest = null;
            foreach (string nodeId in progressionController.Service.UnlockedEvolutionNodeIds)
            {
                if (progressionController.TryGetDefinition(nodeId, out EvolutionDefinition definition))
                {
                    latest = definition;
                }
            }

            if (latest != null)
            {
                ApplyDefinition(latest);
            }
        }

        private void HandleEvolutionApplied(EvolutionApplyResult result)
        {
            if (result.Succeeded &&
                progressionController.TryGetDefinition(
                    result.EvolutionNodeId,
                    out EvolutionDefinition definition))
            {
                ApplyDefinition(definition);
            }
        }

        private void ApplyDefinition(EvolutionDefinition definition)
        {
            CurrentEvolutionNodeId = definition.EvolutionNodeId;
            spriteAnimator.ApplyEvolutionAppearance(definition.Appearance);
            BuildEffect(definition.Appearance);
            pulseElapsed = 0f;
        }

        private void BuildEffect(EvolutionAppearanceProfile appearance)
        {
            if (effectRoot != null)
            {
                Destroy(effectRoot.gameObject);
            }

            effectRenderers.Clear();
            activeEffect = appearance.VisualEffect;
            effectColor = appearance.EffectColor;
            if (activeEffect == EvolutionVisualEffect.None)
            {
                return;
            }

            effectRoot = new GameObject("Evolution Effect").transform;
            effectRoot.SetParent(transform, false);
            effectSprite ??= CreateEffectSprite();

            switch (activeEffect)
            {
                case EvolutionVisualEffect.PredatorSpikes:
                    CreateEffectPart("Left Spike", new Vector2(-0.34f, 0.38f), 32f, 0.42f, -1);
                    CreateEffectPart("Right Spike", new Vector2(0.34f, 0.38f), -32f, 0.42f, -1);
                    break;
                case EvolutionVisualEffect.ArcaneWisps:
                    CreateEffectPart("Arcane Wisp A", Vector2.zero, 0f, 0.22f, 1);
                    CreateEffectPart("Arcane Wisp B", Vector2.zero, 0f, 0.18f, 1);
                    CreateEffectPart("Arcane Wisp C", Vector2.zero, 0f, 0.16f, 1);
                    break;
            }

            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        private void CreateEffectPart(
            string partName,
            Vector2 localPosition,
            float rotation,
            float scale,
            int sortingOrder)
        {
            GameObject part = new(partName);
            part.transform.SetParent(effectRoot, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            part.transform.localScale = Vector3.one * scale;

            SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = effectSprite;
            renderer.color = effectColor;
            renderer.sortingLayerName = SortingLayerNames.World;
            renderer.sortingOrder = sortingOrder;
            effectRenderers.Add(renderer);
        }

        private void AnimatePredatorSpikes()
        {
            float pulse = 1f + Mathf.Sin(pulseElapsed * 4f) * 0.06f;
            effectRoot.localScale = Vector3.one * pulse;
        }

        private void AnimateArcaneWisps()
        {
            for (int index = 0; index < effectRenderers.Count; index++)
            {
                float angle = pulseElapsed * (1.3f + index * 0.14f) +
                              index * Mathf.PI * 2f / effectRenderers.Count;
                float radius = 0.48f + index * 0.06f;
                Transform part = effectRenderers[index].transform;
                part.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0.18f + Mathf.Sin(angle) * radius * 0.42f,
                    0f);

                Color animatedColor = effectColor;
                animatedColor.a *= 0.68f + Mathf.Sin(pulseElapsed * 5f + index) * 0.22f;
                effectRenderers[index].color = animatedColor;
            }
        }

        private static Sprite CreateEffectSprite()
        {
            const int size = 7;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                name = "Evolution Effect Pixel",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int distance = Mathf.Abs(x - 3) + Mathf.Abs(y - 3);
                    texture.SetPixel(x, y, distance <= 3 ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                EffectPixelsPerUnit);
        }
    }
}
