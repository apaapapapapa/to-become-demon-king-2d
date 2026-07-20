# Quest仕様

## DefinitionとRuntime State

Questの静的条件とプレイヤー表示名は `QuestDefinition` / `QuestObjectiveDefinition`、可変進捗はDomainの `QuestProgressState` / `ObjectiveProgressState` が保持します。

Definitionを進捗状態として書き換えません。

`QuestProgressState` は次のライフサイクルを持ちます。

```text
Available
  ↓ AcceptQuest
Active
  ↓ 全Objective完了
Completed
```

未受注の `Available` QuestはGameplay Eventを受けても進捗しません。完了済みQuestも再進捗しません。

## 受注

Questの受注は `QuestProgressionService.AcceptQuest` を明示的に呼び出します。

- 同じQuestを複数回受注しない。
- 受注成功時だけ `QuestAccepted` を通知する。
- Prototypeの最初の訓練Questは、見習い魔術師へ初めてInteractionした時点で受注する。
- NPCはQuest実装を直接参照せず、Prototype CompositionがNPC InteractionとQuest受注を接続する。

## Objective進捗

Questは `GameplayEvent` を入力としてObjective条件を評価します。

`GameplayEvent` は次を持ちます。

- `eventId`: 出来事の種類
- `subjectId`: 対象コンテンツ
- `amount`: 進捗量

`QuestProgressionService` は受注中QuestのObjective条件とEventを照合し、一致したObjectiveだけを更新します。

- 必要数を超えて進捗しない。
- 完了済みObjectiveを再度進捗しない。
- `ProgressChanged` は実際に進捗したときだけ通知する。
- Quest完了通知は `Active` から `Completed` へ遷移したときだけ行う。

## プレイヤーへの表示

`QuestTrackerView` は `QuestProgressionService` を購読し、Quest状態を変更せず表示だけを担当します。

- 未受注Questしかない場合、Questトラッカーを表示しない。
- 受注後は画面右上へQuest名、`受注中` 状態、Objective名と現在数 / 必要数を表示する。
- Objective完了時はチェック表示へ切り替える。
- Quest完了後は状態を `完了` として表示する。
- 受注、進捗、完了の状態遷移時は、ゲーム操作を止めない非モーダル通知を画面上部へ表示する。
- PauseやEvolution等のモーダルInput ContextはQuestトラッカーから変更しない。

## Event境界

Combat、Dialogue等のイベント発生元はQuestを直接参照しません。Feature間の接続は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。

## Prototypeコンテンツ

最初の訓練Questは、見習い魔術師とのInteractionで受注し、訓練用スライムの撃破EventをObjectiveとして使用します。具体的なID、表示名、条件値はUnityのQuest Definitionを正とします。
