# ロードマップ

この文書を、プロジェクト全体の実装状況と開発優先度のSource of Truthとします。設計書・仕様書・READMEには将来タスク一覧を重複記載せず、実装済みの正式な境界は [技術設計](../design/technical-design.md) と [AI Context Map](../ai/context-map.md) を参照してください。

## 開発方針

基盤の先行整備フェーズは完了しました。今後は「将来機能を追加しやすいFrameworkを先に作る」ことより、実際のField / NPC / Enemy / Quest / Storyを追加し、プレイ可能な本編を前へ進めることを優先します。

当面の最重要目標は、次の一連の体験をゲームとして成立させることです。

```text
New Game
  -> 弱い魔物として森で誕生
  -> 育ての親となる魔物との生活
  -> 人間による死別
  -> 森から脱出
  -> 魔物であることを隠して人間の町へ入る
  -> 冒険者として依頼を受ける
  -> Dungeonを攻略する
  -> Bossを倒す
  -> 町へ帰還して報告する
```

新しい技術基盤は、この縦切りゲームループを実装するうえで具体的な不足が発生した場合にだけ追加します。

## 実装済み基盤

### Runtime / Movement / Input

- Scene / Build Settings、3D Rigidbody / Collider、Collision Tilemapマーカー、Isometric描画順
- Input Action / Input Context、uGUI、Camera、Pause、Dodge
- X/Y平面移動とZ Elevationを使うJump / Fall / Flight
- 有限高さ障害物の上空通過
- Domain / Runtime Assembly境界
- ScriptableObject Definition、Runtime State、Save DTO境界

### Combat / Ability / AI / Progression

- Ability共通実行基盤と基本近接攻撃
- 論理 `AbilitySlot` とRuntime `AbilityLoadout`
- `Primary` / `Action1`〜`Action4` の論理入力とRuntime Loadout選択UI
- Player入力とAbility IDの分離、AIからの `AbilityController` 利用
- DamageResult / DefeatContext / RewardService
- 敵AIのIdle / Chase / Attack、索敵・離脱、高度差制御
- Experience / Level
- Art習得・熟練・Ability解放
- 受動Skill取得と汎用Modifier
- Evolution Node条件評価、排他選択、永続補正、選択UI、形態表示
- 火炎魔法Art / Projectile Ability / Progression Grant
- 魔弾術Art / 魔力弾Ability / 魔力循環Skill

### Interaction / Dialogue / Quest / Spawning

- `IInteractable`、データ駆動Dialogue、最新1件Dialogue UI
- Gameplay Event Hub
- Quest / Objective Runtime State
- Available / Active / ReadyToTurnIn / Completedライフサイクル
- Quest常設トラッカーと非モーダル通知
- 依頼会話 -> 受注 -> 討伐 -> 報告会話 -> 報酬の縦切りゲームループ
- 汎用 `SpawnLifecycle<T>`
- `TrainingScenarioDefinition`
- `TrainingQuestFlowController` / `TrainingDummyEventBridge`
- `ProgressionGrantInteractable` による一度きりのField取得経路

### Save / Game Session

- `JsonFileSaveService`
- 起動時Load / Migration / Restore
- 15秒間隔・Pause・Quit時の自動保存
- Character Progression / Art / Skill / Evolution / Ability Loadout / Questの保存・復元
- Field上の一度きりProgression Grant消費状態の保存・復元
- 破損・未対応Saveを復元できない場合の既存ファイル保護
- `PrototypeGameSession` による起動順序の明示化
- `PrototypeGameSaveRestorer`
- `PrototypeGameSaveSnapshotProvider`
- `PrototypeLocalSaveCoordinator` の保存タイミング責務への限定
- Save Version 4
- Stable Field ID / Entry Point IDの保存・Migration

### Content / Composition / UI / Delivery

- `IGameContentDefinition`、Stable Content ID相互リンク、Data Loader検証
- `IGameContentContainer` と再帰CollectorによるGameContent登録経路の一本化
- Stable Content ID衝突検出
- `PrototypeProjectAssets` をComposition Manifestとして維持
- `PrototypeProjectAssetsAutoRepair` の自動書き換え廃止と明示的Repair
- `ModalUiCoordinator`
- Player Runtime Installer分割
- Ability Loadout Policy / EligibilityのSource of Truth集約
- Pause / Evolution / Ability LoadoutのPrefabベースuGUI
- `PlayerInputReader` の論理Ability Slot入力への整理
- Field Definition / Entry Point / `FieldCompositionPipeline`
- Terrain / Collision / Architecture / Nature / Atmosphere / Player / Gameplay Scenario / Pickup / Foreground / CameraのField Installer分割
- VitePress Knowledge Base
- EditMode / PlayModeテスト境界
- semantic-release

## 優先リファクタリング: 完了

第1次・第2次優先リファクタリングは完了扱いとします。以後、リファクタリング自体を目的とした大規模変更は行いません。

### 第1次

