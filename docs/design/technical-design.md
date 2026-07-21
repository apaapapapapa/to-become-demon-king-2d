# 技術設計

## 目的

この文書はUnity固有の基準環境と実装方式を定義します。レイヤー責務は [アーキテクチャ](./architecture.md)、Feature間の接続は [Feature間の責務境界](./feature-boundaries.md)、ゲーム上の振る舞いは各 [仕様書](../specifications/) を参照してください。

## 基準環境

- Unity Editor: `6000.5.4f1`
- Universal Render Pipeline（URP）
- Unity Input System
- Isometric Tilemap / Canvas（uGUI）
- Rigidbody / Collider（3D Physics）
- Unity Test Framework
- `DemonKing.Domain`: Unity非依存Assembly
- `DemonKing.Runtime`: Unity Runtime Assembly

## 起動とComposition

```text
Prototype.unity
  -> FieldBootstrap
  -> PrototypeProjectAssets
     |- PrototypeApplicationSettings
     |- CharacterDefinition
     |- TrainingScenarioDefinition
     |- Progression Pickup definitions
     `- World / UI Asset references
  -> PrototypeApplicationInstaller
     |- PrototypeSaveSession
     |- PrototypeWorldBuilder
     |- GamePauseController
     |- PrototypeUiInstaller
     `- PrototypeLocalSaveCoordinator
```

`FieldBootstrap` は最小のエントリーポイントとします。`PrototypeProjectAssets` はPrototype全体のComposition Manifestです。Quest、状態別Dialogue、Enemy AI、Reward、Progression Grantのように同じ縦切りループで変更される参照は `TrainingScenarioDefinition` へ集約します。訓練シナリオ外のフィールド取得物は `PrototypeProgressionPickupDefinition` としてProjectAssetsへ保持します。

訓練エリアでは `PrototypeGameplayFeatureInstaller` がScenario Definitionを受け取り、次の責務へ接続します。

- `TrainingQuestFlowController`: NPC会話選択、Quest受注・報告、Completion Grant、再訓練要求
- `TrainingDummyEventBridge`: Dummy DefeatをGameplay Eventへ変換
- `SpawnLifecycle<T>`: Dummy生成・復元・Current管理
- `PrototypeGameplayServicesFactory`: Gameplay Event HubからQuest Progressionへの共通配線

撃破Rewardの付与はCombat構成側に置き、Quest Flowへ持ち込みません。

## Scene / Tilemapと3D Physics

正規SceneはCamera、Grid、Bootstrapを保持する最小Composition Rootとします。`PrototypeTilemapContext` は `Ground` / `Collision` / `Props` / `Foreground` を解決・補完します。

表示は2Dアイソメトリック、物理は3Dです。`Collision` Tilemapは衝突セルのマーカーで、`CollisionMapBuilder` が3D `BoxCollider` へ変換します。`TilemapCollider2D` は使用しません。

フィールド空間は `X / Y = 平面`、`Z = Elevation` とし、軸解釈は `FieldSpace3D` に集約します。`CharacterPhysicsBody3D` の `Rigidbody` / `CapsuleCollider` をキャラクター物理の唯一のSource of Truthとします。Jump / Fall / Flightは `CharacterElevationMotor`、平面移動は `CharacterPlanarMotor`、Dodgeは `CharacterDodge` が担当します。

有限高さ障害物はZ方向へ厚みを持つ3D Colliderとして表現し、Collider同士が高さ方向で重ならない場合は特別な例外判定なしで上空通過できます。

## Input / Ability Loadout

`PlayerControls.inputactions` はGameplayとUIのAction Mapを分離し、`PlayerInputReader` がInput Contextを切り替えます。

Ability入力は物理ActionとAbility IDを直接結び付けません。

```text
Input System Action
  -> PlayerInputReader
  -> AbilitySlot
  -> AbilityLoadout (AbilitySlot -> AbilityId)
  -> PlayerAbilityInput
  -> AbilityController
```

`AbilityLoadout` をプレイヤー個体のRuntime割当のSource of Truthとします。`Primary` は基本攻撃等の予約枠、`Action1` から `Action4` はRuntime選択UIから差し替え可能な枠です。初期Loadoutは `CharacterDefinition` と `CharacterProgressionState` の両方から構築し、未習得ArtのAbilityを先回りして割り当てません。

