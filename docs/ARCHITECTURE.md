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

ゲームプレイ側の `CharacterMotor2D` は `MoveInputReader.Move` の論理値だけを利用するため、今後キーバインド変更、コントローラー追加、リバインド機能を導入しても移動ロジックへの影響を抑えられます。

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
- `CharacterMotor2D`: 移動と移動可能範囲
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

現在は `FieldBootstrap` を次の2処理だけに縮小しています。

```text
FieldBootstrap
  ├ PrototypeSceneConfigurator.Configure
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

現時点のプロジェクトは、初期プロトタイプとしては十分に責務分割が進んでおり、今すぐ全面的な作り直しが必要な状態ではありません。

一方で、このままNPC、敵、戦闘、会話、複数マップなどを追加すると変更コストが急増しやすい箇所が残っています。特に以下は、コンテンツ量が増える前に整理します。

### 1. シーンと起動経路が二重化している

現在は `SampleScene.unity` と `Scenes/Prototype/Prototype.unity` が存在し、どちらにもプロトタイプ起動用の `FieldBootstrap` を配置できる状態です。また、Build Settingsの起動シーンと実際に開発対象としているPrototypeシーンが一致しない状態を作りやすくなっています。

今後は「現在の正規のプレイ開始シーン」を1つに決めます。

```text
Bootstrap / Entry Scene
  ↓
Gameplay Scene
```

複数シーン構成が必要になるまでは、`Prototype.unity` を正規のプレイ対象としてBuild Settingsにも登録し、不要になった `SampleScene.unity` は削除または明確に非推奨化します。

### 2. 移動実装とCollision Tilemapの設計が一致していない

現在の `CharacterMotor2D` はTransformを直接更新して移動しています。一方、シーンには `Collision` Tilemapと `TilemapCollider2D` が存在します。

このままでは、本番マップへCollision Tileを配置してもプレイヤー移動が物理衝突を利用しません。

本番フィールドへ移行する前に、以下へ統一します。

```text
MoveInputReader
  ↓
CharacterMotor2D
  ↓
Rigidbody2D
  ↓
