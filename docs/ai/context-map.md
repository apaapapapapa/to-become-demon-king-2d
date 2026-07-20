# AI Context Map

AIエージェントが変更対象に必要な仕様・コード・テストへ最短で到達するための索引です。詳細仕様や設計判断のSource of Truthではありません。

## 使い方

1. 対象Featureの `Spec` と `Code` を先に読む。
2. UIやFeature間配線を変える場合だけ `Integration` を追加で読む。
3. `Related` は必要な場合だけ確認する。
4. テストは記載ファイル、または `Assets/Tests/EditMode/` / `Assets/Tests/PlayMode/` から対象クラス名で探す。

## Input

- **Spec:** `docs/specifications/input.md`
- **Code:** `Assets/Scripts/Core/Input/`
- **Integration:** `Assets/Scripts/Gameplay/Abilities/`, `Assets/Scripts/Field/Prototype/`
- **Tests:** `Assets/Tests/PlayMode/`
- **Related:** Ability, Interaction, Pause, Dodge, Evolution
- **Extra:** Binding変更時はInput Actionsアセット、モーダル入力変更時はPause / EvolutionのInput Contextも確認する。

## Interaction

- **Spec:** `docs/specifications/interaction.md`
- **Code:** `Assets/Scripts/Gameplay/Interaction/PlayerInteractor.cs`, `Assets/Scripts/Gameplay/Interaction/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeNpcInteractable.cs`, `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- **Tests:** `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`
- **Related:** Input, Dialogue
- **Extra:** Combat / Spawn / Questへの通知変更時だけ対象FeatureとPrototype Compositionを追加確認する。

## Dialogue

- **Spec:** `docs/specifications/interaction.md`, `docs/design/extension-foundations.md`
- **Code:** `Assets/Scripts/Gameplay/Dialogue/DialogueDefinition.cs`, `Assets/Scripts/Gameplay/Dialogue/LinearDialogueSequence.cs`, `Assets/Scripts/Gameplay/Dialogue/DialogueLog.cs`
- **Presentation:** `Assets/Scripts/Presentation/UI/DialogueLogView.cs`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeNpcInteractable.cs`, `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`, `Assets/Resources/Settings/Gameplay/ApprenticeMageDialogue.asset`
- **Tests:** `Assets/Tests/PlayMode/LinearDialogueSequenceTests.cs`, `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`, `Assets/Tests/PlayMode/DialogueLogViewPlayModeTests.cs`
- **Related:** Interaction, Gameplay Events
- **Extra:** 会話完了イベント変更時はQuest、NPC会話から再Spawnする流れを変更する場合はSpawningも確認する。

## Combat

- **Spec:** `docs/specifications/combat.md`
- **Code:** `Assets/Scripts/Gameplay/Combat/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeCombatDummy.cs`, `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Ability, Reward, Modifiers
- **Extra:** 撃破報酬はReward / Progression、Quest通知はGameplay Events / QuestとCompositionを確認する。

## Ability

- **Spec:** `docs/specifications/ability.md`
- **Code:** `Assets/Scripts/Gameplay/Abilities/`
- **Integration:** `Assets/Scripts/Gameplay/Combat/`, `Assets/Scripts/Gameplay/Progression/`, `Assets/Scripts/Core/Input/`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Combat, Art, Skill, Evolution, Input
- **Extra:** 受動補正はModifiers、習得・熟練はArt / Progressionを確認する。

## Art

- **Spec:** `docs/specifications/art.md`, `docs/specifications/progression.md`
- **Code:** `Assets/Scripts/Domain/Progression/`, `Assets/Scripts/Gameplay/Progression/ArtProgressionService.cs`, `Assets/Scripts/Gameplay/Progression/`
- **Integration:** `Assets/Scripts/Gameplay/Abilities/`, `Assets/Scripts/Gameplay/Modifiers/`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Ability, Progression, Skill, Evolution, Save
- **Extra:** 永続化変更時はSave、Ability解放・熟練補正変更時はAbility / Modifiersを確認する。

## Skill

- **Spec:** `docs/specifications/skill.md`, `docs/specifications/progression.md`
- **Code:** `Assets/Scripts/Domain/Progression/`, `Assets/Scripts/Gameplay/Progression/SkillProgressionService.cs`, `Assets/Scripts/Gameplay/Progression/`
- **Integration:** `Assets/Scripts/Gameplay/Modifiers/`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Progression, Modifiers, Ability, Art, Evolution, Save
- **Extra:** 取得経路変更時はRewardまたは取得元Feature、補正対象変更時はModifier Source利用側を確認する。

## Evolution

- **Spec:** `docs/specifications/evolution.md`, `docs/specifications/progression.md`
- **Code:** `Assets/Scripts/Domain/Progression/`, `Assets/Scripts/Gameplay/Progression/EvolutionProgressionService.cs`, `Assets/Scripts/Gameplay/Progression/`
- **Presentation:** `Assets/Scripts/Presentation/UI/`
- **Integration:** `Assets/Scripts/Field/Prototype/`, `Assets/Scripts/Gameplay/Modifiers/`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Progression, Skill, Art, Input, Save
- **Extra:** 選択UIはInput / Pause、形態表示はPresentation / Prototype Presenterを確認する。

## Progression

- **Spec:** `docs/specifications/progression.md`
- **Code:** `Assets/Scripts/Domain/Progression/`, `Assets/Scripts/Gameplay/Progression/`
- **Integration:** `Assets/Scripts/Gameplay/Rewards/`, `Assets/Scripts/Field/Prototype/PrototypeGameplayServices.cs`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Art, Skill, Evolution, Reward, Save
- **Extra:** 保存対象変更時はSave。取得元固有条件はProgression Serviceへ埋め込まず取得元側も確認する。

## Reward

- **Spec:** `docs/specifications/combat.md`, `docs/specifications/progression.md`
- **Code:** `Assets/Scripts/Gameplay/Rewards/RewardService.cs`, `Assets/Scripts/Gameplay/Rewards/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Combat, Progression
- **Extra:** 重複付与防止は `DamageResult` / `DefeatContext`、Art / Skill付与はProgressionを確認する。

