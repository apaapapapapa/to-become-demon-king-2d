# ロードマップ

この文書を、プロジェクト全体の実装状況と開発優先度のSource of Truthとします。設計書・仕様書・READMEには実装済み一覧や将来タスク一覧を重複記載しません。

## 実装済み

### 基礎Runtime

- Scene / Build Settings、3D Rigidbody / Collider、Collision Tilemapマーカー、Isometric描画順
- Input Action / Input Context
- uGUI、Camera、Pause、Dodge
- X/Y平面移動とZ Elevationを使うJump / Fall / Flight
- 有限高さ障害物の上空通過
- Domain / Runtime Assembly境界
- ScriptableObject Definition、Runtime State、Save DTO境界

### Combat / Ability / AI / Progression

- Ability共通実行基盤と基本近接攻撃
- DamageResult / DefeatContext / RewardService
- 敵AIのIdle / Chase / Attack、索敵・離脱、高度差による追跡・攻撃制御
- 敵AIから既存Ability Systemを利用する攻撃経路
- Experience / Level
- Art習得・熟練・Ability解放
- 受動Skill取得と汎用Modifier
- Evolution Node条件評価、排他選択、永続補正、選択UI、形態表示
- 火炎魔法Art、Projectile Ability、Progression Grant

### Interaction / Dialogue / Quest / Spawning

- `IInteractable` を使うInteraction基盤
- データ駆動Dialogue Definitionと直線会話進行
- 最新1件だけを表示するDialogue UI
- Gameplay Event Hub
- Quest / Objective Runtime Stateと進捗サービス
- QuestのAvailable / Active / ReadyToTurnIn / Completedライフサイクルと明示的な受注・報告完了
- Questの受注・進捗・報告可能・完了を表示する常設トラッカーと非モーダル通知
- 見習い魔術師のQuest状態別Dialogueと、依頼会話→受注→討伐→報告会話→報酬の縦切りゲームループ
- 最初の訓練Quest報告完了時の火炎魔法取得経路
- 汎用 `SpawnLifecycle<T>` と訓練対象の再生成・復元

### Knowledge Base / Delivery

- VitePress Knowledge Base
- Stable Content ID相互リンクとData Loader検証
- EditMode / PlayModeテスト
- semantic-releaseによるリリース自動化

## 優先リファクタリング

機能追加の速度を落とさずに拡張余地を確保するため、次の正式Runtimeコンテンツ追加前にP0、その後P1を実施します。

この計画では、将来必要になる可能性だけを理由に汎用サービスや抽象層を増やしません。現在すでに複数箇所へ重複している知識、次の機能追加で確実に増幅する依存、テスト容易性を直接改善する境界だけを対象とします。実装完了後の正式な構造は `docs/design/technical-design.md` へ反映し、この節は優先度と移行方針だけを管理します。

### P0: 次のコンテンツ拡張前に対応

#### 1. GameContent登録経路を一本化する

複数Art / Skill追加では `PrototypeProjectAssets.CreateGameContentCatalog()` の手動走査と重複登録が先に問題化するため、従来P1としていたContent Catalog整理をP0へ引き上げます。

- `PrototypeProjectAssets` が `CharacterDefinition` 内部のAbility / Art / Skill / Evolution構造を直接走査しないようにする
- 子Content Definitionを公開する共通契約と再帰Collectorを用意し、Catalog登録対象の列挙を一つの経路へ集約する
- 同じDefinitionインスタンスが複数経路から参照される場合はStable Content ID単位で一度だけ登録する
- 同一Stable Content IDを異なるDefinitionインスタンスが使用している場合は設定エラーとして失敗させる
- Artから解放されるAbilityなど、共有参照を含む構成でもCatalog生成が順序や重複に依存しないようにする

完了条件:

- 新しいArt / Skill / Evolutionを追加しても `PrototypeProjectAssets.CreateGameContentCatalog()` の走査コードを変更しない
- 正常な共有参照では重複ID例外が発生せず、異なるDefinitionによるID衝突だけを検出する
- Catalog収集・重複排除・ID衝突をEditModeテストで検証する

#### 2. Ability入力を固定Ability IDから論理スロットへ移行する

