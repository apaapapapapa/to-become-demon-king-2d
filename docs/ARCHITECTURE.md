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
- アセット参照は可能な限りUnityのシリアライズ参照へ寄せる
- 外部アセットは出典とライセンスをプロジェクト内へ記録する
- 仮図形を恒久的なワールドアートとして残さない
- Steamやコンソール固有処理はゲームプレイコードから直接参照しない

## 現在の主要構成

```text
Assets/
  Art/
    External/
      Kenney/
        grass_a.png
        grass_b.png
        README.md
    World/
      cottage.png
      tree.png
      lamppost.png
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
    Settings/
      PrototypeProjectAssets.asset
  Scenes/
    Prototype/
      Prototype.unity
  Scripts/
    Core/
    Gameplay/
      Characters/
      Interaction/
      Combat/
    Presentation/
      Camera/
      Characters/
      Rendering/
      UI/
    Field/
      Prototype/
    World/
    DemonKing.Runtime.asmdef
    FieldBootstrap.cs
    SlimeController.cs
  Tests/
    EditMode/
    PlayMode/
  Editor/
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

`Field/Prototype` は試作シーンを組み立てる外側の構成層です。CoreやGameplayからPrototype固有クラスを参照しません。

InteractionとCombatの恒久的な契約・ロジックは `Gameplay` 配下に置き、試作NPCや訓練用ダミーだけを `Field/Prototype` に置きます。

## 起動構造

```text
Prototype.unity
  ↓
FieldBootstrap
  ├ PrototypeSceneConfigurator
  ├ PrototypeSortingConfigurator
  ├ PrototypeUiInstaller
  └ PrototypeWorldBuilder
      ├ PrototypeTilemapContext
      ├ PrototypeRuntimeTileFactory
      ├ TerrainBuilder
      ├ CollisionMapBuilder
      ├ PrototypeWorldPrefabFactory
      ├ ArchitectureBuilder
      ├ NatureBuilder
      ├ AtmosphereBuilder
      ├ PrototypeGameplayFeatureInstaller
      ├ PrototypePlayerSpawner
      └ PrototypeCameraInstaller
```

`FieldBootstrap` は起動時に `PrototypeProjectAssets` を1回だけ解決し、その後のPrefab、World Art、Terrain SpriteはScriptableObjectの直接参照を利用します。

## アセット参照

### PrototypeProjectAssets

主要なコンテンツ参照を次のScriptableObjectへ集約します。

```text
Resources/Settings/PrototypeProjectAssets.asset
```

管理対象は次です。

```text
Player Prefab
Cottage Prefab
Tree Prefab
Lamppost Prefab
Cottage Sprite
Tree Sprite
Lamppost Sprite
Grass Tile Sprite
Path Tile Sprite
```

従来は `PrototypePlayerSpawner` と `PrototypeWorldPrefabFactory` が個別の `Resources.Load` 文字列パスを持っていましたが、現在は `PrototypeProjectAssets` から直接参照を受け取ります。

`Resources.Load` は現時点で完全廃止せず、起動時のProjectAssets解決とInput Action Assetの互換フォールバックなど、少数の入口だけに限定します。

今後SerializeFieldによるシーン参照やAddressablesへ移行する場合も、利用側のSpawnerやBuilderを変更せず、アセット供給側を置き換える方針です。

## 入力

`PlayerControls.inputactions` に次のActionを定義しています。

```text
Move
Attack
Interact
Dodge
Pause
```

`PlayerInputReader` がInput Action Assetの実行時インスタンスを所有し、ゲームプレイ側には論理入力だけを公開します。

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
```

Collision Tilemapにはフィールド外周と校舎基部の衝突セルを配置しています。

## Interaction

```text
PlayerInputReader.InteractPressed
  ↓
PlayerInteractor
  ↓
IInteractable
  ↓
NPC / 扉 / 宝箱 / 調査対象
```

`PlayerInteractor` は具体的な対象ロジックを知りません。

## Combat

```text
PlayerInputReader.AttackPressed
  ↓
PlayerMeleeAttack
  ↓
IDamageable
  ↓
Health
```

