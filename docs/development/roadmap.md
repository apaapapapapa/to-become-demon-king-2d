# ロードマップ

この文書を、プロジェクト全体の実装状況と開発優先度のSource of Truthとします。設計書・仕様書・READMEには実装済み一覧や将来タスク一覧を重複記載しません。

## 実装済み

### 基礎Runtime

- Scene / Build Settings、3D Rigidbody / Collider、Collision Tilemapマーカー、Isometric描画順
- Input Action / Input Context、uGUI、Camera、Pause、Dodge
- X/Y平面移動とZ Elevationを使うJump / Fall / Flight
- 有限高さ障害物の上空通過
- Domain / Runtime Assembly境界
- ScriptableObject Definition、Runtime State、Save DTO境界

### Combat / Ability / AI / Progression

- Ability共通実行基盤と基本近接攻撃
- 論理 `AbilitySlot` とRuntime `AbilityLoadout` によるPlayer入力割当
- `Primary` / `Action1`〜`Action4` の論理入力とRuntime Loadout選択UI
- 取得済みArt AbilityのAction Slot割当と、取得済み受動SkillのLoadout画面表示
- Player入力とAbility IDの分離、AIからの `AbilityController` 直接利用
- DamageResult / DefeatContext / RewardService
- 敵AIのIdle / Chase / Attack、索敵・離脱、高度差制御
- Experience / Level
- Art習得・熟練・Ability解放
- 受動Skill取得と汎用Modifier
- Evolution Node条件評価、排他選択、永続補正、選択UI、形態表示
- 火炎魔法Art、Projectile Ability、Progression Grant
- 魔弾術Art / 魔力弾Ability、魔力循環Skill

### Interaction / Dialogue / Quest / Spawning

- `IInteractable`、データ駆動Dialogue、最新1件Dialogue UI
- Gameplay Event Hub
- Quest / Objective Runtime StateとAvailable / Active / ReadyToTurnIn / Completedライフサイクル
- Quest常設トラッカーと非モーダル通知
- Quest表示Projection / 初期表示Selector / Notification Viewの責務分離
- 依頼会話→受注→討伐→報告会話→報酬の縦切りゲームループ
- 汎用 `SpawnLifecycle<T>` と訓練対象の再生成・復元
- `TrainingScenarioDefinition` による訓練シナリオ参照の集約
- `TrainingQuestFlowController` と `TrainingDummyEventBridge` によるComposition責務分離
- Gameplay Event HubからQuest Progressionへの共通Application配線
- `ProgressionGrantInteractable` による一度きりのフィールド取得経路
- 古びた魔導書から魔弾術、魔力結晶から魔力循環を取得するPrototype導線

### Local Save

- `JsonFileSaveService` による `Application.persistentDataPath/save.json` へのJSON保存
- 起動時LoadとRuntime State復元、15秒間隔・Pause・Quit時の自動保存
- Save Version 1 → 2 → 3 MigrationとCollection正規化
- Character Progression、Art / Skill / Evolution状態の復元
- `Action1`〜`Action4` のAbility Loadout保存・復元
- Quest Status / Objective進捗の保存・復元
- フィールド上の一度きりProgression Grant消費状態の保存・復元
- 破損・未対応Saveを復元できない場合の既存ファイル保護
- `PrototypeGameSession` によるLoad → Migration → World構築 → Restore → 保存開始の明示化
- `PrototypeGameSaveSnapshotProvider` によるRuntime State一式からのSave Snapshot生成
- `PrototypeLocalSaveCoordinator` の保存タイミング責務への限定

### Content / Composition / Delivery

- `IGameContentDefinition`、Stable Content ID相互リンク、Data Loader検証
- `IGameContentContainer` と再帰CollectorによるGameContent登録経路の一本化
- 同一Definition共有参照の重複排除とStable Content ID衝突検出
- `PrototypeProjectAssets` をComposition Manifestとして維持
- `PrototypeProjectAssetsAutoRepair` のEditor起動時自動書き換え廃止、検証と明示的手動Repairへの分離
- `ModalUiCoordinator` によるPause / Evolution / Ability LoadoutのModal所有権、Input Context、Time Scaleの共通管理
- VitePress Knowledge Base
- 実行要件に基づくEditMode / PlayModeテスト境界
- semantic-releaseによるリリース自動化

## 第1次優先リファクタリング: 完了

### P0: 完了

次の4項目は実装済みです。正式な実装境界は [技術設計](../design/technical-design.md) を参照してください。

