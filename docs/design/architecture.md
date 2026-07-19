# アーキテクチャ

## この文書の役割

この文書は、プロジェクト全体の責務境界、依存方向、Composition Root、意図的に残している移行境界、今後のリアーキテクチャ判断基準を定義します。

ゲーム体験は `game/`、具体的な技術実装は `technical-design.md`、機能仕様は `specifications/` を参照してください。

## 基本原則

- 1クラスに複数の独立した変更理由を集めない。
- Domain、Core、Gameplay、Presentation、Compositionを分離する。
- プレイ中に変化する状態と、ScriptableObjectのDefinitionを分離する。
- 保存DTOとRuntime Stateを分離する。
- Steamやコンソール固有処理をGameplayへ直接持ち込まない。
- 物理移動はRigidbody2Dを経由する。
- 描画順の計算規則を共通化する。
- ゲームバランス値をPrefabやMonoBehaviourへ重複保持しない。
- UI表示とゲーム状態管理を分離する。
- Bootstrapを肥大化させない。
- 必要性が確認できるまで大規模DIコンテナや過剰なasmdef分割を導入しない。

## レイヤーと責務

### Domain

Unityへ依存しない、ゲーム固有の純C#状態・契約・保存DTOを置きます。

現在の例:

```text
Domain/
  Progression/
    CharacterProgressionState
  Save/
    GameSaveData
    PlayerSaveData
  Combat/
    DamageRequest
    DamageResult
    DefeatContext
  StableContentId
```

`DemonKing.Domain.asmdef` はUnity Engine参照を持ちません。

今後、経験値、スキル解放、進化状態など「ゲームの意味を持ち、SceneやMonoBehaviourに依存しない状態」は原則Domainへ置きます。

### Core

ゲーム固有コンテンツから独立したアプリケーション基盤・入力・共通処理を置きます。

```text
Core/
  Application/
    GamePauseController
    ISaveService
    CharacterProgressionSaveMapper
  Input/
    PlayerInputReader
    MoveInputReader
    PlayerInputContext
  Math/
    WorldSortOrder
```

`ISaveService` は保存先やシリアライズ方式をGameplayから分離する契約です。ファイル保存、クラウド保存、Platform保存はこの契約の外側で実装します。

### Gameplay

Unity上で動くゲームルールとキャラクターの振る舞いを置きます。

```text
Gameplay/
  Characters/
    CharacterMotor2D
    CharacterDodge2D
    Configuration/
      CharacterDefinition
      CharacterStatsDefinition
  Combat/
    Health
    IDamageable
    PlayerMeleeAttack
    Configuration/
      MeleeAttackDefinition
  Interaction/
    IInteractable
    PlayerInteractor
```

GameplayはDomainとCoreを利用できますが、Prototype固有クラスやuGUI Viewへ依存しません。

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

Prototype固有のNPC、訓練用ダミー、ワールド生成順序などを置きます。恒久的なDomainやGameplayルールをここへ蓄積しません。

### Editor

Unity Editorでのみ必要な処理です。

- Prototypeシーン生成
- ProjectAssets / CharacterDefinition参照修復
- 日本語フォント導入

RuntimeコードからEditorツールを参照しません。

## 依存方向

概念上の依存方向:

```text
Field / Prototype Composition
          ↓
Presentation      Gameplay
                      ↓
Core / Application   Domain
          ↘          ↑
             Domain
```

重要なのは、DomainがUnityやPrototypeを知らず、上位のComposition層が具体クラスとUnityアセットを組み合わせることです。

## Definition / Runtime State / Save DTO

成長システムを含む今後のコンテンツでは、次の3種類を分離します。

```text
Definition
  CharacterDefinition
  CharacterStatsDefinition
  MeleeAttackDefinition
  DodgeDefinition
       ↓ 初期化・参照
Runtime State
  CharacterProgressionState
       ↓ Mapper
Save DTO
  GameSaveData / PlayerSaveData
```

### Definition

ScriptableObjectで、変更されないコンテンツ定義・バランス値・安定ID・Prefab参照を保持します。

### Runtime State

プレイ中に変化する状態です。ScriptableObjectそのものを書き換えません。

### Save DTO

保存用のバージョン付きデータ構造です。Runtime Stateと相互変換します。

この分離により、将来のレベル、経験値、スキル解放、進化状態、セーブ移行を扱いやすくします。

