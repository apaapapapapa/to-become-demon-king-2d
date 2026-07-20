using System;
using UnityEngine;

namespace DemonKing.Gameplay.Combat
{
    /// <summary>
    /// ダメージ計算で扱う基本属性です。
    /// </summary>
    public enum DamageType
    {
        Physical = 0,
        Magical = 1,
        Pure = 2
    }

    /// <summary>
    /// 攻撃の由来や追加性質を表す組み合わせ可能なタグです。
    /// </summary>
    [Flags]
    public enum DamageTags
    {
        None = 0,
        BasicAttack = 1 << 0,
        Skill = 1 << 1,
        Critical = 1 << 2,
        Environmental = 1 << 3
    }

    /// <summary>
    /// 攻撃側から被ダメージ側へ渡す型付き要求です。
    /// </summary>
    public readonly struct DamageRequest
    {
        public DamageRequest(
            int amount,
            GameObject source = null,
            string sourceActorId = "",
            string abilityId = "",
            DamageType damageType = DamageType.Physical,
            DamageTags tags = DamageTags.None)
        {
            Amount = amount;
            Source = source;
            SourceActorId = sourceActorId ?? string.Empty;
            AbilityId = abilityId ?? string.Empty;
            DamageType = damageType;
            Tags = tags;
        }

        public int Amount { get; }
        public GameObject Source { get; }
        public string SourceActorId { get; }
        public string AbilityId { get; }
        public DamageType DamageType { get; }
        public DamageTags Tags { get; }
        public bool IsValid => Amount > 0;
    }

    /// <summary>
    /// 近接攻撃の実行結果を、アニメーションや効果音などの演出へ通知するための情報です。
    /// </summary>
    public readonly struct MeleeAttackEvent
    {
        public MeleeAttackEvent(
            Vector2 origin,
            Vector2 center,
            Vector2 facingDirection,
            float radius,
            int hitCount)
        {
            Origin = origin;
            Center = center;
            FacingDirection = facingDirection;
            Radius = radius;
            HitCount = hitCount;
        }

        public Vector2 Origin { get; }
        public Vector2 Center { get; }
        public Vector2 FacingDirection { get; }
        public float Radius { get; }
        public int HitCount { get; }
        public bool DidHit => HitCount > 0;
    }

    /// <summary>
    /// 撃破報酬、クエスト、演出がHealthへ依存せず結果を受け取るための情報です。
    /// </summary>
    public sealed class DefeatContext
    {
        internal DefeatContext(
            DamageRequest request,
            GameObject defeatedTarget,
            string defeatedActorId,
            string rewardDefinitionId)
        {
            DefeatId = Guid.NewGuid();
            Request = request;
            DefeatedTarget = defeatedTarget;
            DefeatedActorId = defeatedActorId ?? string.Empty;
            RewardDefinitionId = rewardDefinitionId ?? string.Empty;
        }

        public Guid DefeatId { get; }
        public DamageRequest Request { get; }
        public GameObject Attacker => Request.Source;
        public GameObject DefeatedTarget { get; }
        public string AttackerActorId => Request.SourceActorId;
        public string DefeatedActorId { get; }
        public string AbilityId => Request.AbilityId;
        public string RewardDefinitionId { get; }
    }

    /// <summary>
    /// 実際に適用されたダメージと撃破結果を攻撃側へ返します。
    /// </summary>
    public readonly struct DamageResult
    {
        internal DamageResult(
            DamageRequest request,
            int appliedAmount,
            int remainingHealth,
            GameObject target,
            DefeatContext defeatContext)
        {
            Request = request;
            AppliedAmount = appliedAmount;
            RemainingHealth = remainingHealth;
            Target = target;
            DefeatContext = defeatContext;
        }

        public DamageRequest Request { get; }
        public int RequestedAmount => Request.Amount;
        public int AppliedAmount { get; }
        public int RemainingHealth { get; }
        public GameObject Target { get; }
        public DefeatContext DefeatContext { get; }
        public bool WasApplied => AppliedAmount > 0;
        public bool WasCritical => (Request.Tags & DamageTags.Critical) != 0;
        public bool WasDefeated => DefeatContext != null;
    }
}
