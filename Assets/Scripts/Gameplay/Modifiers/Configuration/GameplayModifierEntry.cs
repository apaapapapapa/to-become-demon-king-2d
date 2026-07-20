using System;
using DemonKing.Domain;
using UnityEngine;

namespace DemonKing.Gameplay.Modifiers.Configuration
{
    public enum GameplayModifierTarget
    {
        OutgoingDamage = 0,
        AbilityCooldown = 1,
        ArtMasteryGain = 2
    }

    public enum NumericModifierOperation
    {
        AddFlat = 0,
        AddRate = 1
    }

    /// <summary>
    /// Skill、Evolution、将来の装備やBuffが共有する静的な数値補正定義です。
    /// 対象IDが空なら同種の全コンテンツへ作用します。
    /// </summary>
    [Serializable]
    public sealed class GameplayModifierEntry
    {
        [SerializeField] private GameplayModifierTarget target;
        [SerializeField] private NumericModifierOperation operation;
        [SerializeField] private float value;
        [SerializeField] private string targetContentId = string.Empty;

        public GameplayModifierTarget Target => target;
        public NumericModifierOperation Operation => operation;
        public float Value => value;
        public string TargetContentId => targetContentId;

        public bool AppliesTo(GameplayModifierTarget requestedTarget, string contentId)
        {
            return target == requestedTarget &&
                   (string.IsNullOrEmpty(targetContentId) ||
                    string.Equals(targetContentId, contentId, StringComparison.Ordinal));
        }

        public NumericModifier CreateModifier()
        {
            return operation == NumericModifierOperation.AddFlat
                ? new NumericModifier(value, 0d)
                : new NumericModifier(0d, value);
        }

        internal bool IsConfigured
        {
            get
            {
                if (!Enum.IsDefined(typeof(GameplayModifierTarget), target) ||
                    !Enum.IsDefined(typeof(NumericModifierOperation), operation) ||
                    float.IsNaN(value) ||
                    float.IsInfinity(value) ||
                    Mathf.Approximately(value, 0f) ||
                    (operation == NumericModifierOperation.AddRate && value <= -1f))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(targetContentId))
                {
                    return true;
                }

                string requiredPrefix = target == GameplayModifierTarget.ArtMasteryGain
                    ? "art."
                    : "ability.";
                return StableContentId.IsValid(targetContentId) &&
                       targetContentId.StartsWith(requiredPrefix, StringComparison.Ordinal);
            }
        }

        internal void Normalize()
        {
            targetContentId = StableContentId.Normalize(targetContentId);
        }
    }
}
