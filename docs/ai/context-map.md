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
- **Related:** Ability, Interaction, Pause, Dodge, Evolution, Title Screen

## Title Screen / Game Start

- **Spec:** `docs/specifications/title-screen.md`, `docs/specifications/save.md#save-slot--new-game--continue境界`
- **Code:** `Assets/Scripts/Field/Prototype/PrototypeTitleScreenController.cs`, `Assets/Scripts/Core/Application/GameStartSaveService.cs`
- **Presentation:** `Assets/Scripts/Presentation/UI/PrototypeTitleScreenView.cs`
- **Integration:** `Assets/Scripts/FieldBootstrap.cs`, `Assets/Scripts/Field/Prototype/PrototypeApplicationInstaller.cs`, `Assets/Scripts/Core/Application/LocalSaveSlotStore.cs`, `Assets/Scripts/Core/Input/PlayerInputReader.cs`
- **Tests:** `Assets/Tests/EditMode/GameStartSaveServiceTests.cs`, `Assets/Tests/PlayMode/TitleScreenPlayModeTests.cs`
- **Related:** Save, Input, Field / World Composition, Field Transition

## Field / World Composition

- **Design:** `docs/design/technical-design.md#起動とcomposition`, `docs/design/technical-design.md#scene--tilemapと3d-physics`
- **Core Boundary:** `Assets/Scripts/Field/Composition/FieldComposition.cs`
- **Definition / Catalog:** `Assets/Scripts/Field/Prototype/PrototypeFieldDefinition.cs`, `Assets/Scripts/Field/Prototype/Configuration/PrototypeApplicationSettings.cs`
- **Runtime Composition:** `Assets/Scripts/Field/Prototype/PrototypeFieldComposition.cs`, `Assets/Scripts/Field/Prototype/PrototypeWorldBuilder.cs`
- **Scene Entry:** `Assets/Scripts/FieldBootstrap.cs`, `Assets/Scripts/Field/Prototype/PrototypeApplicationInstaller.cs`
- **Session / Save:** `Assets/Scripts/Field/Prototype/PrototypeGameSession.cs`, `Assets/Scripts/Field/Prototype/PrototypeSaveSession.cs`, `Assets/Scripts/Domain/Save/SaveData.cs`
- **Tests:** `Assets/Tests/EditMode/FieldCompositionTests.cs`, `Assets/Tests/EditMode/GameSessionSaveEditModeTests.cs`, `Assets/Tests/EditMode/LocalSaveTests.cs`
- **Related:** Field Transition, Field Physics / Movement, Save, Title Screen, Content / Encyclopedia, Spawning, Quest

## Field Transition

- **Spec:** `docs/specifications/field-transition.md`, `docs/specifications/save.md#world状態`
- **Definition / Routes:** `Assets/Scripts/Field/Prototype/PrototypeFieldDefinition.cs`
- **Application Boundary:** `Assets/Scripts/Field/Prototype/PrototypeFieldTransitionService.cs`, `Assets/Scripts/Field/Prototype/PrototypeApplicationInstaller.cs`
- **Interaction:** `Assets/Scripts/Field/Prototype/PrototypeFieldTransition.cs`
- **Scene Runtime:** `Assets/Scripts/Field/Prototype/PrototypeFieldSceneRuntime.cs`, `Assets/Scripts/Field/Prototype/PrototypeTilemapContext.cs`
- **Session State / Save:** `Assets/Scripts/Field/Prototype/PrototypeGameSession.cs`, `Assets/Scripts/Field/Prototype/PrototypeLocalSaveCoordinator.cs`
- **Tests:** `Assets/Tests/EditMode/FieldCompositionTests.cs`, `Assets/Tests/EditMode/FieldTransitionSaveEditModeTests.cs`, `Assets/Tests/PlayMode/FieldTransitionPlayModeTests.cs`
- **Related:** Field / World Composition, Save, Input, Modal UI / Pause, Quest, Ability, Progression

## Field Physics / Movement