Loadout選択は次の境界で構成します。

```text
CharacterDefinition + CharacterProgressionState
  -> AbilityLoadoutMenuProjection
  -> AbilityLoadoutSelectionController
  -> AbilityLoadout
  -> AbilityLoadoutMenuView
```

- `AbilityLoadoutMenuProjection`: 取得済みArtの現在Rankで解放済みAbilityと取得済み受動SkillをuGUI非依存の表示要素へ変換する
- `AbilityLoadoutSelectionController`: モーダル開閉、候補・Action Slot選択、Runtime Loadout更新を担当する
- `AbilityLoadoutMenuView`: 選択状態と現在のAction1〜4割当をuGUIへ反映する
- Art由来AbilityだけをAction Slotへ割当可能とし、現在のSkillは受動Modifier要素なので状態確認用に表示するだけとする
- 同じAbilityを複数Action Slotへ重複配置せず、別Slotへの割当は移動として扱う

Art / Skill固有のInput Actionは増やしません。新しい能動Abilityが増えても物理入力は論理 `AbilitySlot` を維持します。AIはPlayer Loadoutを経由せず `AbilityController` を直接利用します。

Jump / Flight入力は `PlayerElevationInput` が `CharacterElevationMotor` の論理操作へ変換します。具体的なBindingは [入力仕様](../specifications/input.md) を参照してください。

## Content Catalog

静的Contentは `IGameContentDefinition` を共通契約とします。子Contentを持つDefinitionは `IGameContentContainer` で参照先を公開し、`GameContentDefinitionCollector` がRootから再帰収集します。`GameContentCatalog` は具体的なCharacter / Art / Skill / Evolution構造を知りません。

同じDefinitionインスタンスへ複数経路から到達した場合は一度だけ登録し、異なるDefinitionインスタンスが同一Stable Content IDを使用した場合は設定エラーとします。Prototypeでは `CharacterDefinition` をRootとし、Character配下とArt配下のContentを同じ経路で収集します。

追加した `art.magic.arcane_bolt`、`ability.magic.arcane_bolt`、`skill.magic.mana_flow` もPlayer Character配下へ登録し、専用の手動Catalog登録経路は追加しません。

## Progression Acquisition / Field Pickups

Art / Skillの取得は取得元ごとに専用Progression処理を実装せず、既存 `ProgressionAcquisitionService` を共通境界とします。

```text
PrototypeProjectAssets
  -> PrototypeProgressionPickupDefinition
  -> PrototypeProgressionPickupInstaller
  -> ProgressionGrantInteractable : IInteractable
  -> ProgressionAcquisitionService
  -> ArtProgressionController / SkillProgressionController
```

- `PrototypeProgressionPickupDefinition`: Prototype固有の表示名、配置、色、`ProgressionGrantDefinition` 参照を保持する
- `PrototypeProgressionPickupInstaller`: World composition時に取得物を生成し、既存Acquisition Serviceを注入する
- `ProgressionGrantInteractable`: Gameplay側の汎用一度きりInteraction。具体的なArt / Skill、配置、見た目を知らない
- `ProgressionGrantDefinition`: 付与対象Art / Skillだけを保持し、取得条件や配置を持たない

フィールド取得物のRuntime消費状態は `ProgressionGrantConsumptionState` をSource of Truthとし、Save Version 3の `WorldSaveData.consumedProgressionGrantIds` へStable Grant IDで保存します。ロード済みの消費状態を `PrototypeProgressionPickupInstaller` へ渡し、取得済みGrantは再配置しません。Art / Skillそのものの取得状態は既存 `CharacterProgressionState` / Save DTO境界で別途復元します。

## Local Save

PrototypeのローカルSaveは `JsonFileSaveService`、`PrototypeSaveSession`、各Save Mapper、`PrototypeLocalSaveCoordinator` をComposition Rootで接続します。`PrototypeSaveSession` が起動時にSave Version MigrationとRuntime State復元を行い、`PrototypeLocalSaveCoordinator` がProgression、Ability Loadout、Quest、World消費状態を `GameSaveData` へ集約して、Runtime構築完了直後・15秒ごと・Pause・Quit時に保存します。

Gameplay Featureは具体的な保存先を参照しません。具体的な永続化対象と復元時の扱いは [セーブ仕様](../specifications/save.md) を参照してください。

