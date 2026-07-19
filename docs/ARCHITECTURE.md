# アーキテクチャ方針

## この文書の役割

この文書は、プロジェクト全体の責務境界、依存方向、Composition Root、移行中の境界、今後のリアーキテクチャ優先順位を定義します。

- ゲーム体験とコンテンツ方針: `GAME_DIRECTION.md`
- 現在の実装仕様と開発規約: `TECHNICAL_DESIGN.md`
- アーキテクチャ上の責務と将来の境界: 本書

P0〜P2はすでに完了した整備履歴です。今後のTODOとして読むのではなく、「なぜ現在の構造になっているか」を確認するために残します。

## 目的

現在の遊べるプロトタイプを維持しながら、NPC、会話、敵AI、クエスト、成長、複数マップなどの機能追加に耐えられる構造を保ちます。

過剰な先行設計は避け、実際のゲーム機能が必要とした境界だけを追加します。

## 基本原則

- 1クラスに複数の独立した変更理由を集めない
- Input、Gameplay、Presentation、Application Stateを分離する
- Steamやコンソール固有処理をGameplayへ直接持ち込まない
- 物理移動はRigidbody2Dを経由する
- 描画順の計算規則を共通化する
- ゲームバランス値をPrefabやMonoBehaviourへ重複保持しない
- UI表示とゲーム状態管理を分離する
- Bootstrapを肥大化させない
- アセット参照は可能な範囲でUnityのシリアライズ参照を使う
- 依存性注入コンテナや細かすぎるasmdefは、必要性が確認できるまで導入しない
- コメントと設計ドキュメントは日本語で記述する

## レイヤーと責務

### Core

ゲーム固有コンテンツに依存しない基盤を置きます。

現在の例:

```text
Core/
  Application/
    GamePauseController
  Input/
    PlayerInputReader
    MoveInputReader
    PlayerInputContext
  Math/
    WorldSortOrder
```

Coreから `Gameplay`、`Presentation`、`Field/Prototype` を参照しません。

### Gameplay

ゲームルールとプレイヤー／キャラクターの振る舞いを置きます。

```text
Gameplay/
  Characters/
    CharacterMotor2D
    CharacterDodge2D
    Configuration/
  Combat/
    Health
    IDamageable
    PlayerMeleeAttack
    Configuration/
  Interaction/
    IInteractable
    PlayerInteractor
```

Gameplayは必要に応じてCoreへ依存できますが、Prototype固有クラスやuGUI Viewへ依存しません。

### Presentation

画面表示、描画順、カメラ、アニメーション、uGUI Viewを置きます。

```text
Presentation/
  Camera/
  Characters/
  Rendering/
  UI/
```

Presentationは表示に必要な範囲でCoreやGameplayの状態・イベントを参照できますが、ゲームルールの決定主体にはしません。

例として、`PauseMenuView` は `GamePauseController` の状態変更イベントを表示するだけで、TimeScaleやInput Contextを変更しません。

### Field/Prototype

現在のPrototypeシーンを組み立てるComposition層です。

```text
Field/Prototype/
  Configuration/
  PrototypeApplicationInstaller
  PrototypeWorldBuilder
  PrototypePlayerSpawner
  PrototypeWorldPrefabFactory
  PrototypeGameplayFeatureInstaller
  ...
```

Prototype固有のNPC、訓練用ダミー、ワールド構築順序などはここへ置きます。

CoreやGameplayから `Field/Prototype` を参照しません。

### Editor

シーン生成、アセット参照修復、日本語フォント導入など、Unity Editorでのみ必要な処理を置きます。

RuntimeコードからEditorツールを参照しません。

## 依存方向

概念上の依存方向は次です。

```text
Field / Prototype Composition
       ↓
Presentation    Gameplay
       ↘         ↓
          Core
```

重要なのは「上位のComposition層が具体クラスを組み合わせ、CoreやGameplayがPrototype固有事情を知らない」ことです。

PresentationとGameplayを常に直列の上下関係として扱うのではなく、表示が必要な状態・イベントだけを明示的に接続します。

## 起動構造

