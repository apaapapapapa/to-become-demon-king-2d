# Ability仕様

## 用語

- Ability: 攻撃、防御、回復、バフなど、キャラクターが実行できる行動
- Art: 攻撃魔法や特殊剣攻撃など、習得・熟練によって1つ以上のAbilityを段階解放する能動技能
- Skill: 能力値、コスト、習得条件、Art成長などへ作用する受動的な成長要素
- Evolution: キャラクターの形態や成長経路を変える、不可逆または排他的な選択

Abilityは「何を実行するか」だけを扱い、Artの習得・熟練状態、Skillの取得状態、Evolution条件を知りません。能動行動は生得Ability、またはArtから解放されるAbilityとして表現します。受動補正が必要な箇所は、取得元を限定しないModifier Source契約を参照します。

## 実行フロー

```text
プレイヤー入力 / AI判断
  ↓ Ability ID・使用者・実行方向
AbilityController
  ├ AbilityDefinition
  ├ AbilityRuntimeState
  └ IAbilityCostSource（コストがある場合）
  ↓ AbilityExecutionRequest
IAbilityExecutor
  ↓
Ability固有の効果
```

プレイヤー入力は `PlayerAbilityInput` がデバイス非依存の実行要求へ変換します。AIは同じ `AbilityController.TryUse` を直接呼び出します。

## Definition

`AbilityDefinition` は次の共通情報を持つ抽象ScriptableObjectです。

- `ability.*` 形式のStable Content ID
- 表示名、説明、アイコン
- 実行方式
- クールダウン
- コスト

Ability固有のDefinitionがこれを継承します。最初の実装である `MeleeAttackDefinition` は、`ability.basic_melee` を維持し、ダメージ種別や攻撃範囲など近接攻撃固有の静的値を持ちます。

## Runtime State

`AbilityRuntimeState` はキャラクター個体とAbilityの組み合わせごとに次を保持します。

- 残りクールダウン
- 使用回数
- 実行中かどうか

Definitionを実行時状態として書き換えません。現在のRuntime Stateは一時状態であり、Save対象には含めません。

## AbilityController

`AbilityController` は次を担当します。

- CharacterDefinitionから付与されたAbilityの登録
- 使用者、Ability ID、実行中状態、クールダウン、コスト、Executorの使用可否判定
- Runtime Stateの更新
- `IAbilityExecutor` への実行委譲
- 成否を含む `AbilityUseResult` の返却と通知

使用者は `GameObject` として明示的に要求へ渡し、その個体のControllerと一致する必要があります。入力を持たない敵やNPCにも同じControllerを配置できます。

## Executor

`IAbilityExecutor` はAbility固有の効果だけを発生させます。`MeleeAttackExecutor` は近接範囲を評価し、`DamageRequest` に使用者、Actor ID、`ability.basic_melee`、Damage Type、Damage Tagsを渡します。

Executorは入力、Art習得・熟練度、Skill解放、Evolution条件、経験値、ドロップを処理しません。効果成立の事実は将来の共通通知へ渡しますが、Art進捗を直接変更しません。

## コスト境界

コストなしのAbilityは追加コンポーネントなしで実行できます。コストが設定されたAbilityは、使用者上の `IAbilityCostSource` が支払い可否と消費を担当します。具体的なMP等のリソースは未実装です。

## 現在の実装範囲

- 共通Definition / Runtime State / Controller / Executor契約
- プレイヤー入力Adapter
- 基本近接攻撃 `ability.basic_melee`
- クールダウン、使用回数、実行中状態
- 汎用コスト接続境界
- 実行ごとのExecution ID
- 効果成立通知 `AbilityEffectResolved`
- 実行中の状態を壊さない冪等なAbility追加
- `IAbilityCooldownModifierSource` による受動クールダウン補正

ArtからのAbility付与、近接ダメージの効果成立通知、受動Skillからのクールダウン補正は実装済みです。Abilityコスト補正、Evolution条件判定、非同期Abilityの終了通知元、回復・バフ・デバフExecutorからの効果成立通知は今後実装します。詳細は [Art仕様](./art.md) と [Skill仕様](./skill.md) を参照してください。
