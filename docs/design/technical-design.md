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
  -> PrototypeTitleScreenInstaller
     |- PlayerInputReader (UI Context)
     |- PrototypeTitleScreenView
     `- PrototypeTitleScreenController
        |- LocalSaveSlotStore
        `- SaveSlotId / Start Modeを選択
  -> selected ISaveService
  -> PrototypeApplicationInstaller (DontDestroyOnLoad)
     |- PrototypeGameSession
     |  |- PrototypeSaveSession
     |  |- CharacterProgressionState / QuestProgressionService
     |  |- PrototypeWorldBuilder
     |  |  `- PrototypePlayerSpawner
     |  |     `- PrototypePlayerRuntimeInstaller
     |  |- PrototypeGameSaveRestorer
     |  |- PrototypeGameSaveSnapshotProvider
     |  `- PrototypeLocalSaveCoordinator
     |- PrototypeFieldTransitionService
     |- PrototypeApplicationFieldBinder
     |- ModalUiCoordinator
     |- GamePauseController
     `- per-field PrototypeUiInstaller
```

`FieldBootstrap` は最小のエントリーポイントとし、`PrototypeProjectAssets` を解決した後はGameplay Runtimeを直接構築せずTitle Screenへ開始制御を委譲します。`PrototypeProjectAssets` はPrototype全体のComposition Manifestです。Quest、状態別Dialogue、Enemy AI、Reward、Progression Grantのように同じ縦切りループで変更される参照は `TrainingScenarioDefinition` へ集約します。訓練シナリオ外のフィールド取得物は `PrototypeProgressionPickupDefinition` としてProjectAssetsへ保持します。

`PrototypeTitleScreenController` はNew Game / Continue / Load Gameから開始方法とSave Slotを決定し、`LocalSaveSlotStore` から解決した `ISaveService` を `PrototypeApplicationInstaller` へ渡します。New Gameだけは `FreshGameSaveService` を通して既存SaveのRestoreを抑止します。Gameplay Runtimeはこの選択が完了するまで構築しません。

`PrototypeApplicationInstaller` はApplication全体の構築順序だけを調停します。Application RootはField Sceneより長い寿命を持つため `DontDestroyOnLoad` とし、Save読込からRuntime構築・復元・保存開始までの順序は `PrototypeGameSession` に委譲します。Save Slot IDや具体的なファイルパスはApplication境界で解決済みとし、Game Sessionへ持ち込みません。

`PrototypeGameSession` は `CharacterProgressionState`、`QuestProgressionService`、Progression Grant消費状態、Save CoordinatorをField Sceneより長く保持します。Player GameObject、Field固有Gameplay Service、Camera接続、Field UIはScene寿命として再構築します。`PrototypeApplicationFieldBinder` がField再構築後にPause / Evolution / Ability Loadout / UIを新Playerへ再接続します。

Player生成では `PrototypePlayerSpawner` をPrefab Instantiate、Spawn位置、`CharacterRuntimeContextHost` へのRuntime Context注入開始へ限定します。Gameplay Featureの詳細構築は `PrototypePlayerRuntimeInstaller` が次の単位へ分配します。

- Physics / Movement
- Combat / Ability
- Progression / Evolution
- Player Input / Loadout
- Presentation / Prototype Effect

Prefabで調整値・Authoring情報を持つComponentはPrefab側の契約とし、不足時は `RequireComponent` またはInstallerの明示エラーで検出します。Definition / Runtime Stateから決定できるStateful ComponentはRuntime Installerから注入できます。Spawnerが不足Componentを無条件補完してPrefab構成ミスを隠す方式には戻しません。

訓練エリアでは `PrototypeGameplayFeatureInstaller` がScenario Definitionを受け取り、次の責務へ接続します。

- `TrainingQuestFlowController`: NPC会話選択、Quest受注・報告、Completion Grant、再訓練要求
- `TrainingDummyEventBridge`: Dummy DefeatをGameplay Eventへ変換
- `SpawnLifecycle<T>`: Dummy生成・復元・Current管理
- `PrototypeGameplayServicesFactory`: Gameplay Event HubからQuest Progressionへの共通配線

撃破Rewardの付与はCombat構成側に置き、Quest Flowへ持ち込みません。

## Scene / Tilemapと3D Physics

正規SceneはCamera、Grid、Bootstrapを保持する最小Composition Rootとします。Runtime Composition型の追加Fieldは `PrototypeFieldSceneRuntime` が独立Sceneとして生成・Active化し、Field DefinitionのScene名をScene寿命の識別にだけ使用します。SaveやGameplay上の現在地はScene名 / Build IndexではなくStable `FieldLocation` をSource of Truthとします。

`PrototypeTilemapContext` とCamera解決はActive Field Scene内だけを探索します。遷移中に旧Fieldと新Fieldが同時にLoadされる短い区間があっても、旧SceneのGridやCameraを新Field Compositionが誤参照しないようにします。

表示は2Dアイソメトリック、物理は3Dです。`Collision` Tilemapは衝突セルのマーカーで、`CollisionMapBuilder` が3D `BoxCollider` へ変換します。`TilemapCollider2D` は使用しません。

フィールド空間は `X / Y = 平面`、`Z = Elevation` とし、軸解釈は `FieldSpace3D` に集約します。`CharacterPhysicsBody3D` の `Rigidbody` / `CapsuleCollider` をキャラクター物理の唯一のSource of Truthとします。Jump / Fall / Flightは `CharacterElevationMotor`、平面移動は `CharacterPlanarMotor`、Dodgeは `CharacterDodge` が担当します。

有限高さ障害物はZ方向へ厚みを持つ3D Colliderとして表現し、Collider同士が高さ方向で重ならない場合は特別な例外判定なしで上空通過できます。

### Field Transition / Scene寿命

`PrototypeFieldCatalog` はStable Field IDから `PrototypeFieldDefinition` とEntry Pointを解決します。Field出口は `PrototypeFieldTransitionInteractable` から `FieldLocation(fieldId, entryPointId)` だけを `PrototypeFieldTransitionService` へ要求し、Scene Load方式、Save、Player再生成を知りません。

Field遷移は次の順序で行います。

```text
Field Exit
  -> FieldLocationをCatalogで検証
  -> Player Input停止
  -> 現在Loadoutを一時Snapshot + 旧FieldをSave
  -> 遷移先SceneをActive化
  -> Field Definition / Entry PointからWorld・Playerを再構築
  -> Session所有Progression / Quest / Grant Stateを再接続
  -> Ability Loadoutを新Playerへ再適用
  -> Save Snapshot Providerを新Player / Field Locationへ再Bind
  -> Application UI / Inputを新Playerへ再Bind
  -> 新Field LocationをSave
  -> 旧Field SceneをUnload