```text
Prototype.unity
  ↓
FieldBootstrap
  ↓
PrototypeProjectAssets
  ↓
PrototypeApplicationInstaller
  ├ PrototypeApplicationSettings
  ├ PrototypeSceneConfigurator
  ├ PrototypeSortingConfigurator
  ├ PrototypeWorldBuilder
  │   ├ PrototypeTilemapContext
  │   ├ PrototypeRuntimeTileFactory
  │   ├ TerrainBuilder
  │   ├ CollisionMapBuilder
  │   ├ PrototypeWorldPrefabFactory
  │   ├ ArchitectureBuilder
  │   ├ NatureBuilder
  │   ├ AtmosphereBuilder
  │   ├ PrototypeGameplayFeatureInstaller
  │   ├ PrototypePlayerSpawner
  │   └ PrototypeCameraInstaller
  ├ GamePauseController
  └ PrototypeUiInstaller
      ├ GameHudView
      └ PauseMenuView
```

`FieldBootstrap` は `PrototypeProjectAssets` を解決して `PrototypeApplicationInstaller` へ処理を渡すだけです。

起動順序や設定値をFieldBootstrapへ戻さないことを基本方針とします。

## 設定とアセット参照

### PrototypeProjectAssets

Prototype実行時に必要なアセット参照の集約点です。

主な参照:

```text
PrototypeApplicationSettings
Player Prefab
Player Character Stats
Player Melee Attack
Player Dodge
UI Font
World Prefabs
World Sprites
Terrain Sprites
```

SpawnerやBuilderが個別のResourcesパスを持つ構造へ戻さないようにします。

### PrototypeApplicationSettings

アプリケーション起動時の値を保持します。

```text
playerSpawnPosition
playableTileRadius
pausedTimeScale
```

言語、品質設定、初期シーンなどのアプリケーション全体設定が増えた場合も、FieldBootstrapのSerializeFieldへ直接追加するのではなく、設定アセットまたは専用サービスへ分離します。

### Gameplay Definition

現在のゲームバランス定義:

```text
CharacterStatsDefinition
MeleeAttackDefinition
DodgeDefinition
```

静的な調整値はDefinitionを正とし、PrefabやMonoBehaviourに同じ値を二重管理しません。

## InputとApplication State

Input Action Map:

```text
Gameplay
UI
```

Runtime context:

```text
Gameplay
UI
Disabled
```

`PlayerInputReader` がAction Mapの有効状態を一元管理します。

Pauseでは次の責務を分離します。

```text
GamePauseController
  -> Pause状態
  -> Time.timeScale
  -> Input Context

PauseMenuView
  -> Pause画面の表示
```

今後の会話やメニューも、Gameplay入力を個別に止めるのではなくInput Context切り替えを基本にします。

## 移動と特殊移動

通常移動:

```text
MoveInputReader
  ↓
CharacterMotor2D
  ↓
Rigidbody2D.MovePosition
```

Dodge:

```text
PlayerInputReader.DodgePressed
  ↓
CharacterDodge2D
  ├ CharacterMotor2D.SetMovementLocked
  └ Rigidbody2D.MovePosition
```

通常移動へDodge、ノックバック、カットシーン移動などを直接書き足して巨大化させない方針です。

複数の移動制御が競合する段階になった場合は、単純なboolロックをMovement StateやMotion Controllerへ拡張することを検討します。

## InteractionとCombat

恒久的な契約はGameplayへ置きます。

```text
IInteractable
IDamageable
Health
```

Prototype側はこれらの契約を使って、試作NPCや訓練用ダミーを組み立てます。

NPC会話、敵AI、報酬、死亡演出などを `PlayerInteractor` や `Health` に直接追加しません。

## ワールドとアート

地形はIsometric Tilemapを正とします。

主要な継続利用アセット:

- Terrain Sprite
- Player Prefab
- Cottage Prefab
- Tree Prefab
- Lamppost Prefab

`RuntimeShapeFactory` は完全には削除していません。現在はPrototype専用の軽量な装飾、ランドマーク、雰囲気確認用表現に限定します。

主要アートをRuntimeShapeFactoryへ戻さず、コンテンツ制作が進んだ箇所からPrefabまたは静的アセットへ移行します。

## 描画順

Yソート計算は `WorldSortOrder` を共通ルールとします。

```text
sortingOrder = -round(worldY * precision) + offset
```

複数SpriteRendererを持つ対象は `GroupYSorter` を使用します。

Yソート対象は原則 `World` Sorting Layerを使用し、オブジェクトごとに独自計算式を実装しません。

## UI

本番UI基盤はCanvas（uGUI）です。

```text
UI Root
  ├ GameHudView
  └ PauseMenuView
```

UIはPlayer Prefabから独立したライフサイクルで管理します。

日本語Fontはプロジェクトアセットとして管理し、OSフォントへ依存しません。

## Assembly Definition

現在は必要最小限の構成です。

