# 戦闘仕様

## 現在の範囲

現在は、プレイヤーの近接攻撃によって `IDamageable` を持つ対象へダメージを与え、`Health` が0になると死亡状態へ移行する最小戦闘ループを実装しています。

```text
Attack入力
  ↓
PlayerMeleeAttack
  ↓
IDamageable
  ↓
Health
  ↓
HP減少 / Death
```

## 責務

### PlayerMeleeAttack

- 攻撃入力を受ける。
- 攻撃範囲を判定する。
- 対象へダメージを要求する。

### IDamageable

ダメージを受け取れる対象の契約です。プレイヤー側は具体的な敵種別を知りません。

### Health

- 現在HPを保持する。
- ダメージを反映する。
- 生存／死亡状態を管理する。
- 死亡イベントを通知する。

## ゲームバランス値

攻撃力、攻撃半径、攻撃距離は `MeleeAttackDefinition` のScriptableObjectをSource of Truthとします。

Markdownへ同じ数値を固定値として複製しません。

## 未実装・今後の対象

- 敵AI
- プレイヤー被弾
- 無敵時間
- ノックバック
- 攻撃アニメーションとの同期
- 属性・耐性
- スキル
- ドロップ・経験値

これらは必要になった時点で仕様を分離します。
