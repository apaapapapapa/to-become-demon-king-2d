# AI Context Map

## 目的

この文書は、AIエージェントが変更対象に必要な仕様・コード・テストへ最短で到達するための索引です。

詳細仕様や設計判断のSource of Truthではありません。ここに実装詳細やRuntime数値を複製せず、変更対象を特定した後はリンク先の仕様・コード・テストを確認してください。

## 読み方

1. 変更対象に最も近いFeatureの `Primary specification` と `Primary code` を確認する。
2. UIやPrototype配線を変更する場合だけ `Presentation` / `Composition / Integration` を追加で確認する。
3. `Usually related` は影響範囲を判断する手掛かりとして使い、必要な場合だけ読む。
4. Feature間イベントや状態の責務を変更する場合は `Read additionally when` に従って関連Featureを確認する。
5. テストは記載された主要ファイル、または `Assets/Tests/EditMode/` / `Assets/Tests/PlayMode/` から対象クラス名・Feature名で探す。

---

## Input

**Primary specification**
- `docs/specifications/input.md`

**Primary code**
- `Assets/Scripts/Core/Input/`

**Composition / Integration**
- `Assets/Scripts/Gameplay/Abilities/`
- `Assets/Scripts/Field/Prototype/`

**Tests**
- `Assets/Tests/PlayMode/`

**Usually related**
- Ability
- Interaction
- Pause
- Dodge
- Evolution

**Read additionally when**
- Action MapやBindingを変更する場合はUnity Input Actionsアセットも確認する。
- モーダルUIの入力所有権を変更する場合はPause / EvolutionとInput Contextの競合を確認する。

---

## Interaction

**Primary specification**
- `docs/specifications/interaction.md`

**Primary code**
- `Assets/Scripts/Gameplay/Interaction/PlayerInteractor.cs`
- `Assets/Scripts/Gameplay/Interaction/`

**Composition / Integration**
- `Assets/Scripts/Field/Prototype/PrototypeNpcInteractable.cs`
- `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`

**Tests**
- `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`

**Usually related**
- Input
- Dialogue

**Read additionally when**
- NPC会話の進行・表示を変更する場合はDialogueを確認する。
- NPC Interactionから戦闘・Spawn・Questへの通知を変更する場合はPrototype Compositionと対象Featureを確認する。

---

## Dialogue

**Primary specification**
- `docs/specifications/interaction.md`
- `docs/design/extension-foundations.md`

**Primary code**
- `Assets/Scripts/Gameplay/Dialogue/DialogueDefinition.cs`
- `Assets/Scripts/Gameplay/Dialogue/LinearDialogueSequence.cs`
- `Assets/Scripts/Gameplay/Dialogue/DialogueLog.cs`

**Presentation**
- `Assets/Scripts/Presentation/UI/DialogueLogView.cs`

