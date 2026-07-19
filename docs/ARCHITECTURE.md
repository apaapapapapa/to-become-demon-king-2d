# アーキテクチャ方針

## 目的

現在の遊べるプロトタイプを維持しながら、NPC、敵、戦闘、会話、複数マップなどの機能追加に耐えられる構造へ段階的に移行します。

過剰な先行設計は避け、実際に必要になった境界だけを整備します。コメントと設計ドキュメントは日本語で記述します。

## 基本原則

- 1クラスに複数の独立した変更理由を持たせない
- 入力デバイスの具体的なキー判定をゲームプレイコードへ書かない
- 入力バインディングは `.inputactions` アセットで管理する
- Input Action MapはGameplayとUIを分離し、同時に有効化しない
- 操作を完全停止する状態はDisabledコンテキストとして明示する
- 物理衝突が必要な移動はRigidbody2D経由で行う
- ゲームバランス値はPrefabやMonoBehaviourへ重複保持せずScriptableObjectへ寄せる
- UIフォントはOSフォントへ依存せずプロジェクト管理のFontアセットを使用する
- 地形データはTilemapを正とする
- 描画順の計算規則は共通化する
- Yソート対象は原則 `World` Sorting Layerを使用する
- UIとプレイヤーのライフサイクルを分離する
- カメラはプレイヤー固有クラスへ依存しない
- Interactionは対象固有ロジックに依存しない
- Combatは敵種別や演出に依存しない
- アセット参照は可能な限りUnityのシリアライズ参照へ寄せる
- 外部アセットは出典とライセンスをプロジェクト内へ記録する
- Steamやコンソール固有処理はゲームプレイコードから直接参照しない

## 現在の主要構成

```text
Assets/
  Art/
    External/
    World/
  Fonts/
    README.md
    DotGothic16-Regular.ttf        # Editor導入後に生成
    OFL_DotGothic16.txt            # Editor導入後に生成
  Resources/
    Input/
      PlayerControls.inputactions
    Prefabs/
      Characters/
      World/
    Settings/
      PrototypeProjectAssets.asset
      Gameplay/
        PlayerCharacterStats.asset
        PlayerMeleeAttack.asset
  Scenes/
    Prototype/
      Prototype.unity
  Scripts/
    Core/
      Input/
    Gameplay/
      Characters/
        Configuration/
      Interaction/
      Combat/
        Configuration/
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

`FieldBootstrap` は起動時に `PrototypeProjectAssets` を1回解決し、その後のPrefab、Sprite、UI Font、Gameplay設定はScriptableObjectの直接参照を利用します。

## PrototypeProjectAssets

主要なアセット参照を次のScriptableObjectへ集約します。

```text
Resources/Settings/PrototypeProjectAssets.asset
```

現在の管理対象は次です。

```text
Player Prefab
Player Character Stats
Player Melee Attack
UI Font
Cottage / Tree / Lamppost Prefab
Cottage / Tree / Lamppost Sprite
Grass / Path Tile Sprite
```

`PrototypePlayerSpawner` や `PrototypeWorldPrefabFactory` は個別のResources文字列パスを持たず、ProjectAssetsから直接参照を受け取ります。

`Resources.Load` は起動時のProjectAssets解決とInput Action Assetの互換フォールバックなど、少数の入口だけに限定します。

## Gameplayデータ

ゲームバランス値はScriptableObjectへ分離します。

### CharacterStatsDefinition

```text
PlayerCharacterStats.asset
  ├ moveSpeed
  └ maxHealth
```

利用経路は次です。

```text
PrototypeProjectAssets
  ↓
PrototypePlayerSpawner
  ├ CharacterMotor2D.Configure
  └ Health.ConfigureMaxHealth
```

`CharacterMotor2D` は移動ロジックを担当し、移動速度の調整値そのものは `CharacterStatsDefinition` を正とします。

### MeleeAttackDefinition

```text
PlayerMeleeAttack.asset
  ├ damage
  ├ attackRadius
  └ attackDistance