## Save

- **Spec:** `docs/specifications/save.md`
- **Code:** `Assets/Scripts/Domain/Save/`, `Assets/Scripts/Core/Application/`
- **Tests:** `Assets/Tests/EditMode/`
- **Related:** Progression, Art, Skill, Evolution
- **Extra:** DTO変更時はVersion / Migration / Mapper、保存先実装時は `ISaveService` の外側のPlatform境界を確認する。

## Pause

- **Spec:** `docs/specifications/input.md`, `docs/design/technical-design.md`
- **Code:** `Assets/Scripts/Core/Application/GamePauseController.cs`, `Assets/Scripts/Core/Input/`
- **Presentation:** `Assets/Scripts/Presentation/UI/`
- **Tests:** `Assets/Tests/PlayMode/`
- **Related:** Input, Evolution
- **Extra:** 新しいモーダルUIではInput ContextとTime Scaleの所有権競合を確認する。

## Dodge

- **Spec:** `docs/specifications/input.md`, `docs/design/technical-design.md`
- **Code:** `Assets/Scripts/Gameplay/Characters/`
- **Integration:** `Assets/Scripts/Core/Input/`
- **Tests:** `Assets/Tests/PlayMode/`
- **Related:** Input, Character movement
- **Extra:** 無敵やCombat連携を追加する場合だけCombatも確認する。

## Quest

- **Status:** Quest / Objective Runtime基盤と最初の訓練Questは実装済み。専用 `docs/specifications/quest.md` は未作成。
- **Spec / Design:** `docs/design/extension-foundations.md`
- **Code:** `Assets/Scripts/Domain/Quests/QuestProgressState.cs`, `Assets/Scripts/Gameplay/Quests/QuestDefinition.cs`, `Assets/Scripts/Gameplay/Quests/QuestProgressionService.cs`
- **Integration:** `Assets/Scripts/Gameplay/Events/GameplayEventHub.cs`, `Assets/Scripts/Field/Prototype/PrototypeGameplayServices.cs`, `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`, `Assets/Resources/Settings/Gameplay/FirstTrainingQuest.asset`
- **Tests:** `Assets/Tests/PlayMode/QuestProgressionServiceTests.cs`
- **Related:** Gameplay Events, Combat, Dialogue
- **Extra:** Quest固有処理をCombat / Dialogueへ直接追加せず、Gameplay Event境界とCompositionを確認する。

## Spawning

- **Status:** 汎用 `SpawnLifecycle<T>` とPrototype訓練用スライムのFactory / Compositionは実装済み。
- **Spec / Design:** `docs/design/extension-foundations.md`
- **Code:** `Assets/Scripts/Gameplay/Spawning/SpawnLifecycle.cs`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeCombatDummyFactory.cs`, `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- **Tests:** `Assets/Tests/PlayMode/SpawnLifecycleTests.cs`, `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`
- **Related:** Combat, Interaction, Quest

## Enemy AI

- **Status:** 未実装。Enemy AI専用の実装パスは存在しない。
- **Planning:** `docs/development/roadmap.md`
- **Related:** Ability, Combat, Characters
- **Extra:** 実装開始時はAI判断から既存 `AbilityController` を利用し、Combat / Ability ExecutorへAI固有判断を埋め込まない。
