# アーキテクチャ方針

## 目的

現在の遊べるプロトタイプを維持しながら、NPC、敵、戦闘、会話、複数マップなどの機能追加に耐えられる構造へ段階的に移行します。

過剰な先行設計は避け、実際に必要になった境界だけを整備します。コメントと設計ドキュメントは日本語で記述します。

## 基本原則

- 1クラスに複数の独立した変更理由を持たせない
- 入力デバイスの具体的なキー判定をゲームプレイコードへ書かない
- 入力バインディングは `.inputactions` アセットで管理する
- 1プレイヤー内のInput Action Asset所有者は1つにする
- 物理衝突が必要な移動はRigidbody2D経由で行う
- 地形データはTilemapを正とする
- 描画順の計算規則は共通化する
- Yソート対象は原則 `World` Sorting Layerを使用する
- UIとプレイヤーのライフサイクルを分離する
- カメラはプレイヤー固有クラスへ依存しない
- Interactionは対象固有ロジックに依存しない
- Combatは敵種別や演出に依存しない
- 大きなワールド要素はPrefab境界を持たせる
- プロトタイプ専用処理と恒久的なゲーム機能を分離する
- Steamやコンソール固有処理はゲームプレイコードから直接参照しない

## 現在の主要構成

```text
Assets/
  Resources/
    Art/
      Characters/
        PrototypeSlime/
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
      Input/
        PlayerInputReader.cs
        MoveInputReader.cs
      Math/
    Gameplay/
      Characters/
      Interaction/
        IInteractable.cs
        PlayerInteractor.cs
      Combat/
        IDamageable.cs
        Health.cs
        PlayerMeleeAttack.cs
    Presentation/
      Camera/
        CameraFollow2D.cs
      Characters/
      Rendering/
      UI/
        GameHudView.cs
    Field/
      Prototype/
        PrototypeCameraInstaller.cs
        PrototypeUiInstaller.cs
        PrototypeNpcInteractable.cs
        PrototypeCombatDummy.cs
        PrototypeGameplayFeatureInstaller.cs
    World/
    FieldBootstrap.cs
    SlimeController.cs
```

## 依存方向

基本的な依存方向は次の通りです。

```text
Presentation
    ↓
Gameplay
    ↓
Core
```

`Field/Prototype` は試作シーンを組み立てる外側の層です。CoreやGameplayからPrototype固有実装を参照しません。

InteractionとCombatの恒久的な契約・ロジックは `Gameplay` 配下に置き、試作NPCや訓練用ダミーだけを `Field/Prototype` に置きます。

カメラとUIの恒久的な表示ロジックは `Presentation` 配下に置き、Prototype側は対象の接続とシーン構築だけを担当します。

## 起動とシーン

正規プレイシーンは `Assets/Scenes/Prototype/Prototype.unity` です。

```text
Prototype.unity
  ↓
FieldBootstrap
  ├ PrototypeSceneConfigurator
  ├ PrototypeSortingConfigurator
  ├ PrototypeUiInstaller
  │   └ Canvas / GameHudView
  └ PrototypeWorldBuilder
      ├ PrototypeTilemapContext
      ├ TerrainBuilder
      ├ CollisionMapBuilder
      ├ ArchitectureBuilder
      ├ NatureBuilder
      ├ AtmosphereBuilder
      ├ PrototypeGameplayFeatureInstaller
      │   ├ PrototypeNpcInteractable
      │   └ PrototypeCombatDummy
      ├ PrototypePlayerSpawner
      └ PrototypeCameraInstaller
          └ CameraFollow2D
```

`FieldBootstrap` はプレイヤー初期位置と `playableTileRadius` を所有します。

## 入力アーキテクチャ

入力定義は `PlayerControls.inputactions` に集約します。

```text
Player
  ├ Move
  ├ Attack
  ├ Interact
  ├ Dodge
  └ Pause
```

主要バインディングは次の通りです。

```text
Move     : WASD / Arrow Keys / Gamepad Left Stick
Attack   : J / Gamepad West
Interact : E / Gamepad South
Dodge    : Left Shift / Gamepad East
Pause    : Escape / Gamepad Start
```

`PlayerInputReader` がInput Action Assetの実行時複製を1つだけ所有します。

```text
PlayerControls.inputactions
  ↓
PlayerInputReader
  ├ Move
  ├ AttackPressed
  ├ InteractPressed
  ├ DodgePressed
  └ PausePressed
```

`MoveInputReader` は既存移動系コンポーネントとの互換用アダプターであり、Input Action Assetを独自に所有しません。

DodgeとPauseは入力イベント境界まで実装済みです。実際の回避挙動とポーズ状態管理は後続機能として接続します。

