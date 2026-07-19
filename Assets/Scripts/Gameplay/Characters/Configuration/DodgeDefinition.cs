using UnityEngine;

namespace DemonKing.Gameplay.Characters.Configuration
{
    /// <summary>
    /// 回避移動のゲームバランス値を定義します。
    /// Prefabや実装クラスへ値を重複保持せず、調整対象をScriptableObjectへ集約します。
    /// </summary>
    [CreateAssetMenu(fileName = "DodgeDefinition", menuName = "Demon King/Gameplay/Dodge Definition")]
    public sealed class DodgeDefinition : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float dodgeSpeed = 8f;
        [SerializeField, Min(0.02f)] private float duration = 0.16f;
        [SerializeField, Min(0f)] private float cooldown = 0.35f;

        public float DodgeSpeed => dodgeSpeed;
        public float Duration => duration;
        public float Cooldown => cooldown;
    }
}
