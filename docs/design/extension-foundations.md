# 拡張基盤: Dialogue / Quest / Spawn

## Dialogue

会話コンテンツは `DialogueDefinition` ScriptableObjectへ集約します。

```text
DialogueDefinition
  ├ dialogueId
  ├ speaker
  └ lines[]
       ↓
LinearDialogueSequence
       ↓
PrototypeNpcInteractable
       ↓
DialogueLog
       ↓
DialogueLogView
```

`PrototypeNpcInteractable` は会話本文を保持せず、Interactionと進行State・表示の橋渡しだけを担当します。今後の分岐会話は `DialogueDefinition` をNode/Condition形式へ拡張し、NPC Componentへ分岐条件を埋め込みません。

## Quest / Objective

QuestはGameplay Eventを入力境界とし、CombatやDialogueを直接参照しません。

```text
Combat / Dialogue / Exploration
       ↓
GameplayEventHub
       ↓ GameplayEvent(eventId, subjectId, amount)
QuestProgressionService
       ↓ Objective条件照合
QuestProgressState
  └ ObjectiveProgressState
```

静的条件は `QuestDefinition` / `QuestObjectiveDefinition`、可変進捗はDomainの `QuestProgressState` / `ObjectiveProgressState` に分離します。

Prototypeでは `gameplay.enemy_defeated + character.training_dummy` を `FirstTrainingQuest` のObjectiveへ接続しています。Quest追加時にCombat側へQuest固有処理を追加しないことを原則とします。

## Spawn / Lifecycle

Spawn対象固有の生成と、再利用・再生成のLifecycleを分離します。

```text
PrototypeCombatDummyFactory
       ↓ spawn delegate
SpawnLifecycle<PrototypeCombatDummy>
       ├ canRestore
       └ restore
```

`SpawnLifecycle<T>` は位置、Prefab、敵種別、報酬を知りません。正式な敵Spawnへ拡張する際は、Enemy Factory / Spawn Point Definitionを追加して同じLifecycle境界へ接続します。

## Prototype Composition

`PrototypeTrainingAreaCoordinator` が以下のFeature間配線を担当します。

- NPC Interact → SpawnLifecycleのSpawn / Restore
- Dialogue完了 → Progression Grant + GameplayEvent
- Enemy Defeat → GameplayEvent
- GameplayEventHub → QuestProgressionService
- Quest完了 → Prototypeログ

各Gameplay Featureは互いの具体実装へ依存しません。