1. GameContent登録経路の一本化
2. Ability Loadout / Slot Binding
3. TrainingScenarioDefinitionとProjectAssets / AutoRepair整理
4. Training Area Composition分割

### P1: 完了

次の2項目は実装済みです。正式な実装境界は [技術設計](../design/technical-design.md) を参照してください。

1. Quest Tracker Presentation分割
   - Quest状態から表示Modelを作る `QuestTrackerProjection` を抽出
   - 初期表示対象を副作用なく決める `QuestTrackerSelector` を分離
   - 一時通知と通知寿命を `QuestNotificationView` へ分離
   - `QuestTrackerView` は購読と表示ModelのuGUI反映を中心に整理
   - 手動追跡仕様がないため状態保持型 `QuestTrackingService` は導入しない
2. EditMode / PlayModeテスト境界の整理
   - Quest Runtime State / Service、Quest表示Model / Selector、`LinearDialogueSequence`、`SpawnLifecycle<T>` の純粋ロジックをEditModeで検証
   - PlayModeは `MonoBehaviour` Lifecycle / Event購読、Coroutine / 時間経過、Physics、Runtime uGUI、縦切り統合へ集中
   - Quest UIのPlayModeテストはuGUI生成、Questイベント購読、通知寿命のRuntime統合を検証
   - CIではEditMode / PlayModeの両方を継続実行

## 第2次優先リファクタリング

### P0: 完了

第2次リファクタリングのP0は完了しました。確定した実装境界は [技術設計](../design/technical-design.md) と [AI Context Map](../ai/context-map.md) を参照してください。

1. Save / Game Session起動フローの再編
   - `PrototypeGameSession` がSave読込、Migration、World / Player構築、Runtime復元、保存開始の順序を管理
   - `PrototypeGameSaveRestorer` が構築済みRuntimeへのFeature復元を担当
   - `PrototypeGameSaveSnapshotProvider` がProgression / Ability Loadout / Quest / World状態を `GameSaveData` へ集約
   - `CharacterProgressionSaveMapper` はPlayer状態変換だけを担当
   - `PrototypeWorldBuilder` からQuest Save復元を除去
   - `PrototypeLocalSaveCoordinator` は保存タイミングだけを管理
   - Save Slot / New Game / Continueの保存先解決はApplication / Platform境界へ追加可能
2. Modal UI制御の共通化
   - `ModalUiCoordinator` がModal所有権、排他Open、Input Context、Time Scaleの退避・復元を一元管理
   - Pause / Evolution / Ability Loadoutは同一Coordinatorを利用
   - 各Feature ControllerからTime Scale / Input Contextの独自保存・復元を除去
   - Disable / Destroy時の復元をPlayModeテストで検証
3. Architecture / AI Context Mapの陳腐化修正
   - 現行のGameplay Composition、Save、Modal、EditMode / PlayModeテスト参照へ更新
   - ロードマップは実装状況・優先度、Technical Designは実装済み正式アーキテクチャという役割を維持

## 第2次優先リファクタリング: 実施予定

P0完了後の残作業はP1〜P2です。現在のプレイ可能なゲームループを維持し、リファクタリングだけを目的とした新規Frameworkや過剰な抽象化は導入しません。

### P1-1: Player Runtime Compositionの分割

#### 目的

`PrototypePlayerSpawner` に集中しているPrefab生成、Component補完、各Feature初期化、Player固有UI Controller構築を分割し、キャラクター機能追加時の変更集中とPrefab構成の二重管理を解消します。

#### 要件

1. Spawnerは次の責務を中心とする
   - Character PrefabのInstantiate
   - 親Transform / Spawn Position設定
   - Runtime Context作成・注入の開始
2. Gameplay Component構築・初期化をPlayer Runtime用Installer群へ分離する
3. 少なくとも次の責務群を分離可能な単位として整理する
   - Physics / Movement
   - Combat / Ability
   - Progression / Evolution
   - Player Input / Loadout
   - Presentation / Prototype Effect
4. Prefabへ恒久的に存在すべきComponentと、Runtimeで注入すべきComponentの基準を定義する
5. Prefabに必要なComponentが欠落している場合、無条件にRuntime補完して構成ミスを隠す方式を縮退する
6. `RequireComponent`、Editor Validation、明示的Installer Errorのいずれか適切な方法で構成不備を検出する
7. Character Definition / Runtime State / Save DTO境界は維持する
8. PlayerとEnemyで共用できるCharacter Runtime初期化とPlayer専用初期化を混在させない

