# 戦闘仕様

## 現在の範囲

現在は、プレイヤーの近接攻撃からダメージ要求を生成し、対象の `Health` へ適用した結果を `DamageResult` として返し、撃破時は `DefeatContext` へ接続できる境界まで実装しています。

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
```

## 責務

### PlayerMeleeAttack

- 攻撃入力を受ける。
- 攻撃範囲を判定する。
- `MeleeAttackDefinition` から攻撃Definitionを取得する。
- 対象へ `DamageRequest` を渡す。

### DamageRequest

1回のダメージ要求を表します。

攻撃側のAbility IDやDamage Typeなど、後続処理が「何によるダメージか」を判断するための情報を伝える境界として使用します。

### IDamageable / Health

`IDamageable` はダメージを受け取れる対象の契約です。

`Health` は主に次を担当します。

- 現在HPを保持する。
- ダメージを反映する。
- 生存／死亡状態を管理する。
- ダメージ結果を返す。
- 撃破状態を通知できる境界を提供する。

敵AI、経験値、ドロップ、進化などを `Health` へ直接実装しません。

### DamageResult

ダメージ適用結果を表します。

攻撃側や後続システムが、HP変化や撃破の有無を判断するために使用します。

### DefeatContext

対象が撃破された事実をCombatの外側へ伝えるための境界です。

将来は次の処理へ接続します。

```text
DefeatContext
  ↓
Reward Service
  ├ Experience
  ├ Drop
  └ その他の報酬
```

経験値やドロップを攻撃コンポーネントへ直接埋め込みません。

## ゲームバランス値

基本近接攻撃の静的定義は `MeleeAttackDefinition` のScriptableObjectをSource of Truthとします。

主な責務:

- Stable Ability ID
- Damage Type
- ダメージ
- 攻撃半径
- 攻撃距離

具体的なRuntime数値をMarkdownへ複製して二重管理しません。

## Stable ID

Abilityは表示名やAsset名とは別の安定IDで参照します。

例:

```text
ability.basic_melee
```

Save Dataや将来のSkill・Reward処理から参照するIDは、単純な表示名変更で変更しません。

## 現在未実装の主な機能

- Reward Service
- 経験値付与
- ドロップ
- 敵AI
- プレイヤー被弾の完成版ルール
- 無敵時間
- ノックバック
- 攻撃アニメーションとの同期
- 属性・耐性の完成版ルール
- Skill / Abilityシステム

## 次の接続

直近では、`DefeatContext` からReward Serviceへ接続し、撃破結果を経験値加算へ変換する流れを実装します。

成長状態は `CharacterProgressionState` で管理し、ScriptableObject Definitionをプレイ中に書き換えません。
