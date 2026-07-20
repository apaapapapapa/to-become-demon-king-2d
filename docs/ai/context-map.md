# AI Context Map

AIエージェントが変更対象に必要な仕様・コード・テストへ最短で到達するための索引です。仕様、設計、実装状況をこの文書へ複製しません。

## 使い方

1. 対象Featureの `Spec` と `Code` を先に読む。
2. UIやFeature間配線を変える場合だけ `Integration` を追加で読む。
3. `Related` は必要な場合だけ確認する。
4. テストは記載ファイル、または対象クラス名・Feature名で探す。

## Input

- **Spec:** `docs/specifications/input.md`
- **Code:** `Assets/Scripts/Core/Input/`
- **Integration:** `Assets/Scripts/Gameplay/Abilities/`, `Assets/Scripts/Field/Prototype/`
- **Tests:** `Assets/Tests/PlayMode/`
- **Related:** Ability, Interaction, Pause, Dodge, Evolution

## Field Physics / Movement

- **Design:** `docs/design/technical-design.md#3dフィールド座標`, `docs/design/technical-design.md#移動と物理`
- **Code:** `Assets/Scripts/Gameplay/Characters/CharacterPhysicsBody3D.cs`, `Assets/Scripts/Gameplay/Characters/CharacterPlanarMotor.cs`, `Assets/Scripts/Gameplay/Characters/CharacterDodge.cs`, `Assets/Scripts/Core/Math/FieldSpace3D.cs`
- **Integration:** `Assets/Scripts/Field/Prototype/CollisionMapBuilder.cs`, `Assets/Scripts/Field/Prototype/PrototypePlayerSpawner.cs`, `Assets/Scripts/Field/Prototype/PrototypeTilemapContext.cs`
- **Tests:** `Assets/Tests/PlayMode/FieldPhysics3DPlayModeTests.cs`
- **Related:** Input, Dodge, Combat, Interaction, Enemy AI

## Interaction

- **Spec:** `docs/specifications/interaction.md`
- **Code:** `Assets/Scripts/Gameplay/Interaction/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeNpcInteractable.cs`
- **Tests:** `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`
- **Related:** Input, Dialogue

## Dialogue

- **Spec:** `docs/specifications/dialogue.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Gameplay/Dialogue/`
- **Presentation:** `Assets/Scripts/Presentation/UI/DialogueLogView.cs`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeNpcInteractable.cs`, `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- **Tests:** `Assets/Tests/PlayMode/LinearDialogueSequenceTests.cs`, `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`, `Assets/Tests/PlayMode/DialogueLogViewPlayModeTests.cs`
- **Related:** Interaction, Quest, Spawning

## Combat