Collider2D / TilemapCollider2D
```

移動適用は原則として `FixedUpdate` と `Rigidbody2D.MovePosition` など物理システムに沿った方式へ変更します。

### 3. プレイヤーPrefabにUIが結合している

現在の `PrototypeSlime.prefab` は `PrototypeHud` を自身のコンポーネントとして持っています。

これはプロトタイプでは動作しますが、将来プレイヤーの再生成、シーン遷移、タイトル画面、メニュー、複数キャラクターなどを導入すると、UIのライフサイクルとプレイヤーのライフサイクルが不必要に結合します。

本番UIへ移行する際は以下のように分離します。

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

### 4. 設定値に複数の所有者が存在する

移動速度は `SlimeController` と `CharacterMotor2D` の双方に存在し、描画順精度は `SlimeController` と `GroupYSorter` の双方に存在します。また、フィールド境界は `PrototypePlayerSpawner` にハードコードされています。

同じ意味の設定値を複数箇所で持つと、Inspector変更時にどちらが正しい値か分からなくなります。

短期的には各設定の所有者を1つにします。

```text
CharacterMotor2D    -> moveSpeed
GroupYSorter        -> sortingPrecision
World / Map Config  -> playableBounds
```

プレイヤー固有パラメータが増えた段階で、必要に応じて `PlayerConfig` などのScriptableObjectへまとめます。ただし、設定項目が少ない段階で巨大な設定基盤は作りません。

### 5. 実行時生成ワールドとTilemapが並存している

現在はIsometric Tilemapの器がシーンに存在する一方、実際の草原、建物、自然物の多くは `PrototypeWorldBuilder` と各Builderが実行時に生成しています。

移行期間中は問題ありませんが、両方を恒久運用すると「どちらが正しいワールドデータか」が曖昧になります。

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

### 6. アイソメトリック描画順のルールを早期に固定する必要がある

現在は動的SpriteをY座標から `sortingOrder` へ変換し、Tilemap側は固定のSorting Orderを使用しています。またSorting Layerは実質的にDefaultのみです。

アートやPrefabが増える前に、次を決定します。

- Ground、World、Character、Foreground、UIなどのSorting Layer方針
- TilemapRendererのSorting Order / Mode
- 動的オブジェクトのYソート範囲
- Foregroundが常に前面になる条件
- 2D RendererのTransparency Sort設定

描画順ルールを後から変更すると、多数のPrefabとTilemapの再調整が必要になるため、地形・建物の本番化より前に固定します。

### 7. `Resources.Load` と文字列パスは暫定利用に留める

現在の入力アセットとプレイヤーPrefabは `Resources.Load` で取得しています。

少数アセットのプロトタイプでは実用上問題ありませんが、アセットが増えると参照関係が追跡しにくくなります。

次の段階では、まずシーン上のBootstrapまたはInstallerからPrefabや設定アセットをSerializeFieldで参照する方式を優先します。

Addressablesは、大規模な非同期ロード、追加コンテンツ、シーンストリーミングなどが実際に必要になった時点で導入します。

### 8. Assembly Definitionによる依存境界がまだない

現在のRuntimeスクリプトは基本的に `Assembly-CSharp`、Editorスクリプトは `Assembly-CSharp-Editor` にまとめてコンパイルされます。

フォルダによる責務分割はできていますが、誤った依存をコンパイラが防ぐ状態ではありません。

機能追加が本格化する前に、少なくとも以下の境界を検討します。

```text
DemonKing.Core
DemonKing.Gameplay
DemonKing.Presentation
DemonKing.Field.Prototype
DemonKing.Editor
DemonKing.Tests
```

ただし、Assembly Definitionはクラス配置と依存方向を整理した後に導入します。先に細かく分割しすぎると開発速度を落とすため、必要最小限から開始します。

### 9. 入力のライフサイクルは将来拡張を見越して整理する

現在はプレイヤーPrefab内の `MoveInputReader` がInput Action Assetを実行時に複製しています。1人用プロトタイプとしては問題ありません。

今後、Pause、メニュー、会話中の操作停止、リバインド、複数Action Mapを導入する段階では、入力の有効・無効をプレイヤー個体ではなくプレイ状態から制御できる構造へ移行します。

```text
Input Context
  ├ Gameplay
  ├ UI
  └ Disabled / Cutscene
```

現時点では大規模な入力マネージャーを作らず、Attack、Interact、Dodge、Pauseを追加するタイミングで再設計します。

### 10. プロトタイプ固有のグローバル設定を本番構成から分離する

`PrototypeSceneConfigurator` は `Application.targetFrameRate` やQuality設定、カメラ設定を変更します。

シーンが増えた場合、これらをフィールド固有の初期化と同じ場所で変更すると責務が曖昧になります。

複数シーン化する段階で、アプリケーション全体の設定とフィールド固有設定を分けます。

```text
ApplicationBootstrap
  -> フレームレート、共通サービス、永続設定

FieldBootstrap
  -> フィールド固有の生成、配置