## Stable Content ID

キャラクター、Ability、将来のスキル・進化Nodeなどは、表示名やUnity Asset名ではなく安定IDで関連付ける方針です。

例:

```text
character.player.slime
ability.basic_melee
```

Save DataやKnowledge Baseで参照するIDは、リネームで意味が変わらない安定IDを使用します。

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
```

`FieldBootstrap` はProjectAssetsを解決してApplicationInstallerへ処理を渡すだけです。起動順序や設定値をFieldBootstrapへ戻しません。

## 設定とアセット参照

### PrototypeProjectAssets

Prototype実行時に必要な主要参照を集約します。

現在、プレイヤー関連は個別のPrefab・Stats・Attack・Dodge参照を直接並べるのではなく、`CharacterDefinition` から辿る構造へ寄せています。

### CharacterDefinition

安定Character IDと、キャラクターを構成する主要Definition・Prefab参照を集約します。

```text
CharacterDefinition
  ├ characterId
  ├ prefab
  ├ statsDefinition
  ├ basicMeleeAttackDefinition
  └ dodgeDefinition
```

キャラクター数が増えた際も、Composition側が複数の個別アセット参照を知りすぎない構造を維持します。

## InputとApplication State

`PlayerInputReader` はGameplay / UI / Disabledを排他的に切り替えます。

Pause状態は `GamePauseController` が管理し、uGUIの `PauseMenuView` は状態変更を表示するだけです。

将来、会話・メニュー・カットシーンなどが増えた場合も、個別Gameplayコンポーネントを場当たり的にEnable/Disableせず、入力・ゲーム状態の境界を明示します。

## Combat境界

Combatは単なるHP減算だけでなく、成長・報酬・撃破結果へ接続できる境界を持ちます。

```text
DamageRequest
  ↓
Damageable / Health
  ↓
DamageResult
  ↓
DefeatContext
  ↓ 将来
Reward / Experience / Drop
```

具体的な経験値や報酬の付与はHealthやPlayerMeleeAttackへ直接埋め込まず、後続のReward Service等へ接続します。

## 意図的に残している移行境界

### `Field/Prototype`

完成版の恒久層ではなく、現在のPrototypeをCompositionする領域です。

### `SlimeController`

既存Prototype Player Prefab互換のための薄いRequireComponent集約／マーカーです。新しいゲームロジックを追加しません。

### `RuntimeShapeFactory`

Prototype専用の軽量装飾・雰囲気確認用途です。主要な地形、キャラクター、建物の正にはしません。

### `Resources`

現在は少数の入口で利用しています。コンテンツ量やロード要件が増えるまでAddressablesへ先行移行しません。

### `PrototypeProjectAssetsAutoRepair`

Editor上の参照切れやImport不整合を復旧する保守ツールです。Runtimeの通常動作がAutoRepairへ依存しないことを前提とします。

## 完了済みの基盤整備

P0〜P2に加え、成長システム実装前の境界整備まで完了しています。

- Rigidbody2D / Collision Tilemap
- Isometric描画順
- Input Action / Input Context
- Interaction / Combat
- uGUI / Camera / Pause / Dodge
- ScriptableObject Definition
- ApplicationInstaller
- EditMode / PlayModeテスト
- Domain assembly
- CharacterDefinition
- CharacterProgressionState
- Save DTO / ISaveService境界
- Combat Result / Defeat Context境界

## 直近の拡張方針

現在の次段階は、成長・報酬の実装です。

1. 経験値テーブル
2. Reward Service
3. 撃破結果から経験値加算への接続
4. Ability / Skill
5. Evolution
6. NPC・会話・敵AI・クエスト

セーブの具体的な保存先、Platform実装、Addressables、Sceneストリーミングは必要性が発生した段階で追加します。

## 新しいリアーキテクチャを行う判断基準

- 同じ変更理由で複数箇所を毎回修正している。
- Platform固有コードがGameplayへ漏れ始めた。
- コンテンツ量によりResourcesや単一Sceneの運用が限界になった。
- テストが困難なため責務分離が必要になった。
- ScriptableObjectだけでは大量データの整合性管理が難しくなった。
- 複数機能が同じRuntime Stateを別々に管理し始めた。

「将来使うかもしれない」だけでは新しい抽象化を追加しません。
