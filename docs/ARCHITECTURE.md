# アーキテクチャ方針

## 目的

このプロジェクトは、現在の遊べるプロトタイプを壊さずに、機能追加へ耐えられる構造へ段階的に移行します。

プロトタイプでは1つのMonoBehaviourに複数の責務をまとめる方が速い一方、規模が大きくなると入力、移動、描画、UI、戦闘、会話、セーブなどの変更が互いに影響しやすくなります。

今後は、機能単位で責務を分離し、Unity固有のシーン制御とゲームルールを必要以上に密結合させない方針で進めます。

## 現在のディレクトリ方針

```text
Assets/
  Resources/
    Input/
      PlayerControls.inputactions     プレイヤー入力定義
    Prefabs/
      Characters/
        PrototypeSlime.prefab         現在の試作プレイヤーPrefab
  Scenes/
    Prototype/
      Prototype.unity                 現在の正規プレイシーン
  Scripts/
    Core/
      Input/                           入力の抽象化、入力値の提供
      Math/                            座標計算など副作用の少ない共通処理
    Gameplay/
      Characters/                      キャラクターの移動や状態などゲームプレイ処理
    Presentation/
      Characters/                      キャラクターの見た目・アニメーション
      Rendering/                       描画順などレンダリング補助
      UI/                              画面表示
    Field/
      Prototype/                       現在の実行時生成プロトタイプ専用実装
        TerrainBuilder                 背景・地面・小道・前景
        ArchitectureBuilder            建物・柵・ランドマーク・街灯
        NatureBuilder                  池・木・草花
        AtmosphereBuilder              蛍・夕霧などの演出配置
        AmbientEffectController        環境アニメーション更新
        RuntimeShapeFactory            仮図形Sprite生成
        PrototypeSlimeView             試作スライムの仮ビジュアル生成
        PrototypePlayerSpawner         試作プレイヤーPrefabの配置
        PrototypeSortingConfigurator   Tilemap描画順の初期化
        PrototypeWorldBuilder          各Builderの組み立て
    World/                             ワールド上の汎用コンポーネント
    FieldBootstrap.cs                  プロトタイプ起動の最小構成ルート
    SlimeController.cs                 試作プレイヤーの構成ルート
  Editor/                              Unity Editor専用ツール
```

新しい恒久機能は、原則として既存の巨大クラスへ直接追加せず、対応する機能ディレクトリに独立したコンポーネントまたは通常のC#クラスとして追加します。

`Field/Prototype` は現在の実行時生成プロトタイプを保守するための領域です。本番用のTilemap、Prefab、シーン構成へ移行した後も、恒久的なゲームプレイ機能をこの領域へ追加しません。

`Resources` は現在のプロトタイプで、シーン上に参照を持たずに入力アセットとプレイヤーPrefabを読み込むための暫定的な配置です。アセット数やシーン数が増えた段階で、必要に応じてシーン参照、専用の設定アセット、Addressablesなどへ移行します。

## 依存方向

基本的な依存方向は次の通りです。

```text
Presentation
    ↓
Gameplay
    ↓
Core
```

`Core` はUIや特定のフィールド表現を知りません。

`Gameplay` は入力デバイスの具体的なキー配置を知りません。

`Presentation` はゲームルールを直接変更せず、状態を見た目へ反映する責務を持ちます。

`Field/Prototype` は試作シーンを組み立てる外側の層として、Core、Gameplay、Presentationのコンポーネントを利用できます。逆方向に、CoreやGameplayからPrototype固有クラスを参照しないようにします。

Steamや将来のコンソールSDKなどのプラットフォーム固有処理は、必要になった時点で `Platform` 境界を追加し、ゲームプレイコードから直接参照しない構造にします。

## 正規シーンと起動経路

現在の正規プレイシーンは次の1つです。

```text
Assets/Scenes/Prototype/Prototype.unity
```

`ProjectSettings/EditorBuildSettings.asset` もこのシーンのみを起動対象とします。

旧 `Assets/Scenes/SampleScene.unity` は削除済みです。今後、タイトル画面やBootstrap Sceneを追加するまでは、Prototypeシーンを唯一のゲーム起動経路として扱います。