```

## リファクタリング・リアーキテクチャの優先順位

今後は次の優先順位で進めます。

### 優先度P0: ゲーム機能を増やす前に実施する

#### P0-1. 正規シーンとBuild Settingsを統一する

- `Prototype.unity` を現在の正規プレイシーンとして明確化する
- Build Settingsを正規シーンへ合わせる
- `SampleScene.unity` の役割を廃止または明文化する
- Bootstrapが二重に存在しない状態にする

**理由:** 起動経路が複数ある状態で機能追加すると、シーンごとの設定差分や「片方では動く」問題が発生しやすいため。

#### P0-2. プレイヤー移動をRigidbody2Dベースへ移行する

- `CharacterMotor2D` のTransform直接更新を廃止する
- Rigidbody2DとCollider2DをプレイヤーPrefabへ追加する
- Collision Tilemapとの衝突を成立させる
- 移動処理と入力取得の分離は維持する
- フィールド境界のハードコード依存を削減する

**理由:** マップ制作を始めた後に移動・衝突方式を変更すると、全マップと全キャラクターへ影響するため。

#### P0-3. アイソメトリック描画順ルールを確定する

- Sorting Layerを定義する
- Tilemapと動的Spriteの描画順ルールを統一する
- Foreground、建物、キャラクターの前後関係を検証する
- Yソートの適用対象と例外ルールを決める

**理由:** 本番アートとPrefabが増える前に決めないと、後から大量の再設定が必要になるため。

#### P0-4. 設定値の二重管理を解消する

- `moveSpeed` の所有者を1つにする
- `sortingPrecision` の所有者を1つにする
- `FieldExtents` をSpawnerの固定値から切り離す
- 必要になった設定だけをScriptableObject化する

**理由:** Inspectorとコードのどちらが正しいか分からなくなる状態を早期に防ぐため。

#### P0-5. HUDをプレイヤーPrefabから分離する

- `PrototypeHud` をプレイヤーPrefabの必須コンポーネントから外す
- UI Rootをシーン側へ置く
- プレイヤー状態の表示は参照経由にする

**理由:** プレイヤー生成・破棄とUIのライフサイクルを分離し、会話UIやメニュー追加に備えるため。

### 優先度P1: NPC・戦闘・会話を追加する前後で実施する

#### P1-1. 実行時生成地形をIsometric Tilemapへ移行する

- `TerrainBuilder` から移行を開始する
- GroundとCollisionをTilemapで管理する
- PropsとForegroundの役割を整理する
- 実行時生成とTilemapの二重管理を段階的に終了する

#### P1-2. プレイヤー入力Actionを拡張する

`PlayerControls.inputactions` に以下を追加します。

```text
Move
Attack
Interact
Dodge
Pause
```

会話中やメニュー中の入力制御が必要になった時点で、GameplayとUIのAction Map切り替えを導入します。

#### P1-3. 戦闘とインタラクションをFeatureとして独立させる

推奨する境界例:

```text
Gameplay/
  Combat/
    Health
    IDamageable
    DamageReceiver
    AttackController
  Interaction/
    IInteractable
    InteractionDetector
  Characters/
    CharacterMotor2D
```

プレイヤー、NPC、敵の個別クラスへ共通ルールを直接重複実装しません。

#### P1-4. カメラをプレイヤー生成から独立したシステムにする

- プレイヤー追従を独立コンポーネント化する
- プレイヤー参照をSpawnerまたはScene Compositionから渡す
- 将来のカットシーンやエリア固定カメラへ拡張できるようにする

Cinemachineは必要性が確認できた時点で導入します。

#### P1-5. 建物・木・街灯をPrefabまたはTileアセットへ移行する

- `ArchitectureBuilder`
- `NatureBuilder`
- `RuntimeShapeFactory`

の順に依存を減らし、本番アセットへ置き換えます。

### 優先度P2: コンテンツ量が増える前に実施する

#### P2-1. Assembly Definitionを導入する

最初は過度に細分化せず、Runtime、Editor、Testsを中心に分離し、その後必要に応じてCore、Gameplay、Presentationを分けます。

依存方向をコンパイル時に検証できる状態を目標にします。

#### P2-2. `Resources.Load` を直接参照へ置き換える

- プレイヤーPrefab
- 入力アセット
- 将来追加する設定アセット

は、可能なものからSerializeFieldまたは設定用ScriptableObject経由の参照へ移行します。

#### P2-3. データと振る舞いを分離する

武器、敵、NPC、アイテム、スキルなど種類が増えた時点で、静的なマスターデータはScriptableObjectなどへ分離します。

ただし、すべてを最初からデータ駆動にせず、同種データが複数存在する機能から順に適用します。

#### P2-4. テスト基盤を追加する

優先して自動化する対象:

- `WorldSortOrder` のEditModeテスト
- アイソメトリック座標変換のEditModeテスト
- HP・ダメージ計算のEditModeテスト
- Prototypeシーンが起動できるPlayModeスモークテスト
- プレイヤーPrefabの必須コンポーネント検証

ゲームの見た目そのものより、壊れると広範囲へ影響する共通ルールを優先します。

### 優先度P3: 本番規模へ移行する段階で実施する

#### P3-1. セーブ境界を導入する

セーブ機能が必要になった時点で `ISaveService` を導入し、ゲームロジックからファイルシステムやプラットフォームSDKを直接呼ばない構造にします。

#### P3-2. Platform層を追加する

Steam実績、クラウドセーブ、ユーザー識別などを導入するタイミングでPlatform境界を追加します。

```text
Gameplay / Application
  ↓
