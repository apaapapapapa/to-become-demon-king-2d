# Ability仕様

Abilityはキャラクターが実行できる行動です。Art / Skill / Evolutionとの概念境界は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。

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

プレイヤー入力は `PlayerAbilityInput` がデバイス非依存の実行要求へ変換します。AIは同じ `AbilityController.TryUse` を利用します。

## Definition

`AbilityDefinition` は次の共通情報を持つ抽象ScriptableObjectです。

- `ability.*` 形式のStable Content ID
- 表示名、説明、アイコン
- 実行方式
- クールダウン
- コスト

Ability固有のDefinitionがこれを継承します。具体的なRuntime値はUnity Definitionを正とします。

## Runtime State

`AbilityRuntimeState` はキャラクター個体とAbilityの組み合わせごとに次を保持します。

- 残りクールダウン
- 使用回数
- 実行中かどうか

Definitionを実行時状態として書き換えません。現在のAbility Runtime StateはSave対象に含めません。

## AbilityController

`AbilityController` は次を担当します。

- CharacterDefinitionから付与されたAbilityの登録
- 使用者、Ability ID、実行中状態、クールダウン、コスト、Executorの使用可否判定
- Runtime Stateの更新
- `IAbilityExecutor` への実行委譲
- 成否を含む `AbilityUseResult` の返却と通知

使用者は `GameObject` として要求へ渡し、その個体のControllerと一致する必要があります。入力を持たない敵やNPCにも同じ実行入口を使用できます。

## Executor

`IAbilityExecutor` はAbility固有の効果だけを発生させます。

Executorは入力、Art習得・熟練度、Skill解放、Evolution条件、経験値、ドロップを処理しません。CombatやArt成長との接続方向は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。

## コスト境界

コストなしのAbilityは追加コンポーネントなしで実行できます。コストが設定されたAbilityは使用者上の `IAbilityCostSource` が支払い可否と消費を担当します。

現在の実装状況と今後の対象は [ロードマップ](../development/roadmap.md) を参照してください。
