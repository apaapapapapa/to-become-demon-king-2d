# アーキテクチャ方針

## 目的

現在の遊べるプロトタイプを維持しながら、NPC、敵、戦闘、会話、複数マップなどの機能追加に耐えられる構造へ段階的に移行します。

過剰な先行設計は避け、実際に必要になった境界だけを整備します。コメントと設計ドキュメントは日本語で記述します。

## 基本原則

- 1クラスに複数の独立した変更理由を持たせない
- 入力デバイスの具体的なキー判定をゲームプレイコードへ書かない
- 入力バインディングは `.inputactions` アセットで管理する
- 物理衝突が必要な移動はRigidbody2D経由で行う
- 描画順の計算規則は共通化する
- Yソート対象は原則 `World` Sorting Layerを使用する
- UIとプレイヤーのライフサイクルを分離する
- 同じ意味の設定値を複数コンポーネントで所有しない
- プロトタイプ専用処理と本番機能を分離する
- Steamやコンソール固有処理はゲームプレイコードから直接参照しない

## 現在の主要構成

```text
Assets/
  Resources/
    Input/PlayerControls.inputactions
    Prefabs/Characters/PrototypeSlime.prefab
  Scenes/Prototype/Prototype.unity
  Scripts/
    Core/
    Gameplay/
    Presentation/
    Field/Prototype/
    World/
    FieldBootstrap.cs
    SlimeController.cs
  Editor/
```

依存方向は原則として次の通りです。

```text
Presentation
    ↓
Gameplay
    ↓
Core
```

`Field/Prototype` は試作シーンを組み立てる外側の層です。CoreやGameplayからPrototype固有実装を参照しません。

## 起動とシーン

正規プレイシーンは `Assets/Scenes/Prototype/Prototype.unity` のみです。

```text
Prototype.unity
  ↓
FieldBootstrap
  ├ PrototypeSceneConfigurator
  ├ PrototypeSortingConfigurator
  ├ PrototypeUiInstaller
  └ PrototypeWorldBuilder
```

`FieldBootstrap` はフィールド固有の構成ルートです。プレイヤー初期位置とプレイ可能範囲もここが所有し、ワールド構築側へ明示的に渡します。

## 入力

入力定義は `PlayerControls.inputactions` に集約します。

現在は `Player/Move` を持ち、WASD、矢印キー、ゲームパッド左スティックに対応しています。

`MoveInputReader` が論理入力を提供し、`CharacterMotor2D` は具体的なキーを知りません。

## プレイヤー移動と衝突

```text
MoveInputReader
  ↓
CharacterMotor2D
  ↓
Rigidbody2D.MovePosition
  ↓
Collider2D / TilemapCollider2D
```

`CharacterMotor2D` は `Update` で入力値を取得し、`FixedUpdate` で物理移動を適用します。

設定値の所有者は次の通りです。

```text
CharacterMotor2D -> moveSpeed
GroupYSorter     -> precision
FieldBootstrap   -> playerSpawnPosition / playableHalfExtents
```

プレイ可能範囲は `FieldBootstrap` から `PrototypeWorldBuilder`、`PrototypePlayerSpawner` を経由して `CharacterMotor2D.SetBounds` へ渡します。

## プレイヤーPrefab

`PrototypeSlime.prefab` はプレイヤー個体の責務だけを持ちます。

```text
PrototypeSlime
  ├ MoveInputReader
  ├ CharacterMotor2D
  ├ Rigidbody2D
  ├ CircleCollider2D
  ├ CharacterSquashAnimator
  ├ GroupYSorter
  ├ PrototypeSlimeView
  └ SlimeController
```

`SlimeController` は必要コンポーネント構成を保証する互換用コンポーネントです。移動速度や描画順精度は保持しません。

## UI

`PrototypeHud` はプレイヤーPrefabから分離済みです。

`FieldBootstrap` から `PrototypeUiInstaller` を起動し、プレイヤーとは独立した `UI Root` を生成します。

```text
Prototype Scene
  ├ World
  │   └ Player
  └ UI Root
      └ PrototypeHud
```

これにより、プレイヤーの再生成とUIの生成・破棄を分離しています。

本番UIでは `UI Root` 配下にHUD、Dialogue、Menu、Notificationなどを配置します。

## アイソメトリック描画順

Sorting Layerは次の4つです。

```text
Ground
World
Foreground
UI
```

- 地面は `Ground`
- プレイヤー、NPC、敵、遮蔽物などYソート対象は `World`
- 常に手前へ表示する要素は `Foreground`
- UIは `UI`

動的SpriteはY座標から `sortingOrder` を計算します。

```text
sortingOrder = -round(worldY * precision)
```

`Props` Tilemapは `World` Sorting Layer / Individual Modeを使用します。Transparency Sort AxisはY軸基準です。

## 実行時生成ワールドの移行

現在はIsometric Tilemapの器と実行時生成ワールドが並存しています。恒久運用はせず、次の順序で移行します。