```

利用経路は次です。

```text
PrototypeProjectAssets
  ↓
PrototypePlayerSpawner
  ↓
PlayerMeleeAttack.Configure
```

これにより、プレイヤーPrefabからゲームバランス値を分離し、将来の複数キャラクター・装備・敵種別ごとの設定差し替えに対応します。

## 入力コンテキスト

`PlayerControls.inputactions` は次のAction Mapへ分離します。

```text
Gameplay
  ├ Move
  ├ Attack
  ├ Interact
  ├ Dodge
  └ Pause

UI
  ├ Navigate
  ├ Submit
  ├ Cancel
  └ Pause
```

`PlayerInputReader` は `PlayerInputContext` を管理します。

```text
Gameplay
  -> Gameplay Action Mapのみ有効

UI
  -> UI Action Mapのみ有効

Disabled
  -> すべてのAction Mapを無効
```

公開APIは次を基本とします。

```text
EnableGameplayInput()
EnableUiInput()
DisableInput()
SetContext(PlayerInputContext)
```

メニュー、会話、ポーズなどの画面状態を実装する際は、個別コンポーネントを直接Enable/Disableするのではなく入力コンテキストを切り替えます。

GameplayとUIのAction Mapを同時に有効化しないことで、メニュー表示中の誤攻撃やキャラクター移動を防ぎます。

## プレイヤー移動と衝突

```text
PlayerInputReader
  ↓ Gameplay Context
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
MeleeAttackDefinition
  ↓
IDamageable
  ↓
Health
```

`Health` はHP、ダメージ、生存状態、死亡イベントだけを担当します。最大HPはキャラクター設定から注入できます。

## UIと日本語フォント

本番UI基盤はCanvas（uGUI）です。

```text
UI Root
  ├ Canvas
  ├ CanvasScaler
  ├ GraphicRaycaster
  └ GameHudView
```

`GameHudView` はOSフォントを探索しません。`PrototypeProjectAssets.UiFont` からFontアセットを受け取ります。

標準日本語フォントは `DotGothic16-Regular.ttf` とし、Editorツール `JapaneseUiFontInstaller` が公式Google Fontsリポジトリからフォント本体とOFLライセンスを `Assets/Fonts` へ導入します。

```text
JapaneseUiFontInstaller
  ↓
Assets/Fonts/DotGothic16-Regular.ttf
Assets/Fonts/OFL_DotGothic16.txt
  ↓
PrototypeProjectAssets.uiFont
  ↓
PrototypeUiInstaller
  ↓