- **Spec:** `docs/specifications/movement.md`
- **Design:** `docs/design/technical-design.md`
- **Code:** `Assets/Scripts/Gameplay/Characters/CharacterPhysicsBody3D.cs`, `Assets/Scripts/Gameplay/Characters/CharacterPlanarMotor.cs`, `Assets/Scripts/Gameplay/Characters/CharacterDodge.cs`, `Assets/Scripts/Core/Math/FieldSpace3D.cs`
- **Integration:** `Assets/Scripts/Field/Prototype/CollisionMapBuilder.cs`, `Assets/Scripts/Field/Prototype/PrototypePlayerSpawner.cs`, `Assets/Scripts/Field/Prototype/PrototypeTilemapContext.cs`
- **Tests:** `Assets/Tests/PlayMode/FieldPhysics3DPlayModeTests.cs`
- **Related:** Input, Dodge, Combat, Interaction, Enemy AI, Field Transition

## Interaction

- **Spec:** `docs/specifications/interaction.md`
- **Code:** `Assets/Scripts/Gameplay/Interaction/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeNpcInteractable.cs`, `Assets/Scripts/Field/Prototype/PrototypeFieldTransition.cs`
- **Tests:** `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`, `Assets/Tests/PlayMode/FieldTransitionPlayModeTests.cs`
- **Related:** Input, Dialogue, Field Transition

## Dialogue

- **Spec:** `docs/specifications/dialogue.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Gameplay/Dialogue/`
- **Presentation:** `Assets/Scripts/Presentation/UI/DialogueLogView.cs`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeNpcInteractable.cs`, `Assets/Scripts/Field/Prototype/TrainingQuestFlowController.cs`
- **Tests:** `Assets/Tests/EditMode/LinearDialogueSequenceTests.cs`, `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`, `Assets/Tests/PlayMode/DialogueLogViewPlayModeTests.cs`
- **Related:** Interaction, Quest, Spawning

## Combat

- **Spec:** `docs/specifications/combat.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Gameplay/Combat/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeCombatDummy.cs`, `Assets/Scripts/Field/Prototype/PrototypeGameplayFeatureInstaller.cs`, `Assets/Scripts/Field/Prototype/TrainingDummyEventBridge.cs`
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
- **Related:** Combat, Art, Skill, Evolution, Input, Field Transition

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
- **Integration:** `Assets/Scripts/Field/Prototype/`, `Assets/Scripts/Gameplay/Modifiers/`, `Assets/Scripts/Core/Application/ModalUiCoordinator.cs`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Progression, Skill, Art, Input, Save, Modal UI, Field Transition

## Progression

- **Spec:** `docs/specifications/progression.md`
- **Code:** `Assets/Scripts/Domain/Progression/`, `Assets/Scripts/Gameplay/Progression/`
- **Integration:** `Assets/Scripts/Gameplay/Rewards/`, `Assets/Scripts/Field/Prototype/PrototypeGameplayServices.cs`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Art, Skill, Evolution, Reward, Save, Field Transition

## Reward

- **Spec:** `docs/specifications/combat.md`, `docs/specifications/progression.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Gameplay/Rewards/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeGameplayFeatureInstaller.cs`
- **Tests:** `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- **Related:** Combat, Progression

## Save

