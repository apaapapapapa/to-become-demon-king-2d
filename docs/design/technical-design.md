# 技術設計

## 基準環境

- Unity Editor: `6000.5.4f1`
- C#
- Universal Render Pipeline（URP）
- Unity Input System
- Isometric Tilemap
- Canvas（uGUI）
- Rigidbody2D / TilemapCollider2D
- Unity Test Framework
- `DemonKing.Domain`: Unity非依存
- `DemonKing.Runtime`: Unity Runtime

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

## Scene / Tilemap

```text
Grid
  ├ Ground
  ├ Collision
  ├ Props
  └ Foreground
```

表示データと衝突データを分離します。

## Input

`PlayerControls.inputactions` にGameplayとUIのAction Mapを分離します。`PlayerInputReader` がGameplay / UI / Disabledを排他的に切り替えます。

詳細は [入力仕様](../specifications/input.md) を参照してください。

## Character Definition

```text
CharacterDefinition
  ├ characterId
  ├ prefab
  ├ statsDefinition
  ├ basicMeleeAttackDefinition
  ├ dodgeDefinition
  └ experienceTableDefinition
```

Character IDはSaveや将来のEvolution/Skillから参照できる安定IDです。

## Runtime State / Experience

`CharacterProgressionState` がLevel、累積経験値、Skill解放ID、Evolution Node解放IDを保持します。

`ExperienceTable` は累積必要経験値をUnity非依存で評価し、`LevelUpResult` が1回の経験値加算結果を表します。

Unity側の `ExperienceTableDefinition` がDomainのExperienceTableを構築します。

## Combat / Reward

```text
Attack
  ↓
DamageRequest
  ↓
IDamageable / Health
  ↓
DamageResult
  ↓
DefeatContext
  ↓
RewardService
  ↓
CharacterProgressionState.GainExperience
```

`RewardDefinition` が報酬IDと静的報酬内容を持ちます。同一Defeatへの重複報酬を防ぎます。

詳細は [戦闘仕様](../specifications/combat.md) と [成長仕様](../specifications/progression.md) を参照してください。

## Save

```text
CharacterProgressionState
  ↕ CharacterProgressionSaveMapper
PlayerSaveData
  ↓
GameSaveData
  ↓
ISaveService
```

具体的な保存先は `ISaveService` の外側で実装します。

## 移動 / Dodge

通常移動は `CharacterMotor2D`、Dodgeは `CharacterDodge2D` が担当し、いずれもRigidbody2D経由で移動します。

## Interaction

`PlayerInteractor` は `IInteractable` のみに依存し、NPC、扉、宝箱などの固有処理を知りません。

Prototype NPCが複数発言の進行位置を保持し、`DialogueLog` は現在表示する1件だけを管理します。`DialogueLogView` は表示中の発言をuGUIへ反映し、会話終了時は非表示にします。Interactionは発言内容や表示階層へ依存しません。

## Pause / UI

`GamePauseController` がTimeScaleとInput Contextを管理し、`PauseMenuView` は表示だけを担当します。本番UI基盤はCanvas（uGUI）です。

## ScriptableObject Definition

主なDefinition:

- `CharacterDefinition`
- `CharacterStatsDefinition`
- `MeleeAttackDefinition`
- `DodgeDefinition`
- `ExperienceTableDefinition`
- `RewardDefinition`
- `PrototypeApplicationSettings`
- `PrototypeProjectAssets`

静的値はDefinition、プレイ中に変化する値はRuntime Stateを正とします。

## Resources

Resourcesは少数の起動入口や互換用途に限定します。コンテンツ量や非同期ロード要件が必要性を示した段階でAddressablesを検討します。

## テスト

- Domain: ExperienceTable、CharacterProgressionState、Reward関連、Save Mapper等
- EditMode: Definitionや描画順など
- PlayMode: Input Context、移動、Dodge、Pause、Camera等

Unity依存が不要なルールはDomain側の高速なテストを優先します。

## Editorツール

- `IsometricPrototypeSceneBuilder`
- `PrototypeProjectAssetsAutoRepair`
- `JapaneseUiFontInstaller`

Runtimeの通常動作をEditor保守ツールへ依存させません。

## Platform移植性

Save、実績、クラウド、ユーザー識別などのPlatform依存機能は専用境界の外側へ置き、GameplayからPlatform SDKを直接呼び出しません。