`PlayerAbilityInput` の `basicAttackAbilityId` / `artAbilityId` 固定割当を、Runtime Loadoutが管理する論理スロットへ置き換えます。

- Input Systemの物理Action名とAbility IDを直接結び付けない
- `PlayerInputReader` は `AbilitySlot` などの論理スロット入力として通知する
- Runtime Stateとして `AbilityLoadout` を持ち、`AbilitySlot -> AbilityId` の対応をSource of Truthとする
- `PlayerAbilityInput` はスロット入力を受けてLoadoutを解決し、既存 `AbilityController.TryUse` へ委譲する
- 基本攻撃は必要なら予約スロットとして扱い、プレイヤーが編集可能なArt用スロットと区別する
- ArtはAbilityを解放する進行要素、Skillは現在の受動Modifier要素として扱い、Skill自体を一律にAbility入力へ割り当てない
- 将来アクティブSkillが必要になった場合は、その実行単位をAbilityとしてモデル化して同じスロット経路へ載せる

完了条件:

- Abilityを追加しても `PlayerAbilityInput` へAbility固有フィールドやイベントを追加しない
- Loadout差し替えだけで同じ入力スロットから使用Abilityを変更できる
- AIは引き続き `AbilityController` を直接利用し、Player Loadoutへ依存しない
- Loadout解決と入力アダプタを自動テストする

#### 3. Prototypeのシナリオ構成を明示的なDefinitionへ集約する

`PrototypeProjectAssets` 全体を巨大な汎用設定へ置き換えるのではなく、一つの縦切りゲームループとして同時に変更される参照だけを `TrainingScenarioDefinition` にまとめます。

対象:

- Training Quest
- Quest状態別Dialogue Set
- Training Enemy AI
- Defeat Reward
- Quest報告時のProgression Grant

対象外:

- Application Settings
- Player Character
- UI Font
- World Prefab / Sprite / Terrain Art

`PrototypeProjectAssets` は引き続きPrototype全体のComposition Manifestとし、個別の訓練シナリオ参照群を `TrainingScenarioDefinition` 一つへ置き換えます。`QuestDefinitions.FirstOrDefault()` を訓練Questの意味として利用せず、シナリオDefinitionが対象Questを明示的に保持します。

`PrototypeProjectAssetsAutoRepair` は第二のComposition Manifestにしません。

- Editor起動時に多数のSerialized Fieldを既知パスから自動上書きする動作を縮退する
- 自動処理は参照・設定の検証を中心とし、書き換えを伴う修復は明示的な手動操作に限定する
- Stable Content IDで一意に探索できるDefinitionはIDベースで修復候補を解決する
- Prefab / Spriteなど一意なContent IDを持たない参照は、推測による自動上書きを行わず不足を報告する
- 一時的な移行用パス定数を残す場合は、恒久的なRuntime / CompositionのSource of Truthとして扱わない

完了条件:

- DialogueやRewardを追加しても `PrototypeProjectAssets` とAutoRepairへ同じ意味のフィールド追加を繰り返さない
- 訓練シナリオの必須参照整合性を `TrainingScenarioDefinition.IsConfigured` で検証できる
- Editor起動だけで有効な手動設定が別アセットへ自動的に差し戻されない

#### 4. Training AreaのComposition責務を分割する

従来案の `TrainingDummyLifecycleController` 新設は採用しません。Dummyの生成・復元自体は既存 `SpawnLifecycle<T>` がすでに担当しているため、新しいLifecycle層を重ねない方針とします。

代わりに現在 `PrototypeTrainingAreaCoordinator` に集まっている異なる責務を境界ごとに分けます。

- Gameplay Event HubからQuest Progressionへの共通配線はTraining Area固有ではないため、Gameplay Service生成または専用Event Routerへ移動する
- `TrainingQuestFlowController` はNPC会話選択、Quest受注、報告完了、Progression Grant、シナリオ上の再生成要求を担当する
- `TrainingDummyEventBridge` は現在のDummyインスタンスのDefeatをGameplay Eventへ変換する
- `RewardGranted` / `QuestCompleted` のDebug LogはCoordinator責務から外し、必要なら開発用Observerへ分離する
- `PrototypeGameplayFeatureInstaller` は個々のDialogue / Quest / Reward / Grantを列挙せず、`TrainingScenarioDefinition` を受け取る

