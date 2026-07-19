# To Become Demon King 2D

『To Become Demon King 2D』は、Witchbrook などの作品が持つビジュアルの方向性に着想を得た、Unity 製のアイソメトリック 2D／2.5D RPG 試作です。

## 技術方針

- Unity 6
- C#
- Universal Render Pipeline（URP）
- 2D Renderer／2D Lighting
- Isometric Tilemap
- Unity Input System
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

## スクリプト構成

プロトタイプ完成後の拡張に備え、スクリプトは責務ごとに段階的に分離します。

```text
Assets/
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
    World/
    FieldBootstrap.cs
    SlimeController.cs
  Editor/
```

`FieldBootstrap.cs` は現在の試作ワールドを維持するための移行境界として残しています。新しい恒久機能は原則として `Core`、`Gameplay`、`Presentation` などの責務別ディレクトリへ追加します。

## 開発順序

1. 現在の遊べるプロトタイプを維持しながら責務分割を進める。
2. Input Actions をアセット管理へ移行する。
3. プレイヤーを Prefab またはシーン管理へ移行する。
4. `FieldBootstrap` の環境生成処理を複数の Builder へ分割する。
5. NPC 会話とインタラクションを独立機能として追加する。
6. 戦闘、HP、ダメージ、死亡処理を独立機能として追加する。
7. クエスト、インベントリ、セーブ機能を段階的に追加する。
8. Steam 固有機能が必要になった時点で Platform 層を追加する。

詳細は以下を参照してください。

- `docs/GAME_DIRECTION.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/ARCHITECTURE.md`
