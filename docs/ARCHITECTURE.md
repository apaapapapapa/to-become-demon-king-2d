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

## 今後の優先順位

1. 実行時生成の地形を本番用Isometric Tilemapへ段階的に置き換える
2. 校舎、木、街灯などをPrefabまたはアートアセット管理へ移行する
3. 試作スライムの実行時図形ビジュアルを本番スプライト／アニメーションへ置き換える
4. 本番UIをCanvas / UI Toolkitのどちらかに統一する
5. `PlayerControls.inputactions` にAttack、Interact、Dodge、Pauseを追加する
6. 戦闘、会話、クエスト、インベントリをそれぞれ独立したFeatureとして追加する
7. セーブ機能追加時に `ISaveService` を導入する
8. Steam固有機能追加時にPlatform層を導入する
9. EditMode / PlayModeテストを機能単位で追加する

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
