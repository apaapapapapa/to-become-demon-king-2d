using System;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Rewards.Configuration;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Combat機能を確認するための訓練用ダミーです。
    /// HPと死亡判定は汎用Healthへ委譲し、このクラスは試作表示と結果ログだけを担当します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(GroupYSorter))]
    [RequireComponent(typeof(Health))]
    public sealed class PrototypeCombatDummy : MonoBehaviour
    {
        [SerializeField] private string actorId = "character.training_dummy";
        [SerializeField] private string rewardDefinitionId = "reward.training_dummy";

        private Health health;

        public event Action<DefeatContext> Defeated;

        public string ActorId => actorId;
        public string RewardDefinitionId => rewardDefinitionId;
        public bool IsAlive => health != null && health.IsAlive;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.ConfigureCombatIdentity(actorId, rewardDefinitionId);

            CircleCollider2D hitCollider = GetComponent<CircleCollider2D>();
            hitCollider.isTrigger = true;
            hitCollider.radius = 0.48f;
            hitCollider.offset = new Vector2(0f, 0.24f);

            if (GetComponentInChildren<SpriteRenderer>(includeInactive: true) == null)
            {
                CreateVisuals();
            }
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (health == null)
            {
                return;
            }

            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        public void ConfigureReward(RewardDefinition rewardDefinition)
        {
            if (rewardDefinition == null || !rewardDefinition.IsConfigured)
            {
                throw new ArgumentException(
                    "訓練用ダミーの報酬定義が正しく設定されていません。",
                    nameof(rewardDefinition));
            }

            rewardDefinitionId = rewardDefinition.RewardId;
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            health.ConfigureCombatIdentity(actorId, rewardDefinitionId);
        }

        public void RestoreToFull()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            health.RestoreToFull();
        }

        private void HandleDamaged(DamageResult result)
        {
            Debug.Log($"訓練用スライムに{result.AppliedAmount}ダメージ。残りHP: {health.CurrentHealth}/{health.MaxHealth}", this);
        }

        private void HandleDied(DefeatContext context)
        {
            Debug.Log($"訓練用スライムを倒した。報酬ID: {context.RewardDefinitionId}", this);
            Defeated?.Invoke(context);
            Destroy(gameObject);
        }

        private void CreateVisuals()
        {
            var shapes = new RuntimeShapeFactory();
            shapes.CreateEllipse("敵の影", new Vector2(0f, -0.33f), new Vector2(1.0f, 0.28f),
                new Color(0.06f, 0.10f, 0.12f, 0.62f), -2, transform);
            shapes.CreateEllipse("敵の輪郭", new Vector2(0f, 0.04f), new Vector2(1.0f, 0.78f),
                new Color(0.35f, 0.10f, 0.18f), 0, transform);
            shapes.CreateEllipse("敵のからだ", new Vector2(0f, 0.08f), new Vector2(0.86f, 0.66f),
                new Color(0.82f, 0.24f, 0.32f), 1, transform);
            shapes.CreateEllipse("左目", new Vector2(-0.17f, 0.10f), new Vector2(0.08f, 0.12f),
                new Color(0.08f, 0.04f, 0.05f), 2, transform);
            shapes.CreateEllipse("右目", new Vector2(0.17f, 0.10f), new Vector2(0.08f, 0.12f),
                new Color(0.08f, 0.04f, 0.05f), 2, transform);
        }
    }
}