複数シーン構成へ移行する場合は、次のように責務を分離します。

```text
ApplicationBootstrap / Entry Scene
    ↓
Gameplay Scene
```

## 入力アーキテクチャ

入力バインディングは `Assets/Resources/Input/PlayerControls.inputactions` に集約します。

現在のAction Mapは次の通りです。

```text
Player
  └ Move
      ├ WASD
      ├ Arrow Keys
      └ Gamepad Left Stick
```

`MoveInputReader` は個別キーをコード内で生成・監視せず、`.inputactions` アセットから `Player/Move` を取得します。

ゲームプレイ側の `CharacterMotor2D` は論理入力だけを利用するため、今後キーバインド変更、コントローラー追加、リバインド機能を導入しても移動ロジックへの影響を抑えられます。

## プレイヤー移動と衝突

`CharacterMotor2D` はTransformを直接更新せず、次の経路で移動します。

```text
MoveInputReader
    ↓ Updateで入力値を取得
CharacterMotor2D
    ↓ FixedUpdate
Rigidbody2D.MovePosition
    ↓
Collider2D / TilemapCollider2D
```

`PrototypeSlime.prefab` には `Rigidbody2D` と `CircleCollider2D` を持たせています。

`Rigidbody2D` は重力を使用せず、回転を固定します。`Collision` Tilemapには `TilemapCollider2D` があるため、Collision Tileを配置すればプレイヤーとの物理衝突が成立する構成です。

現在の `fieldExtents` による移動範囲制限は、実行時生成プロトタイプからTilemapへ移行する間のフォールバックとして残しています。設定値の所有者整理は次のP0対象として扱います。

## アイソメトリック描画順

描画順は次のSorting Layerを基準にします。

```text
Ground
World
Foreground
UI
```

役割は次の通りです。

- `Ground`: 地面や常に最背面にあるTilemap
- `World`: プレイヤー、NPC、敵、木、建物などY座標による前後関係が必要な要素
- `Foreground`: 常にWorldより手前に表示する前景
- `UI`: ゲームワールドとは独立したUI

キャラクター専用Sorting Layerは設けず、キャラクターと遮蔽物を同じ `World` Layerへ置きます。これにより、キャラクターが木や建物の手前・奥を移動できるようにします。

動的Spriteの基本ルールは次の通りです。

```text
sortingOrder = -round(worldY * precision)
```

単一Spriteは `YSortSprite`、複数Spriteからなるキャラクターは `GroupYSorter` を使用します。実行時生成Spriteも `World` Layerへ統一します。

Tilemapは次のルールを使用します。

```text
Ground      -> Ground Layer / Chunk
Props       -> World Layer / Individual
Foreground  -> Foreground Layer / Chunk
Collision   -> 非表示 / TilemapCollider2Dのみ利用
```

`Props` はキャラクターなど他Rendererとの前後関係を持つため `Individual` Modeを使用します。

現在のGridは通常の `Isometric` レイアウトであり、2D RendererのTransparency Sort AxisはY軸基準のCustom Axis `(0, 1, 0)` とします。

将来 `Isometric Z as Y` へ変更する場合は、Z成分を含むCustom Axisを含め、描画順ルールを再検証します。

## SlimeControllerの責務分割

従来の `SlimeController` は次の責務を持っていました。

- Input SystemのAction生成
- 移動
- フィールド境界制御
- 潰れ・伸びアニメーション
- Y座標による描画順制御
- 試作HUD表示
- HUD用テクスチャの生成と破棄

現在は以下へ分離しています。

- `MoveInputReader`: `.inputactions` アセットから論理移動入力を提供
- `CharacterMotor2D`: Rigidbody2Dを利用した移動と移動可能範囲
- `CharacterSquashAnimator`: 試作キャラクターの見た目
- `GroupYSorter`: 複数SpriteRendererの描画順
- `PrototypeHud`: 試作案内UI
- `PrototypeSlimeView`: 現在の仮スライムビジュアル生成
- `SlimeController`: 各コンポーネントの試作設定をまとめる構成ルート

