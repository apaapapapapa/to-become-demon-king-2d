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

正規SceneはCamera、Grid、Bootstrapを保持する最小Composition Rootとします。`PrototypeTilemapContext` がRuntimeで次のTilemap構成を解決・補完します。

```text
Grid
  ├ Ground
  ├ Collision
  ├ Props
  └ Foreground
```

表示データと物理判定を分離します。

`Ground` / `Props` / `Foreground` は2Dアイソメトリック表示を担当します。`Collision` Tilemapは衝突セルを配置するためのマーカーとして使用し、Runtimeでは `CollisionMapBuilder` が各セルを3D `BoxCollider` へ変換します。`TilemapCollider2D` は使用しません。

## 3Dフィールド座標

現在の2Dアイソメトリック表示資産とCameraを維持したまま3D Physicsを使用するため、フィールド空間は次の軸規約を使用します。

```text
X / Y : フィールド平面
Z     : Elevation（高さ）
```

座標変換・軸解釈は `FieldSpace3D` に集約し、Gameplayコードへ個別の軸変換を分散させません。

Unity標準の重力方向はY軸であり、このプロジェクトのElevation軸とは一致しないため、キャラクターの `Rigidbody.useGravity` は使用しません。`CharacterElevationMotor` がZ方向のJump初速、落下加速度、Flight上昇・下降を制御します。

## Input実装

`PlayerControls.inputactions` にGameplayとUIのAction Mapを分離し、`PlayerInputReader` がInput Contextを切り替えます。

`PlayerElevationInput` はPlayer固有の入力アダプタとして、Jump / Flight Toggle / Flight Elevation入力を `CharacterElevationMotor` の論理操作へ変換します。`CharacterElevationMotor` 自体はInput Systemへ依存しません。

Action / BindingとContextの振る舞いは [入力仕様](../specifications/input.md) を参照してください。

## 移動と物理

通常の平面移動は `CharacterPlanarMotor`、Dodge移動は `CharacterDodge` が担当し、3D `Rigidbody` 経由でX/Y平面を移動します。高さ方向は `CharacterElevationMotor` が独立してZ軸を制御します。

`CharacterElevationMotor` は `Grounded` / `Airborne` / `Flying` の状態を持ちます。地上ではZを固定し、JumpまたはFlight開始時にZ固定を解除します。Jump後とFlight解除後はElevation専用の落下加速度で下降し、地面または上向きの3D Collider接触面へ着地します。

`CharacterPhysicsBody3D` の3D `Rigidbody` / `CapsuleCollider` をキャラクター物理の唯一のSource of Truthとします。Runtimeコード、Player Prefab、正規Scene、PlayModeテストは `Rigidbody2D` / `Collider2D` / `TilemapCollider2D` に依存しません。

有限高さの建物・障害物はZ方向へ厚みを持つ3D Colliderとして表現します。ActorのCollider下端が障害物上端を超え、3D Collider同士が重ならない高度ではPhysics上も衝突しないため、Flight中は特別な高さ例外判定なしで上空を通過できます。フィールド外周など高度に関係なく越えさせない境界は十分大きいZ高さを持つHard Boundaryとして扱います。

2D表示では `CharacterElevationPresenter` がPhysics上のElevationをVisualの画面Yオフセットへ変換します。SortingはActor RootのフィールドYを基準とするため、Jump / FlightによるVisual移動で前後関係を変更しません。

具体的な移動状態と操作の振る舞いは [移動仕様](../specifications/movement.md) を参照してください。

## Combat / Interactionの空間Query

Interaction、近接攻撃、Projectile命中判定は3D Physics Queryのみを使用し、X/Y距離だけでなくZ方向のCollider重なりも判定対象にします。これにより地上Actorと高空Actorを同一平面座標に配置しても、物理体積が重ならなければ相互作用・攻撃対象になりません。

## 敵AI実装

敵AIは `Gameplay/AI` に配置し、AI自身へ移動物理やダメージ処理を重複実装しません。追跡移動は `CharacterPhysicsBody3D` へ移動要求を渡し、攻撃はプレイヤーと共通の `AbilityController` を実行入口として使用します。Prototype側はTargetとDefinitionの注入だけを担当します。

具体的な状態遷移、索敵・離脱、高度差の振る舞いは [敵AI仕様](../specifications/enemy-ai.md) を参照してください。

## UI

本番UI基盤はCanvas（uGUI）です。

Viewは表示を担当し、ゲーム状態の変更主体にしません。モーダルUIのInput ContextとTime Scaleの振る舞いは [入力仕様](../specifications/input.md) を参照してください。

`QuestTrackerView` は `QuestProgressionService` の `QuestAccepted` / `ProgressChanged` / `QuestCompleted` を購読し、常設トラッカーと非モーダル通知へ反映します。Quest受注はComposition層がNPC Interactionと `AcceptQuest` を接続し、PresentationからQuest状態を変更しません。具体的な表示ルールは [Quest仕様](../specifications/quest.md) を参照してください。

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
- Jump / Fall / Flightと有限高さ障害物の上空通過は3D PhysicsのPlayModeテストで検証する。
- Combat / Interactionの空間回帰テストも3D Collider / `Physics` APIで構築し、Physics2Dのテストfixtureを作らない。
- Enemy AIは追跡、Ability経由の攻撃、高度差による非交戦をPlayModeで検証する。
- Questは未受注時の非進捗、明示的受注、進捗、完了のライフサイクルとUI反映をPlayModeで検証する。
- 単一クラスが主対象なら原則 `<ClassName>Tests` とする。
- Unity実行モードを名前で明示する必要がある場合は `<ClassName>PlayModeTests` 等を使用できる。
- 統合テストはFeatureとFlowが分かる名前を使用し、無関係な責務を1クラスへ追加し続けない。

## Platform実装

Platform固有SDKの隔離方針は [アーキテクチャ](./architecture.md#platform境界) を参照してください。