```

Ability LoadoutはPlayer Component自体がScene寿命のため、Scene破棄前に `AbilityLoadoutSaveData` へ一時Snapshotします。ただしField間のRuntime State受け渡しをSaveファイル経由にするわけではなく、Game Session内の一時境界として利用します。Level / Art / Skill / Evolution / Quest / World persistent stateはGame Session所有Runtime Stateを直接次Fieldへ接続します。

具体的なField ID、遷移規則、Save / Continueの受入条件は [Field Transition仕様](../specifications/field-transition.md) を参照してください。

## Input / Ability Loadout

`PlayerControls.inputactions` はGameplayとUIのAction Mapを分離し、`PlayerInputReader` がInput Contextを切り替えます。Ability系Action名は論理Slotと同じ `Primary` / `Action1`〜`Action4` とします。

Ability入力は物理ActionとAbility IDを直接結び付けません。`PlayerInputReader` はInput System Actionを論理入力へ変換するだけとし、Ability入力は `AbilitySlotPressed` を単一イベント境界とします。

```text
Input System Action
  -> PlayerInputReader
  -> AbilitySlot
  -> AbilityLoadout (AbilitySlot -> AbilityId)
  -> PlayerAbilityInput
  -> AbilityController
```

`AbilityLoadout` をプレイヤー個体のRuntime割当のSource of Truthとします。`Primary` は基本攻撃等の予約枠、`Action1` から `Action4` は `AbilityLoadoutPolicy` が定義するユーザー編集可能枠です。編集Slotへ同じAbilityを再割当した場合は `AbilityLoadout` 自体が既存Slotから除去して新しいSlotへ移動させます。

初期Loadout、Menu、Selection、Save復元で必要な「現在割当可能なAbility」は `AbilityLoadoutEligibility` が `CharacterDefinition` と `CharacterProgressionState` から判定します。未習得ArtのAbilityを先回りして割り当てません。

```text
CharacterDefinition + CharacterProgressionState
  -> AbilityLoadoutEligibility
     |- AbilityLoadoutController (initial loadout)
     |- AbilityLoadoutMenuProjection
     |- AbilityLoadoutSelectionController
     `- AbilityLoadoutSaveMapper

AbilityLoadoutPolicy
  -> editable Action1..Action4
  -> Selection / Save / Runtime assignment
```

