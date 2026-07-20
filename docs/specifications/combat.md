# 戦闘仕様

## ダメージ適用

```text
Ability Executor
  ↓
DamageRequest
  ↓
IDamageable / Health
  ↓
DamageResult
  ↓ 撃破時
DefeatContext
```

- `DamageRequest`: ダメージ適用要求と発生元情報を渡す。
- `IDamageable`: ダメージを受け取れる対象の契約。
- `Health`: HPと生存状態を管理し、`DamageResult` を返す。
- `DamageResult`: 1回のダメージ適用結果を表す。
- `DefeatContext`: 撃破事実と発生元情報をCombat外へ渡す。

経験値、ドロップ、Art / Skill取得、Evolution処理はCombatへ埋め込みません。RewardやProgressionへの接続方向は [Feature間の責務境界](../design/feature-boundaries.md#ability--combat--reward--progression) を参照してください。

## 戦闘演出

演出コンポーネントはCombatの通知を購読するだけとし、ダメージ量、死亡判定、報酬付与を変更しません。

- 攻撃成立通知から攻撃エフェクトを表示する。
- `Health.Died` から撃破エフェクトを表示する。
- 撃破エフェクトは対象の破棄後も再生できる独立したGameObjectとして生成する。
- Prototypeの図形生成演出は本番アート確定後にPrefab、Animator、Particle System等へ差し替え可能な境界とする。

訓練用スライムの再生成・復元はCombatではなく [Spawning仕様](./spawning.md) を参照してください。

## 攻撃Definition

Ability共通情報は `AbilityDefinition`、近接固有情報は `MeleeAttackDefinition`、飛翔体固有情報は `ProjectileAttackDefinition` が保持します。

具体的なダメージ、速度、範囲等はUnity Definitionを正とします。Ability実行側の責務は [Ability仕様](./ability.md) を参照してください。

## 効果成立通知

命中等の実効果が成立した場合、Ability実行系は `AbilityEffectResolved` を通知します。CombatはArt熟練度を直接変更しません。

Art熟練との接続は [Feature間の責務境界](../design/feature-boundaries.md#art熟練とability効果) を参照してください。

## Passive Modifier

Combatは汎用Modifier Sourceから補正を受け取り、SkillやEvolutionの取得状態を直接参照しません。

Modifierの接続方針は [Feature間の責務境界](../design/feature-boundaries.md#受動modifier) を参照してください。

現在の実装状況と今後の対象は [ロードマップ](../development/roadmap.md) を参照してください。
