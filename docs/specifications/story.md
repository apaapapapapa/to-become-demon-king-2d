# Story Progression仕様

## 目的

本編進行をQuest進捗から分離し、章・Story Flag・一度きりStory EventをGame Session単位で管理します。

Story Runtimeは巨大なシナリオエンジンや専用Graphを持たず、次の最小状態だけをSource of Truthとします。

```text
StoryProgressState
  currentChapterId
  flags[]
  executedEventIds[]
```

- `currentChapterId`: 現在の本編章を示すStable ID
- `flags`: 成立済みのStory条件を示すStable ID集合
- `executedEventIds`: 一度きりStory Eventの再実行を防ぐStable ID集合

## Questとの責務分離

QuestとStoryは互いのRuntime Stateを直接変更しません。

```text
Gameplay Event Source
  -> GameplayEventHub
     |- QuestProgressionService
     `- StoryProgressionService
```

`QuestProgressionService` はObjective / Quest Statusだけを変更します。Quest完了時は `gameplay.quest_completed` を共有 `GameplayEventHub` へ発行し、Story側が必要に応じて購読します。

Story EventからQuest Stateを直接変更しません。Story / Quest双方の状態を表示条件に使う場合はDialogue Composition等の読み取り境界で組み合わせます。

## Gameplay Event境界

`GameplayEvent` はQuest固有型ではなく `DemonKing.Domain.Events` の汎用Domain Eventです。

```text
GameplayEvent
  eventId
  subjectId
  amount
```

P0では少なくとも次の発生元を共通Event Hubへ接続します。

- Field進入: `gameplay.field_entered`
- Interaction完了: `gameplay.interaction_completed`
- Dialogue完了: `gameplay.dialogue_completed`
- Enemy Defeat: `gameplay.enemy_defeated`
- Quest完了: `gameplay.quest_completed`

Event発生元FeatureはStory Flag、Story Chapter、Story Event Definitionを参照しません。

## Story Event

`StoryEventDefinition` は次だけを保持します。

```text
storyEventId
triggerEventId
triggerSubjectId (optional)
requiredFlagIds[]
setFlagIds[]
nextChapterId (optional)
```

`StoryProgressionService` は受け取った `GameplayEvent` をDefinitionへ照合し、条件成立時にFlag / Chapterを更新します。

Story Eventは一度きりです。成功した `storyEventId` を `executedEventIds` へ記録し、同じGameplay Eventを再度受信しても再実行しません。

### Prototype P0定義

P0ではRuntime境界確認用に次を定義します。

```text
story.chapter.prologue
story.chapter.first_journey

prologue.met_human
prologue.left_forest
prologue.found_ruins
prologue.training_completed
```

代表Event:

- 最初の人間NPCとのInteraction -> `prologue.met_human`
- 訓練場から森門Fieldへ進入 -> `prologue.left_forest`
- 訓練Quest完了 -> `prologue.training_completed` + `story.chapter.first_journey`

P0定義はRuntime境界を検証するための最小Contentです。本編シナリオ量やCutscene Sequenceをこのクラスへ集約しません。

## Dialogue選択

同一NPCのDialogueはStory / Quest状態の読み取り結果からComposition層で選択できます。

```text
StoryProgressState
QuestProgressStatus (optional)
  -> StoryDialogueSelector
  -> DialogueDefinition candidate
  -> PrototypeNpcInteractable
```

`StoryDialogueSelector` は優先度順の候補から最初に条件を満たすDialogueを返し、一致しなければQuest等が決めたfallback Dialogueを返します。

P0では見習い魔術師で `prologue.left_forest` とQuest Statusを組み合わせた候補選択を行い、同一NPCでもStory進行により選択結果が変わる経路を検証します。新規会話Contentの本格Authoringは後続Story/Contentタスクへ分離します。

## Game Session寿命

次はField Sceneより長く保持します。

- `StoryProgressState`
- `StoryProgressionService`
- `GameplayEventHub`
- `QuestProgressionService`

Field遷移でPlayer / Worldを再生成してもStory Flagや実行済みEventは失いません。共有Event Hubも再生成せず、Quest / Story購読をGame Session開始時に一度だけ設定します。

## Save / Load

Story状態はSave Version 5から保存します。

```text
story
  currentChapterId
  flags[]
  executedEventIds[]
```

保存しないもの:

- Story Event Definition参照
- 条件評価結果
- Dialogue Definition参照
- Event Hub購読状態
- Scene / GameObject参照

Version 4 -> 5 Migrationでは空の `StorySaveData` を追加します。旧Saveをロードした場合はPrototypeのInitial Chapter `story.chapter.prologue` を適用します。

Continue / Load Gameでは `PrototypeSaveSession` がStory DTOを `StoryProgressState` へ復元し、その同じStateをGame Session中の `StoryProgressionService` とSave Snapshotが共有します。

## テスト

EditModeでは少なくとも次を検証します。

- 必須FlagがないStory Eventは発火しない
- 条件成立EventがFlag / Chapterを更新する
- 一度実行したStory Eventは再実行しない
- Story SaveのRound Trip
- Save Version 4 -> 5 Migration
- Story Flag + Quest StatusによるDialogue候補選択
- 同一Gameplay EventをQuestとStoryの双方が購読できる
- Save SessionでStory Flag / 実行済みEventを復元し、次Snapshotへ維持する

PlayMode / IntegrationではField進入、NPC Interaction / Dialogue、Combat Defeat、Quest完了の各発生元が共有Gameplay Event境界を壊さないことを既存Featureテストと縦切りテストで検証します。