- `AbilityLoadoutEligibility`: 現在習得済みArtとRankから入力割当可能なAbilityを返す。表示文言やSave DTOを知らない
- `AbilityLoadoutPolicy`: 編集可能Slotと予約Slotのルールを一元管理する
- `AbilityLoadoutMenuProjection`: Eligibility結果と取得済み受動SkillをuGUI非依存の表示要素へ変換する
- `AbilityLoadoutSelectionController`: モーダル開閉、候補・Action Slot選択、Runtime Loadout更新を担当する
- `AbilityLoadoutSaveMapper`: 同じPolicy / Eligibilityを使い、不正Slot、未習得・削除済みAbility、重複Abilityを無視して復元する
- `AbilityLoadoutMenuView`: 選択状態と現在のAction1〜4割当をuGUIへ反映する
- Art由来AbilityだけをAction Slotへ割当可能とし、現在のSkillは受動Modifier要素なので状態確認用に表示するだけとする

Art / Skill固有のInput Actionは増やしません。新しい能動Abilityが増えても物理入力は論理 `AbilitySlot` を維持します。AIはPlayer Loadoutを経由せず `AbilityController` を直接利用します。

Jump / Flight入力は `PlayerElevationInput` が `CharacterElevationMotor` の論理操作へ変換します。具体的なBindingは [入力仕様](../specifications/input.md) を参照してください。

## Content Catalog

静的Contentは `IGameContentDefinition` を共通契約とします。子Contentを持つDefinitionは `IGameContentContainer` で参照先を公開し、`GameContentDefinitionCollector` がRootから再帰収集します。`GameContentCatalog` は具体的なCharacter / Art / Skill / Evolution構造を知りません。

同じDefinitionインスタンスへ複数経路から到達した場合は一度だけ登録し、異なるDefinitionインスタンスが同一Stable Content IDを使用した場合は設定エラーとします。Prototypeでは `CharacterDefinition` をRootとし、Character配下とArt配下のContentを同じ経路で収集します。

追加Runtime ContentもPlayer Character配下のDefinition参照から同じ再帰収集経路へ登録し、コンテンツごとの手動Catalog登録経路は追加しません。個別のStable Content ID・表示名・参照関係はRuntime DefinitionをSource of Truthとします。

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

PrototypeのローカルSaveは次の順序で構成します。

```text
LocalSaveSlotStore
  -> SaveSlotId -> ISaveService
  -> PrototypeSaveSession: Load / Migration / Player・World基礎Runtime State復元
  -> PrototypeWorldBuilder: World / Player Runtime構築
  -> PrototypeGameSaveRestorer: Ability Loadout / Questを構築済みRuntimeへ適用
  -> PrototypeGameSaveSnapshotProvider: Runtime State一式 -> GameSaveData
  -> PrototypeLocalSaveCoordinator: 保存タイミング
  -> selected ISaveService
```

`LocalSaveSlotStore` は3つの固定Slotから具体的なローカルファイルを解決します。Slot 1は既存 `save.json` を維持し、Slot 2 / 3は独立ファイルを使用します。表示用MetadataはGame Saveとは別sidecarへ保存し、最終Save日時、累積Play Time、Level、Current Fieldと `Empty` / `Ready` / `Corrupted` / `UnsupportedVersion` 状態をTitle / Load Game側へ提供します。MetadataはRuntime復元元にはしません。

