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
- 地形データはTilemapを正とし、1マスごとのGameObject生成へ戻さない
- 描画順の計算規則は共通化する
- Yソート対象は原則 `World` Sorting Layerを使用する
- UIとプレイヤーのライフサイクルを分離する
- 同じ意味の設定値を複数コンポーネントで所有しない
- 大きなワールド要素はPrefab境界を持たせる
- Interactionは対象固有ロジックに依存しない
- Combatは敵種別や演出に依存しない
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
      Characters/
      Rendering/
      UI/
    Field/
      Prototype/
        PrototypeNpcInteractable.cs
        PrototypeCombatDummy.cs
        PrototypeGameplayFeatureInstaller.cs
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

InteractionとCombatの恒久的な契約・ロジックは `Gameplay` 配下に置き、試作NPCや訓練用ダミーだけを `Field/Prototype` に置きます。

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
      ├ PrototypeGameplayFeatureInstaller
      │   ├ PrototypeNpcInteractable
      │   └ PrototypeCombatDummy
      └ PrototypePlayerSpawner
```

`FieldBootstrap` はプレイヤー初期位置と `playableTileRadius` を所有します。

プレイ可能範囲の物理的な境界は `CollisionMapBuilder` がCollision Tilemapへ構築します。プレイヤーの座標Clampは使用しません。

## 入力アーキテクチャ

入力定義は `PlayerControls.inputactions` に集約します。

現在のPlayer Action Mapは次の通りです。

```text
Player
  ├ Move
  ├ Attack
  ├ Interact
  ├ Dodge
  └ Pause
```

現在の主要バインディングは次の通りです。

```text
Move     : WASD / Arrow Keys / Gamepad Left Stick
Attack   : J / Gamepad West
Interact : E / Gamepad South
Dodge    : Left Shift / Gamepad East
Pause    : Escape / Gamepad Start
```

### PlayerInputReader

`PlayerInputReader` がInput Action Assetの実行時複製を1つだけ所有します。

```text
PlayerControls.inputactions
  ↓
PlayerInputReader
  ├ Move値
  ├ AttackPressed
  ├ InteractPressed
  ├ DodgePressed
  └ PausePressed
```

ゲームプレイコードはKeyboardやGamepadの具体的なボタンを知りません。

### MoveInputReader

既存の移動系コンポーネントとの互換性を維持するため、`MoveInputReader` は残します。

ただしInput Action Assetを独自に生成せず、`PlayerInputReader.Move` を転送するだけのアダプターです。

```text
PlayerInputReader
  ↓
MoveInputReader
  ↓
CharacterMotor2D / PrototypeSlimeSpriteAnimator
```

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

Interactionの恒久部分は `Gameplay/Interaction` に分離しています。

```text
PlayerInputReader.InteractPressed
  ↓
PlayerInteractor
  ↓
IInteractable
  ↓
NPC / 扉 / 宝箱 / 調査対象
```

### IInteractable

`IInteractable` は次の責務だけを定義します。

- 現在相互作用可能か判断する
- 相互作用を実行する

`PlayerInteractor` はNPC会話、宝箱、扉などの具体的な処理を知りません。

### PlayerInteractor

Interact入力時だけ周辺Colliderを探索し、最も近い `IInteractable` を1件選んで実行します。

対象探索はInteraction Feature内に閉じ、具体的な対象ロジックは各実装側へ委譲します。

### プロトタイプ確認対象

`PrototypeGameplayFeatureInstaller` が `PrototypeNpcInteractable` を1体配置します。

現在は会話システム未導入のため、相互作用結果をConsoleへ出力します。今後の会話システムは `IInteractable` 実装の上へ追加し、`PlayerInteractor` 自体は変更しない方針です。

## Combat Feature

Combatの恒久部分は `Gameplay/Combat` に分離しています。

```text
PlayerInputReader.AttackPressed
  ↓
PlayerMeleeAttack
  ↓
IDamageable
  ↓
