# To Become Demon King 2D

『To Become Demon King 2D』は、Witchbrook などの作品が持つビジュアルの方向性に着想を得た、Unity 製のアイソメトリック 2D／2.5D RPG 試作です。

## 技術方針

- Unity 6
- C#
- Universal Render Pipeline（URP）
- 2D Renderer／2D Lighting
- Isometric Tilemap
- Unity Input System
- `.inputactions` アセットによる入力定義管理
- ピクセルアートを重視した表現
- 開発初期からキーボードとゲームパッドに対応
- まず Steam 向けに開発し、将来のコンソール移植も考慮

## 最初のプレイ可能マイルストーン

最初の縦切り試作では、プレイヤーが次の操作を行えるようにします。

1. キーボードまたはゲームパッドでアイソメトリックマップ内を移動する。
2. スプライトが正しく並び替えられ、風景の手前や奥を歩く。
3. 1人の NPC と会話する。
4. 1体の敵を攻撃する。
5. 敵を倒し、簡単な結果または報酬を受け取る。

最初の目標は、完成度の高いゲームを作ることではありません。移動、カメラ、描画順、インタラクション、戦闘を検証できる、小さなプレイ可能ループを作ることです。

## 現在の構成

プロトタイプ完成後の拡張に備え、スクリプトとアセットを責務ごとに段階的に分離しています。

```text
Assets/
  Resources/
    Input/
      PlayerControls.inputactions
    Prefabs/
      Characters/
        PrototypeSlime.prefab
  Scripts/
    Core/
      Input/
      Math/
    Gameplay/
      Characters/
    Presentation/
      Characters/
      Rendering/
      UI/
    Field/
      Prototype/      現在の実行時生成プロトタイプ専用
    World/
    FieldBootstrap.cs
    SlimeController.cs
  Editor/
```

`FieldBootstrap.cs` はシーン初期設定と `PrototypeWorldBuilder` の起動だけを担当します。実行時生成プロトタイプの具体的な地形・建物・自然物・環境演出は `Field/Prototype` 配下へ分離しています。

入力バインディングは `PlayerControls.inputactions` で管理し、`MoveInputReader` は `Player/Move` の論理入力だけを提供します。

試作プレイヤーは `PrototypeSlime.prefab` として管理し、`PrototypePlayerSpawner` はPrefabのインスタンス化と配置だけを担当します。現在の仮スライムビジュアルは `PrototypeSlimeView` が生成しており、本番アートへ差し替えやすい境界にしています。

新しい恒久機能は原則として `Core`、`Gameplay`、`Presentation` などの責務別ディレクトリへ追加し、`Field/Prototype` には本番機能を増やしません。

## 開発順序

1. 現在の遊べるプロトタイプを維持しながら責務分割を進める。
2. 実行時生成の地形を本番用 Isometric Tilemap へ段階的に置き換える。
3. 校舎、木、街灯などを Prefab またはアートアセット管理へ移行する。
4. 試作スライムを本番スプライト／アニメーションへ置き換える。
5. `PlayerControls.inputactions` に攻撃、会話、回避、ポーズ操作を追加する。
6. NPC 会話とインタラクションを独立機能として追加する。
7. 戦闘、HP、ダメージ、死亡処理を独立機能として追加する。
8. クエスト、インベントリ、セーブ機能を段階的に追加する。
9. Steam 固有機能が必要になった時点で Platform 層を追加する。

詳細は以下を参照してください。

- `docs/GAME_DIRECTION.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/ARCHITECTURE.md`