完了条件:

- Installerの引数追加がシナリオ内Content数に比例しない
- Quest以外のGameplay Event購読を追加してもTraining Quest Flowを変更しない
- Dummy Spawn / DefeatとQuest会話フローを独立してテストできる
- `PrototypeTrainingAreaCoordinator` は削除するか、単純なComposition Facadeに縮小する

### P1: P0完了後に保守性を改善

#### 1. Quest Trackerの表示ポリシーと通知寿命をViewから分離する

前回案に含めた状態保持型 `QuestTrackingService` の先行導入は行いません。プレイヤーが追跡Questを手動選択する仕様がまだないため、現時点では過剰抽象化になります。

- Quest状態から表示用Modelを作る純粋なPresenter / Projectionを抽出する
- 初期表示対象の選択規則を副作用のないSelectorとして分離し、複数Quest時の優先順位を自動テスト可能にする
- 一時通知は常設Trackerと寿命が異なるため `QuestNotificationView` へ分離する
- `QuestTrackerView` は表示ModelをUIへ反映する責務を中心にする
- 現在のRuntime UI生成方式はこのリファクタリングだけを理由にPrefab / UI Toolkitへ移行しない
- プレイヤーが追跡Questを選択する仕様が追加された時点で、初めて状態保持型Tracking Serviceを導入する

完了条件:

- Questの表示順や状態文言のテストにPlayModeを必要としない
- 通知Coroutineの変更が常設Trackerの表示ロジックへ影響しない
- UI階層生成方式を変えずに表示ポリシーだけ交換できる

#### 2. EditMode / PlayModeテストの境界を実行要件で整理する

テストファイルの大きさや対象Feature名ではなく、Unity Runtimeの実行要件で分類します。

EditModeへ寄せる対象:

- 純粋なRuntime State / Serviceの状態遷移
- Content ID収集・重複検証
- Loadout解決
- Quest表示Model / Selector
- ScriptableObject Definitionの設定検証でフレーム進行を必要としないもの

PlayModeへ残す対象:

- `MonoBehaviour` のLifecycleやEvent購読解除
- Coroutine / 時間経過
- Physics / Collider / Rigidbody
- Runtime生成したuGUIの実表示
- Spawn / Interaction / Combatをまたぐ縦切り統合

既存テストを機械的に全移動せず、EditMode Assemblyが不足している場合は先に明示的なTest Assembly境界を作成してから段階移行します。

完了条件:

- Pure Logicの失敗確認にPlayMode起動を必要としない
- PlayModeテストはUnity Runtime統合を検証するケースへ集中する
- CIでは引き続きEditMode / PlayModeの両方を実行する

### 対象外

現時点では次をこのリファクタリングへ含めません。

- `AbilityController` のExecutor / Cost / Modifier Registry分割
- 3D Physics / Elevation / Movementの再設計
- Enemy AIのBehavior Tree化
- AddressablesやScene Streaming導入
- Questの手動Tracking Service

これらは現在の機能規模では投資対効果が低く、具体的な要件または性能問題が発生した時点で再評価します。

### 実施順

1. P0: GameContent登録経路の一本化
2. P0: Ability Loadout / Slot Binding
3. P0: TrainingScenarioDefinitionとProjectAssets / AutoRepair整理
4. P0: Training Area Composition分割
5. P1: Quest Tracker Presentation分割
6. P1: EditMode / PlayModeテスト再配置
7. 複数Art / Skillの選択UIと入力割当

## 次の開発フェーズ

1. 上記P0 / P1リファクタリング
2. 複数Art / Skillの選択UIと入力割当
3. 追加の正式Runtimeコンテンツと取得経路
4. 実際のローカルSave実装

## 将来候補

必要性が発生した時点で着手します。

- Steam機能とPlatform層
- クラウドセーブ
- 将来のコンソール向けPlatform実装
- Addressables / 非同期ロード
- Scene分割・ストリーミング
- パフォーマンス予算
- ScriptableObjectだけでは整合性管理が難しくなった場合の構造化データ化

技術基盤を先回りして増やすのではなく、プレイ可能なゲームループを優先します。