GameHudView.Initialize
```

自動導入に失敗した場合は次のUnityメニューから再実行します。

```text
Demon King > Project > Install Japanese UI Font
```

フォント導入後はFont本体・ライセンス・更新されたProjectAssetsをGitへコミットし、ローカルPC、CI、Steam／将来のコンソールビルドで同一アセットを使用します。

## カメラ

`CameraFollow2D` は任意のTransformを追従対象として受け取り、プレイヤー固有クラスには依存しません。

## Tilemapと外部地形アセット

Ground Tilemapの描画データにはKenney Isometric Tiles Landscape由来のSpriteを利用します。

`PrototypeRuntimeTileFactory` はインポート済みSpriteを受け取り、Unity Tileオブジェクトだけを生成します。実行時Texture生成は削除済みです。

外部アセットの出典とライセンスは `Assets/Art/External/Kenney/README.md` に記録します。

## World Prefabアート

校舎、木、街灯はPrefab境界を維持しつつ、プロジェクト管理の静的Spriteアートを利用します。

Builderは配置だけを担当し、見た目はPrefab側へ閉じ込めます。

## Assembly Definition

Runtimeコードは `DemonKing.Runtime.asmdef` にまとめています。

```text
DemonKing.Runtime
DemonKing.EditMode.Tests
DemonKing.PlayMode.Tests
```

初期段階ではCore、Gameplay、Presentationを細かく分割せず、1つのRuntime asmdefでテスト可能な境界だけを作ります。

## テスト

### EditMode

- `WorldSortOrderTests`

### PlayMode

- `GameplayAndCameraPlayModeTests`
- `PlayerInputContextPlayModeTests`

`PlayerInputContextPlayModeTests` では、Gameplay / UI / Disabledの切り替え時に有効なAction Mapが意図どおり1つ以下になることを検証します。

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

### P2-2. `Resources.Load` の文字列参照を減らす — 完了

### P2-3. 外部の本番Tileアセットを導入する — 完了

### P2-4. World Prefab内部の仮図形ビジュアルを本番アートへ置き換える — 完了

### P2-5. uGUI用の日本語対応フォントをプロジェクトアセットとして管理する — 完了

- `GameHudView` のOSフォント依存を削除
- `PrototypeProjectAssets.uiFont` からuGUIへFontを注入
- `JapaneseUiFontInstaller` を追加
- DotGothic16とOFLライセンスを `Assets/Fonts` へ導入する構造を追加
- `PrototypeProjectAssetsAutoRepair` がFont参照を自動修復

### P2-6. Assembly Definitionを必要最小限で導入する — 完了

### P2-7. 設定値が増えた段階でScriptableObjectへデータ分離する — 完了

- `CharacterStatsDefinition` を追加
- `PlayerCharacterStats.asset` に移動速度・最大HPを分離
- `MeleeAttackDefinition` を追加
- `PlayerMeleeAttack.asset` にダメージ・攻撃半径・攻撃距離を分離
- `PrototypePlayerSpawner` から各Gameplayコンポーネントへ設定を注入
- プレイヤーPrefabから重複するバランス値を削除

### P2-8. Input ActionのGameplay / UI / Disabledコンテキストを整理する — 完了

- `Player` Action Mapを `Gameplay` へ改名
- `UI` Action Mapを追加
- Navigate / Submit / CancelをUI入力として分離
- `PlayerInputContext` を追加
- `PlayerInputReader` がGameplay / UI / Disabledを排他的に切り替える構造へ変更
- `PlayerInputContextPlayModeTests` を追加

### P2-9. Dodgeの実挙動とPause状態管理を実装する — 未着手

### P2-10. アプリケーション全体設定をFieldBootstrapから分離する — 未着手

## P3: 本番規模へ移行する段階で実施する

1. セーブ機能追加時に `ISaveService` を導入する
2. Steam固有機能追加時にPlatform層を導入する
3. 必要になった時点でAddressablesや非同期ロードを導入する
4. 大規模マップではシーン分割またはストリーミングを検討する
5. コンソール移植向けに描画・メモリ・ロード時間の予算を設定する

## 直近の推奨実施順序

P2-1〜P2-8のうち、P2-5・P2-7・P2-8まで対応済みです。次は次の順序を推奨します。

1. Unity Editorで日本語フォントの初回導入とProjectAssets参照を確定し、生成されたFont・OFL・ProjectAssets差分をGit管理する
2. EditMode / PlayModeテストをすべて実行する
3. Dodgeの実挙動を実装する
4. Pause状態管理を実装し、Pause中に `PlayerInputContext.UI` へ切り替える
5. 会話UIやメニューUIでGameplay / UIコンテキスト切り替えを利用する
6. InteractionとCombatの物理当たり判定テストを追加する
7. FieldBootstrapからアプリケーション全体設定を分離する

## 移行方針

大規模な一括書き換えではなく、機能追加のタイミングで既存コードを小さく置き換えます。

常に `main` のプロトタイプが遊べる状態を維持し、本番用のシーン、Prefab、Tilemap、UI、アートが揃った機能から `Field/Prototype` への依存を減らします。
