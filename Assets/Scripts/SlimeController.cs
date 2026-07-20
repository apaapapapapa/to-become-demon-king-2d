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
    /// Prototype Player Prefabの主要コンポーネントをRequireComponentで束ねる薄いマーカーです。
    /// ゲームプレイロジックは持たず、入力、平面移動、高さ移動、Interaction、Combat、3D物理、描画、アニメーションの実処理は各担当コンポーネントへ委譲します。
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterPlanarMotor))]
    [RequireComponent(typeof(CharacterPhysicsBody3D))]
    [RequireComponent(typeof(CharacterElevationMotor))]
    [RequireComponent(typeof(PlayerElevationInput))]
    [RequireComponent(typeof(PlayerInteractor))]
    [RequireComponent(typeof(AbilityController))]
    [RequireComponent(typeof(AbilityLoadoutController))]
    [RequireComponent(typeof(PlayerAbilityInput))]
    [RequireComponent(typeof(MeleeAttackExecutor))]
    [RequireComponent(typeof(CharacterSquashAnimator))]
    [RequireComponent(typeof(GroupYSorter))]
    [RequireComponent(typeof(PrototypeSlimeSpriteAnimator))]
    [RequireComponent(typeof(CharacterElevationPresenter))]
    public sealed class SlimeController : MonoBehaviour
    {
    }
}
