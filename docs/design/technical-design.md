# 技術設計

## 目的

この文書はUnity固有の基準環境と実装方式を定義します。レイヤー責務は [アーキテクチャ](./architecture.md)、Feature間の接続は [Feature間の責務境界](./feature-boundaries.md)、ゲーム上の振る舞いは各 [仕様書](../specifications/) を参照してください。

## 基準環境

- Unity Editor: `6000.5.4f1`
- C#
- Universal Render Pipeline（URP）
- Unity Input System
- Isometric Tilemap
- Canvas（uGUI）
- Rigidbody2D / TilemapCollider2D
- Unity Test Framework
- `DemonKing.Domain`: Unity非依存Assembly
- `DemonKing.Runtime`: Unity Runtime Assembly

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
  ├ PrototypeWorldBuilder
  ├ GamePauseController
  └ PrototypeUiInstaller
```

`FieldBootstrap` は最小のエントリーポイントに保ち、具体的な構築とFeature間配線を下位のInstaller / Builder / Coordinatorへ委譲します。

## Scene / Tilemap

```text
Grid
  ├ Ground
  ├ Collision
  ├ Props
  └ Foreground
```

表示データと衝突データを分離します。

## Input実装

`PlayerControls.inputactions` にGameplayとUIのAction Mapを分離し、`PlayerInputReader` がInput Contextを切り替えます。

Action / BindingとContextの振る舞いは [入力仕様](../specifications/input.md) を参照してください。

## 移動と物理

通常移動は `CharacterMotor2D`、Dodge移動は `CharacterDodge2D` が担当し、Rigidbody2D経由で移動します。

Dodgeの入力と将来のCombat連携は [入力仕様](../specifications/input.md) を参照してください。

## UI

本番UI基盤はCanvas（uGUI）です。

Viewは表示を担当し、ゲーム状態の変更主体にしません。モーダルUIのInput ContextとTime Scaleの振る舞いは [入力仕様](../specifications/input.md) を参照してください。

## ScriptableObject

静的コンテンツ定義、バランス値、Asset参照はScriptableObject Definitionとして管理します。Definition / Runtime State / Save DTOの責務分離は [アーキテクチャ](./architecture.md) を参照してください。

具体的な値のSource of Truth規則は [ドキュメント規約](../development/documentation-rules.md) を参照してください。

## Resources

Resourcesは少数の起動入口や互換用途に限定します。コンテンツ量や非同期ロード要件が必要性を示した段階でAddressablesを検討します。

## Editorツール

- `IsometricPrototypeSceneBuilder`
- `PrototypeProjectAssetsAutoRepair`
- `JapaneseUiFontInstaller`

Runtimeの通常動作をEditor保守ツールへ依存させません。

## テスト

- Unity非依存ルールはDomain側の高速なテストを優先する。
- Unity Objectが必要なDefinition等はEditModeで検証する。
- Scene、Input Context、移動、UI等のRuntime統合はPlayModeで検証する。
- 単一クラスが主対象なら原則 `<ClassName>Tests` とする。
- Unity実行モードを名前で明示する必要がある場合は `<ClassName>PlayModeTests` 等を使用できる。
- 統合テストはFeatureとFlowが分かる名前を使用し、無関係な責務を1クラスへ追加し続けない。

## Platform実装

Platform固有SDKの隔離方針は [アーキテクチャ](./architecture.md#platform境界) を参照してください。