プレイヤー本体は `PrototypeSlime.prefab` として管理し、`PrototypePlayerSpawner` はPrefabのインスタンス化と初期配置だけを担当します。

これにより、プレイヤーキャラクターの見た目を本番スプライトへ差し替える場合も、フィールド構築コードや移動ロジックを変更する必要がありません。

## FieldBootstrapの責務分割

従来の `FieldBootstrap` は、約600行の1クラスで次の処理を担当していました。

- アプリケーションとカメラの初期設定
- 背景・地面・小道の生成
- 校舎・柵・ランドマークの生成
- 池・木・草花の生成
- 水面や蛍の環境アニメーション
- 実行時図形Spriteの生成
- プレイヤー生成

現在は `FieldBootstrap` を次の処理だけに縮小しています。

```text
FieldBootstrap
  ├ PrototypeSceneConfigurator.Configure
  ├ PrototypeSortingConfigurator.Configure
  └ PrototypeWorldBuilder.Build
```

具体的な生成処理は以下へ分割しています。

```text
PrototypeWorldBuilder
  ├ TerrainBuilder
  ├ ArchitectureBuilder
  ├ NatureBuilder
  ├ AtmosphereBuilder
  ├ AmbientEffectController
  └ PrototypePlayerSpawner

共通補助
  ├ RuntimeShapeFactory
  ├ PrototypePalette
  └ PrototypeWorldMath
```

この分割は、現在のプロトタイプの見た目を維持しながら、本番アセットへの置き換え単位を明確にするためのものです。

例えばTilemapへ移行する場合は `TerrainBuilder`、校舎を本番Prefabへ置き換える場合は `ArchitectureBuilder` を個別に変更できます。プレイヤーは既にPrefab境界へ移行済みです。

## 現状の構造評価

現時点のプロジェクトは、初期プロトタイプとしては十分に責務分割が進んでおり、全面的な作り直しは不要です。

一方で、このままNPC、敵、戦闘、会話、複数マップなどを追加すると変更コストが増えやすい箇所は残っています。

### 1. シーンと起動経路の二重化 — 解消済み

`Prototype.unity` を正規プレイシーンに統一し、Build Settingsも同シーンのみへ変更しました。旧 `SampleScene.unity` は削除しています。

### 2. 移動実装とCollision Tilemapの不一致 — 解消済み

`CharacterMotor2D` をRigidbody2Dベースへ変更し、プレイヤーPrefabへCollider2Dを追加しました。Collision Tilemapの `TilemapCollider2D` と物理衝突できる構造になっています。

### 3. プレイヤーPrefabにUIが結合している — 未解消

現在の `PrototypeSlime.prefab` は `PrototypeHud` を自身のコンポーネントとして持っています。

将来プレイヤーの再生成、シーン遷移、タイトル画面、メニュー、複数キャラクターなどを導入すると、UIのライフサイクルとプレイヤーのライフサイクルが不必要に結合します。

本番UIへ移行する際は次のように分離します。

```text
Gameplay Scene
  ├ Player
  └ UI Root
      ├ HUD
      ├ Dialogue
      ├ Menu
      └ Notification
```

HUDはプレイヤーを参照して状態を表示しますが、プレイヤーPrefabの構成要素にはしません。

### 4. 設定値に複数の所有者が存在する — 未解消

移動速度は `SlimeController` と `CharacterMotor2D` の双方に存在し、描画順精度は `SlimeController` と `GroupYSorter` の双方に存在します。また、フィールド境界は `PrototypePlayerSpawner` にハードコードされています。

短期的には各設定の所有者を1つにします。

```text
CharacterMotor2D    -> moveSpeed
GroupYSorter        -> sortingPrecision
World / Map Config  -> playableBounds
```

プレイヤー固有パラメータが増えた段階で、必要に応じて `PlayerConfig` などのScriptableObjectへまとめます。ただし、設定項目が少ない段階で巨大な設定基盤は作りません。

### 5. 実行時生成ワールドとTilemapが並存している — 移行中

現在はIsometric Tilemapの器がシーンに存在する一方、実際の草原、建物、自然物の多くは `PrototypeWorldBuilder` と各Builderが実行時に生成しています。

