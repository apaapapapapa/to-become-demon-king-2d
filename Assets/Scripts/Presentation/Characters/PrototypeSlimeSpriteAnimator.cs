using System;
using System.Collections.Generic;
using DemonKing.Core.Input;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Presentation.Characters
{
    /// <summary>
    /// Resources配下のピクセルフレームアセットをSpriteへ変換し、試作スライムの待機・移動アニメーションを再生します。
    /// 従来のRuntimeShapeFactoryによる多層図形生成を廃止し、アートデータ単位で差し替えられる構造にします。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(GroupYSorter))]
    public sealed class PrototypeSlimeSpriteAnimator : MonoBehaviour
    {
        private const string SpriteRoot = "Art/Characters/PrototypeSlime/";
        private const float PixelsPerUnit = 16f;

        [SerializeField, Min(0.05f)] private float idleFrameDuration = 0.34f;
        [SerializeField, Min(0.05f)] private float moveFrameDuration = 0.14f;

        private readonly List<Sprite> generatedSprites = new();
        private MoveInputReader inputReader;
        private SpriteRenderer spriteRenderer;
        private Sprite[] idleFrames;
        private Sprite[] moveFrames;
        private float elapsed;
        private int frameIndex;
        private bool previousMoving;
        private EvolutionAppearanceProfile appearance;

        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public EvolutionAppearanceProfile Appearance => appearance;

        private void Awake()
        {
            inputReader = GetComponent<MoveInputReader>();
            spriteRenderer = ResolveRenderer();
            idleFrames = LoadFrames("IdleA", "IdleB");
            moveFrames = LoadFrames("MoveA", "MoveB");
            ApplyFrame(false, 0);
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        private void Update()
        {
            Vector2 input = inputReader == null ? Vector2.zero : inputReader.Move;
            bool moving = input.sqrMagnitude > 0.0001f;

            if (moving != previousMoving)
            {
                previousMoving = moving;
                elapsed = 0f;
                frameIndex = 0;
                ApplyFrame(moving, frameIndex);
            }

            if (moving && Mathf.Abs(input.x) > 0.05f)
            {
                spriteRenderer.flipX = input.x < 0f;
            }

            elapsed += Time.deltaTime;
            float frameDuration = moving ? moveFrameDuration : idleFrameDuration;
            if (elapsed < frameDuration)
            {
                return;
            }

            elapsed -= frameDuration;
            frameIndex = (frameIndex + 1) % 2;
            ApplyFrame(moving, frameIndex);
        }

        private void OnDestroy()
        {
            ReleaseGeneratedSprites();
        }

        public void ApplyEvolutionAppearance(EvolutionAppearanceProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            appearance = profile;
            if (spriteRenderer == null)
            {
                return;
            }

            ReleaseGeneratedSprites();
            idleFrames = LoadFrames("IdleA", "IdleB");
            moveFrames = LoadFrames("MoveA", "MoveB");
            spriteRenderer.transform.localScale = new Vector3(
                profile.VisualScale.x,
                profile.VisualScale.y,
                1f);
            ApplyFrame(previousMoving, frameIndex);
        }

        private void ReleaseGeneratedSprites()
        {
            foreach (Sprite sprite in generatedSprites)
            {
                if (sprite == null)
                {
                    continue;
                }

                Texture2D texture = sprite.texture;
                Destroy(sprite);
                if (texture != null)
                {
                    Destroy(texture);
                }
            }

            generatedSprites.Clear();
        }

        private SpriteRenderer ResolveRenderer()
        {
            SpriteRenderer existing = GetComponentInChildren<SpriteRenderer>(includeInactive: true);
            if (existing != null)
            {
                return existing;
            }

            GameObject visual = new("Visual");
            visual.transform.SetParent(transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = SortingLayerNames.World;
            renderer.sortingOrder = 0;
            return renderer;
        }

        private Sprite[] LoadFrames(string first, string second)
        {
            return new[]
            {
                LoadFrame(first),
                LoadFrame(second)
            };
        }

        private Sprite LoadFrame(string frameName)
        {
            TextAsset frameAsset = Resources.Load<TextAsset>(SpriteRoot + frameName);
            if (frameAsset == null)
            {
                Debug.LogError(
                    $"スライムのピクセルフレームが見つかりません。Resources/{SpriteRoot}{frameName}.txt を確認してください。",
                    this);
                return null;
            }

            Sprite sprite = CreateSprite(frameName, frameAsset.text);
            generatedSprites.Add(sprite);
            return sprite;
        }

        private Sprite CreateSprite(string name, string frameText)
        {
            string[] rows = frameText
                .Replace("\r", string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            int height = rows.Length;
            int width = 0;
            foreach (string row in rows)
            {
                width = Mathf.Max(width, row.Length);
            }

            Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int rowIndex = 0; rowIndex < height; rowIndex++)
            {
                string row = rows[rowIndex];
                int pixelY = height - 1 - rowIndex;
                for (int x = 0; x < width; x++)
                {
                    char token = x < row.Length ? row[x] : '.';
                    texture.SetPixel(x, pixelY, ResolveColor(token));
                }
            }

            texture.Apply();
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
        }

        private Color ResolveColor(char token)
        {
            if (appearance != null)
            {
                Color outlineShadow = appearance.OutlineColor;
                outlineShadow.a = 0.47f;
                return token switch
                {
                    's' => outlineShadow,
                    'o' => appearance.OutlineColor,
                    'g' => appearance.BodyColor,
                    'd' => appearance.ShadowColor,
                    'h' => appearance.HighlightColor,
                    'e' => appearance.EyeColor,
                    _ => Color.clear
                };
            }

            return token switch
            {
                's' => new Color32(14, 45, 40, 120),
                'o' => new Color32(20, 78, 64, 255),
                'g' => new Color32(76, 219, 135, 255),
                'd' => new Color32(45, 166, 112, 255),
                'h' => new Color32(194, 255, 205, 255),
                'e' => new Color32(10, 29, 26, 255),
                _ => Color.clear
            };
        }

        private void ApplyFrame(bool moving, int index)
        {
            Sprite[] frames = moving ? moveFrames : idleFrames;
            if (frames == null || frames.Length == 0 || index < 0 || index >= frames.Length || frames[index] == null)
            {
                return;
            }

            spriteRenderer.sprite = frames[index];
        }
    }
}