Health
  ├ Damaged
  ├ HealthChanged
  └ Died
```

### IDamageable

攻撃側は対象の敵種別を知りません。

`IDamageable` を実装する対象であれば、敵、破壊可能オブジェクト、将来のボスなどへ同じ攻撃経路を利用できます。

### Health

`Health` は次だけを担当します。

- 最大HP
- 現在HP
- ダメージ適用
- 生存状態
- HP変化イベント
- ダメージイベント
- 死亡イベント

敵AI、死亡演出、ドロップ、経験値、クエスト進行には依存しません。

### PlayerMeleeAttack

Attack入力時に、最後の移動方向を向きとして近距離範囲判定を行います。

同一対象に複数Colliderが存在しても、1回の攻撃で同じ `IDamageable` へ重複ダメージを与えない構造です。

### プロトタイプ確認対象

`PrototypeGameplayFeatureInstaller` が `PrototypeCombatDummy` を1体配置します。

訓練用ダミーは `Health` を利用し、攻撃、HP減少、死亡までの最小ループを確認できます。

死亡後の報酬や敵AIはCombat Featureへ直接追加せず、Healthのイベントを購読する外側の機能として追加します。

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

`SlimeController` は試作プレイヤーに必要なコンポーネント構成を保証する互換用コンポーネントであり、入力、Interaction、Combatのロジック自体は持ちません。

## Isometric Tilemap地形

地形の描画先は実際のIsometric Tilemapへ移行済みです。

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

`ArchitectureBuilder` と `NatureBuilder` は配置を担当し、内部ビジュアル構築を持ちません。

## UI

`PrototypeHud` はプレイヤーPrefabから分離済みです。

```text
Prototype Scene
  ├ World
  │   ├ Player
  │   ├ PrototypeNpcInteractable
  │   └ PrototypeCombatDummy
  └ UI Root
      └ PrototypeHud
```

プレイヤー再生成とUIの生成・破棄は独立しています。

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

- `PlayerControls.inputactions` に4 Actionを追加
- Keyboard / Gamepad双方のBindingを追加
- `PlayerInputReader` を追加
- Moveを含むプレイヤー入力のAsset所有とライフサイクルを1か所へ集約
- `MoveInputReader` は互換アダプターへ縮小
- Dodge / Pauseはイベント境界まで実装

### P1-6. Interactionを独立Featureとして追加する — 完了

- `Gameplay/Interaction` を追加
- `IInteractable` を追加
- `PlayerInteractor` を追加
- Input、対象探索、対象固有処理を分離
- `PrototypeNpcInteractable` を配置し、最小Interactionループを追加

### P1-7. Combatを独立Featureとして追加する — 完了

- `Gameplay/Combat` を追加
- `IDamageable` を追加
- `Health` を追加
- `PlayerMeleeAttack` を追加
- Attack入力と敵固有ロジックを分離
- `PrototypeCombatDummy` を配置し、攻撃→HP減少→死亡の最小ループを追加

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

P1-5〜P1-7実装後は次の順序で進めます。

1. Play ModeでAttack / Interact入力がKeyboardとGamepad双方で発火することを確認する
2. 試作NPCとのInteractionを確認する
3. 訓練用スライムへの攻撃、HP減少、死亡を確認する
4. P1-8としてカメラ追従を独立コンポーネント化する
5. P1-9として本番UIへ移行する
6. 会話システムをInteraction Featureの上へ追加する
7. 敵AI、攻撃リアクション、報酬処理をCombat Featureの外側へ追加する
8. EditMode / PlayModeテストを追加する
9. 本番タイルセットとワールドアートへの差し替えを進める
10. コンテンツ増加状況を見てasmdefとResources削減を進める

## 移行方針

大規模な一括書き換えではなく、機能追加のタイミングで既存コードを小さく置き換えます。

常に `main` のプロトタイプが遊べる状態を維持し、本番用のシーン、Prefab、Tilemapが揃った機能から `Field/Prototype` への依存を減らします。
