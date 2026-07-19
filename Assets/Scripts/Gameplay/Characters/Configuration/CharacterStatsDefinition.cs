using UnityEngine;

namespace DemonKing.Gameplay.Characters.Configuration
{
    /// <summary>
    /// キャラクターの基礎能力値を定義するScriptableObjectです。
    /// プレイヤーPrefabや移動コンポーネントへ数値を直接重複保持せず、ゲームバランス調整箇所を集約します。
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterStats", menuName = "Demon King/Gameplay/Character Stats")]
    public sealed class CharacterStatsDefinition : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.4f;
        [SerializeField, Min(1)] private int maxHealth = 5;

        public float MoveSpeed => moveSpeed;
        public int MaxHealth => maxHealth;
    }
}