`Health` はHP、ダメージ、生存状態、死亡イベントだけを担当します。敵AI、報酬、死亡演出は外側の機能として追加します。

## カメラ

`CameraFollow2D` は任意のTransformを追従対象として受け取り、プレイヤー固有クラスには依存しません。

```text
PrototypeWorldBuilder
  ↓ Player生成
PrototypeCameraInstaller
  ↓
CameraFollow2D
  ↓
Main Camera
```

## UI

本番UI基盤はCanvas（uGUI）です。

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

UIはゲームルールやプレイヤー生成ライフサイクルから分離します。

## Tilemapと外部地形アセット

Ground Tilemapの描画データには外部アセットを導入しています。

```text
Kenney Isometric Tiles Landscape
  ↓
Assets/Art/External/Kenney/grass_a.png
Assets/Art/External/Kenney/grass_b.png
  ↓
PrototypeProjectAssets
  ↓
PrototypeRuntimeTileFactory
  ↓
Ground Tilemap
```

`PrototypeRuntimeTileFactory` は従来、Texture2Dと菱形Spriteを実行時生成していました。

現在はインポート済みSpriteを受け取り、Unity Tileオブジェクトだけを生成します。実行時Texture生成は削除済みです。

外部アセットの出典とライセンスは `Assets/Art/External/Kenney/README.md` に記録します。

## World Prefabアート

校舎、木、街灯はPrefab境界を維持しつつ、RuntimeShapeFactoryによる多層仮図形からプロジェクト管理の静的Spriteアートへ移行しています。

```text
PrototypeProjectAssets
  ├ cottage.png
  ├ tree.png
  └ lamppost.png
      ↓
PrototypeWorldPrefabFactory
      ↓
PrototypeCottageVisual / PrototypeTreeVisual / PrototypeLamppostVisual
      ↓
SpriteRenderer
```

Builderは配置だけを担当し、見た目はPrefab側へ閉じ込めます。

今後アーティスト制作の最終Spriteへ差し替える場合は `PrototypeProjectAssets` の参照を変更し、配置ロジックを変更しない方針です。

## Assembly Definition

Runtimeコードは `DemonKing.Runtime.asmdef` にまとめています。

```text
DemonKing.Runtime
DemonKing.EditMode.Tests
DemonKing.PlayMode.Tests
```

初期段階ではCore、Gameplay、Presentationを細かく分割せず、1つのRuntime asmdefでテスト可能な境界だけを作ります。

依存関係が実際に複雑化した段階で、Core / Gameplay / Presentationへ追加分割します。

## テスト

### EditMode

`WorldSortOrderTests` でYソートの共通計算規則を検証します。

### PlayMode

`GameplayAndCameraPlayModeTests` で次を検証します。

- `Health` の致死ダメージと死亡イベント
- `CameraFollow2D` の追従
- カメラZ座標の維持

テストはUnity Test RunnerからEditMode / PlayModeを分離して実行します。

今後Interaction、Combat当たり判定、Input、uGUIへテスト範囲を拡張します。

# リファクタリング・リアーキテクチャ優先順位

## P0: ゲーム機能を増やす前に実施する — 完了

### P0-1. 正規シーンとBuild Settingsを統一する — 完了
### P0-2. プレイヤー移動をRigidbody2Dベースへ移行する — 完了
### P0-3. アイソメトリック描画順ルールを確定する — 完了
### P0-4. 設定値の二重管理を解消する — 完了
### P0-5. HUDをプレイヤーPrefabから分離する — 完了

## P1: 戦闘・NPC・会話を追加する前後で実施する — 完了

### P1-1. Collision Tilemapへ実際の衝突タイルを配置する — 完了
### P1-2. TerrainBuilderをIsometric Tilemapへ移行する — 完了
### P1-3. 建物、木、街灯をPrefab管理へ移行する — 完了
### P1-4. 試作スライムをスプライト／アニメーション構造へ置き換える — 完了
### P1-5. Attack / Interact / Dodge / PauseをInput Actionsへ追加する — 完了
### P1-6. Interactionを独立Featureとして追加する — 完了
### P1-7. Combatを独立Featureとして追加する — 完了
### P1-8. カメラ追従をプレイヤーから独立させる — 完了
### P1-9. 本番UIをCanvas（uGUI）へ移行する — 完了