- **Spec:** `docs/specifications/combat.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Gameplay/Combat/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeCombatDummy.cs`, `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Ability, Reward, Modifiers, Quest, Field Physics / Movement

## Content / Encyclopedia

- **Rule:** `docs/development/documentation-rules.md`
- **Code:** `Assets/Scripts/Gameplay/Content/`, `Assets/Scripts/Gameplay/Characters/Configuration/`, `Assets/Scripts/Gameplay/Abilities/Configuration/`, `Assets/Scripts/Gameplay/Progression/Configuration/`
- **Web:** `docs/.vitepress/theme/content-catalog.data.ts`, `docs/.vitepress/theme/ContentCatalog.vue`, `docs/.vitepress/theme/RuntimeContentHeader.vue`, `docs/database/`
- **Tests:** `Assets/Tests/EditMode/GameContentCatalogTests.cs`
- **Related:** Ability, Art, Skill, Evolution, Save

## Ability

- **Spec:** `docs/specifications/ability.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Gameplay/Abilities/`
- **Integration:** `Assets/Scripts/Gameplay/Combat/`, `Assets/Scripts/Gameplay/Progression/`, `Assets/Scripts/Core/Input/`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Combat, Art, Skill, Evolution, Input

## Art

- **Spec:** `docs/specifications/art.md`
- **Code:** `Assets/Scripts/Domain/Progression/`, `Assets/Scripts/Gameplay/Progression/`
- **Integration:** `Assets/Scripts/Gameplay/Abilities/`, `Assets/Scripts/Gameplay/Modifiers/`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Ability, Progression, Skill, Evolution, Save

## Skill

- **Spec:** `docs/specifications/skill.md`
- **Code:** `Assets/Scripts/Domain/Progression/`, `Assets/Scripts/Gameplay/Progression/`
- **Integration:** `Assets/Scripts/Gameplay/Modifiers/`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Progression, Modifiers, Ability, Art, Evolution, Save

## Evolution

- **Spec:** `docs/specifications/evolution.md`
- **Code:** `Assets/Scripts/Domain/Progression/`, `Assets/Scripts/Gameplay/Progression/`
- **Presentation:** `Assets/Scripts/Presentation/UI/`
- **Integration:** `Assets/Scripts/Field/Prototype/`, `Assets/Scripts/Gameplay/Modifiers/`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Progression, Skill, Art, Input, Save

## Progression

- **Spec:** `docs/specifications/progression.md`
- **Code:** `Assets/Scripts/Domain/Progression/`, `Assets/Scripts/Gameplay/Progression/`
- **Integration:** `Assets/Scripts/Gameplay/Rewards/`, `Assets/Scripts/Field/Prototype/PrototypeGameplayServices.cs`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Art, Skill, Evolution, Reward, Save

## Reward

- **Spec:** `docs/specifications/combat.md`, `docs/specifications/progression.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Gameplay/Rewards/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Combat, Progression

## Save

- **Spec:** `docs/specifications/save.md`
- **Code:** `Assets/Scripts/Domain/Save/`, `Assets/Scripts/Core/Application/`
- **Tests:** `Assets/Tests/EditMode/`
- **Related:** Progression, Art, Skill, Evolution, Content / Encyclopedia

## Pause

- **Spec:** `docs/specifications/input.md`
- **Code:** `Assets/Scripts/Core/Application/GamePauseController.cs`, `Assets/Scripts/Core/Input/`
- **Presentation:** `Assets/Scripts/Presentation/UI/`
- **Tests:** `Assets/Tests/PlayMode/`
- **Related:** Input, Evolution

## Dodge

- **Spec:** `docs/specifications/input.md`
- **Code:** `Assets/Scripts/Gameplay/Characters/`
- **Integration:** `Assets/Scripts/Core/Input/`
- **Tests:** `Assets/Tests/PlayMode/`
- **Related:** Input, Combat, Field Physics / Movement

## Quest

- **Spec:** `docs/specifications/quest.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Domain/Quests/`, `Assets/Scripts/Gameplay/Quests/`
- **Integration:** `Assets/Scripts/Gameplay/Events/GameplayEventHub.cs`, `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- **Tests:** `Assets/Tests/PlayMode/QuestProgressionServiceTests.cs`
- **Related:** Gameplay Events, Combat, Dialogue

## Spawning

- **Spec:** `docs/specifications/spawning.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Gameplay/Spawning/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeCombatDummyFactory.cs`, `Assets/Scripts/Field/Prototype/PrototypeTrainingAreaCoordinator.cs`
- **Tests:** `Assets/Tests/PlayMode/SpawnLifecycleTests.cs`, `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`
- **Related:** Combat, Interaction

## Enemy AI

- **Spec:** `docs/specifications/enemy-ai.md`
- **Code:** `Assets/Scripts/Gameplay/AI/`
- **Configuration:** `Assets/Resources/Settings/Gameplay/TrainingSlimeAi.asset`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeGameplayFeatureInstaller.cs`, `Assets/Scripts/Field/Prototype/PrototypeCombatDummy.cs`
- **Tests:** `Assets/Tests/PlayMode/EnemyAiPlayModeTests.cs`
- **Related:** Ability, Combat, Characters, Field Physics / Movement, Spawning