## プレイヤー移動と衝突

```text
PlayerInputReader
  ↓
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

Collision Tilemapにはフィールド外周と校舎基部の衝突Tileを配置しています。

## Interaction Feature

```text
PlayerInputReader.InteractPressed
  ↓
PlayerInteractor
  ↓
IInteractable
  ↓
NPC / 扉 / 宝箱 / 調査対象
```

`PlayerInteractor` はInteract入力時だけ周辺Colliderを探索し、最も近い `IInteractable` を1件選んで実行します。

具体的なNPC会話、扉、宝箱などの処理は各 `IInteractable` 実装側へ委譲します。

現在は `PrototypeNpcInteractable` を配置し、最小Interactionループを確認できます。

## Combat Feature

```text
PlayerInputReader.AttackPressed
  ↓
PlayerMeleeAttack
  ↓
IDamageable
  ↓
Health
  ├ HealthChanged
  ├ Damaged
  └ Died
```

`PlayerMeleeAttack` は敵種別を知りません。

`Health` はHP、ダメージ、生存状態、死亡イベントだけを担当し、敵AI、報酬、死亡演出には依存しません。

現在は `PrototypeCombatDummy` を配置し、攻撃、HP減少、死亡までの最小ループを確認できます。

## カメラアーキテクチャ

カメラ追従はプレイヤーPrefabから分離しています。

```text
PrototypeWorldBuilder
  ↓ Player生成
PrototypeCameraInstaller
  ↓ SetTarget
CameraFollow2D
  ↓ LateUpdate
Main Camera
```

`CameraFollow2D` は任意の `Transform` を追従対象にできます。プレイヤー固有クラスや `SlimeController` を参照しません。

追従処理は `LateUpdate` で実行し、`Vector3.SmoothDamp` によって滑らかに追従します。

カメラのZ座標は固定し、既存プロトタイプの初期構図を維持するためY方向にオフセットを持たせています。

将来、イベントカメラや注視対象を追加する場合は `CameraFollow2D.SetTarget` の切り替えで対応し、プレイヤー側へカメラ制御を追加しません。

## UIアーキテクチャ

旧IMGUIの `PrototypeHud` は削除し、Canvas（uGUI）へ移行しました。

```text
UI Root
  ├ Canvas
  ├ CanvasScaler
  ├ GraphicRaycaster
  └ GameHudView
      └ HUD
          ├ Location Panel
          └ Controls Panel
```

`PrototypeUiInstaller` はシーン側の構成ルートとしてCanvasを生成します。

Canvasは `Screen Space - Overlay` を使用し、`CanvasScaler` は次の設定です。

```text
Scale Mode           : Scale With Screen Size
Reference Resolution : 1920 x 1080
Match                 : 0.5
```

`GameHudView` は表示階層と見た目だけを担当し、ゲームルールやプレイヤー制御を持ちません。

今後のUIは同じUI Root配下へ追加します。

```text
UI Root
  ├ HUD
  ├ Dialogue
  ├ Menu
  └ Notification
```

現在のuGUIテキストはBuilt-in Fontを暫定利用しています。日本語表示とコンソール移植を確実にするため、本番配布前にプロジェクト管理の日本語対応フォントへ置き換えます。

## プレイヤーPrefab

現在の `PrototypeSlime.prefab` は次の構成です。

```text
PrototypeSlime
  ├ PlayerInputReader
  ├ MoveInputReader
  ├ CharacterMotor2D
  ├ PlayerInteractor
  ├ PlayerMeleeAttack
  ├ Rigidbody2D
  ├ CircleCollider2D
  ├ CharacterSquashAnimator
  ├ GroupYSorter
  ├ PrototypeSlimeSpriteAnimator
  └ SlimeController
```

`SlimeController` は必要コンポーネント構成を保証する互換用コンポーネントであり、入力、Interaction、Combat、カメラ、UIのロジックを持ちません。

## Isometric Tilemap地形

地形の描画先はIsometric Tilemapです。

```text
TerrainBuilder
  ↓
PrototypeTilemapContext
  ↓
Ground Tilemap
```

草地と小道は1マスごとのGameObjectとして生成しません。

現在のTile画像は `PrototypeRuntimeTileFactory` が暫定生成します。本番タイルセット導入時はTile供給部分をアセット参照へ置き換え、TerrainBuilderの配置ロジックは維持します。

## ワールドPrefab

校舎、木、街灯はPrefab境界を持ちます。

```text
PrototypeWorldPrefabFactory
  ├ PrototypeCottage.prefab
  ├ PrototypeTree.prefab
  └ PrototypeLamppost.prefab