移行単位を明確にし、以下の順で実行時生成を削減します。

```text
TerrainBuilder
  ↓ Isometric Tilemapへ移行
ArchitectureBuilder / NatureBuilder
  ↓ Prefab・Tile・本番アートへ移行
AtmosphereBuilder
  ↓ 必要な演出コンポーネントのみ残す
RuntimeShapeFactory
  ↓ 最終的にPrototype専用または削除
```

### 6. アイソメトリック描画順ルール — 確定済み

`Ground`、`World`、`Foreground`、`UI` Sorting Layerを定義し、Yソートが必要なキャラクターと遮蔽物を `World` に統一しました。

`Props` TilemapはIndividual Modeを使用し、キャラクターなど他Rendererとの前後関係を持てる構成です。

### 7. `Resources.Load` と文字列パス — 暫定利用

現在の入力アセットとプレイヤーPrefabは `Resources.Load` で取得しています。

少数アセットのプロトタイプでは実用上問題ありませんが、アセットが増えると参照関係が追跡しにくくなります。

次の段階では、まずシーン上のBootstrapまたはInstallerからPrefabや設定アセットをSerializeFieldで参照する方式を優先します。

Addressablesは、大規模な非同期ロード、追加コンテンツ、シーンストリーミングなどが実際に必要になった時点で導入します。

### 8. Assembly Definitionによる依存境界がまだない — 未着手

現在のRuntimeスクリプトは基本的に `Assembly-CSharp`、Editorスクリプトは `Assembly-CSharp-Editor` にまとめてコンパイルされます。

フォルダによる責務分割はできていますが、誤った依存をコンパイラが防ぐ状態ではありません。

機能追加が本格化する前に、必要最小限で以下の境界を検討します。

```text
DemonKing.Core
DemonKing.Gameplay
DemonKing.Presentation
DemonKing.Field.Prototype
DemonKing.Editor
DemonKing.Tests
```

### 9. 入力のライフサイクル — 将来整理

現在はプレイヤーPrefab内の `MoveInputReader` がInput Action Assetを実行時に複製しています。1人用プロトタイプとしては問題ありません。

Pause、メニュー、会話中の操作停止、リバインド、複数Action Mapを導入する段階では、入力の有効・無効をプレイヤー個体ではなくプレイ状態から制御できる構造へ移行します。

```text
Input Context
  ├ Gameplay
  ├ UI
  └ Disabled / Cutscene
```

### 10. プロトタイプ固有のグローバル設定 — 将来分離

`PrototypeSceneConfigurator` は `Application.targetFrameRate` やQuality設定、カメラ設定を変更します。

複数シーン化する段階で、アプリケーション全体の設定とフィールド固有設定を分けます。

```text
ApplicationBootstrap
  -> フレームレート、共通サービス、永続設定

FieldBootstrap
  -> フィールド固有の生成、配置
```

## リファクタリング・リアーキテクチャの優先順位

### P0: ゲーム機能を増やす前に実施する

#### P0-1. 正規シーンとBuild Settingsを統一する — 完了

- `Prototype.unity` を正規プレイシーンとして統一済み
- Build Settingsを `Prototype.unity` のみに統一済み
- 旧 `SampleScene.unity` を削除済み
- 現在の起動用 `FieldBootstrap` はPrototypeシーンの1経路のみ

#### P0-2. プレイヤー移動をRigidbody2Dベースへ移行する — 完了

- `CharacterMotor2D` のTransform直接更新を廃止済み
- `Update` で入力値を取得し、`FixedUpdate` で `Rigidbody2D.MovePosition` を適用
- `PrototypeSlime.prefab` に `Rigidbody2D` と `CircleCollider2D` を追加済み
- `Collision` Tilemapの `TilemapCollider2D` と物理衝突できる構成へ移行済み
- `fieldExtents` は移行期間中のフォールバックとして継続

#### P0-3. アイソメトリック描画順ルールを確定する — 完了