**Composition / Integration**
- `Assets/Scripts/Field/Prototype/PrototypeNpcInteractable.cs`
- `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- `Assets/Resources/Settings/Gameplay/ApprenticeMageDialogue.asset`

**Tests**
- `Assets/Tests/PlayMode/LinearDialogueSequenceTests.cs`
- `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`
- `Assets/Tests/PlayMode/DialogueLogViewPlayModeTests.cs`

**Usually related**
- Interaction
- Presentation
- Gameplay Events

**Read additionally when**
- 会話完了イベントを変更する場合はQuestと `GameplayEventHub` を確認する。
- NPCへのInteract時に訓練用スライムの再生成・復元を変更する場合はSpawningとPrototype Compositionを確認する。

---

## Combat

**Primary specification**
- `docs/specifications/combat.md`

**Primary code**
- `Assets/Scripts/Gameplay/Combat/`

**Composition / Integration**
- `Assets/Scripts/Field/Prototype/PrototypeCombatDummy.cs`
- `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`

**Tests**
- `Assets/Tests/EditMode/`
- `Assets/Tests/PlayMode/`

**Usually related**
- Ability
- Reward
- Modifiers

**Read additionally when**
- 撃破後の経験値・Skill付与を変更する場合はReward / Progressionを確認する。
- 撃破イベントをQuestへ接続する場合はGameplay Events / QuestとPrototype Compositionを確認する。

---

## Ability

**Primary specification**
- `docs/specifications/ability.md`

**Primary code**
- `Assets/Scripts/Gameplay/Abilities/`

**Composition / Integration**
- `Assets/Scripts/Gameplay/Combat/`
- `Assets/Scripts/Gameplay/Progression/`
- `Assets/Scripts/Core/Input/`

**Tests**
- `Assets/Tests/EditMode/`
- `Assets/Tests/PlayMode/`

**Usually related**
- Combat
- Art
- Skill
- Evolution
- Input

**Read additionally when**
- 受動補正を変更する場合はModifiersとSkill / Evolutionを確認する。
- Abilityの習得・熟練条件を変更する場合はArt / Progressionを確認する。

---

## Art

**Primary specification**
- `docs/specifications/art.md`
- `docs/specifications/progression.md`

**Primary code**
- `Assets/Scripts/Domain/Progression/`
- `Assets/Scripts/Gameplay/Progression/ArtProgressionService.cs`
- `Assets/Scripts/Gameplay/Progression/`

**Composition / Integration**
- `Assets/Scripts/Gameplay/Abilities/`
- `Assets/Scripts/Gameplay/Modifiers/`

**Tests**
- `Assets/Tests/EditMode/`
- `Assets/Tests/PlayMode/`

**Usually related**
- Ability
- Progression
- Skill
- Evolution
- Save

**Read additionally when**
- Art進捗の永続化形式を変更する場合はSaveを確認する。
- Ability解放や熟練ポイント補正を変更する場合はAbility / Modifiersを確認する。

---

## Skill

**Primary specification**
- `docs/specifications/skill.md`
- `docs/specifications/progression.md`

**Primary code**
- `Assets/Scripts/Domain/Progression/`
- `Assets/Scripts/Gameplay/Progression/SkillProgressionService.cs`
- `Assets/Scripts/Gameplay/Progression/`

**Composition / Integration**
- `Assets/Scripts/Gameplay/Modifiers/`

**Tests**
- `Assets/Tests/EditMode/`
- `Assets/Tests/PlayMode/`

**Usually related**
- Progression
- Modifiers
- Ability
- Art
- Evolution
- Save

**Read additionally when**
- Skill取得経路を変更する場合はRewardまたは取得元Featureを確認する。
- 補正対象を変更する場合はModifier Source利用側を確認する。

---

## Evolution

**Primary specification**
- `docs/specifications/evolution.md`
- `docs/specifications/progression.md`

**Primary code**
- `Assets/Scripts/Domain/Progression/`
- `Assets/Scripts/Gameplay/Progression/EvolutionProgressionService.cs`
- `Assets/Scripts/Gameplay/Progression/`

**Presentation**
- `Assets/Scripts/Presentation/UI/`

**Composition / Integration**
- `Assets/Scripts/Field/Prototype/`
- `Assets/Scripts/Gameplay/Modifiers/`

**Tests**
- `Assets/Tests/EditMode/`
- `Assets/Tests/PlayMode/`

**Usually related**
- Progression
- Skill
- Art
- Modifiers
- Input
- Save

**Read additionally when**
- Evolution選択UIやInput Contextを変更する場合はInput / Pauseを確認する。
- 形態表示を変更する場合はPresentationとPrototypeのPresenterを確認する。

---

## Progression

**Primary specification**
- `docs/specifications/progression.md`

**Primary code**
- `Assets/Scripts/Domain/Progression/`
- `Assets/Scripts/Gameplay/Progression/`

**Composition / Integration**
- `Assets/Scripts/Gameplay/Rewards/`
- `Assets/Scripts/Field/Prototype/PrototypeGameplayServices.cs`

**Tests**
- `Assets/Tests/EditMode/`
- `Assets/Tests/PlayMode/`

**Usually related**
- Art
- Skill
- Evolution
- Reward
- Save

**Read additionally when**
- Runtime Stateの保存対象を変更する場合はSaveを確認する。
- 取得元を追加する場合はProgression Serviceへ取得元固有条件を埋め込まず、取得元側のCompositionも確認する。

---

## Reward

**Primary specification**
- `docs/specifications/combat.md`
- `docs/specifications/progression.md`

**Primary code**
- `Assets/Scripts/Gameplay/Rewards/RewardService.cs`
- `Assets/Scripts/Gameplay/Rewards/`

**Composition / Integration**
- `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`

**Tests**
- `Assets/Tests/EditMode/`
- `Assets/Tests/PlayMode/`

**Usually related**
- Combat
- Progression

**Read additionally when**
- 撃破判定や重複付与防止を変更する場合はCombatの `DamageResult` / `DefeatContext` を確認する。
- 報酬からArt / Skill等を付与する場合はProgressionを確認する。

---

## Save

**Primary specification**
- `docs/specifications/save.md`

**Primary code**
- `Assets/Scripts/Domain/Save/`
- `Assets/Scripts/Core/Application/`

**Tests**
- `Assets/Tests/EditMode/`

**Usually related**
- Progression
- Art
- Skill
- Evolution

**Read additionally when**
- Save DTOを変更する場合はVersion / MigrationとRuntime State Mapperを同時に確認する。
- 保存先を実装する場合は `ISaveService` の外側にPlatform依存を置く。

---

## Pause

**Primary specification**
- `docs/specifications/input.md`
- `docs/design/technical-design.md`

**Primary code**
- `Assets/Scripts/Core/Application/GamePauseController.cs`
- `Assets/Scripts/Core/Input/`

**Presentation**
- `Assets/Scripts/Presentation/UI/`

**Tests**
- `Assets/Tests/PlayMode/`

**Usually related**
- Input
- Evolution

**Read additionally when**
- 新しいモーダルUIを追加する場合はInput ContextとTime Scaleの所有権競合を確認する。

---

## Dodge

**Primary specification**
- `docs/specifications/input.md`
- `docs/design/technical-design.md`

**Primary code**
- `Assets/Scripts/Gameplay/Characters/`

**Composition / Integration**
- `Assets/Scripts/Core/Input/`

**Tests**
- `Assets/Tests/PlayMode/`

**Usually related**
- Input
- Character movement

**Read additionally when**
- 移動方法や無敵・Combat連携を追加する場合はCombatとCharacter movementの責務境界を確認する。

---

## Quest

**Status**
- Quest / ObjectiveのRuntime基盤と最初の訓練Questは実装済み。
- 専用の `docs/specifications/quest.md` は現時点では存在しない。

**Primary specification / design**
- `docs/design/extension-foundations.md`

**Primary code**
- `Assets/Scripts/Domain/Quests/QuestProgressState.cs`
- `Assets/Scripts/Gameplay/Quests/QuestDefinition.cs`
- `Assets/Scripts/Gameplay/Quests/QuestProgressionService.cs`

**Composition / Integration**
- `Assets/Scripts/Gameplay/Events/GameplayEventHub.cs`
- `Assets/Scripts/Field/Prototype/PrototypeGameplayServices.cs`
- `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- `Assets/Resources/Settings/Gameplay/FirstTrainingQuest.asset`

