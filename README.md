# To Become Demon King 2D

『To Become Demon King 2D』は、Witchbrookなどの作品が持つビジュアルの方向性に着想を得た、Unity製のアイソメトリック2D／2.5D RPGです。

## 技術方針

- Unity 6
- C#
- Universal Render Pipeline（URP）
- 2D Renderer／2D Lighting
- Isometric Tilemap
- Unity Input System
- Canvas（uGUI）
- Rigidbody2D／TilemapCollider2D
- ScriptableObjectによるゲームバランス・起動設定管理
- Unity Test Framework
- Assembly DefinitionによるRuntime / Test分離
- キーボードとゲームパッドに対応
- Steamを第一ターゲットとし、将来のコンソール移植も考慮

## 現在の操作

| 操作 | キーボード | ゲームパッド |
| --- | --- | --- |
| 移動 | WASD / 矢印キー | 左スティック |
| 攻撃 | J | Westボタン |
| 調べる・話す | E | Southボタン |
| 回避 | Left Shift | Eastボタン |
| ポーズ | Escape | Startボタン |

Attack、Interact、Dodge、Pauseはいずれもゲームプレイへ接続済みです。

DodgeはRigidbody2Dによる短時間の回避移動として実装し、Pause中は `Time.timeScale` を停止してInput Contextを `UI` へ切り替えます。Escape、Start、UIのCancel入力でゲームへ復帰できます。

## Input Actionコンテキスト

Input Actionsは用途別に分離しています。

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

`PlayerInputReader` は次の3コンテキストを排他的に切り替えます。

- `Gameplay`: Gameplay Action Mapのみ有効
- `UI`: UI Action Mapのみ有効
- `Disabled`: すべてのAction Mapを無効

`GamePauseController` はPause開始時に `UI`、復帰時に `Gameplay` へ切り替えます。

## Gameplay設定データ

プレイヤーのゲームバランス値はPrefabからScriptableObjectへ分離しています。

```text
Assets/Resources/Settings/Gameplay/
  PlayerCharacterStats.asset
    ├ moveSpeed
    └ maxHealth

  PlayerMeleeAttack.asset
    ├ damage
    ├ attackRadius
    └ attackDistance

  PlayerDodge.asset
    ├ dodgeSpeed
    ├ duration
    └ cooldown
```

`PrototypeProjectAssets` がこれらを参照し、`PrototypePlayerSpawner` が `CharacterMotor2D`、`Health`、`PlayerMeleeAttack`、`CharacterDodge2D` へ設定を注入します。

## Application Settings

FieldBootstrapはゲーム設定値を直接保持しません。

```text
PrototypeApplicationSettings.asset
  ├ playerSpawnPosition
  ├ playableTileRadius
  └ pausedTimeScale
```

起動経路は次です。

```text
FieldBootstrap
  ↓
PrototypeProjectAssets
  ↓
PrototypeApplicationInstaller
  ├ PrototypeSceneConfigurator
  ├ PrototypeSortingConfigurator
  ├ PrototypeWorldBuilder
  ├ GamePauseController
  └ PrototypeUiInstaller
```

これにより、`FieldBootstrap` はProjectAssetsを解決してApplicationInstallerへ委譲するだけの最小エントリーポイントになっています。

## uGUI日本語フォント

uGUIはOSフォントへ依存せず、プロジェクト内のFontアセットを利用します。

標準フォントはGoogle FontsのDotGothic16です。Unity Editor起動時にフォントが未導入の場合、Editorツールが次へ配置します。

```text
Assets/Fonts/
  DotGothic16-Regular.ttf
  OFL_DotGothic16.txt
```

手動導入はUnityメニューから実行できます。

```text
Demon King > Project > Install Japanese UI Font
```

導入後は `PrototypeProjectAssets.uiFont` を経由してuGUIへFontを渡します。生成されたFont・ライセンス・ProjectAssetsの差分はGit管理対象です。

## 現在のプレイ可能ループ

1. アイソメトリックTilemap上を移動する
2. Collision Tilemapによる物理境界に衝突する
3. NPCへ近づいてInteractする
4. 訓練用スライムへAttackする
5. HPを減らして対象を倒す
6. Shift／Eastボタンで回避移動する
7. Escape／StartでPauseし、uGUIのPause画面を表示する
8. カメラがプレイヤーへ追従する

## コンテンツアセット構造

主要なPrefab、Sprite、Gameplay設定、Application設定、UI Font参照は `PrototypeProjectAssets` に集約しています。

```text
Assets/
  Art/
    External/
    World/
  Fonts/
  Resources/
    Input/
      PlayerControls.inputactions
    Settings/
      PrototypeProjectAssets.asset
      PrototypeApplicationSettings.asset
      Gameplay/
        PlayerCharacterStats.asset
        PlayerMeleeAttack.asset
        PlayerDodge.asset
    Prefabs/
      Characters/
      World/
```

## テスト

Runtimeコードは `DemonKing.Runtime.asmdef` にまとめ、EditMode / PlayModeテストを独立assemblyで管理します。

```text
Assets/Tests/
  EditMode/
    WorldSortOrderTests.cs
  PlayMode/
    GameplayAndCameraPlayModeTests.cs
    PlayerInputContextPlayModeTests.cs
    DodgeAndPausePlayModeTests.cs
```

現在の自動テスト対象は次です。

- Y座標による描画順計算
- Healthの致死ダメージと死亡イベント
- CameraFollow2Dの追従とZ座標維持
- Gameplay / UI / Disabled入力コンテキスト切り替え
- Dodge開始時のRigidbody2D移動
- Pause / Resume時のTimeScaleとInput Context切り替え

## 開発フェーズ

P0、P1、P2のアーキテクチャ整備項目は完了しています。

P2では次を実装済みです。

- EditMode / PlayModeテスト基盤
- `Resources.Load` 文字列参照の削減
- 外部地形Tileアセット導入
- World Prefabの静的Spriteアート化
- uGUI日本語フォントのプロジェクト管理基盤
- 必要最小限のAssembly Definition導入
- キャラクター能力値・近接攻撃・Dodge設定のScriptableObject化
- Gameplay / UI / Disabled入力コンテキスト分離
- Rigidbody2DベースのDodge実挙動
- Pause状態管理とuGUI Pause画面
- FieldBootstrapからApplication Settingsと起動構成を分離

今後はP3として、セーブ、Steam／コンソール向けPlatform層、必要に応じたAddressablesやマップ分割へ進みます。

詳細は以下を参照してください。

- `docs/GAME_DIRECTION.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/ARCHITECTURE.md`
