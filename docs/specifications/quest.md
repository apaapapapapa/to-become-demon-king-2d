# Quest仕様

## DefinitionとRuntime State

Questの静的条件は `QuestDefinition` / `QuestObjectiveDefinition`、可変進捗はDomainの `QuestProgressState` / `ObjectiveProgressState` が保持します。

Definitionを進捗状態として書き換えません。

## Objective進捗

Questは `GameplayEvent` を入力としてObjective条件を評価します。

`GameplayEvent` は次を持ちます。

- `eventId`: 出来事の種類
- `subjectId`: 対象コンテンツ
- `amount`: 進捗量

`QuestProgressionService` は登録済みQuest DefinitionのObjective条件とEventを照合し、一致したObjectiveだけを更新します。

- 必要数を超えて進捗しない。
- 完了済みObjectiveを再度進捗しない。
- Quest完了通知は未完了から完了へ遷移したときだけ行う。

## Event境界

Combat、Dialogue等のイベント発生元はQuestを直接参照しません。Feature間の接続は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。

## Prototypeコンテンツ

最初の訓練Questは、訓練用スライムの撃破EventをObjectiveとして使用します。具体的なIDや条件値はUnityのQuest Definitionを正とします。
