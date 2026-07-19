# アーキテクチャ方針

## 目的

現在の遊べるプロトタイプを維持しながら、NPC、敵、戦闘、会話、複数マップなどの機能追加に耐えられる構造へ段階的に移行します。

過剰な先行設計は避け、実際に必要になった境界だけを整備します。コメントと設計ドキュメントは日本語で記述します。

## 基本原則

- 1クラスに複数の独立した変更理由を持たせない
- 入力デバイスの具体的なキー判定をゲームプレイコードへ書かない
- 入力バインディングは `.inputactions` アセットで管理する
- 物理衝突が必要な移動はRigidbody2D経由で行う
- 地形データはTilemapを正とし、1マスごとのGameObject生成へ戻さない
- 描画順の計算規則は共通化する
- Yソート対象は原則 `World` Sorting Layerを使用する
- UIとプレイヤーのライフサイクルを分離する
- 同じ意味の設定値を複数コンポーネントで所有しない
- 大きなワールド要素はPrefab境界を持たせる
- プロトタイプ専用処理と恒久的なゲーム機能を分離する
- Steamやコンソール固有処理はゲームプレイコードから直接参照しない

## 現在の主要構成

```text
Assets/
  Resources/
    Art/
      Characters/
        PrototypeSlime/              ピクセルフレームアセット
    Input/
      PlayerControls.inputactions
    Prefabs/
      Characters/
        PrototypeSlime.prefab
      World/
        PrototypeCottage.prefab
        PrototypeTree.prefab
        PrototypeLamppost.prefab
  Scenes/
    Prototype/
      Prototype.unity
  Scripts/
    Core/
    Gameplay/
    Presentation/
    Field/
      Prototype/
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
      ├ PrototypeTilemapContext
      ├ TerrainBuilder
      ├ CollisionMapBuilder
      ├ ArchitectureBuilder
      ├ NatureBuilder
      ├ AtmosphereBuilder
      └ PrototypePlayerSpawner
```

`FieldBootstrap` はプレイヤー初期位置と `playableTileRadius` を所有します。

プレイ可能範囲の物理的な境界は `CollisionMapBuilder` がCollision Tilemapへ構築します。プレイヤーの座標Clampは使用しません。

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
CircleCollider2D
  ↕
TilemapCollider2D
  ↑
Collision Tilemap
```

`CharacterMotor2D` は `Update` で入力値を取得し、`FixedUpdate` で物理移動を適用します。

Collision Tilemapには実際の衝突Tileを配置します。

現在は次を衝突対象としています。

- フィールド外周2セル分の衝突帯
- 校舎基部の衝突領域

Collision TilemapのRendererは非表示ですが、`TilemapCollider2D` が物理衝突を担当します。

設定値の所有者は次の通りです。

```text
CharacterMotor2D -> moveSpeed
GroupYSorter     -> precision
FieldBootstrap   -> playerSpawnPosition / playableTileRadius
```

## Isometric Tilemap地形

P1で地形の描画先を実際のIsometric Tilemapへ移行しました。

```text
TerrainBuilder
  ↓
PrototypeTilemapContext
  ↓
Ground Tilemap
```

`TerrainBuilder` は草地と小道を1マスごとのGameObjectとして生成せず、`Ground` TilemapへTileを配置します。

現在のTile画像は外部タイルセット導入前の暫定として `PrototypeRuntimeTileFactory` が生成します。

重要なのは、ワールドデータと描画単位がすでにTilemapへ移行していることです。今後、本番タイルセットを導入する際は `PrototypeRuntimeTileFactory` のTile生成をアセット参照へ置き換え、TerrainBuilderの配置ロジックは維持できます。

背景、手前フレーム、小石など一部の装飾演出は移行期間中のみ `RuntimeShapeFactory` を利用します。

## ワールドPrefab

校舎、木、街灯はBuilder内で直接組み立てず、Resources配下のPrefabを経由して配置します。

```text
PrototypeWorldPrefabFactory
  ├ PrototypeCottage.prefab
  ├ PrototypeTree.prefab
  └ PrototypeLamppost.prefab
```

`ArchitectureBuilder` と `NatureBuilder` は配置位置を決めますが、各オブジェクトの内部ビジュアル構成を持ちません。

現在のPrefab内部では、既存プロトタイプの見た目を維持するため、次のコンポーネントが仮ビジュアルを生成します。

- `PrototypeCottageVisual`
- `PrototypeTreeVisual`
- `PrototypeLamppostVisual`

この境界により、本番アート導入時はPrefab内部を差し替えればよく、フィールド配置ロジックを変更する必要がありません。

街灯の光輪アニメーションは `PrototypeGlowPulse` としてPrefab内部へ閉じ込めています。

## プレイヤーPrefabとスプライトアニメーション

`PrototypeSlime.prefab` は次の責務を持ちます。

```text
PrototypeSlime
  ├ MoveInputReader
  ├ CharacterMotor2D
  ├ Rigidbody2D
  ├ CircleCollider2D
  ├ CharacterSquashAnimator
  ├ GroupYSorter
  ├ PrototypeSlimeSpriteAnimator
  └ SlimeController
```

従来の `PrototypeSlimeView` と `RuntimeShapeFactory` による多層図形生成は削除しました。

現在は `Resources/Art/Characters/PrototypeSlime` のピクセルフレームアセットを読み込み、`PrototypeSlimeSpriteAnimator` がSpriteRendererで次のアニメーションを再生します。

```text
IdleA <-> IdleB
MoveA <-> MoveB
```

移動方向に応じた左右反転にも対応します。

現在のフレームはプロトタイプ用のピクセルアートですが、見た目のデータがコード内の図形定義からアートアセットへ分離されたため、本番スプライトへはフレームアセットを置き換える形で移行できます。

## UI

`PrototypeHud` はプレイヤーPrefabから分離済みです。

```text
Prototype Scene
  ├ World
  │   └ Player
  └ UI Root
      └ PrototypeHud