## P2: コンテンツ量が増える前に実施する

### P2-1. EditMode / PlayModeテストを追加する — 完了

- `DemonKing.Runtime.asmdef` を追加
- EditModeテストassemblyを追加
- PlayModeテストassemblyを追加
- Yソート計算をEditModeで検証
- HealthとCameraFollow2DをPlayModeで検証

### P2-2. `Resources.Load` の文字列参照を減らす — 完了

- `PrototypeProjectAssets` を追加
- Player Prefab参照をSpawnerの文字列パスから直接参照へ変更
- World Prefab参照をFactoryの文字列パスから直接参照へ変更
- Terrain SpriteとWorld ArtもProjectAssetsへ集約
- `Resources.Load` は少数の入口・互換フォールバックに限定

### P2-3. 外部の本番Tileアセットを導入する — 完了

- Kenney Isometric Tiles Landscape由来の地形Spriteを導入
- 外部アセットの出典とライセンスを記録
- `PrototypeRuntimeTileFactory` からRuntime Texture2D生成を削除
- Factoryはインポート済みSpriteからTileだけを生成

### P2-4. World Prefab内部の仮図形ビジュアルを本番アートへ置き換える — 完了

- Cottage / Tree / Lamppost用の静的Spriteアートを追加
- World PrefabのRuntimeShapeFactory依存を削除
- `PrototypeWorldPrefabFactory` がProjectAssetsからSpriteを接続
- 将来の最終アート差し替え時にBuilderを変更しない構造へ移行

### P2-5. uGUI用の日本語対応フォントをプロジェクトアセットとして管理する — 未着手

### P2-6. Assembly Definitionを必要最小限で導入する — 完了

- `DemonKing.Runtime` を追加
- EditMode / PlayModeテストassemblyを分離
- 既存EditorツールのAssembly-CSharp固定文字列依存を削除

### P2-7. 設定値が増えた段階でScriptableObjectへデータ分離する — 一部完了

- アセット参照は `PrototypeProjectAssets` へ分離済み
- キャラクター能力値や戦闘データの本格的なデータ分離は未着手

### P2-8. Input ActionのGameplay / UI / Disabledコンテキストを整理する — 未着手

### P2-9. Dodgeの実挙動とPause状態管理を実装する — 未着手

### P2-10. アプリケーション全体設定をFieldBootstrapから分離する — 未着手

## P3: 本番規模へ移行する段階で実施する

1. セーブ機能追加時に `ISaveService` を導入する
2. Steam固有機能追加時にPlatform層を導入する
3. 必要になった時点でAddressablesや非同期ロードを導入する
4. 大規模マップではシーン分割またはストリーミングを検討する
5. コンソール移植向けに描画・メモリ・ロード時間の予算を設定する

## 直近の推奨実施順序

P2-1〜P2-4とP2-6まで完了しました。次は次の順序で進めます。

1. Unity Test RunnerでEditMode / PlayModeテストを実行し、テストassembly構成を確定する
2. InteractionとCombatの物理当たり判定をPlayModeテストへ追加する
3. uGUI用の日本語対応フォントをプロジェクトアセット化する
4. Input ActionをGameplay / UIコンテキストへ分離する
5. Dodgeの実挙動を実装する
6. Pause状態管理とUI入力切り替えを実装する
7. キャラクター能力値、攻撃データ、NPCデータを必要に応じてScriptableObject化する
8. FieldBootstrapからアプリケーション全体設定を分離する

## 移行方針

大規模な一括書き換えではなく、機能追加のタイミングで既存コードを小さく置き換えます。

常に `main` のプロトタイプが遊べる状態を維持し、本番用のシーン、Prefab、Tilemap、UI、アートが揃った機能から `Field/Prototype` への依存を減らします。
