# Quest仕様

## DefinitionとRuntime State

Questの静的条件とプレイヤー表示名は `QuestDefinition` / `QuestObjectiveDefinition`、可変進捗はDomainの `QuestProgressState` / `ObjectiveProgressState` が保持します。

Definitionを進捗状態として書き換えません。

`QuestProgressState` は次のライフサイクルを持ちます。

```text
Available
  ↓ AcceptQuest
Active
  ↓ 全Objective達成
ReadyToTurnIn
  ↓ CompleteQuest
Completed
```

未受注の `Available` QuestはGameplay Eventを受けても進捗しません。`ReadyToTurnIn` と `Completed` のQuestもObjectiveを再進捗しません。

## 受注

Questの受注は `QuestProgressionService.AcceptQuest` を明示的に呼び出します。

- 同じQuestを複数回受注しない。
- 受注成功時だけ `QuestAccepted` を通知する。
- Prototypeの最初の訓練Questは、見習い魔術師の依頼会話を最後まで完了した時点で受注する。
- NPCはQuest実装を直接参照せず、Prototype CompositionがNPC会話完了とQuest受注を接続する。

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
- 全Objective達成時は `Active` から `ReadyToTurnIn` へ遷移し、`QuestReadyToTurnIn` を1回だけ通知する。
- Objective達成だけではQuestを `Completed` にしない。

## 報告と完了

報告が必要なQuestは、`ReadyToTurnIn` の状態で指定NPCとの完了報告を終えた後に `QuestProgressionService.CompleteQuest` を呼び出します。

- `CompleteQuest` は `ReadyToTurnIn` のQuestにだけ成功する。
- 完了成功時だけ `QuestCompleted` を通知する。
- Quest固有の報酬は報告完了と同じComposition境界で付与する。
- 報酬付与のためにNPCやQuest Runtimeが互いを直接参照しない。

## プレイヤーへの表示

`QuestTrackerView` は `QuestProgressionService` を購読し、Quest状態を変更せず表示だけを担当します。

- 未受注Questしかない場合、Questトラッカーを表示しない。
- 受注後は画面右上へQuest名、`受注中` 状態、Objective名と現在数 / 必要数を表示する。
- Objective完了時はチェック表示へ切り替える。
- 全Objective達成後は状態を `報告可能` とし、報告先NPCへ戻るよう通知する。
- NPCへの報告完了後は状態を `完了` として表示する。
- 受注、進捗、報告可能、完了の状態遷移時は、ゲーム操作を止めない非モーダル通知を画面上部へ表示する。
- PauseやEvolution等のモーダルInput ContextはQuestトラッカーから変更しない。

## NPC会話との縦切りループ

Prototypeの見習い魔術師はQuest状態に応じて会話内容を切り替えます。NPC自身はQuest状態を判定せず、Composition層が会話開始時にDialogue Definitionを選択します。

```text
Available
  ↓ 依頼会話を完了
Active
  ↓ 訓練用スライムを討伐
ReadyToTurnIn
  ↓ 報告会話を完了
Completed
  ↓
完了後会話
```

- `Available`: Quest依頼会話を表示する。
- `Active`: 目標を案内する進行中会話を表示する。
- `ReadyToTurnIn`: 達成報告と報酬案内の会話を表示する。
- `Completed`: 完了後の通常会話を表示する。
- 会話途中でQuest状態によるDialogue Definitionの再選択を行わず、会話開始時だけ選択する。

## Event境界

Combat、Dialogue等のイベント発生元はQuestを直接参照しません。Feature間の接続は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。

## Prototypeコンテンツ

最初の訓練Questは、見習い魔術師の依頼会話を完了して受注し、訓練用スライムの撃破EventをObjectiveとして使用します。Objective達成後に見習い魔術師へ戻って報告会話を完了するとQuestが完了し、火炎魔法のProgression Grantを受け取ります。具体的なID、表示名、条件値、会話本文はUnityのDefinitionを正とします。