## Combat / Interaction / AI

Interaction、近接攻撃、Projectile命中判定は3D Physics Queryのみを使用します。敵AIは移動を `CharacterPhysicsBody3D`、攻撃を `AbilityController` へ委譲し、AI自身へ物理・ダメージ処理を重複実装しません。具体的なAI仕様は [敵AI仕様](../specifications/enemy-ai.md) を参照してください。

## UI

本番UI基盤はCanvas（uGUI）です。Viewは表示を担当し、ゲーム状態の変更主体にしません。

Quest Trackerは次の境界で構成します。

- `QuestTrackerProjection`: Quest Definition / Runtime StateをuGUI非依存の表示Modelへ変換する
- `QuestTrackerSelector`: 手動追跡状態を持たず、現在のQuest状態だけから初期表示対象を決定する
- `QuestTrackerView`: `QuestProgressionService` を購読し、選択された表示Modelを常設Trackerへ反映する
- `QuestNotificationFormatter`: Questイベントを通知文へ変換する
- `QuestNotificationView`: 一時通知のuGUI表示とRealtime基準の通知寿命を管理する

常設Trackerの表示ポリシーと一時通知の寿命を分離し、通知Coroutineの変更が常設Trackerへ波及しない構造とします。プレイヤーが追跡Questを手動選択する仕様が追加されるまでは、状態保持型 `QuestTrackingService` を導入しません。具体的な表示ルールは [Quest仕様](../specifications/quest.md) を参照してください。

EvolutionメニューとAbility Loadoutメニューは同じInput Context原則に従うモーダルUIです。開いている間はUI Contextと停止Time Scaleを所有し、確定またはキャンセルで直前状態へ戻します。別モーダルがUI Contextを所有している場合は重ねて開きません。

## ScriptableObject / Resources

静的コンテンツ定義、バランス値、Asset参照はScriptableObject Definitionとして管理します。Definition / Runtime State / Save DTOの責務分離は [アーキテクチャ](./architecture.md) を参照してください。

Resourcesは少数の起動入口や互換用途に限定し、必要性が発生した段階でAddressablesを検討します。

## Editorツール

`PrototypeProjectAssetsAutoRepair` はEditor起動時に参照状態を検証しますが自動書き換えは行いません。参照を書き換えるRepairは明示的なEditorメニュー操作時だけ実行します。シナリオ内部Contentは `TrainingScenarioDefinition` 自身をSource of Truthとします。

## テスト

テスト種別はFeature名やファイルサイズではなく、Unity Runtimeの実行要件で分類します。

EditModeで検証する対象:

- Unity Runtimeのフレーム進行を必要としないRuntime State / Service状態遷移
- Content収集、共有参照の重複排除、Stable Content ID衝突、Ability LoadoutのSlot解決
- Ability LoadoutのRuntime進捗初期化、取得済みArt / Skillの表示Projection
- 追加Runtime ContentとProgression Pickup Definition / Grantの設定整合性
- Save Migration、JSON File round trip、Ability Loadout / Quest / World Save Mapper
- Quest TrackerのProjection / Selector / Notification Formatter
- `LinearDialogueSequence`、`SpawnLifecycle<T>` の純粋ロジック
- フレーム進行を必要としないScriptableObject Definition検証

PlayModeで検証する対象:

- `MonoBehaviour` LifecycleとEvent購読・解除
- Coroutine / Realtime時間経過
- Scene、Input Context、3D Physics、移動
- Runtime生成したuGUI
- Jump / Fall / Flight、Combat / Interaction、Enemy AI
- Progression Grant InteractionからArt / Skill Runtime Stateまでの取得統合
- Training Quest Flow、Dummy Defeat Event Bridge、Spawn / Interaction / Combatをまたぐ縦切り統合

Quest UIのPlayModeテストは、uGUI階層生成、Questイベント購読、`QuestNotificationView` の通知寿命といったRuntime統合へ集中させ、状態文言・表示対象選択などの純粋な表示ポリシーはEditModeで検証します。Ability Loadoutも候補生成と割当可否はEditMode、Runtime uGUIとInput Context統合はPlayMode側の責務とします。CIでは引き続きEditMode / PlayModeの両方を実行します。

## Platform実装

Platform固有SDKの隔離方針は [アーキテクチャ](./architecture.md#platform境界) を参照してください。
