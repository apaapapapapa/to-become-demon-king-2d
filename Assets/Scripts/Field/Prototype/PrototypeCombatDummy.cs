using DemonKing.Gameplay.Combat;
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
        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();

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

        private void HandleDamaged(int amount, GameObject source)
        {
            Debug.Log($"訓練用スライムに{amount}ダメージ。残りHP: {health.CurrentHealth}/{health.MaxHealth}", this);
        }

        private void HandleDied(GameObject source)
        {
            Debug.Log("訓練用スライムを倒した。Combatの攻撃→HP→死亡ループを確認しました。", this);
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