`PrototypeWorldBuilder` はSave DTOを参照せず、Quest復元も行いません。`CharacterProgressionSaveMapper` は `CharacterProgressionState` と `PlayerSaveData` の変換だけを担当します。`PrototypeGameSaveSnapshotProvider` がProgression、Ability Loadout、Quest、World消費状態を現在Versionの `GameSaveData` へ集約します。`PrototypeLocalSaveCoordinator` はRuntime構築完了直後・15秒ごと・Application Pause・Quit時の保存だけを管理します。

Field遷移前は旧Field Locationを一度保存し、遷移先Runtimeの再Bind完了後に新しいField / Entry Pointを即時保存します。Continue / Load GameはSave Version 4のStable `FieldLocation` をCatalogで解決して対象Fieldを構築します。未知のField / Entry PointはInitial FieldのDefault Entry PointへFallbackします。

Slotや開始方法に応じた `ISaveService` の解決はTitle/Application側で行います。Runtime State、Save Mapper、`GameSaveData` はSlot数に依存させません。Gameplay Featureは具体的な保存先を参照しません。New Gameでは `FreshGameSaveService` によりLoadを抑止し、Continue / Load Gameでは選択済みSlotの `ISaveService` をそのまま `PrototypeApplicationInstaller` へ注入します。具体的な開始規則は [Title Screen仕様](../specifications/title-screen.md)、Field遷移は [Field Transition仕様](../specifications/field-transition.md)、永続化対象と復元時の扱いは [セーブ仕様](../specifications/save.md) を参照してください。

## Combat / Interaction / AI

Interaction、近接攻撃、Projectile命中判定は3D Physics Queryのみを使用します。敵AIは移動を `CharacterPhysicsBody3D`、攻撃を `AbilityController` へ委譲し、AI自身へ物理・ダメージ処理を重複実装しません。具体的なAI仕様は [敵AI仕様](../specifications/enemy-ai.md) を参照してください。

## UI

本番UI基盤はCanvas（uGUI）です。Viewは表示を担当し、ゲーム状態の変更主体にしません。

Pause、Evolution、Ability Loadoutの主要HierarchyはPrefabで保持します。`PrototypeProjectAssets` が各Prefabを参照し、`PrototypeUiInstaller` が1920x1080基準・`Scale With Screen Size` のCanvas配下へInstantiateします。各Prefabの `PauseMenuLayout` / `EvolutionMenuLayout` / `AbilityLoadoutMenuLayout` がSerializeField参照を保持し、Viewはその参照へ表示状態を反映するだけとします。画面ごとの `CreateRect` / `CreateText` / `StretchToParent` による主要Hierarchy生成は行いません。

Prototype専用のHUD、Dialogue Log、Quest Tracker / Notificationなど、現在Runtime Compositionする補助UIは既存方式を維持します。Title ScreenはP0のApplication起動導線としてGameplay構築前にRuntime Compositionしますが、表示と開始判断は `PrototypeTitleScreenView` / `PrototypeTitleScreenController` に分離します。本格的なTitle演出・設定画面へ拡張する段階でPrefab Authoring境界へ移行します。UI Toolkitへの全面移行は行いません。

Field遷移ではApplication常駐の `PrototypeApplicationFieldBinder` が `ModalUiCoordinator`、`GamePauseController`、`EvolutionSelectionController` を新PlayerのInput / Progressionへ再Bindします。Ability Loadout Selectionは新Player上のControllerへ共有Modal Coordinatorを注入し直し、HUD / Dialogue / Quest Tracker等のField UI Rootは新Field Sceneへ再生成します。旧PlayerへのInput購読は各Controllerの再Initialize時に解除します。

Quest Trackerは次の境界で構成します。

- `QuestTrackerProjection`: Quest Definition / Runtime StateをuGUI非依存の表示Modelへ変換する
- `QuestTrackerSelector`: 手動追跡状態を持たず、現在のQuest状態だけから初期表示対象を決定する
- `QuestTrackerView`: `QuestProgressionService` を購読し、選択された表示Modelを常設Trackerへ反映する
- `QuestNotificationFormatter`: Questイベントを通知文へ変換する
- `QuestNotificationView`: 一時通知のuGUI表示とRealtime基準の通知寿命を管理する