```

Builderは配置を担当し、内部ビジュアルはPrefab側へ分離しています。

## アイソメトリック描画順

Sorting Layerは次の4つです。

```text
Ground
World
Foreground
UI
```

動的SpriteはY座標から描画順を計算します。

```text
sortingOrder = -round(worldY * precision)
```

## ResourcesとAssembly Definition

`Resources.Load` は現在の少数アセットでは暫定利用します。

主な対象は次です。

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

## P1: 戦闘・NPC・会話を追加する前後で実施する — 完了

### P1-1. Collision Tilemapへ実際の衝突タイルを配置する — 完了

- 外周衝突帯を配置
- 校舎基部へ衝突領域を配置
- Rigidbody2D / Collider2D / TilemapCollider2Dを物理境界の正とする

### P1-2. TerrainBuilderをIsometric Tilemapへ段階移行する — 完了

- 草地と小道をGround Tilemapへ移行
- 地形1マスごとのGameObject生成を廃止

### P1-3. 建物、木、街灯をPrefab管理へ移行する — 完了

- Cottage / Tree / LamppostをPrefab化
- 配置ロジックと内部ビジュアルを分離

### P1-4. 試作スライムをスプライト／アニメーション構造へ置き換える — 完了

- RuntimeShapeFactoryによるプレイヤービジュアル生成を廃止
- ピクセルフレームアセットとSpriteAnimatorへ移行

### P1-5. Attack / Interact / Dodge / PauseをInput Actionsへ追加する — 完了

- Keyboard / Gamepad双方のBindingを追加
- `PlayerInputReader` へInput Action Asset所有を集約

### P1-6. Interactionを独立Featureとして追加する — 完了

- `IInteractable` と `PlayerInteractor` を追加
- 入力、対象探索、対象固有処理を分離

### P1-7. Combatを独立Featureとして追加する — 完了

- `IDamageable`、`Health`、`PlayerMeleeAttack` を追加
- 攻撃入力、HP、敵固有処理を分離

### P1-8. カメラ追従をプレイヤーから独立させる — 完了

- `CameraFollow2D` をPresentation層へ追加
- 任意のTransformを追従できる構造へ変更
- `PrototypeCameraInstaller` でプレイヤー生成後に追従対象を接続
- プレイヤーPrefabとカメラ制御の直接依存を排除

### P1-9. 本番UIをCanvas（uGUI）へ移行する — 完了

- IMGUIの `PrototypeHud` を削除
- Canvas（uGUI）へ移行
- `CanvasScaler` による解像度スケーリングを追加
- `GraphicRaycaster` をUI Rootへ追加
- `GameHudView` をPresentation層へ追加
- HUD表示とゲームルールを分離

## P2: コンテンツ量が増える前に実施する

1. EditMode / PlayModeテストを追加する
2. `Resources.Load` の文字列参照を減らす
3. 外部の本番Tileアセットを導入し `PrototypeRuntimeTileFactory` を縮小または削除する
4. World Prefab内部の仮図形ビジュアルを本番アートへ置き換える
5. uGUI用の日本語対応フォントをプロジェクトアセットとして管理する
6. Assembly Definitionを必要最小限で導入する
7. 設定値が増えた段階でScriptableObjectへデータ分離する
8. Input ActionのGameplay / UI / Disabledコンテキストを整理する
9. Dodgeの実挙動とPause状態管理を実装する
10. アプリケーション全体設定を `FieldBootstrap` から分離する

## P3: 本番規模へ移行する段階で実施する

1. セーブ機能追加時に `ISaveService` を導入する
2. Steam固有機能追加時にPlatform層を導入する
3. 必要になった時点でAddressablesや非同期ロードを導入する
4. 大規模マップではシーン分割またはストリーミングを検討する
5. コンソール移植向けに描画・メモリ・ロード時間の予算を設定する

## 直近の推奨実施順序

P0とP1は完了しました。次はP2として次の順序を推奨します。

1. EditMode / PlayModeテストを追加する
2. InteractionとCombatのPlay Modeテストを追加する
3. カメラ追従とuGUI HUDのPlay Modeテストを追加する
4. 本番タイルセットとワールドアートへの差し替えを進める
5. uGUI用の日本語対応フォントをアセット化する
6. DodgeとPauseの実挙動を追加する
7. Input ActionのGameplay / UIコンテキストを整理する
8. コンテンツ増加状況を見てasmdefを導入する
9. `Resources.Load` を段階的に削減する

## 移行方針

大規模な一括書き換えではなく、機能追加のタイミングで既存コードを小さく置き換えます。

常に `main` のプロトタイプが遊べる状態を維持し、本番用のシーン、Prefab、Tilemap、UIが揃った機能から `Field/Prototype` への依存を減らします。
