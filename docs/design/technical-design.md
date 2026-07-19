# 技術設計

## この文書の役割

この文書は、現在の実装を基準にした技術設計と開発規約を定義します。

- ゲーム方針: `../game/vision.md`
- 責務境界: `architecture.md`
- 機能仕様: `../specifications/`

## 基準環境

- Unity Editor: `6000.5.4f1`
- C#
- Universal Render Pipeline（URP）
- Unity Input System
- Isometric Tilemap
- Canvas（uGUI）
- Rigidbody2D / TilemapCollider2D
- Unity Test Framework
- Runtime assembly: `DemonKing.Runtime`

## 起動フロー

```text
Prototype.unity
  ↓
FieldBootstrap
  ↓
PrototypeProjectAssets
  ↓
PrototypeApplicationInstaller
  ├ PrototypeApplicationSettings
  ├ PrototypeSceneConfigurator
  ├ PrototypeSortingConfigurator
  ├ PrototypeWorldBuilder
  ├ GamePauseController
  └ PrototypeUiInstaller
```

`FieldBootstrap` は設定値やゲームロジックを持ちません。

## SceneとTilemap

正規のPrototypeシーンは `Assets/Scenes/Prototype/Prototype.unity` です。

```text
Grid
  ├ Ground
  ├ Collision
  ├ Props
  └ Foreground
```

表示データと衝突データを分離します。

## Tileアセット

```text
Imported Sprite
  ↓
PrototypeProjectAssets
  ↓
PrototypeRuntimeTileFactory
  ↓
Unity Tile
  ↓
Tilemap
```

`PrototypeRuntimeTileFactory` はTexture2DやSpriteを生成せず、渡されたSpriteから実行時Tileだけを構築します。

## RuntimeShapeFactory

主要アートの正ではありません。Prototype専用の軽量装飾、未アセット化の柵やランドマーク、雰囲気確認用の簡易演出に限定します。

## 描画順

```text
sortingOrder = -round(worldY * precision) + offset
```

計算は `WorldSortOrder` に集約し、複数SpriteRendererを持つオブジェクトは `GroupYSorter` を使用します。

## Input

Input Actionsは `Assets/Resources/Input/PlayerControls.inputactions` で管理します。

### Gameplay

```text
Move
Attack
Interact
Dodge
Pause
```

### UI

```text
Navigate
Submit
Cancel
Pause
```

`PlayerInputReader` がGameplay / UI / Disabledを排他的に切り替えます。

詳細は [入力仕様](../specifications/input.md) を参照してください。

## 移動とDodge

通常移動:

```text
MoveInputReader
  ↓
CharacterMotor2D
  ↓
Rigidbody2D.MovePosition
```

Dodge:

```text
Dodge Input
  ↓
CharacterDodge2D
  ├ CharacterMotor2Dを一時ロック
  └ Rigidbody2D.MovePosition
```

移動速度は `CharacterStatsDefinition`、Dodge調整値は `DodgeDefinition` を正とします。

## Interaction

```text
Interact Input
  ↓
PlayerInteractor
  ↓
IInteractable
```

詳細は [インタラクション仕様](../specifications/interaction.md) を参照してください。

## Combat

```text
Attack Input
  ↓
PlayerMeleeAttack
  ↓
IDamageable
  ↓
Health
```

攻撃調整値は `MeleeAttackDefinition` を正とします。

詳細は [戦闘仕様](../specifications/combat.md) を参照してください。

## Pause

`GamePauseController` が状態を管理します。Pause時はTimeScaleを停止しInput ContextをUIへ切り替え、Resume時は元へ戻します。

`PauseMenuView` は状態変更イベントを表示するだけです。

## UI

本番UI基盤はCanvas（uGUI）です。

```text
UI Root
  ├ Canvas
  ├ CanvasScaler
  ├ GraphicRaycaster
  ├ GameHudView
  └ PauseMenuView
```

日本語Fontは `PrototypeProjectAssets` から注入します。

## ScriptableObject

現在の主なDefinition:

- `CharacterStatsDefinition`
- `MeleeAttackDefinition`
- `DodgeDefinition`
- `PrototypeApplicationSettings`
- `PrototypeProjectAssets`

Runtime数値はこれらのアセットを正とし、Markdownへ同じ値を恒常的に複製しません。

## Resources

Resourcesは少数の起動入口や互換用途に限定します。新しいコンテンツごとに個別の文字列パスを増やしません。

Addressablesは、コンテンツ量や非同期ロード要件が必要性を示した段階で導入します。

## テスト

```text
DemonKing.Runtime
DemonKing.EditMode.Tests
DemonKing.PlayMode.Tests
```

主なテスト対象:

- 描画順
- Healthの死亡処理
- CameraFollow2D
- Input Context
- Dodge
- Pause / Resume

機能追加時は、ルールや状態遷移を可能な範囲で自動テストへ追加します。

## Editorツール

- `IsometricPrototypeSceneBuilder`: Prototypeシーンの基礎構造を再生成
- `PrototypeProjectAssetsAutoRepair`: Editor上の参照切れやImport不整合を復旧
- `JapaneseUiFontInstaller`: 日本語Fontをプロジェクトアセットとして導入

Runtimeの通常動作をEditor保守ツールへ依存させません。

## Platform移植性

セーブ、実績、クラウド、ユーザー識別などPlatform依存機能を追加するときは専用境界を設計します。GameplayコードからPlatform SDKを直接呼び出しません。

## パフォーマンス

- 透明描画の過剰な重なりを避ける。
- 2D Lightを無制限に増やさない。
- 必要になった段階でSprite Atlasを利用する。
- 控えめな性能のハードウェアでも計測する。
- 大規模マップでは一括ロードを前提にしない。
- 最適化は計測結果に基づいて行う。
