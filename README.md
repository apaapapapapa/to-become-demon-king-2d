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
- Rigidbody2D／TilemapCollider2Dによる2D物理衝突
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

最初の目標は、移動、衝突、描画順、インタラクション、戦闘を検証できる小さなプレイ可能ループを作ることです。

## 現在の構成

プロトタイプ完成後の拡張に備え、スクリプトとアセットを責務ごとに分離しています。

```text
Assets/
  Resources/
    Art/
      Characters/
        PrototypeSlime/          試作ピクセルフレーム
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
      Math/
    Gameplay/
      Characters/
    Presentation/
      Characters/
      Rendering/
      UI/
    Field/
      Prototype/
    World/
    FieldBootstrap.cs
    SlimeController.cs
  Editor/
```

## 現在のワールド構築

`FieldBootstrap` から `PrototypeWorldBuilder` を起動し、次の要素を組み合わせてプロトタイプフィールドを構築します。

- `Ground` Tilemap：草地と小道
- `Collision` Tilemap：フィールド外周と校舎基部の衝突
- `Props` Tilemap：今後のワールド配置用
- `Foreground` Tilemap：今後の前景タイル用
- World Prefab：校舎、木、街灯
- RuntimeShapeFactory：移行途中の小規模な装飾と演出のみ

地形1マスごとのGameObject生成は廃止し、草地と小道のデータはIsometric Tilemapを正としています。

試作プレイヤーは `PrototypeSlime.prefab` として管理します。現在の見た目は `PrototypeSlimeSpriteAnimator` がピクセルフレームアセットからSpriteを生成し、Idle／Moveアニメーションと左右反転を担当します。旧 `PrototypeSlimeView` による多層図形生成は使用しません。

校舎、木、街灯はそれぞれPrefab境界を持ち、本番アートへ差し替える際にフィールド配置ロジックを変更しなくて済む構造にしています。

## 開発順序

P0の基盤整備と、P1のうちワールド制作基盤に関するP1-1〜P1-4は実装済みです。

次は以下を進めます。

1. Unity Play ModeでCollision Tilemapの外周と校舎衝突を確認する。
2. `PlayerControls.inputactions` にAttack、Interact、Dodge、Pauseを追加する。
3. NPCとのInteraction／会話を独立機能として追加する。
4. Combat、HP、ダメージ、死亡処理を独立機能として追加する。
5. 本番タイルセットと本番キャラクター／ワールドアートへ段階的に差し替える。
6. EditMode／PlayModeテストを追加する。
7. コンテンツ量に応じてasmdefとResources依存を整理する。
8. Steam固有機能が必要になった時点でPlatform層を追加する。

詳細は以下を参照してください。

- `docs/GAME_DIRECTION.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/ARCHITECTURE.md`
