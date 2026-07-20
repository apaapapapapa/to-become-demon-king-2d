using DemonKing.Core.Input;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Interaction;
using DemonKing.Presentation.Characters;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// 既存のPrototype Player Prefabとの互換性を保つため、主要コンポーネントをRequireComponentで束ねる薄いマーカーです。
    /// ゲームプレイロジックは持たず、入力、平面移動、Interaction、Combat、3D物理、描画、アニメーションの実処理は各担当コンポーネントへ委譲します。
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterPlanarMotor))]
    [RequireComponent(typeof(CharacterPhysicsBody3D))]
    [RequireComponent(typeof(PlayerInteractor))]
    [RequireComponent(typeof(AbilityController))]
    [RequireComponent(typeof(PlayerAbilityInput))]
    [RequireComponent(typeof(MeleeAttackExecutor))]
    [RequireComponent(typeof(CharacterSquashAnimator))]
    [RequireComponent(typeof(GroupYSorter))]
    [RequireComponent(typeof(PrototypeSlimeSpriteAnimator))]
    public sealed class SlimeController : MonoBehaviour
    {
    }
}
