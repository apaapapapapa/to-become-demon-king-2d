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
- Player入力とAbility IDの分離、AIからの `AbilityController` 直接利用
- DamageResult / DefeatContext / RewardService
- 敵AIのIdle / Chase / Attack、索敵・離脱、高度差制御
- Experience / Level
- Art習得・熟練・Ability解放
- 受動Skill取得と汎用Modifier
- Evolution Node条件評価、排他選択、永続補正、選択UI、形態表示
- 火炎魔法Art、Projectile Ability、Progression Grant

### Interaction / Dialogue / Quest / Spawning

- `IInteractable`、データ駆動Dialogue、最新1件Dialogue UI
- Gameplay Event Hub
- Quest / Objective Runtime StateとAvailable / Active / ReadyToTurnIn / Completedライフサイクル
- Quest常設トラッカーと非モーダル通知
- 依頼会話→受注→討伐→報告会話→報酬の縦切りゲームループ
- 汎用 `SpawnLifecycle<T>` と訓練対象の再生成・復元
- `TrainingScenarioDefinition` による訓練シナリオ参照の集約
- `TrainingQuestFlowController` と `TrainingDummyEventBridge` によるComposition責務分離
- Gameplay Event HubからQuest Progressionへの共通Application配線

### Content / Composition / Delivery

- `IGameContentDefinition`、Stable Content ID相互リンク、Data Loader検証
- `IGameContentContainer` と再帰CollectorによるGameContent登録経路の一本化
- 同一Definition共有参照の重複排除とStable Content ID衝突検出
- `PrototypeProjectAssets` をComposition Manifestとして維持
- `PrototypeProjectAssetsAutoRepair` のEditor起動時自動書き換え廃止、検証と明示的手動Repairへの分離
- VitePress Knowledge Base
- EditMode / PlayModeテスト
- semantic-releaseによるリリース自動化

## 優先リファクタリング

### P0: 完了

次の4項目は実装済みです。正式な実装境界は [技術設計](../design/technical-design.md) を参照してください。

1. GameContent登録経路の一本化
2. Ability Loadout / Slot Binding
3. TrainingScenarioDefinitionとProjectAssets / AutoRepair整理
4. Training Area Composition分割

### P1: 次に対応

#### Quest Tracker Presentation分割

- Quest状態から表示Modelを作る純粋なProjectionを抽出する
- 初期表示対象を決める副作用のないSelectorを分離する
- 一時通知を `QuestNotificationView` へ分離する
- `QuestTrackerView` は表示Modelの反映を中心にする
- 手動追跡仕様がないため状態保持型 `QuestTrackingService` は導入しない

完了条件:

- 表示順と状態文言をEditModeでテストできる
- 通知Coroutineの変更が常設Trackerへ影響しない
- UI階層生成方式を変えず表示ポリシーを交換できる

#### EditMode / PlayModeテスト境界の整理

EditModeへ寄せる対象:

- 純粋なRuntime State / Service状態遷移
- Content ID収集・重複検証、Loadout解決
- Quest表示Model / Selector
- フレーム進行不要なDefinition検証

PlayModeへ残す対象:

- `MonoBehaviour` Lifecycle / Event購読解除
- Coroutine / 時間経過
- Physics / Collider / Rigidbody
- Runtime uGUI実表示
- Spawn / Interaction / Combatをまたぐ統合

完了条件:

- Pure Logicの確認にPlayMode起動を必要としない
- PlayModeはUnity Runtime統合へ集中する
- CIではEditMode / PlayModeの両方を実行する

### 対象外

- `AbilityController` のRegistry分割
- 3D Physics / Elevation / Movement再設計
- Enemy AIのBehavior Tree化
- Addressables / Scene Streaming
- Questの手動Tracking Service

具体的な要件または性能問題が発生した時点で再評価します。

## 次の開発フェーズ

1. P1リファクタリング
2. 複数Art / Skillの選択UIと入力割当
3. 追加の正式Runtimeコンテンツと取得経路
4. 実際のローカルSave実装

## 将来候補

- Steam機能とPlatform層
- クラウドセーブ
- 将来のコンソール向けPlatform実装
- Addressables / 非同期ロード
- Scene分割・ストリーミング
- パフォーマンス予算
- ScriptableObjectだけでは整合性管理が難しくなった場合の構造化データ化

技術基盤を先回りして増やすのではなく、プレイ可能なゲームループを優先します。
