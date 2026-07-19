using DemonKing.Core.Input;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Presentation.Characters
{
    /// <summary>
    /// Resources配下のピクセルスプライトを使って試作スライムの待機・移動アニメーションを再生します。
    /// 実行時の図形生成には依存せず、見た目をアートアセットとして差し替えられる構造にします。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(GroupYSorter))]
    public sealed class PrototypeSlimeSpriteAnimator : MonoBehaviour
    {
        private const string SpriteRoot = "Art/Characters/PrototypeSlime/";

        [SerializeField, Min(0.05f)] private float idleFrameDuration = 0.34f;
        [SerializeField, Min(0.05f)] private float moveFrameDuration = 0.14f;

        private MoveInputReader inputReader;
        private SpriteRenderer spriteRenderer;
        private Sprite[] idleFrames;
        private Sprite[] moveFrames;
        private float elapsed;
        private int frameIndex;
        private bool previousMoving;

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

        private static Sprite[] LoadFrames(string first, string second)
        {
            Sprite firstSprite = Resources.Load<Sprite>(SpriteRoot + first);
            Sprite secondSprite = Resources.Load<Sprite>(SpriteRoot + second);

            if (firstSprite == null || secondSprite == null)
            {
                Debug.LogError(
                    $"スライムのスプライトが見つかりません。Resources/{SpriteRoot} 配下のPNGとImport Settingsを確認してください。");
            }

            return new[] { firstSprite, secondSprite };
        }

        private void ApplyFrame(bool moving, int index)
        {
            Sprite[] frames = moving ? moveFrames : idleFrames;
            if (frames == null || frames.Length == 0 || frames[index] == null)
            {
                return;
            }

            spriteRenderer.sprite = frames[index];
        }
    }
}
