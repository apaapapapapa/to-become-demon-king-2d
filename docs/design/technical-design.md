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
- Rigidbody / Collider（3D Physics）
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

表示データと物理判定を分離します。

`Ground` / `Props` / `Foreground` は2Dアイソメトリック表示を担当します。`Collision` Tilemapは衝突セルを配置するためのマーカーとして使用し、Runtimeでは `CollisionMapBuilder` が各セルを3D `BoxCollider` へ変換します。`TilemapCollider2D` はRuntime物理のSource of Truthにしません。

## 3Dフィールド座標

現在の2Dアイソメトリック表示資産とCameraを維持したまま3D Physicsへ移行するため、フィールド空間は次の軸規約を使用します。

```text
X / Y : フィールド平面
Z     : Elevation（高さ）
```

座標変換・軸解釈は `FieldSpace3D` に集約し、Gameplayコードへ個別の軸変換を分散させません。

Unity標準の重力方向はY軸であり、このプロジェクトのElevation軸とは一致しないため、キャラクターの `Rigidbody.useGravity` は使用しません。将来のJump / Fall / FlightではElevation専用の移動コンポーネントがZ方向速度と重力相当の加速度を制御します。

## Input実装

`PlayerControls.inputactions` にGameplayとUIのAction Mapを分離し、`PlayerInputReader` がInput Contextを切り替えます。

Action / BindingとContextの振る舞いは [入力仕様](../specifications/input.md) を参照してください。

## 移動と物理

通常の平面移動は `CharacterPlanarMotor`、Dodge移動は `CharacterDodge` が担当し、3D `Rigidbody` 経由でX/Y平面を移動します。現在は `CharacterPhysicsBody3D` がZ位置を固定し、既存ゲームプレイと同じ地上移動を維持します。

`CharacterPhysicsBody3D` は3D `Rigidbody` / `CapsuleCollider` をキャラクター物理のSource of Truthとし、旧Prefabに残る `Rigidbody2D` / `Collider2D` はRuntimeで無効化します。

既存PrefabのMonoScript GUID互換を保つため、`CharacterMotor2D` と `CharacterDodge2D` は当面それぞれ `CharacterPlanarMotor` / `CharacterDodge` を継承する移行用ラッパーとして残します。新規コードは新しいクラス名へ依存します。

有限高さの建物・障害物はZ方向へ厚みを持つ3D Colliderとして表現します。Actorの3D Colliderと高さ方向で重ならなければPhysics上も衝突しないため、将来飛行で建物上端を超えた場合は特別な高さ例外判定なしで上空を通過できます。フィールド外周など高度に関係なく越えさせない境界は十分大きいZ高さを持つHard Boundaryとして扱います。

Dodgeの入力と将来のCombat連携は [入力仕様](../specifications/input.md) を参照してください。

## Combat / Interactionの空間Query

Interaction、近接攻撃、Projectile命中判定は3D Physics Queryを使用し、X/Y距離だけでなくZ方向のCollider重なりも判定対象にします。これにより地上Actorと高空Actorを同一平面座標に配置しても、物理体積が重ならなければ相互作用・攻撃対象になりません。

既存PlayModeテストfixtureの段階移行用としてCombatには地上限定のPhysics2D互換Queryを一時的に残します。正式なRuntime Actorは `CharacterPhysicsBody3D` がCollider2Dを無効化するため、Runtimeの物理判定は3D Queryを使用します。この互換Queryは既存テストfixtureを3D化した後に削除します。

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
- Scene、Input Context、3D Physics、移動、UI等のRuntime統合はPlayModeで検証する。
- 単一クラスが主対象なら原則 `<ClassName>Tests` とする。
- Unity実行モードを名前で明示する必要がある場合は `<ClassName>PlayModeTests` 等を使用できる。
- 統合テストはFeatureとFlowが分かる名前を使用し、無関係な責務を1クラスへ追加し続けない。

## Platform実装

Platform固有SDKの隔離方針は [アーキテクチャ](./architecture.md#platform境界) を参照してください。
