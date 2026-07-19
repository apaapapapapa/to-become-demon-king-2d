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
- `DemonKing.Domain`: Unity非依存の純C# assembly
- `DemonKing.Runtime`: Unity Runtime assembly

## Assembly境界

```text
DemonKing.Domain
  ↑
DemonKing.Runtime
  ↑
EditMode / PlayMode Tests
```

`DemonKing.Domain` はUnity Engine参照を持ちません。

成長状態、保存DTO、安定Content ID、Combat結果など、Unity SceneやMonoBehaviourを必要としないルール・状態をDomainへ置きます。

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

## Character Definition

プレイヤーを含むキャラクターの静的定義は `CharacterDefinition` に集約します。

```text
CharacterDefinition
  ├ characterId
  ├ prefab
  ├ statsDefinition
  ├ basicMeleeAttackDefinition
  └ dodgeDefinition
```

`characterId` はSave Dataや将来の進化・Skill解放でも参照できる安定IDです。

## Runtime StateとSave

プレイ中に変化する成長状態は `CharacterProgressionState` として純C#で保持します。

```text
CharacterDefinition
  ↓ initial identity
CharacterProgressionState
  ├ level
  ├ currentExperience
  ├ unlockedSkillIds
  └ unlockedEvolutionNodeIds
```

保存時は `CharacterProgressionSaveMapper` を経由して `PlayerSaveData` へ変換します。

```text
CharacterProgressionState
  ↕ CharacterProgressionSaveMapper
PlayerSaveData
  ↓
GameSaveData
```

保存先は `ISaveService` で抽象化します。現時点では保存形式・保存先の具体実装をGameplayへ持ち込みません。

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

基本攻撃:

```text
Attack Input
  ↓
PlayerMeleeAttack
  ↓
DamageRequest
  ↓
IDamageable / Health
  ↓
DamageResult
  ↓
DefeatContext
```

`MeleeAttackDefinition` は安定Ability ID、Damage Type、ダメージ、攻撃範囲などの静的定義を持ちます。

`DamageResult` と `DefeatContext` により、Healthや攻撃コンポーネントへ経験値・ドロップ処理を直接埋め込まず、後続のReward Serviceへ接続できます。

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

## ScriptableObject Definition

現在の主なDefinition:

- `CharacterDefinition`
- `CharacterStatsDefinition`
- `MeleeAttackDefinition`
- `DodgeDefinition`
- `PrototypeApplicationSettings`
- `PrototypeProjectAssets`

静的なRuntime設定値はこれらを正とします。プレイ中に変化する状態はDefinitionへ書き戻しません。

## Resources

Resourcesは少数の起動入口や互換用途に限定します。新しいコンテンツごとに個別の文字列パスを増やしません。

Addressablesは、コンテンツ量や非同期ロード要件が必要性を示した段階で導入します。

## テスト

```text
DemonKing.Domain
DemonKing.Runtime
DemonKing.EditMode.Tests
DemonKing.PlayMode.Tests
```

現在の主なテスト対象:

- 描画順
- Healthの死亡処理
- `DamageRequest` / `DamageResult` / `DefeatContext`
- `CharacterProgressionState`
- Save DTOとの相互変換
- `CharacterDefinition` の必須参照
- CameraFollow2D
- Input Context
- Dodge
- Pause / Resume

ルールや状態遷移は、Unity依存が不要なものほどDomain側の高速なテストを優先します。

## Editorツール

- `IsometricPrototypeSceneBuilder`: Prototypeシーンの基礎構造を再生成
- `PrototypeProjectAssetsAutoRepair`: ProjectAssetsとCharacterDefinitionの参照不整合を復旧
- `JapaneseUiFontInstaller`: 日本語Fontをプロジェクトアセットとして導入

Runtimeの通常動作をEditor保守ツールへ依存させません。

## Platform移植性

`ISaveService` の具体実装、実績、クラウド、ユーザー識別などPlatform依存機能は専用境界の外側へ置きます。GameplayコードからPlatform SDKを直接呼び出しません。

## 直近の実装方針

次の優先対象:

1. 経験値テーブル
2. Reward Service
3. `DefeatContext` から経験値加算への接続
4. Ability / Skill
5. Evolution

成長・進化のRuntime状態をScriptableObjectへ直接保存しない方針を維持します。

## パフォーマンス

- 透明描画の過剰な重なりを避ける。
- 2D Lightを無制限に増やさない。
- 必要になった段階でSprite Atlasを利用する。
- 控えめな性能のハードウェアでも計測する。
- 大規模マップでは一括ロードを前提にしない。
- 最適化は計測結果に基づいて行う。
