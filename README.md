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

AttackとInteractはゲームプレイ機能へ接続済みです。DodgeとPauseはInput Actionとイベント境界まで実装済みで、実際の挙動は後続実装です。

## 現在のプレイ可能ループ

1. アイソメトリックTilemap上を移動する
2. Collision Tilemapによる物理境界に衝突する
3. NPCへ近づいてInteractする
4. 訓練用スライムへAttackする
5. HPを減らして対象を倒す
6. カメラがプレイヤーへ追従する
7. Canvas（uGUI）のHUDを表示する

## コンテンツアセット構造

主要なPrefabとSprite参照は `PrototypeProjectAssets` に集約しています。

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
    Settings/
      PrototypeProjectAssets.asset
    Prefabs/
      Characters/
        PrototypeSlime.prefab
      World/
        PrototypeCottage.prefab
        PrototypeTree.prefab
        PrototypeLamppost.prefab
```

`PrototypePlayerSpawner` と `PrototypeWorldPrefabFactory` はResourcesパス文字列を持たず、ProjectAssetsから直接参照を受け取ります。

## 地形アセット

Ground TilemapにはKenney「Isometric Tiles Landscape」由来の外部Spriteを使用します。

`PrototypeRuntimeTileFactory` はTexture2DやSpriteを実行時生成せず、インポート済みSpriteからUnity Tileだけを構築します。

外部アセットの出典とライセンスは次に記録しています。

```text
Assets/Art/External/Kenney/README.md
```

## World Prefab

校舎、木、街灯はPrefab境界を維持しつつ、従来のRuntimeShapeFactoryによる仮図形からプロジェクト管理の静的Spriteアートへ移行しています。

```text
PrototypeProjectAssets
  ↓
PrototypeWorldPrefabFactory
  ↓
Cottage / Tree / Lamppost Prefab
  ↓
Static Sprite Art
```

最終アートを差し替える際はProjectAssets側の参照を変更し、Builderの配置ロジックは維持します。

## テスト

Runtimeコードは `DemonKing.Runtime.asmdef` にまとめ、EditMode / PlayModeテストを独立assemblyで管理します。

```text
Assets/Tests/
  EditMode/
    WorldSortOrderTests.cs
  PlayMode/
    GameplayAndCameraPlayModeTests.cs
```

現在の自動テスト対象は次です。

- Y座標による描画順計算
- Healthの致死ダメージと死亡イベント
- CameraFollow2Dの追従とZ座標維持

Unity Test RunnerからEditMode / PlayModeを実行します。

## 現在の主要ランタイム構造

```text
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
      ├ PrototypeGameplayFeatureInstaller
      ├ PrototypePlayerSpawner
      └ PrototypeCameraInstaller
```

Gameplay機能は次のように分離しています。

```text
Gameplay/
  Characters/
  Interaction/
    IInteractable
    PlayerInteractor
  Combat/
    IDamageable
    Health
    PlayerMeleeAttack
```

## 開発フェーズ

P0とP1は完了しています。

P2では次を実装済みです。

- EditMode / PlayModeテスト基盤
- `Resources.Load` 文字列参照の削減
- 外部地形Tileアセット導入
- `PrototypeRuntimeTileFactory` のRuntime Texture生成削除
- World Prefabの静的Spriteアート化
- 必要最小限のAssembly Definition導入

次は日本語フォントのプロジェクトアセット化、Input ActionのGameplay / UIコンテキスト分離、Dodge / Pause実挙動を優先します。

詳細は以下を参照してください。

- `docs/GAME_DIRECTION.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/ARCHITECTURE.md`