```

プレイヤーの再生成とUIの生成・破棄は独立しています。

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

`Props` Tilemapは `World` Sorting Layer / Individual Modeを使用します。Transparency Sort AxisはY軸基準です。Unity 6のIndividual Modeは、Tilemap内のSpriteを個別にレンダリングして他Rendererとのソートに参加させる用途で使用します。

## ResourcesとAssembly Definition

`Resources.Load` は現在の少数アセットでは暫定利用します。

対象は主に次です。

- Input Action Asset
- Player Prefab
- World Prefab
- PrototypeSlimeのピクセルフレーム

コンテンツ量が増える前にSerializeField参照や設定アセットへ段階的に移行します。

asmdefはフォルダ構成と依存方向が安定した段階で必要最小限から導入します。

# リファクタリング・リアーキテクチャの優先順位

## P0: ゲーム機能を増やす前に実施する — 完了

### P0-1. 正規シーンとBuild Settingsを統一する — 完了

### P0-2. プレイヤー移動をRigidbody2Dベースへ移行する — 完了

### P0-3. アイソメトリック描画順ルールを確定する — 完了

### P0-4. 設定値の二重管理を解消する — 完了

### P0-5. HUDをプレイヤーPrefabから分離する — 完了

## P1: 戦闘・NPC・会話を追加する前後で実施する

### P1-1. Collision Tilemapへ実際の衝突タイルを配置する — 実装完了

- Collision Tilemapへ外周衝突帯を配置
- 校舎基部へ衝突領域を配置
- `CharacterMotor2D` の座標Clampを無効化
- Rigidbody2D / Collider2D / TilemapCollider2Dを物理境界の正とする

マージ前にPlay Modeで外周と校舎を通り抜けないことを確認します。

### P1-2. TerrainBuilderをIsometric Tilemapへ段階移行する — 完了

- 草地を `Ground` Tilemapへ移行
- 小道を `Ground` Tilemapへ移行
- 地形1マスごとのGameObject生成を廃止
- Tilemap参照を `PrototypeTilemapContext` に集約
- 外部タイルセット未導入のためTile画像生成のみ `PrototypeRuntimeTileFactory` を暫定利用

### P1-3. 建物、木、街灯をPrefab管理へ移行する — 完了

- `PrototypeCottage.prefab` を追加
- `PrototypeTree.prefab` を追加
- `PrototypeLamppost.prefab` を追加
- `PrototypeWorldPrefabFactory` へPrefab生成経路を集約
- `ArchitectureBuilder` と `NatureBuilder` から対象オブジェクトの内部ビジュアル生成を分離

### P1-4. 試作スライムをスプライト／アニメーション構造へ置き換える — 完了

- `PrototypeSlimeView` を削除
- RuntimeShapeFactoryによるプレイヤービジュアル生成を廃止
- ピクセルフレームアセットを追加
- `PrototypeSlimeSpriteAnimator` を追加
- Idle / Moveの2フレームアニメーションを追加
- 左右反転に対応

現在のアート自体はプロトタイプ用です。本番アート制作後は同じ表示境界を維持したままフレームデータを置き換えます。

### P1-5. Attack / Interact / Dodge / PauseをInput Actionsへ追加する — 未着手

### P1-6. Interactionを独立Featureとして追加する — 未着手

### P1-7. Combatを独立Featureとして追加する — 未着手

### P1-8. カメラ追従をプレイヤーから独立させる — 未着手

### P1-9. 本番UIへ移行する — 未着手

## P2: コンテンツ量が増える前に実施する

1. `Resources.Load` の文字列参照を減らす
2. 外部の本番Tileアセットを導入し `PrototypeRuntimeTileFactory` を縮小または削除する
3. World Prefab内部の仮図形ビジュアルを本番アートへ置き換える
4. Assembly Definitionを必要最小限で導入する
5. EditMode / PlayModeテストを追加する
6. 設定値が増えた段階でScriptableObjectへデータ分離する
7. Input ActionのGameplay / UI / Disabledコンテキストを整理する
8. アプリケーション全体設定を `FieldBootstrap` から分離する

## P3: 本番規模へ移行する段階で実施する

1. セーブ機能追加時に `ISaveService` を導入する
2. Steam固有機能追加時にPlatform層を導入する
3. 必要になった時点でAddressablesや非同期ロードを導入する
4. 大規模マップではシーン分割またはストリーミングを検討する
5. コンソール移植向けに描画・メモリ・ロード時間の予算を設定する

## 直近の推奨実施順序

P1-1〜P1-4の実装後は次の順序で進めます。

1. Play ModeでCollision Tilemapの外周・校舎衝突を確認する
2. `PlayerControls.inputactions` にAttack / Interact / Dodge / Pauseを追加する
3. Interaction機能を独立Featureとして追加する
4. NPCを1体配置し、会話可能な最小ループを作る
5. Combat機能を独立Featureとして追加する
6. 敵を1体配置し、攻撃・HP・死亡までの最小ループを作る
7. カメラ追従を独立コンポーネント化する
8. EditMode / PlayModeテストを追加する
9. 本番タイルセットとワールドアートへの差し替えを開始する
10. コンテンツ増加状況を見てasmdefとResources削減を進める

## 移行方針

大規模な一括書き換えではなく、機能追加のタイミングで既存コードを小さく置き換えます。

常に `main` のプロトタイプが遊べる状態を維持し、本番用のシーン、Prefab、Tilemapが揃った機能から `Field/Prototype` への依存を減らします。