常設Trackerの表示ポリシーと一時通知の寿命を分離し、通知Coroutineの変更が常設Trackerへ波及しない構造とします。プレイヤーが追跡Questを手動選択する仕様が追加されるまでは、状態保持型 `QuestTrackingService` を導入しません。具体的な表示ルールは [Quest仕様](../specifications/quest.md) を参照してください。

Pause、Evolution、Ability Loadoutは同じ `ModalUiCoordinator` を利用します。Coordinatorだけが現在のModal所有者、Open前のInput Context / Time Scale、UI Contextへの切替、Close時の復元を管理し、Modalの同時Openを拒否します。各Feature ControllerはPause状態、Evolution選択、Loadout選択・割当といったFeature固有状態だけを保持します。Component Disable / Destroy時はCoordinatorまたはOwner側からModal所有権を解放し、停止Time ScaleやUI Contextを残留させません。

## ScriptableObject / Resources

静的コンテンツ定義、バランス値、Asset参照はScriptableObject Definitionとして管理します。Definition / Runtime State / Save DTOの責務分離は [アーキテクチャ](./architecture.md) を参照してください。

Resourcesは少数の起動入口や互換用途に限定し、必要性が発生した段階でAddressablesを検討します。

## Editorツール

`PrototypeProjectAssetsAutoRepair` はEditor起動時に参照状態を検証しますが自動書き換えは行いません。参照を書き換えるRepairは明示的なEditorメニュー操作時だけ実行します。シナリオ内部Contentは `TrainingScenarioDefinition` 自身をSource of Truthとします。主要Modal UI PrefabもProjectAssetsの必須参照として検証・手動Repair対象に含めます。

## テスト

テスト種別はFeature名やファイルサイズではなく、Unity Runtimeの実行要件で分類します。

EditModeで検証する対象:

- Unity Runtimeのフレーム進行を必要としないRuntime State / Service状態遷移
- Content収集、共有参照の重複排除、Stable Content ID衝突、Ability LoadoutのSlot解決
- Ability Loadout Policy / Eligibility、Runtime進捗初期化、取得済みArt / Skillの表示Projection
- Input Actionの論理Slot名とKeyboard / Gamepad Binding
- 追加Runtime ContentとProgression Pickup Definition / Grantの設定整合性
- Save Migration、JSON File round trip、Save Slot解決 / Metadata、Ability Loadout / Quest / World Save Mapper
- New GameのFresh Start Save境界
- Game SessionのLoad → Migration → Runtime復元 → Snapshot → Save境界
- Stable Field / Entry Point解決、Field A/B遷移定義、Field切替中のSave Snapshot再Bind
- Quest TrackerのProjection / Selector / Notification Formatter
- `LinearDialogueSequence`、`SpawnLifecycle<T>` の純粋ロジック
- フレーム進行を必要としないScriptableObject Definition検証

PlayModeで検証する対象:

- `MonoBehaviour` LifecycleとEvent購読・解除
- Coroutine / Realtime時間経過
- Scene、Input Context、3D Physics、移動
- Title ScreenのNew Game / Continue / Load Game開始導線とSave Slot選択
- Field A -> B -> AのScene遷移、Progression / Loadout / Quest保持、保存FieldからのContinue
- `ModalUiCoordinator` の排他Open、Input Context / Time Scale復元、Disable / Destroy時復元
- Prefabベースの主要Modal uGUIとRuntime生成するPrototype補助UI
- Jump / Fall / Flight、Combat / Interaction、Enemy AI
- Progression Grant InteractionからArt / Skill Runtime Stateまでの取得統合
- Training Quest Flow、Dummy Defeat Event Bridge、Spawn / Interaction / Combatをまたぐ縦切り統合

Quest UIのPlayModeテストは、uGUI階層生成、Questイベント購読、`QuestNotificationView` の通知寿命といったRuntime統合へ集中させ、状態文言・表示対象選択などの純粋な表示ポリシーはEditModeで検証します。Ability Loadoutも候補生成と割当可否はEditMode、Prefab uGUIとInput Context統合はPlayMode側の責任とします。CIでは引き続きEditMode / PlayModeの両方を実行します。

## Platform実装

Platform固有SDKの隔離方針は [アーキテクチャ](./architecture.md#platform境界) を参照してください。
