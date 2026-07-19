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
- ScriptableObjectによるゲームバランスデータ管理
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

AttackとInteractはゲームプレイ機能へ接続済みです。DodgeとPauseは入力境界まで実装済みで、実際の挙動は後続実装です。

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

ポーズメニューや会話UIは、このコンテキストAPIを利用してGameplay入力とUI入力を切り替えます。

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
```

`PrototypeProjectAssets` がこれらを参照し、`PrototypePlayerSpawner` が `CharacterMotor2D`、`Health`、`PlayerMeleeAttack` へ設定を注入します。

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

導入後は `PrototypeProjectAssets.uiFont` を経由して `GameHudView` へFontを渡します。生成されたFont・ライセンス・ProjectAssetsの差分はGit管理対象です。

## 現在のプレイ可能ループ

1. アイソメトリックTilemap上を移動する
2. Collision Tilemapによる物理境界に衝突する
3. NPCへ近づいてInteractする
4. 訓練用スライムへAttackする
5. HPを減らして対象を倒す
6. カメラがプレイヤーへ追従する
7. Canvas（uGUI）のHUDを表示する

## コンテンツアセット構造

主要なPrefab、Sprite、Gameplay設定、UI Font参照は `PrototypeProjectAssets` に集約しています。

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
      Gameplay/
        PlayerCharacterStats.asset
        PlayerMeleeAttack.asset
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
```

現在の自動テスト対象は次です。

- Y座標による描画順計算
- Healthの致死ダメージと死亡イベント
- CameraFollow2Dの追従とZ座標維持
- Gameplay / UI / Disabled入力コンテキスト切り替え

## 開発フェーズ

P0とP1は完了しています。

P2では次を実装済みです。

- EditMode / PlayModeテスト基盤
- `Resources.Load` 文字列参照の削減
- 外部地形Tileアセット導入
- World Prefabの静的Spriteアート化
- uGUI日本語フォントのプロジェクト管理基盤
- 必要最小限のAssembly Definition導入
- キャラクター能力値と近接攻撃データのScriptableObject化
- Gameplay / UI / Disabled入力コンテキスト分離

次はDodgeの実挙動、Pause状態管理、FieldBootstrapからのアプリケーション全体設定分離を優先します。

詳細は以下を参照してください。

- `docs/GAME_DIRECTION.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/ARCHITECTURE.md`
