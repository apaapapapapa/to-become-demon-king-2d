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
    Field/
      Prototype/      現在の実行時生成プロトタイプ専用
    World/
    FieldBootstrap.cs
    SlimeController.cs
  Editor/
```

`FieldBootstrap.cs` は現在、シーン初期設定と `PrototypeWorldBuilder` の起動だけを担当します。実行時生成プロトタイプの具体的な地形・建物・自然物・環境演出・プレイヤー生成は `Field/Prototype` 配下へ分離しています。

新しい恒久機能は原則として `Core`、`Gameplay`、`Presentation` などの責務別ディレクトリへ追加し、`Field/Prototype` には本番機能を増やしません。

## 開発順序

1. 現在の遊べるプロトタイプを維持しながら責務分割を進める。
2. Input Actions をアセット管理へ移行する。
3. プレイヤーを Prefab またはシーン管理へ移行する。
4. 実行時生成の地形を本番用 Isometric Tilemap へ段階的に置き換える。
5. 校舎、木、街灯などを Prefab またはアートアセット管理へ移行する。
6. NPC 会話とインタラクションを独立機能として追加する。
7. 戦闘、HP、ダメージ、死亡処理を独立機能として追加する。
8. クエスト、インベントリ、セーブ機能を段階的に追加する。
9. Steam 固有機能が必要になった時点で Platform 層を追加する。

詳細は以下を参照してください。

- `docs/GAME_DIRECTION.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/ARCHITECTURE.md`
