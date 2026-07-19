using DemonKing.Core.Input;
using DemonKing.Gameplay.Characters;
using DemonKing.Presentation.Characters;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// 試作プレイヤーに必要なコンポーネント構成だけを保証する互換用コンポーネントです。
    /// 移動速度や描画順精度などの設定値は各担当コンポーネントが所有し、このクラスでは重複管理しません。
    /// </summary>
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterMotor2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(CharacterSquashAnimator))]
    [RequireComponent(typeof(GroupYSorter))]
    public sealed class SlimeController : MonoBehaviour
    {
    }
}
