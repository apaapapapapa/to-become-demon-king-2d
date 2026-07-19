# To Become Demon King 2D

『To Become Demon King 2D』は、Witchbrook などの作品が持つビジュアルの方向性に着想を得た、Unity 製のアイソメトリック 2D／2.5D RPG 試作です。

## 技術方針

- Unity 6
- C#
- Universal Render Pipeline（URP）
- 2D Renderer／2D Lighting
- Isometric Tilemap
- Unity Input System
- Canvas（uGUI）
- Rigidbody2D／TilemapCollider2Dによる2D物理衝突
- ピクセルアートを重視した表現
- キーボードとゲームパッドに対応
- Steamを第一ターゲットとし、将来のコンソール移植も考慮

## 現在の操作

| 操作 | キーボード | ゲームパッド |
| --- | --- | --- |
| 移動 | WASD / 矢印キー | 左スティック |
| 攻撃 | J | Westボタン |
| 調べる・話す | E | Southボタン |
| 回避 | Left Shift | Eastボタン |
| ポーズ | Escape | Startボタン |

AttackとInteractは現在のプロトタイプ機能へ接続済みです。DodgeとPauseはInput Actionとイベント境界まで実装済みで、実際の回避・ポーズ挙動は後続機能で接続します。

## 現在のプレイ可能ループ

1. キーボードまたはゲームパッドでアイソメトリックマップ内を移動する。
2. Collision Tilemapによる物理境界に衝突する。
3. カメラがプレイヤーへ滑らかに追従する。
4. NPCへ近づきInteract入力で相互作用する。
5. 訓練用スライムへAttack入力で攻撃する。
6. HPを減らし、HPが0になると対象を倒す。
7. Canvas（uGUI）のHUDで場所と操作案内を表示する。

## 現在の主要構成

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
      Math/
    Gameplay/
      Characters/
      Interaction/
      Combat/
    Presentation/
      Camera/
        CameraFollow2D.cs
      Characters/
      Rendering/
      UI/
        GameHudView.cs
    Field/
      Prototype/
        PrototypeCameraInstaller.cs
        PrototypeUiInstaller.cs
    World/
    FieldBootstrap.cs
    SlimeController.cs
```

## 入力構造

`PlayerInputReader` が `PlayerControls.inputactions` の実行時インスタンスを所有し、Move / Attack / Interact / Dodge / Pauseを論理入力として提供します。

`MoveInputReader` はInput Action Assetを所有せず、`PlayerInputReader` のMove値を転送する互換アダプターです。

## Interaction

```text
PlayerInputReader.InteractPressed
  ↓
PlayerInteractor
  ↓
IInteractable
  ↓
NPC / 扉 / 宝箱 / 調査対象
```

現在は `PrototypeNpcInteractable` を1体配置し、Interact入力で相互作用できる最小ループを確認できます。

## Combat

```text
PlayerInputReader.AttackPressed
  ↓
PlayerMeleeAttack
  ↓
IDamageable
  ↓
Health
  ↓
Damaged / HealthChanged / Died
```

現在は訓練用スライムを1体配置し、攻撃、HP減少、死亡までの最小ループを確認できます。

## カメラ

カメラ追従はプレイヤーPrefabから独立しています。

```text
PrototypeWorldBuilder
  ↓ Player生成
PrototypeCameraInstaller
  ↓ Target設定
CameraFollow2D
  ↓ LateUpdate
Main Camera
```

`CameraFollow2D` はプレイヤー固有クラスを参照せず、任意の `Transform` を追従できます。将来のプレイヤー差し替え、イベントカメラ、一時的な注視対象切り替えへ拡張できる境界です。

## UI

旧IMGUIの `PrototypeHud` は廃止し、Canvas（uGUI）へ移行しました。

```text
UI Root
  ├ Canvas
  ├ CanvasScaler
  ├ GraphicRaycaster
  └ GameHudView
      └ HUD
          ├ Location Panel
          └ Controls Panel
```

`CanvasScaler` は1920x1080を基準とした `Scale With Screen Size` を使用します。ゲームルールとUI表示は分離し、今後の会話UI、メニュー、通知などは同じUI Root配下へ追加します。

## ワールド構築

`FieldBootstrap` から `PrototypeWorldBuilder` を起動し、次の要素を組み合わせます。

- `Ground` Tilemap：草地と小道
- `Collision` Tilemap：フィールド外周と校舎基部の衝突
- `Props` Tilemap：ワールド配置用
- `Foreground` Tilemap：前景用
- World Prefab：校舎、木、街灯
- Prototype Gameplay：試作NPC、訓練用スライム
- RuntimeShapeFactory：移行途中の小規模な装飾と試作表示のみ

## 開発状況

アーキテクチャ整理のP0とP1-1〜P1-9は実装済みです。

次はP2として、テスト自動化、Resources依存削減、本番タイル／アート導入、asmdef、入力コンテキスト整理などを進めます。

詳細は以下を参照してください。

- `docs/GAME_DIRECTION.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/ARCHITECTURE.md`
