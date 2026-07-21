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

### Content / Composition / Delivery

- `IGameContentDefinition`、Stable Content ID相互リンク、Data Loader検証
- `IGameContentContainer` と再帰CollectorによるGameContent登録経路の一本化
- 同一Definition共有参照の重複排除とStable Content ID衝突検出
- `PrototypeProjectAssets` をComposition Manifestとして維持
- `PrototypeProjectAssetsAutoRepair` のEditor起動時自動書き換え廃止、検証と明示的手動Repairへの分離
- VitePress Knowledge Base
- 実行要件に基づくEditMode / PlayModeテスト境界
- semantic-releaseによるリリース自動化

## 優先リファクタリング

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

### 対象外

- `AbilityController` のRegistry分割
- 3D Physics / Elevation / Movement再設計
- Enemy AIのBehavior Tree化
- Addressables / Scene Streaming
- Questの手動Tracking Service

具体的な要件または性能問題が発生した時点で再評価します。

## 次の開発フェーズ

1. Save SlotとNew Game / Continue導線
   - 現在の単一 `save.json` を前提にした自動保存から、明示的な新規開始・継続開始へ接続する
   - 複数Slotが必要になった場合も `ISaveService` とSave DTO境界を維持する

## 将来候補

- Steam機能とPlatform層
- クラウドセーブ
- 将来のコンソール向けPlatform実装
- Addressables / 非同期ロード
- Scene分割・ストリーミング
- パフォーマンス予算
- ScriptableObjectだけでは整合性管理が難しくなった場合の構造化データ化

技術基盤を先回りして増やすのではなく、プレイ可能なゲームループを優先します。
