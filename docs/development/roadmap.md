# ロードマップ

この文書を、プロジェクト全体の実装状況と開発優先度のSource of Truthとします。設計書・仕様書・READMEには実装済み一覧や将来タスク一覧を重複記載しません。

## 実装済み

### 基礎Runtime

- Scene / Build Settings、Rigidbody2D、Collision Tilemap、Isometric描画順
- Input Action / Input Context
- uGUI、Camera、Pause、Dodge
- Domain / Runtime Assembly境界
- ScriptableObject Definition、Runtime State、Save DTO境界

### Combat / Ability / Progression

- Ability共通実行基盤と基本近接攻撃
- DamageResult / DefeatContext / RewardService
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
- 最初の訓練Quest
- 汎用 `SpawnLifecycle<T>` と訓練対象の再生成・復元

### Knowledge Base / Delivery

- VitePress Knowledge Base
- Stable Content ID相互リンクとData Loader検証
- EditMode / PlayModeテスト
- semantic-releaseによるリリース自動化

## 次の開発フェーズ

1. 敵AI
2. Questの受注・進捗・完了をプレイヤーへ提示するUI / UX
3. NPC会話とQuestを使った縦切りゲームループの拡張
4. 複数Art / Skillの選択UIと入力割当
5. 追加の正式Runtimeコンテンツと取得経路
6. 実際のローカルSave実装

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
