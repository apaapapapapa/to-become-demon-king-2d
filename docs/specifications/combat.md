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