- **Spec:** `docs/specifications/save.md`, `docs/specifications/field-transition.md#save--continue`
- **Design:** `docs/design/technical-design.md#local-save`
- **Code:** `Assets/Scripts/Domain/Save/`, `Assets/Scripts/Core/Application/SaveBoundary.cs`, `Assets/Scripts/Core/Application/JsonFileSaveService.cs`, `Assets/Scripts/Core/Application/LocalSaveSlotStore.cs`, `Assets/Scripts/Gameplay/Abilities/AbilityLoadoutSaveMapper.cs`, `Assets/Scripts/Gameplay/Quests/QuestProgressSaveMapper.cs`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeGameSession.cs`, `Assets/Scripts/Field/Prototype/PrototypeSaveSession.cs`, `Assets/Scripts/Field/Prototype/PrototypeLocalSaveCoordinator.cs`, `Assets/Scripts/Field/Prototype/PrototypeApplicationInstaller.cs`, `Assets/Scripts/Field/Prototype/PrototypeTitleScreenController.cs`, `Assets/Scripts/Field/Prototype/PrototypeFieldTransitionService.cs`
- **Tests:** `Assets/Tests/EditMode/LocalSaveTests.cs`, `Assets/Tests/EditMode/SaveSlotTests.cs`, `Assets/Tests/EditMode/GameStartSaveServiceTests.cs`, `Assets/Tests/EditMode/GameSessionSaveEditModeTests.cs`, `Assets/Tests/EditMode/FieldTransitionSaveEditModeTests.cs`, `Assets/Tests/PlayMode/TitleScreenPlayModeTests.cs`, `Assets/Tests/PlayMode/FieldTransitionPlayModeTests.cs`
- **Related:** Title Screen, Field / World Composition, Field Transition, Progression, Art, Skill, Evolution, Quest, Ability, Content / Encyclopedia

## Modal UI / Pause

- **Spec:** `docs/specifications/input.md`
- **Design:** `docs/design/technical-design.md#ui`
- **Code:** `Assets/Scripts/Core/Application/ModalUiCoordinator.cs`, `Assets/Scripts/Core/Application/GamePauseController.cs`, `Assets/Scripts/Gameplay/Progression/EvolutionSelectionController.cs`, `Assets/Scripts/Gameplay/Abilities/AbilityLoadoutSelectionController.cs`
- **Presentation:** `Assets/Scripts/Presentation/UI/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeApplicationInstaller.cs`
- **Tests:** `Assets/Tests/PlayMode/ModalUiCoordinatorPlayModeTests.cs`, `Assets/Tests/PlayMode/DodgeAndPausePlayModeTests.cs`, `Assets/Tests/PlayMode/GameplayAndCameraPlayModeTests.cs`, `Assets/Tests/PlayMode/FieldTransitionPlayModeTests.cs`
- **Related:** Input, Evolution, Ability, Field Transition

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
- **Presentation:** `Assets/Scripts/Presentation/UI/QuestTrackerView.cs`
- **Integration:** `Assets/Scripts/Gameplay/Events/GameplayEventHub.cs`, `Assets/Scripts/Field/Prototype/TrainingQuestFlowController.cs`, `Assets/Scripts/Field/Prototype/PrototypeUiInstaller.cs`, `Assets/Scripts/Field/Prototype/PrototypeGameSession.cs`
- **Tests:** `Assets/Tests/EditMode/QuestProgressionServiceTests.cs`, `Assets/Tests/EditMode/QuestTrackerPresentationTests.cs`, `Assets/Tests/PlayMode/QuestTrackerViewPlayModeTests.cs`, `Assets/Tests/PlayMode/FieldTransitionPlayModeTests.cs`
- **Related:** Gameplay Events, Combat, Dialogue, Interaction, Field Transition

## Spawning

- **Spec:** `docs/specifications/spawning.md`
- **Boundary:** `docs/design/feature-boundaries.md`
- **Code:** `Assets/Scripts/Gameplay/Spawning/`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeCombatDummyFactory.cs`, `Assets/Scripts/Field/Prototype/PrototypeGameplayFeatureInstaller.cs`, `Assets/Scripts/Field/Prototype/TrainingQuestFlowController.cs`, `Assets/Scripts/Field/Prototype/TrainingDummyEventBridge.cs`
- **Tests:** `Assets/Tests/EditMode/SpawnLifecycleTests.cs`, `Assets/Tests/PlayMode/PrototypeNpcInteractablePlayModeTests.cs`, `Assets/Tests/PlayMode/TrainingAreaCompositionPlayModeTests.cs`
- **Related:** Combat, Interaction

## Enemy AI

- **Spec:** `docs/specifications/enemy-ai.md`
- **Code:** `Assets/Scripts/Gameplay/AI/`
- **Configuration:** `Assets/Resources/Settings/Gameplay/TrainingSlimeAi.asset`
- **Integration:** `Assets/Scripts/Field/Prototype/PrototypeGameplayFeatureInstaller.cs`, `Assets/Scripts/Field/Prototype/PrototypeCombatDummy.cs`
- **Tests:** `Assets/Tests/PlayMode/EnemyAiPlayModeTests.cs`
- **Related:** Ability, Combat, Characters, Field Physics / Movement, Spawning
