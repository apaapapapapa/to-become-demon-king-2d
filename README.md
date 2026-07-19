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

## 推奨プロジェクト構成

```text
Assets/
  Art/
    Characters/
    Environment/
    Tiles/
    UI/
  Audio/
  Prefabs/
    Characters/
    Environment/
    UI/
  Scenes/
    Prototype/
  Scripts/
    Core/
    Player/
    Combat/
    Interaction/
    World/
    UI/
  Settings/
```

## 開発順序

1. アイソメトリックの試作シーンを作成する。
2. Input System を設定する。
3. プレイヤーの移動を実装する。
4. カメラ追従を追加する。
5. Y座標に基づくスプライトの並び替えを追加する。
6. 1人の NPC とのインタラクションを追加する。
7. 単純な敵を1体と攻撃アクションを追加する。
8. 基本的な HP、ダメージ、死亡処理を追加する。

詳細は `docs/GAME_DIRECTION.md` と `docs/TECHNICAL_DESIGN.md` を参照してください。
