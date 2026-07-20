# 戦闘仕様

## 現在のフロー

```text
Attack入力
  ↓
PlayerMeleeAttack
  ↓
DamageRequest
  ↓
IDamageable / Health
  ↓
DamageResult
  ↓ 撃破時
DefeatContext
  ↓
RewardService
```

## 責務

- `PlayerMeleeAttack`: 入力、攻撃範囲、DamageRequest生成
- `IDamageable`: ダメージを受け取れる対象の契約
- `Health`: HP、生存状態、DamageResult生成
- `DamageResult`: 1回のダメージ適用結果
- `DefeatContext`: 撃破事実と攻撃者・Ability・対象・報酬参照をCombat外へ渡す

経験値、ドロップ、進化処理をHealthや攻撃コンポーネントへ直接埋め込みません。

## 戦闘演出

- `PlayerMeleeAttack.AttackPerformed` が攻撃位置、向き、範囲、命中数を通知し、`PrototypeMeleeAttackEffect` が斬撃を表示する
- `Health.Died` を `PrototypeMonsterDefeatEffect` が購読し、撃破位置へ破裂・消滅表現を表示する
- 撃破エフェクトは対象の子にせず独立したGameObjectとして生成し、対象が同じフレームで破棄されても最後まで再生する
- 現在の図形生成エフェクトはPrototype境界とし、本番アート確定後はPrefab、Animator、Particle Systemなどへ差し替える

演出コンポーネントはCombatの通知を購読するだけとし、ダメージ量、死亡判定、報酬付与を変更しません。

## Definition

`MeleeAttackDefinition` がStable Ability ID、Damage Type、ダメージ、攻撃半径、攻撃距離などの静的定義を持ちます。

Runtime数値はScriptableObjectを正とし、Markdownへ複製しません。

## Reward接続

`RewardService` は `DefeatContext` と `RewardDefinition` を使って報酬を適用します。現在は訓練用ダミー撃破からプレイヤーの経験値加算まで接続済みです。

同一Defeat IDへの報酬重複付与を防ぎます。

## 今後

- プレイヤー被弾の完成版ルール
- 無敵時間
- ノックバック
- 攻撃アニメーション同期
- Skill / Ability拡張
- ドロップ
- 属性・耐性
