using DemonKing.Gameplay.Characters;
using UnityEngine;

namespace DemonKing.Presentation.Characters
{
    /// <summary>
    /// Physics上のElevation（Z）を2Dアイソメトリック表示の画面上高さへ変換します。
    /// 物理RootのX/YとSorting基準は変更せず、VisualだけをY方向へ持ち上げます。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterPhysicsBody3D))]
    [RequireComponent(typeof(PrototypeSlimeSpriteAnimator))]
    public sealed class CharacterElevationPresenter : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float screenOffsetPerElevation = 0.32f;

        private CharacterPhysicsBody3D physicsBody;
        private PrototypeSlimeSpriteAnimator spriteAnimator;
        private Transform visual;
        private Vector3 baseLocalPosition;

        private void Awake()
        {
            physicsBody = GetComponent<CharacterPhysicsBody3D>();
            spriteAnimator = GetComponent<PrototypeSlimeSpriteAnimator>();
        }

        private void Start()
        {
            ResolveVisual();
        }

        private void LateUpdate()
        {
            if (visual == null)
            {
                ResolveVisual();
            }

            if (visual == null || physicsBody == null)
            {
                return;
            }

            Vector3 position = baseLocalPosition;
            position.y += physicsBody.Elevation * screenOffsetPerElevation;
            visual.localPosition = position;
        }

        private void ResolveVisual()
        {
            SpriteRenderer renderer = spriteAnimator == null ? null : spriteAnimator.SpriteRenderer;
            if (renderer == null)
            {
                return;
            }

            visual = renderer.transform;
            baseLocalPosition = visual.localPosition;
        }
    }
}