- GameContent登録経路の一本化
- Ability Loadout / Slot Binding
- TrainingScenarioDefinitionとProjectAssets / AutoRepair整理
- Training Area Composition分割
- Quest Tracker Presentation分割
- EditMode / PlayModeテスト境界整理

### 第2次

- Save / Game Session起動フロー再編
- Modal UI制御共通化
- Architecture / AI Context Map同期
- Player Runtime Composition分割
- Ability Loadout Policy / Eligibility集約
- uGUI Runtime生成コード整理
- PlayerInputReader論理入力整理
- Field / World CompositionのScene単位アーキテクチャ化
- Stable Field / Entry Point IDとSave Version 4

Field CompositionはPR #64で基盤実装済みです。次はArchitectureをさらに抽象化するのではなく、実際の2つ目のFieldとScene Transitionで利用します。

# P0: 最初のPlayable Chapter

P0では、New Gameから最初のBoss討伐・帰還までを一続きで遊べる状態にします。

## 実施順序

| 順序 | Issue | 内容 | 主な依存 |
| --- | --- | --- | --- |
| 1 | #65 | Save SlotとSave Metadata | 既存Save / Game Session |
| 2 | #66 | Title Screen / New Game / Continue / Load Game | #65 |
| 3 | #67 | 2つ目のFieldとField Transition | Field Composition / Save v4 |
| 4 | #68 | Story Progression / Story Event Trigger | 既存Dialogue / Quest / Save |
| 5 | #69 | Playable Prologue Part 1 | #67, #68 |
| 6 | #70 | Playable Prologue Part 2 | #69 |
| 7 | #71 | Human Town Vertical Slice | #70 |
| 8 | #72 | Inventory / Item / Currency | 既存Content / Save |
| 9 | #73 | Shop / Quest Board | #71, #72 |
| 10 | #74 | 最初のDungeon / Boss | #67, #73 |

機能変更と大規模構造変更は原則として同一PRへ混在させず、各Issueを独立PRで完了させます。

## P0-1: Game Start / Save UX

対象: #65, #66

### 目標

アプリ起動から次を成立させます。

```text
Title
  -> New Game
  -> Play
  -> Save
  -> Quit
  -> Continue
```

最低3 Slotを扱い、各Slotで最終Save日時、プレイ時間、Level、現在地を表示できるようにします。

### 完了条件

- SlotごとのSave / Loadが独立する
- New Gameが既存Runtime Stateを引き継がない
- Continue / Load Gameが正しいSlotを復元する
- 破損・未対応Saveを破壊しない
- Keyboard / Gamepadで開始導線を操作できる

## P0-2: Multi Field Runtime

対象: #67

### 目標

PR #64で整備したField Architectureを実際の複数Sceneで利用します。

```text
Field A
  -> FieldTransitionService
  -> FieldLocation(fieldId, entryPointId)
  -> Scene Load
  -> Field Definition Resolve
  -> Entry Point Resolve
  -> Player Spawn
  -> Field B
```

### 完了条件

- Field A / Bを往復できる
- Field遷移でProgression / Loadout / Quest等を失わない
- Field BでSaveしてField BからContinueできる
- 2つ目のField実装で既存巨大Builderをコピーしない
- Addressablesなしで成立する

## P0-3: Story Progression

対象: #68

### 目標

Questとは別に、本編の章・一度きりEvent・重要な選択状態を表現する軽量なStory Runtimeを追加します。

初期Story Flag例:

- `prologue.awakened`
- `prologue.met_guardian`
- `prologue.guardian_killed`
- `prologue.left_forest`
- `prologue.entered_human_town`

Field進入、Interaction、Dialogue完了、Battle / Defeat Event、Quest完了からStoryを進行できるようにします。

### 制約

- 巨大なStory Engineを作らない
- Story固有ロジックをNPC / Quest / Combat Featureへ直接埋め込まない
- 独自Dialogue DSLやVisual Scriptingを先に導入しない

## P0-4: Playable Prologue

対象: #69, #70

### Part 1

- 主人公が弱い魔物として森で生まれる
- 別の魔物に拾われ、育てられる
- Dialogue / Interaction / Exploration / Combatで基本操作を自然に学ぶ
- 育ての親との関係性をGameplayの中で描く

### Part 2

- 育ての親が人間または冒険者によって殺される
- 主人公が人間への不信を抱く
- 森から脱出する
- 魔物であることを隠してHuman Townへ入る

### 完了条件

New Gameから人間の町への到達まで、途中Save / Continueを含めて一続きでプレイできます。

## P0-5: Human Town / Adventurer Loop

対象: #71, #72, #73

### Human Town

最低限、次を配置します。

- 冒険者ギルド
- 宿屋
- 道具屋
- 一般NPC
- Quest Board
- Field出口

### Inventory / Currency

最低限、次を扱います。

- Consumable
- Material
- Key Item
- Stackable Item
- Currency

Equipment / Craftingはこの段階では追加しません。

### Shop / Quest Board

- CurrencyでItemを購入する
- Quest Boardから最初の冒険者依頼を受ける
- 既存Quest Runtimeを再利用する
- Quest報酬としてItem / Currencyを受け取る