```text
DemonKing.Runtime
DemonKing.EditMode.Tests
DemonKing.PlayMode.Tests
```

Core / Gameplay / Presentationを別asmdefへ分割するのは、コンパイル時間や依存違反が実際の問題になった段階で検討します。

フォルダ構成だけを理由にassemblyを増やしません。

## テスト方針

現在の主な自動テスト:

```text
EditMode
  WorldSortOrderTests

PlayMode
  GameplayAndCameraPlayModeTests
  PlayerInputContextPlayModeTests
  DodgeAndPausePlayModeTests
```

優先してテストする対象:

- 純粋な計算ルール
- 状態遷移
- Input Context切り替え
- HPや死亡などのGameplayルール
- 物理やUnityライフサイクルを伴う重要境界

将来追加する候補:

- Interaction対象選択
- CombatのOverlap判定
- NPC会話状態
- Quest進行
- Save / Load

## 現在残している移行境界

大筋のリアーキテクチャは完了していますが、次は意図的に残しています。

### Prototype名前空間／クラス

現在のゲーム世界はまだPrototypeシーン中心です。

実際の複数マップや本番ゲームフローが作られるまでは、無理に `Prototype` 名を消しません。本番シーンへ移行する際に、再利用する機能とPrototype固有機能を分離します。

### SlimeController

`SlimeController` は既存Prototype Player Prefabとの互換性を保つための薄いマーカー／RequireComponent集約です。

ゲームプレイロジックを再びSlimeControllerへ追加しません。プレイヤーキャラクターがスライム以外へ変わる、または正式なPlayer compositionが確立した段階で削除または改名を検討します。

### RuntimeShapeFactory

主要アートの生成には使用しませんが、Prototype専用の補助表示には残しています。

### Resources

`Resources.Load` は少数の入口とフォールバックに限定しています。

コンテンツ量やロード要件が増えるまではAddressablesへ一括移行しません。

### PrototypeProjectAssetsAutoRepair

Unityアセット参照が破損した場合にEditor上で復旧する保守ツールです。

Runtimeの正常動作がAutoRepairの毎回実行を前提とする構造にはしません。参照が安定したアセットは通常のUnityシリアライズ参照を正とします。

## 完了した基礎整備

### P0 — 完了

ゲーム機能を増やす前の構造整理:

- 正規シーンとBuild Settingsの統一
- Rigidbody2Dベースの移動
- Yソートルール統一
- 設定値の二重管理解消
- HUDのPlayer Prefabからの分離

### P1 — 完了

Gameplay Feature追加に必要な境界:

- Collision Tilemap
- Isometric Terrain
- World Prefab化
- Player Sprite / Animation構造
- Input Actions
- Interaction
- Combat
- Camera分離
- Canvas（uGUI）

### P2 — 完了

コンテンツ量増加前の基盤:

- EditMode / PlayModeテスト
- Resources文字列参照の削減
- 外部Terrainアセット導入
- 主要World Prefabの静的Sprite化
- 日本語Font管理
- 最小asmdef
- ScriptableObject設定分離
- Gameplay / UI / Disabled Input Context
- Dodge
- Pause状態管理
- FieldBootstrapからApplication Settingsと起動構成を分離

詳細な実装仕様は `TECHNICAL_DESIGN.md` を参照してください。

## P3 — 本番規模へ進む段階

P3は先行してすべて実装するのではなく、実際のゲーム機能に合わせて導入します。

優先候補:

1. NPC会話、敵AI、クエスト、成長など実ゲームコンテンツを増やす
2. セーブが必要になった時点で `ISaveService` と保存データ境界を導入する
3. Steam機能が必要になった時点でPlatform層を導入する
4. コンテンツ量とロード時間が増えた段階でAddressablesやシーン分割を検討する
5. コンソール移植の具体化後に描画・メモリ・ロード時間の予算を設定する

## リファクタリング判断基準

今後は「きれいに見えるから」ではなく、次の兆候が出たときに追加リファクタを行います。

- 同じルールが3箇所以上へ重複し始めた
- 1機能の変更で無関係な複数レイヤーを同時修正する必要がある
- Prototype固有コードを本番シーンでも再利用したくなった
- Input / Pause / Dialogue / Menuの状態競合が増えた
- CharacterMotor2Dの単純な移動ロックでは複数Motionを管理できなくなった
- Resourcesロードが起動時間やメモリの問題になった
- Runtime assemblyが大きくなりコンパイル時間や依存違反が問題になった

常に `main` が遊べる状態を維持し、機能追加と同時に必要な境界だけを段階的に整備します。
