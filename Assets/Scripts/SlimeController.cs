using DemonKing.Core.Input;
using DemonKing.Gameplay.Characters;
using DemonKing.Presentation.Characters;
using DemonKing.Presentation.Rendering;
using DemonKing.Presentation.UI;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// 既存のFieldBootstrapとの互換性を保つための試作プレイヤー構成ルートです。
    /// 入力・移動・物理衝突・見た目・描画順・UIの実処理は個別コンポーネントへ委譲します。
    /// </summary>
    [RequireComponent(typeof(MoveInputReader))]
    [RequireComponent(typeof(CharacterMotor2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(CharacterSquashAnimator))]
    [RequireComponent(typeof(GroupYSorter))]
    [RequireComponent(typeof(PrototypeHud))]
    public sealed class SlimeController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.4f;
        [SerializeField, Min(1)] private int sortingPrecision = 100;

        private CharacterMotor2D motor;
        private GroupYSorter ySorter;

        private void Awake()
        {
            motor = GetComponent<CharacterMotor2D>();
            ySorter = GetComponent<GroupYSorter>();

            motor.SetMoveSpeed(moveSpeed);
            ySorter.SetPrecision(sortingPrecision);
        }

        public void Configure(Vector2 extents)
        {
            if (motor == null)
            {
                motor = GetComponent<CharacterMotor2D>();
            }

            motor.Configure(moveSpeed, extents);
        }
    }
}
