# アーキテクチャ方針

## 目的

このプロジェクトは、現在の遊べるプロトタイプを壊さずに、機能追加へ耐えられる構造へ段階的に移行します。

プロトタイプでは1つのMonoBehaviourに複数の責務をまとめる方が速い一方、規模が大きくなると入力、移動、描画、UI、戦闘、会話、セーブなどの変更が互いに影響しやすくなります。

今後は、機能単位で責務を分離し、Unity固有のシーン制御とゲームルールを必要以上に密結合させない方針で進めます。

## 現在のディレクトリ方針

```text
Assets/
  Scripts/
    Core/
      Input/          入力の抽象化、入力値の提供
      Math/           座標計算など副作用の少ない共通処理
    Gameplay/
      Characters/     キャラクターの移動や状態などゲームプレイ処理
    Presentation/
      Characters/     キャラクターの見た目・アニメーション
      Rendering/      描画順などレンダリング補助
      UI/             画面表示
    World/            ワールド上の汎用コンポーネント
    FieldBootstrap.cs 現在の試作ワールド生成の移行境界
    SlimeController.cs 既存プロトタイプ互換の構成ルート
  Editor/              Unity Editor専用ツール
```

新しい機能は、原則として既存の巨大クラスへ直接追加せず、対応する機能ディレクトリに独立したコンポーネントまたは通常のC#クラスとして追加します。

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

Steamや将来のコンソールSDKなどのプラットフォーム固有処理は、必要になった時点で `Platform` 境界を追加し、ゲームプレイコードから直接参照しない構造にします。

## 今回のリファクタリング

従来の `SlimeController` は次の責務を持っていました。

- Input SystemのAction生成
- 移動
- フィールド境界制御
- 潰れ・伸びアニメーション
- Y座標による描画順制御
- 試作HUD表示
- HUD用テクスチャの生成と破棄

これらを以下へ分離しました。

- `MoveInputReader`: 論理移動入力
- `CharacterMotor2D`: 移動と移動可能範囲
- `CharacterSquashAnimator`: 試作キャラクターの見た目
- `GroupYSorter`: 複数SpriteRendererの描画順
- `PrototypeHud`: 試作案内UI
- `SlimeController`: 既存の `FieldBootstrap` から利用する構成ルート

これにより、今後プレイヤーキャラクターの見た目を差し替えたり、UIをCanvasへ移行したり、入力設定をInput Actionsアセットへ移行しても、移動ロジックへの影響を抑えられます。

## FieldBootstrapの扱い

`FieldBootstrap` は現在、フィールド生成、背景、建物、植生、水面、環境アニメーション、プレイヤー生成などをまとめて担当しています。

これはプロトタイプとしては有効ですが、今後の本番開発では段階的に次のように分割します。

```text
FieldBootstrap
  ├ WorldSceneInstaller        シーン初期化
  ├ EnvironmentBuilder         地形・背景
  ├ PropBuilder                建物・木・小物
  ├ AmbientEffectController    水面・蛍などの環境表現
  └ PlayerSpawner              プレイヤー生成
```

ただし、現時点で一度に分割するとプロトタイプの見た目を壊すリスクが高いため、今回は `FieldBootstrap` を「移行境界」として残します。

新規の恒久機能を `FieldBootstrap` に追加することは避けます。

## 今後の優先順位

1. Input Actionをコード生成から `.inputactions` アセット管理へ移行する
2. プレイヤーをシーンまたはPrefab管理へ移行し、`FieldBootstrap` の動的生成から切り離す
3. `FieldBootstrap` の環境生成処理を複数Builderへ分割する
4. 本番UIをCanvas / UI Toolkitのどちらかに統一する
5. 戦闘、会話、クエスト、インベントリをそれぞれ独立したFeatureとして追加する
6. セーブ機能追加時に `ISaveService` を導入する
7. Steam固有機能追加時にPlatform層を導入する
8. EditMode / PlayModeテストを機能単位で追加する

## 実装ルール

- 1クラスに複数の独立した変更理由を持たせない
- 入力デバイスの具体的なキー判定をゲームプレイコードへ書かない
- 描画順の計算規則は共通化する
- UIから直接ゲームルールを変更しない
- プロトタイプ専用処理には、そのことが分かる名前を付ける
- 将来使うかもしれないという理由だけで抽象化を増やさない
- プラットフォーム依存が実際に発生した時点で境界を追加する
- コメントや設計ドキュメントは日本語で記述する

## 移行方針

大規模な一括書き換えではなく、機能追加のタイミングで既存コードを小さく置き換えていきます。

常に `main` のプロトタイプが遊べる状態を維持しながら、古い実装を新しい構造へ徐々に寄せることを優先します。
