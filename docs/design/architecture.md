# アーキテクチャ

## この文書の役割

この文書は、プロジェクト全体の責務境界、依存方向、Composition Root、意図的に残している移行境界、今後のリアーキテクチャ判断基準を定義します。

ゲーム体験は `game/`、具体的な技術実装は `technical-design.md`、機能仕様は `specifications/` を参照してください。

## 基本原則

- 1クラスに複数の独立した変更理由を集めない。
- Input、Gameplay、Presentation、Application Stateを分離する。
- Steamやコンソール固有処理をGameplayへ直接持ち込まない。
- 物理移動はRigidbody2Dを経由する。
- 描画順の計算規則を共通化する。
- ゲームバランス値をPrefabやMonoBehaviourへ重複保持しない。
- UI表示とゲーム状態管理を分離する。
- Bootstrapを肥大化させない。
- アセット参照は可能な範囲でUnityのシリアライズ参照を使う。
- 必要性が確認できるまで大規模DIコンテナや過剰なasmdef分割を導入しない。

## レイヤーと責務

### Core

ゲーム固有コンテンツに依存しない基盤です。

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

CoreからGameplay、Presentation、Field/Prototypeを参照しません。

### Gameplay

ゲームルールとキャラクターの振る舞いを置きます。

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

Presentationは状態やイベントを表示します。ゲームルールの決定主体にはしません。

### Field / Prototype

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

Prototype固有のNPC、訓練用ダミー、ワールド生成順序などを置きます。恒久的なGameplayルールをここへ蓄積しません。

### Editor

Unity Editorでのみ必要な処理です。

- Prototypeシーン生成
- ProjectAssets参照修復
- 日本語フォント導入

RuntimeコードからEditorツールを参照しません。

## 依存方向

```text
Field / Prototype Composition
       ↓
Presentation    Gameplay
       ↘         ↓
          Core
```

上位のComposition層が具体クラスを組み合わせ、CoreやGameplayがPrototype固有事情を知らないことを重視します。

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
  │   ├ Terrain / Collision / World Prefab
  │   ├ Prototype Gameplay Features
  │   ├ PrototypePlayerSpawner
  │   └ PrototypeCameraInstaller
  ├ GamePauseController
  └ PrototypeUiInstaller
      ├ GameHudView
      └ PauseMenuView
```

`FieldBootstrap` はProjectAssetsを解決してApplicationInstallerへ処理を渡すだけです。起動順序や設定値をFieldBootstrapへ戻さないことを基本方針とします。

## 設定とアセット参照

### PrototypeProjectAssets

Prototype実行時に必要な主要参照を集約します。

- PrototypeApplicationSettings
- Player Prefab
- Player Character Stats
- Player Melee Attack
- Player Dodge
- UI Font
- World Prefabs / Sprites
- Terrain Sprites

SpawnerやBuilderが個別のResourcesパスを持つ構造へ戻さないようにします。

### PrototypeApplicationSettings

アプリケーション起動時の値を保持します。

```text
playerSpawnPosition
playableTileRadius
pausedTimeScale
```

言語、品質設定、初期シーンなどが増えた場合も、FieldBootstrapへ直接追加せず設定アセットまたは専用サービスへ分離します。

### Gameplay Definition

現在の主なゲームバランス設定:

```text
CharacterStatsDefinition
MeleeAttackDefinition
DodgeDefinition
```

Runtime値はScriptableObjectを正とします。Knowledge Baseへ同じ数値を恒常的にコピーしません。

## InputとApplication State

`PlayerInputReader` はGameplay / UI / Disabledを排他的に切り替えます。

Pause状態は `GamePauseController` が管理し、uGUIの `PauseMenuView` は状態変更を表示するだけです。

将来、会話・メニュー・カットシーンなどが増えた場合も、個別Gameplayコンポーネントを場当たり的にEnable/Disableせず、入力・ゲーム状態の境界を明示します。

## 意図的に残している移行境界

### `Field/Prototype`

完成版の恒久層ではなく、現在のPrototypeをCompositionする領域です。実際の機能が成熟したら恒久的なGameplayやPresentationへ移動します。

### `SlimeController`

既存Prototype Player Prefab互換のための薄いRequireComponent集約／マーカーです。新しいゲームロジックを追加しません。

### `RuntimeShapeFactory`

Prototype専用の軽量装飾・雰囲気確認用途です。主要な地形、キャラクター、建物の正にはしません。

### `Resources`

現在は少数の入口で利用しています。コンテンツ量やロード要件が増えるまでAddressablesへ先行移行しません。

### `PrototypeProjectAssetsAutoRepair`

Editor上の参照切れやImport不整合を復旧する保守ツールです。Runtimeの通常動作がAutoRepairへ依存しないことを前提とします。

## P0〜P2で完了した基盤整備

- 正規Scene / Build Settings統一
- Rigidbody2D移動・Collision Tilemap
- Isometric描画順ルール
- 設定値所有者の整理
- HUD分離とuGUI化
- Tilemap / World Prefab / Spriteアート基盤
- Input ActionとGameplay / UI / Disabled Context
- Interaction / Combat Feature分離
- Camera Follow分離
- ScriptableObjectによるCharacter / Attack / Dodge設定
- Dodge実挙動とPause状態管理
- Application SettingsとApplicationInstaller
- EditMode / PlayModeテスト基盤
- 日本語Font管理

この履歴は今後のTODOではなく、現在の構造になった理由を示すものです。

## 今後のP3

実際に必要になった順に導入します。

1. セーブ機能と `ISaveService` 境界
2. Steam固有機能とPlatform層
3. コンソール移植向けPlatform実装
4. Addressables / 非同期ロード
5. 大規模マップのScene分割・ストリーミング
6. 描画・メモリ・ロード時間の性能予算

## 新しいリアーキテクチャを行う判断基準

次のいずれかが実際に発生したときに境界を追加します。

- 同じ変更理由で複数箇所を毎回修正している。
- Platform固有コードがGameplayへ漏れ始めた。
- コンテンツ量によりResourcesや単一Sceneの運用が限界になった。
- テストが困難なため責務分離が必要になった。
- ScriptableObjectだけでは大量データの整合性管理が難しくなった。
- 複数機能が同じゲーム状態を別々に管理し始めた。

「将来使うかもしれない」だけでは新しい抽象化を追加しません。
