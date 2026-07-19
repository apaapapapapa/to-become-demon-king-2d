using DemonKing.Core.Input;
using DemonKing.Gameplay.Characters;
using DemonKing.Presentation.Characters;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// 試作プレイヤーに必要なコンポーネント構成だけを保証する互換用コンポーネントです。
    /// 移動、物理、描画、スプライトアニメーションの実処理は各担当コンポーネントへ委譲します。
    /// </summary>
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterMotor2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(CharacterSquashAnimator))]
    [RequireComponent(typeof(GroupYSorter))]
    [RequireComponent(typeof(PrototypeSlimeSpriteAnimator))]
    public sealed class SlimeController : MonoBehaviour
    {
    }
}
