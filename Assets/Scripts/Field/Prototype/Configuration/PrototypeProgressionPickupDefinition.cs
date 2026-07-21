using System;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEngine;

namespace DemonKing.Field.Prototype.Configuration
{
    /// <summary>
    /// Prototypeフィールド上へ配置する一度きりのProgression取得物を定義します。
    /// Gameplay側のGrant内容と、Prototype固有の配置・見た目設定を分離します。
    /// </summary>
    [Serializable]
    public sealed class PrototypeProgressionPickupDefinition
    {
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private ProgressionGrantDefinition grantDefinition;
        [SerializeField] private Vector3 position;
        [SerializeField] private Color color = new(0.45f, 0.72f, 1f, 1f);

        public string DisplayName => displayName;
        public ProgressionGrantDefinition GrantDefinition => grantDefinition;
        public Vector3 Position => position;
        public Color Color => color;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(displayName) &&
            grantDefinition != null &&
            grantDefinition.IsConfigured;
    }
}