Platform Abstraction
  ├ Steam
  └ Console
```

#### P3-3. 大規模アセットロード戦略を導入する

複数エリア、長時間プレイ、大量アセット、DLCなどの要件が明確になった場合に、Addressablesやシーン分割、非同期ロードを導入します。

#### P3-4. パフォーマンス予算を定義する

- ターゲットFPS
- 画面内Sprite数
- 2D Light数
- 透明描画の重なり
- メモリ使用量
- ロード時間

を実機または想定最低スペックで計測し、Steam版だけでなく将来のコンソール移植を意識した基準を持ちます。

## 当面導入しないもの

現段階では以下を目的なく導入しません。

- 大規模なDIコンテナ
- ECS/DOTSへの全面移行
- 汎用イベントバスによる全面的な疎結合化
- すべてのクラスへのInterface追加
- 必要性がない段階でのAddressables全面導入
- 独自フレームワーク化
- 将来使うかもしれない機能の先行実装

このプロジェクトでは、抽象化そのものを目的にせず、実際に発生した変更理由を分離するためにリファクタリングします。

## 次に実施する推奨順序

直近の作業順は以下とします。

```text
1. 正規シーンとBuild Settingsの統一
2. CharacterMotor2DのRigidbody2D化とCollision Tilemap連携
3. Sorting Layer / Yソート方針の確定
4. プレイヤー設定値の二重管理解消
5. PrototypeHudのプレイヤーPrefabからの分離
6. TerrainBuilderからIsometric Tilemapへの移行開始
7. Attack / Interact / Dodge / Pause入力追加
8. 戦闘・インタラクション機能の追加
9. Assembly Definitionとテスト基盤の導入
10. Resources依存の段階的削減
```

この順序は、今後のゲーム機能追加を止めるためのものではありません。後から変更すると広範囲へ影響する「シーン、移動、衝突、描画順、設定所有権」を先に安定させ、その後に戦闘、会話、クエストなどの機能を追加することを目的とします。

## 実装ルール

- 1クラスに複数の独立した変更理由を持たせない
- 入力デバイスの具体的なキー判定をゲームプレイコードへ書かない
- 入力バインディングは原則として `.inputactions` アセットで管理する
- 描画順の計算規則は共通化する
- UIから直接ゲームルールを変更しない
- プロトタイプ専用処理には、そのことが分かる名前を付ける
- 将来使うかもしれないという理由だけで抽象化を増やさない
- プラットフォーム依存が実際に発生した時点で境界を追加する
- コメントや設計ドキュメントは日本語で記述する

## 移行方針

大規模な一括書き換えではなく、機能追加のタイミングで既存コードを小さく置き換えていきます。

常に `main` のプロトタイプが遊べる状態を維持しながら、古い実装を新しい構造へ徐々に寄せることを優先します。

`Field/Prototype` 配下は移行期間中の互換層として扱い、本番用のシーン、Prefab、Tilemapが揃った機能から順に削減していきます。