## P0-6: First Dungeon / Boss

対象: #74

### 目標

最初の完成した冒険ループを成立させます。

```text
Town
  -> Quest受注
  -> Field / Dungeon探索
  -> 通常Enemy
  -> Boss
  -> Reward
  -> Town帰還
  -> Quest報告
```

Bossは既存Ability / Combat / Enemy AIを再利用し、最初はHP割合による最低1回のPhase変更だけを追加します。Behavior Treeや巨大なBoss Frameworkは導入しません。

### P0完了条件

- New GameからPrologueを完走できる
- Human Townで冒険者として依頼を受けられる
- Inventory / Currency / Shopが利用できる
- Dungeonへ移動できる
- Bossを倒せる
- Townへ戻ってQuestを報告できる
- Save / Continueを含めて全体が破綻しない
- Keyboard / Gamepadで完走できる

# P1: 本作固有システムと世界拡張

P0完了後に優先順位を再評価します。現時点の候補は次です。

## Monster Identity / Trust / Choice

主人公が魔物であることを隠して人間社会で生活する、本作固有のゲームプレイを追加します。

- Monster Identity
- 正体露見につながる行動
- 重要NPC / Faction単位のTrust
- 必要な場合だけSuspicion
- Story Choice
- 選択結果のStory State保存

全NPCに巨大な好感度パラメータを持たせることは前提にしません。

## Demon Awakening

既存Evolution SystemとStoryを接続し、魔王覚醒を単純なLevel UpではなくStory / Evolution / Choice / Eventの複合結果として扱います。

「魔王になること」と「人間を滅ぼすこと」は同義にせず、覚醒後にも選択肢を残します。

## Equipment / Enemy Variation

P0のInventoryとCombatを実際に遊んだ結果を踏まえて追加します。

候補:

- Weapon / Armor / Accessory
- Melee Enemy
- Ranged Enemy
- Flying Enemy
- Caster Enemy
- 追加Art / Skill

## Region Expansion

世界地図のRegionを1つずつPlayableにします。

推奨順序:

```text
Forest
  -> Human Frontier Town
  -> Human Kingdom
  -> Ancient Ruins
  -> Former Demon Territory
  -> Other Nations
```

各RegionはFieldだけを量産せず、Town / Field / Dungeon / NPC / Enemy / Quest / Story / Progression Contentを一まとまりとして追加します。

# P2: Production / Steam Demo

本編Vertical Sliceが成立してからProduction品質を上げます。

## Presentation

- Hit Stop
- Camera Shake
- Damage Number
- Knockback
- Damage Flash
- Enemy Death Animation
- Ability VFX
- Boss演出
- Character Animation
- BGM / Ambient Sound
- Weather / Lighting / Particle
- HUD / Inventory / Equipment / Quest Journal / Map / Settingsの本番UI

## Content Authoring Tools

実際のコンテンツ量産で必要性が確認できたものだけ追加します。

候補:

- Quest Definition Editor
- Dialogue Editor
- Enemy Definition Editor
- Drop Table Editor
- Field Validation
- Content ID Validation
- Story Flag Validation
- Missing Reference Check

ScriptableObjectで整合性管理が難しくなった時点で、構造化データ形式への移行を再評価します。

## Steam Demo

最初のTown + Dungeon + BossまでをDemoとして切り出せる品質にします。

- Windows Build
- Steam Input
- Resolution / Fullscreen
- Audio Settings
- Key Config
- Controller対応
- Save互換性
- Crash対策
- Performance計測
- Localization基盤
- Steam Deck確認

# Full GameのStory到達点

章構成はStory文書で詳細化しますが、ロードマップ上の大きな流れは次とします。

1. 森での誕生と育ての親との死別
2. 人間社会への潜入
3. 冒険者としての生活と良い人間との出会い
4. 世界と人間・魔族の歴史を知る
5. 信頼していた人間からの裏切り
6. 魔王覚醒
7. 人間を信じるか、魔族の王になるか、どちらにも属さない道を選ぶかを決断する

最終的な分岐は単発の選択肢だけで決めず、ゲーム中の行動・関係・Story Stateを反映できる形を目標とします。

# 当面やらないもの

具体的な機能要件または性能問題が発生するまで、次は導入しません。

- Behavior Tree
- ECS
- 独自DI Framework
- Addressables
- Scene Streaming
- Multiplayer
- Cloud Save
- 大規模Dialogue Framework
- 大規模Quest Framework
- 大規模UI Framework
- Procedural Dungeon
- Switch固有実装
- ScriptableObjectからの全面的なデータ基盤移行

# 判断基準

新規タスクを追加する際は、次を優先します。

1. この変更でゲーム本編が前へ進むか
2. 既存Runtime / Composition境界を再利用できないか
3. 実際の要求がないFrameworkを先回りして作っていないか
4. Save / Stable ID / Input / Test境界を壊していないか
5. 独立PRとして検証可能な大きさにできているか

当面の大きなマイルストーンは、**New Gameから開始して、主人公が人間の町へ入り、冒険者として最初のBossを倒して帰還できる状態**です。
