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
    /// Prototype Player Prefabへ恒久配置するAuthoring ComponentをRequireComponentで束ねる薄いマーカーです。
    /// Definition / Runtime Stateから構成できるPhysics Body、Elevation、Loadout、Progression、Effect等は
    /// PrototypePlayerRuntimeInstallerが注入します。
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterPlanarMotor))]
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