```text
TerrainBuilder
  ↓ Isometric Tilemap
ArchitectureBuilder / NatureBuilder
  ↓ Prefab・Tile・本番アート
AtmosphereBuilder
  ↓ 必要な演出コンポーネントのみ残す
RuntimeShapeFactory
  ↓ Prototype専用化または削除
```

## ResourcesとAssembly Definition

`Resources.Load` は現在の少数アセットでは暫定利用します。アセット数やシーン数が増える前に、SerializeField参照や設定アセットへ段階的に移行します。

asmdefはフォルダ構成と依存方向が安定した段階で必要最小限から導入します。

# リファクタリング・リアーキテクチャの優先順位

## P0: ゲーム機能を増やす前に実施する — 完了

### P0-1. 正規シーンとBuild Settingsを統一する — 完了

- `Prototype.unity` を正規プレイシーンへ統一
- Build Settingsを `Prototype.unity` のみに統一
- 旧 `SampleScene.unity` を削除

### P0-2. プレイヤー移動をRigidbody2Dベースへ移行する — 完了

- Transform直接更新を廃止
- `Rigidbody2D.MovePosition` へ移行
- `Rigidbody2D` と `CircleCollider2D` をプレイヤーPrefabへ追加
- Collision Tilemapと衝突可能な構造へ移行

### P0-3. アイソメトリック描画順ルールを確定する — 完了

- `Ground`、`World`、`Foreground`、`UI` Sorting Layerを定義
- 動的キャラクターと遮蔽物を `World` でYソート
- `Props` Tilemapを `World` / Individual Modeへ統一
- Transparency Sort AxisをY軸基準へ固定

### P0-4. 設定値の二重管理を解消する — 完了

- `moveSpeed` は `CharacterMotor2D` のみが所有
- 描画順 `precision` は `GroupYSorter` のみが所有
- `SlimeController` から重複設定を削除
- プレイ可能範囲をSpawnerの固定値から削除
- プレイヤー初期位置とプレイ可能範囲を `FieldBootstrap` が所有
- 現段階では設定数が少ないためScriptableObjectは導入しない

### P0-5. HUDをプレイヤーPrefabから分離する — 完了

- `PrototypeHud` を `PrototypeSlime.prefab` から削除
- `SlimeController` のHUD依存を削除
- `PrototypeUiInstaller` を追加
- 独立した `UI Root` でHUDを管理
- プレイヤー再生成とUIライフサイクルを分離

## P1: 戦闘・NPC・会話を追加する前後で実施する

1. Collision Tilemapへ実際の衝突タイルを配置し、物理挙動を検証する
2. TerrainBuilderを本番用Isometric Tilemapへ段階移行する
3. 建物、木、街灯などをPrefab・アートアセット管理へ移行する
4. 試作スライムの実行時図形を本番スプライト／アニメーションへ置き換える
5. `PlayerControls.inputactions` にAttack、Interact、Dodge、Pauseを追加する
6. Interactionを独立Featureとして追加する
7. Combatを独立Featureとして追加する
8. カメラ追従をプレイヤーから独立させる
9. 本番UIをCanvasまたはUI Toolkitへ移行する

## P2: コンテンツ量が増える前に実施する

1. `Resources.Load` の文字列参照を減らす
2. Assembly Definitionを必要最小限で導入する
3. EditMode / PlayModeテストを追加する
4. 設定値が増えた段階でScriptableObjectへデータ分離する
5. Input ActionのGameplay / UI / Disabledコンテキストを整理する
6. アプリケーション全体設定を `FieldBootstrap` から分離する

## P3: 本番規模へ移行する段階で実施する

1. セーブ機能追加時に `ISaveService` を導入する
2. Steam固有機能追加時にPlatform層を導入する
3. 必要になった時点でAddressablesや非同期ロードを導入する
4. 大規模マップではシーン分割またはストリーミングを検討する
5. コンソール移植向けに描画・メモリ・ロード時間の予算を設定する

## 直近の推奨実施順序

P0は完了しました。次は次の順序で進めます。

1. Collision Tilemapへテスト用衝突タイルを配置してRigidbody2D移動を検証する
2. TerrainBuilderをIsometric Tilemapへ段階移行する
3. 建物・木・街灯などをPrefab化する
4. Attack / Interact / Dodge / PauseをInput Actionsへ追加する
5. Interaction機能を追加する
6. Combat機能を追加する
7. NPCと敵を含む最小プレイ可能ループを完成させる
8. EditMode / PlayModeテストを追加する
9. コンテンツ増加状況を見てasmdefを導入する
10. `Resources.Load` を段階的に削減する

## 移行方針

大規模な一括書き換えではなく、機能追加のタイミングで既存コードを小さく置き換えます。

常に `main` のプロトタイプが遊べる状態を維持し、本番用のシーン、Prefab、Tilemapが揃った機能から `Field/Prototype` への依存を減らします。
