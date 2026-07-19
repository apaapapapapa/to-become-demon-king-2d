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

## 現在の操作

| 操作 | キーボード | ゲームパッド |
| --- | --- | --- |
| 移動 | WASD / 矢印キー | 左スティック |
| 攻撃 | J | Westボタン |
| 調べる・話す | E | Southボタン |
| 回避 | Left Shift | Eastボタン |
| ポーズ | Escape | Startボタン |

AttackとInteractは現在のプロトタイプ機能へ接続済みです。DodgeとPauseはInput Actionとイベント境界まで実装済みで、実際の回避・ポーズ挙動は後続機能で接続します。

## 最初のプレイ可能マイルストーン

現在のプロトタイプでは、次の縦切りループを検証できます。

1. キーボードまたはゲームパッドでアイソメトリックマップ内を移動する。
2. Collision Tilemapによる物理境界に衝突する。
3. NPCへ近づきInteract入力で相互作用する。
4. 訓練用スライムへAttack入力で攻撃する。
5. HPを減らし、HPが0になると対象を倒す。

会話本文、敵AI、報酬などはまだ本番機能ではありません。InteractionとCombatの責務境界を先に確立し、その上へゲーム固有ロジックを追加する方針です。

## 現在の構成

```text
Assets/
  Resources/
    Art/
      Characters/
        PrototypeSlime/
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
        PlayerInputReader.cs
        MoveInputReader.cs
      Math/
    Gameplay/
      Characters/
      Interaction/
        IInteractable.cs
        PlayerInteractor.cs
      Combat/
        IDamageable.cs
        Health.cs
        PlayerMeleeAttack.cs
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

## 入力構造

`PlayerInputReader` が `PlayerControls.inputactions` の実行時インスタンスを所有し、Move / Attack / Interact / Dodge / Pauseを論理入力として提供します。

既存の移動系コンポーネントは `MoveInputReader` を利用しますが、`MoveInputReader` 自身はInput Action Assetを所有せず、`PlayerInputReader` のMove値を転送する互換アダプターです。

これにより、1プレイヤー内でInput Action Assetを重複生成せず、InteractionやCombatも同じ入力境界を共有します。

## Interaction

`Gameplay/Interaction` はフィールドやNPC実装から独立しています。

```text
PlayerInputReader.InteractPressed
  ↓
PlayerInteractor
  ↓
IInteractable
  ↓
具体的なNPC・扉・宝箱など
```

現在は試作NPC `PrototypeNpcInteractable` を1体配置し、Interact入力で相互作用できる最小ループを確認できます。

## Combat

`Gameplay/Combat` は敵種別やフィールド実装から独立しています。

```text
PlayerInputReader.AttackPressed
  ↓
PlayerMeleeAttack
  ↓
IDamageable
  ↓
Health
  ↓
Damaged / Diedイベント
```

現在は訓練用スライムを1体配置し、攻撃、HP減少、死亡までの最小ループを確認できます。

## 現在のワールド構築

`FieldBootstrap` から `PrototypeWorldBuilder` を起動し、次の要素を組み合わせます。

- `Ground` Tilemap：草地と小道
- `Collision` Tilemap：フィールド外周と校舎基部の衝突
- `Props` Tilemap：今後のワールド配置用
- `Foreground` Tilemap：今後の前景タイル用
- World Prefab：校舎、木、街灯
- Prototype Gameplay：試作NPC、訓練用スライム
- RuntimeShapeFactory：移行途中の小規模な装飾と試作表示のみ

## 開発順序

P0とP1-1〜P1-7は実装済みです。

次は以下を優先します。

1. Unity Play ModeでInteractionとCombatの入力・当たり判定を確認する。
2. P1-8としてカメラ追従を独立コンポーネント化する。
3. P1-9として本番UIへ移行する。
4. NPC会話システムを `IInteractable` の上へ追加する。
5. 敵AI、攻撃リアクション、報酬処理をCombat Featureの外側へ追加する。
6. EditMode／PlayModeテストを追加する。
7. 本番タイルセットと本番キャラクター／ワールドアートへ段階的に差し替える。
8. コンテンツ量に応じてasmdefとResources依存を整理する。

詳細は以下を参照してください。

- `docs/GAME_DIRECTION.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/ARCHITECTURE.md`