**Tests**
- `Assets/Tests/PlayMode/QuestProgressionServiceTests.cs`

**Usually related**
- Gameplay Events
- Combat
- Dialogue

**Read additionally when**
- 新しいObjectiveイベントを追加する場合はイベント発生元FeatureとCompositionを確認する。
- Quest固有処理をCombatやDialogueへ直接追加せず、Gameplay Event境界を確認する。

---

## Spawning

**Status**
- 汎用 `SpawnLifecycle<T>` とPrototype訓練用スライムのFactory / Compositionは実装済み。

**Primary specification / design**
- `docs/design/extension-foundations.md`

**Primary code**
- `Assets/Scripts/Gameplay/Spawning/SpawnLifecycle.cs`

**Composition / Integration**
- `Assets/Scripts/Field/Prototype/PrototypeCombatDummyFactory.cs`
- `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`

**Tests**
- `Assets/Tests/PlayMode/SpawnLifecycleTests.cs`
- `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`

**Usually related**
- Combat
- Interaction
- Quest

**Read additionally when**
- 敵生成を正式システムへ拡張する場合は具体Factory / Spawn Pointと汎用Lifecycleを分離する。

---

## Enemy AI

**Status**
- 未実装。現時点でEnemy AI専用の実装パスは存在しない。

**Primary planning**
- `docs/development/roadmap.md`

**Usually related**
- Ability
- Combat
- Characters

**Read additionally when**
- 実装を開始する場合は、AI判断から既存 `AbilityController` を利用し、CombatやAbility ExecutorへAI固有判断を埋め込まない方針を確認する。
