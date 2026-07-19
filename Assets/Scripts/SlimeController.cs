using DemonKing.Core.Input;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Interaction;
using DemonKing.Presentation.Characters;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// 試作プレイヤーに必要なコンポーネント構成だけを保証する互換用コンポーネントです。
    /// 入力、移動、Interaction、Combat、物理、描画、スプライトアニメーションの実処理は各担当コンポーネントへ委譲します。
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterMotor2D))]
    [RequireComponent(typeof(PlayerInteractor))]
    [RequireComponent(typeof(PlayerMeleeAttack))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(CharacterSquashAnimator))]
    [RequireComponent(typeof(GroupYSorter))]
    [RequireComponent(typeof(PrototypeSlimeSpriteAnimator))]
    public sealed class SlimeController : MonoBehaviour
    {
    }
}
