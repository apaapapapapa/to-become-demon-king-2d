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

P0で使用する章のStable IDは次です。

```text
story.chapter.prologue
story.chapter.first_journey
```

Story FlagのStable IDは次です。

```text
prologue.born
prologue.met_guardian
prologue.found_food
prologue.first_hunt
prologue.part1_completed
prologue.met_human
prologue.left_forest
prologue.found_ruins
prologue.training_completed
```

Playable Prologue Part 1のStory Eventは次です。

| Story Event ID | Gameplay Event / Subject | 必須Flag | 設定Flag |
| --- | --- | --- | --- |
| `story.event.prologue.born` | `gameplay.field_entered` / `field.prologue.deep_forest` | なし | `prologue.born` |
| `story.event.prologue.met_guardian` | `gameplay.dialogue_completed` / `dialogue.prologue.guardian.intro` | なし | `prologue.met_guardian` |
| `story.event.prologue.found_food` | `gameplay.interaction_completed` / `interaction.prologue.forage` | `prologue.met_guardian` | `prologue.found_food` |
| `story.event.prologue.first_hunt` | `gameplay.enemy_defeated` / `character.prologue.forest_whelp` | `prologue.met_guardian` | `prologue.first_hunt` |
| `story.event.prologue.part1_completed` | `gameplay.dialogue_completed` / `dialogue.prologue.guardian.complete` | `prologue.found_food`, `prologue.first_hunt` | `prologue.part1_completed` |

既存Prototype検証用Eventは次です。

- `story.event.met_first_human`
  - `gameplay.interaction_completed`
  - Subjectは見習い魔術師DialogueのStable ID
  - `prologue.met_human` を設定
- `story.event.left_forest`
  - `gameplay.field_entered` / `field.prototype.forest_gate`
  - `prologue.left_forest` を設定
- `story.event.training_completed`
  - `gameplay.quest_completed` / Training Quest ID
  - `prologue.training_completed` を設定
  - Chapterを `story.chapter.first_journey` へ進める

`prologue.met_human` は任意のInteractionではなく、見習い魔術師に対応するSubject IDのInteractionだけで成立します。

P0定義はRuntime境界とPlayable Storyを検証するための最小Contentです。本編シナリオ量やCutscene Sequenceを `PrototypeStoryDefinitions` へ集約しません。Prologue固有のDialogue / Interaction / Enemy / Story Event定義は `PrototypePrologueContent` が所有し、共通Story Runtimeへ登録します。

## Dialogue選択

同一NPCのDialogueはStory / Quest状態の読み取り結果からComposition層で選択できます。

```text
StoryProgressState
QuestProgressStatus (optional)
  -> Dialogue Selector / Content Selection
  -> DialogueDefinition candidate
  -> PrototypeNpcInteractable
```

`StoryDialogueSelector` は優先度順の候補から最初に条件を満たすDialogueを返し、一致しなければQuest等が決めたfallback Dialogueを返します。

P0では次の選択経路を持ちます。

- 見習い魔術師: `prologue.left_forest` とQuest Statusを組み合わせる
- 育ての親:
  - `prologue.met_guardian` 未成立: 初回会話
  - 初回会話後、目標未完了: 探索・戦闘目標の会話
  - `prologue.found_food` と `prologue.first_hunt` 成立: 帰還完了会話
  - `prologue.part1_completed` 成立: 完了後会話

Dialogue完了時にGameplay Eventを発行し、会話Component自身はStory Flagを直接変更しません。

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

Prologue Part 1の進行はStory Flagと実行済みEvent IDだけで復元します。復元後のField Compositionは現在のFlagを読み、未完了の食料・戦闘目標だけを再生成し、完了済みEventを重複発火させません。

## テスト

EditModeでは少なくとも次を検証します。

- 必須FlagがないStory Eventは発火しない
- 条件成立EventがFlag / Chapterを更新する
- 一度実行したStory Eventは再実行しない
- Story SaveのRound Trip
- Save Version 4 -> 5 Migration
- Story Flag + Quest StatusによるDialogue候補選択
- Prologue Part 1のFlag順序と育ての親Dialogue選択
- `prologue.met_human` が対象NPC以外のInteractionでは成立しない
- 同一Gameplay EventをQuestとStoryの双方が購読できる
- Save SessionでStory Flag / 実行済みEventを復元し、次Snapshotへ維持する

PlayMode / Integrationでは次を検証します。

- Field進入、NPC Interaction / Dialogue、Combat Defeat、Quest完了の各発生元が共有Gameplay Event境界を壊さない
- New Gameから育ての親との会話、食料探索、森の幼獣撃破、帰還会話まで完走できる
- Prologue途中のSave状態から未完了目標だけを復元できる
- Part 1完了Eventが重複発火しない