#### 受入条件

- `PrototypePlayerSpawner` が各Gameplay Featureの詳細初期化を直接列挙し続ける構造ではなくなっている
- Prefab構成不備が起動時の暗黙補完だけで隠れない
- Player生成の主要なComposition単位を個別テスト可能にできる
- 既存Movement / Combat / Progression / Loadout / Evolution動作を維持する
- Unity TestsのEditMode / PlayModeが成功する

### P1-2: Ability LoadoutルールのSource of Truth集約

#### 目的

編集可能Slot、重複Ability禁止、割当可能Ability判定がController / Menu Projection / Save Mapperへ分散している状態を解消し、Loadoutルールを一つの境界から利用できるようにします。

#### 要件

1. `Action1`〜`Action4` の編集可能Slot定義を1箇所へ集約する
2. `AbilityLoadoutController`、`AbilityLoadoutSelectionController`、`AbilityLoadoutSaveMapper` が個別に同じSlot配列を保持しない
3. 同一Abilityを複数編集Slotへ重複配置しないルールをRuntime Loadout側の明示的な操作として表現する
4. UIだけが重複禁止ルールを知る構造にしない
5. Save復元時も同じルールを利用する
6. Character Definition + Character Progression Stateから「現在割当可能なAbility」を判定するロジックをMenu表示専用Projectionから分離する
7. Menu Projection、Save復元、Selection Controllerは同じEligibility / Policy境界を利用する
8. `Primary` は予約Slotとしてユーザー編集対象外とする現在仕様を維持する
9. 未習得・削除済みAbility、不正Slot、重複AbilityをSaveから安全に無視する現在仕様を維持する

#### 受入条件

- 編集可能Slotの追加・削除が1箇所の変更で反映される
- 重複Ability禁止をUI経由以外のRuntime操作でも保証できる
- Save MapperがMenu Projectionへ依存しない
- Loadout Policy / EligibilityをEditModeで単体検証できる
- 既存Loadout Save round-tripが成功する

### P1-3: uGUI Runtime生成コードの整理

#### 目的

Pause、Evolution、Ability Loadout等に重複しているRuntime UI Hierarchy生成・RectTransform設定・Text生成・共通Style定義を整理し、本格的な画面追加時の保守コストを下げます。

#### 要件

1. Canvas/uGUIを本番UI基盤として継続する
2. UI Toolkitへの全面移行は行わない
3. Pause / Evolution / Ability LoadoutのRuntime UI生成コードを整理する
4. 本番利用する画面は原則としてPrefabベースのuGUIへ移行する
5. ViewはSerializeField等で必要なUI参照を受け取り、表示更新を中心とする
6. `CreateRect`、`CreateText`、`StretchToParent` 等の画面ごとの重複実装を削減する
7. 共通色・Typography・Panel Styleは必要な範囲で共通化するが、巨大な独自UI Frameworkは作らない
8. Runtime生成を残す場合はPrototype専用補助UIなど明確な理由がある箇所に限定する
9. Viewからゲーム状態を変更しない現在のPresentation境界を維持する

#### 受入条件

- Pause / Evolution / Ability Loadoutの主要Hierarchyが画面ごとの大量C#生成に依存しない
- ViewクラスがHierarchy生成より表示反映を中心とした責務になっている
- 共通UI生成コードの重複が削減されている
- 1920x1080基準の既存表示を維持し、Scale With Screen Sizeが継続する
- UI PlayModeテストが成功する

### P1-4: PlayerInputReaderの論理入力整理

#### 目的

Ability Loadout導入前の `Attack` / `Art` 命名とObsoleteイベントを整理し、Input Action・論理Ability Slot・Runtime Loadoutの用語を一致させます。

#### 要件

1. Ability系Input Action名を論理Slotと対応する命名へ統一する
   - `Primary`
   - `Action1`
   - `Action2`
   - `Action3`
   - `Action4`
2. `AttackPressed` / `ArtPressed` のObsolete互換イベントについて利用箇所を除去した上で削除する
3. `AbilitySlotPressed` をAbility入力の単一イベント境界とする
4. `PlayerInputReader` はInput System Actionを論理入力へ変換する責務に限定する
5. Feature固有のAbility IDをInput層へ持ち込まない
6. Gameplay / UI / DisabledのInput Context境界を維持する
7. Jump / Flight / Interact / Dodge / Pause等、Ability Slotではない入力は現在の論理イベントを維持する
8. Input Action AssetのBinding変更時もKeyboard / Gamepadの既存操作を維持する