- `Ground`、`World`、`Foreground`、`UI` Sorting Layerを定義済み
- 動的キャラクターと遮蔽物は `World` LayerでYソート
- `Props` Tilemapは `World` Layer / Individual Mode
- `Ground` と `Foreground` は専用Sorting Layerへ分離
- 2D RendererのTransparency Sort AxisをY軸基準へ固定
- シーン生成ツールと実行時プロトタイプの双方へ同じルールを適用

#### P0-4. 設定値の二重管理を解消する — 未着手

- `moveSpeed` の所有者を1つにする
- `sortingPrecision` の所有者を1つにする
- `FieldExtents` をSpawnerの固定値から切り離す
- 必要になった設定だけをScriptableObject化する

#### P0-5. HUDをプレイヤーPrefabから分離する — 未着手

- `PrototypeHud` をプレイヤーPrefabから外す
- Scene側に `UI Root` を持たせる
- プレイヤー再生成とUIライフサイクルを分離する

### P1: 戦闘・NPC・会話を追加する前後で実施する

1. 実行時生成の地形を本番用Isometric Tilemapへ段階的に置き換える
2. 校舎、木、街灯などをPrefabまたはアートアセット管理へ移行する
3. 試作スライムの実行時図形ビジュアルを本番スプライト／アニメーションへ置き換える
4. `PlayerControls.inputactions` にAttack、Interact、Dodge、Pauseを追加する
5. 戦闘、会話、クエスト、インベントリをそれぞれ独立したFeatureとして追加する
6. カメラ追従をプレイヤーから独立したコンポーネントとして管理する

### P2: コンテンツ量が増える前に実施する

1. `Resources.Load` の文字列参照を減らし、シーン参照または設定アセットへ移行する
2. Assembly Definitionを必要最小限で導入し、依存方向をコンパイラで制約する
3. EditMode / PlayModeテストを機能単位で追加する
4. キャラクターやマップの設定値が増えた段階でScriptableObjectによるデータ分離を導入する
5. Input ActionのGameplay / UI / Disabledなどのコンテキスト切り替えを整理する

### P3: 本番規模へ移行する段階で実施する

1. セーブ機能追加時に `ISaveService` を導入する
2. Steam固有機能追加時にPlatform層を導入する
3. 必要になった時点でAddressablesや非同期ロードを導入する
4. 大規模マップではシーン分割またはストリーミングを検討する
5. コンソール移植を見据えて描画・メモリ・ロード時間のパフォーマンス予算を設定する

## 直近の推奨実施順序

1. P0-4 設定値の二重管理解消
2. P0-5 HUDをプレイヤーPrefabから分離
3. Collision Tilemapへ実際の衝突タイルを配置して物理挙動を検証
4. Isometric TilemapへTerrainBuilderの責務を段階移行
5. 建物・木・街灯などをPrefab化
6. Attack / Interact / Dodge / PauseをInput Actionsへ追加
7. Interaction機能を独立Featureとして追加
8. Combat機能を独立Featureとして追加
9. EditMode / PlayModeテストを追加
10. コンテンツ増加状況を見てasmdefとResources削減を実施

## 実装ルール

- 1クラスに複数の独立した変更理由を持たせない
- 入力デバイスの具体的なキー判定をゲームプレイコードへ書かない
- 入力バインディングは原則として `.inputactions` アセットで管理する
- キャラクター移動は物理衝突が必要な場合、Rigidbody2D経由で行う
- 描画順の計算規則は共通化する
- Yソートが必要なワールド要素は原則として `World` Sorting Layerを使用する
- UIから直接ゲームルールを変更しない
- プロトタイプ専用処理には、そのことが分かる名前を付ける
- 将来使うかもしれないという理由だけで抽象化を増やさない
- プラットフォーム依存が実際に発生した時点で境界を追加する
- コメントや設計ドキュメントは日本語で記述する

## 移行方針

大規模な一括書き換えではなく、機能追加のタイミングで既存コードを小さく置き換えていきます。

常に `main` のプロトタイプが遊べる状態を維持しながら、古い実装を新しい構造へ徐々に寄せることを優先します。

`Field/Prototype` 配下は移行期間中の互換層として扱い、本番用のシーン、Prefab、Tilemapが揃った機能から順に削減していきます。
