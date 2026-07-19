# 試作シーンのセットアップ

このプロジェクトには、Unity 内から最初のアイソメトリックシーン階層を安全に作成するための Editor ユーティリティが含まれています。

## シーンの作成

1. Unity でプロジェクトを開きます。
2. スクリプトのコンパイルが完了するまで待ちます。
3. Unity のメニューから次を選択します。

```text
Demon King
  -> Prototype
    -> Create Isometric Scene
```

ツールは次のシーンを作成して保存します。

```text
Assets/Scenes/Prototype/Prototype.unity
```

作成される階層は次のとおりです。

```text
Main Camera
Global Light Placeholder
Grid
  Ground
  Collision
  Props
  Foreground
Runtime Prototype Bootstrap
```

## Grid の設定

生成される Grid は次の設定を使用します。

```text
Cell Layout: Isometric
Cell Size: (1, 0.5, 1)
```

Tilemap は役割ごとに分けます。

- `Ground`: 歩行可能な地面の表示タイル
- `Collision`: `TilemapCollider2D` を持つ非表示の進入禁止タイル
- `Props`: 木、岩、家具などのワールド内の小物
- `Foreground`: キャラクターに視覚的に重なる可能性がある要素

これは、ゲームのアイソメトリック 2D／2.5D マップで長期的に採用する構成です。

## 現在の移行状態

既存の `FieldBootstrap` は、実行時に一時的な試作草原を引き続き生成します。実際の試作タイルを導入する間もプロジェクトをすぐに遊べる状態に保つため、これは意図した動作です。

移行手順は次のとおりです。

1. 実行時生成の試作を使ってアイソメトリック構図を検証する。
2. 小規模な試作用タイルセットを作成またはインポートする。
3. 最初のマップを `Ground` Tilemap に描く。
4. 進入禁止領域を `Collision` Tilemap に移す。
5. 木や風景を `Props` と `Foreground` に移す。
6. `FieldBootstrap` の役割を、ゲームプレイ用キャラクターと一時的なテストコンテンツの生成だけになるまで縮小する。
7. 最終的に、実行時のワールド生成を完全に削除する。

## 最初のマップの目標

最初に手作業で制作するマップは、意図的に小さく保ちます。次の要素を検証できる広さがあれば十分です。

- プレイヤーの8方向移動
- コントローラー入力
- コリジョン
- Y座標に基づく並び替え
- 小物の手前や奥を歩くこと
- インタラクション可能な NPC またはオブジェクト1つ
- 敵との遭遇1回

これらのインタラクションが手作業で制作した Isometric Tilemap 上で正しく動作する前に、大規模なワールドを構築しないでください。