#### 受入条件

- Ability入力に旧 `Attack` / `Art` という内部互換名が残らない
- `AbilitySlotPressed` だけでPlayer Ability実行が成立する
- Input Action Assetとコード上のSlot名が一致する
- Keyboard / Gamepadの既存Bindingテストが成功する
- Unity PlayModeテストが成功する

### P2: Field / World CompositionのScene単位アーキテクチャ化

#### 目的

現在の単一Prototype World Builderから、王国・都市・森・魔族領・遺跡・ダンジョン等の複数フィールドを追加できる構造へ移行します。2つ目以降の本格フィールドで `PrototypeWorldBuilder` 相当の巨大Builderを複製しないことを目的とします。

#### 要件

1. Field単位の静的定義とRuntime Compositionを分離する
2. Field Definitionは少なくとも次を表現可能にする
   - Field ID
   - SceneまたはScene識別情報
   - Player Spawn Point / Entry Point
   - Field固有Content / Scenario参照
   - 必要なWorld Asset参照
3. Scene側のComposition Rootを最小化し、Field固有FeatureはInstallerへ委譲する
4. Terrain / Collision / Architecture / Nature / Atmosphere / Gameplay Scenario / Pickup / Camera等を、単一巨大Builderへ固定せずField Composition単位で組み合わせ可能にする
5. Prototype専用のRuntime Shape / Tile生成を、将来の正式Field Sceneへ必須依存させない
6. Player Runtime、Game Session、Saveの境界をField切替と独立させる
7. SaveにはFieldをStable IDで識別できる拡張余地を確保する
8. Field間移動時にGame Session全体を破棄しなければならない構造を避ける
9. Addressables / Scene StreamingはこのP2の必須要件にしない
10. まず通常のUnity Scene分割で成立する境界を作り、非同期ロード・Streamingは性能またはコンテンツ量の要求が出た段階で追加する
11. 現在のPrototype Sceneを新Field境界へ段階移行し、既存ゲームループを維持する

#### 受入条件

- 2つ目のFieldを追加する際、既存 `PrototypeWorldBuilder` 相当を丸ごとコピーする必要がない
- Field固有設定とGame全体のRuntime Stateが分離されている
- Scene変更後もPlayer Progression / Loadout / Quest等のGame Session Stateを保持できる設計になっている
- Prototype Sceneが新しいField Composition境界を利用して動作する
- AddressablesなしでScene単位の追加・遷移を実装可能である
- Unity TestsのEditMode / PlayModeが成功する

## 第2次リファクタリングの実施順序

P0完了後は次の順序で実施します。

1. P1-2: Ability LoadoutルールのSource of Truth集約
2. P1-4: PlayerInputReaderの論理入力整理
3. P1-1: Player Runtime Compositionの分割
4. P1-3: uGUI Runtime生成コードの整理
5. P2: Field / World CompositionのScene単位アーキテクチャ化

各項目は可能な限り独立PRとし、機能変更と大規模構造変更を同一PRへ混在させません。

## 次の開発フェーズ

1. 第2次優先リファクタリング P1〜P2
   - 上記実施順序で全項目を対応する
   - 各項目の受入条件を満たしてから次へ進む
2. Save SlotとNew Game / Continue導線
   - P0-1で整備したGame Session / Save境界を利用する
   - 現在の単一 `save.json` を前提にした自動保存から、明示的な新規開始・継続開始へ接続する
   - 複数Slotが必要になった場合も `ISaveService` とSave DTO境界を維持する

## 今回のリファクタリング対象外

- `AbilityController` のRegistry分割
- 3D Physics / Elevation / Movement再設計
- Enemy AIのBehavior Tree化
- Questの手動Tracking Service
- Addressables / 非同期ロード
- Scene Streaming

上記はP1〜P2完了後も、具体的な機能要件または性能問題が発生した時点で再評価します。

## 将来候補

- Steam機能とPlatform層
- クラウドセーブ
- 将来のコンソール向けPlatform実装
- Addressables / 非同期ロード
- Scene Streaming
- パフォーマンス予算
- ScriptableObjectだけでは整合性管理が難しくなった場合の構造化データ化

技術基盤を先回りして増やすのではなく、必要な境界だけを段階的に整理しながらプレイ可能なゲームループを維持します。
