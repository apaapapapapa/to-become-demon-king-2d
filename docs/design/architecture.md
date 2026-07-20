# アーキテクチャ

## 目的

この文書は、責務境界、依存方向、Composition Root、意図的に残す移行境界、将来のリアーキテクチャ判断基準を定義します。

## 基本原則

- Domain、Core、Gameplay、Presentation、Compositionを分離する。
- プレイ中に変化する状態とScriptableObject Definitionを分離する。
- Runtime StateとSave DTOを分離する。
- Platform固有処理をGameplayへ直接持ち込まない。
- UI表示とゲーム状態管理を分離する。
- Bootstrapを肥大化させない。
- 必要性が確認できるまで過剰な抽象化を導入しない。

## レイヤー

### Domain

`DemonKing.Domain` はUnity非依存の純C#領域です。

```text
Progression/
  CharacterProgressionState
  ExperienceTable
  LevelUpResult
Save/
  GameSaveData
  PlayerSaveData
Combat/
  DamageRequest
  DamageResult
  DefeatContext
StableContentId
```

### Core

アプリケーション基盤と共通処理を置きます。

```text
Core/
  Application/
    GamePauseController
    ISaveService
    CharacterProgressionSaveMapper
  Input/
  Math/
```

### Gameplay

Unity上で動くゲームルールとキャラクター挙動を置きます。

```text
Gameplay/
  Characters/
  Combat/
  Interaction/
  Progression/
  Rewards/
```

GameplayはDomain/Coreを利用できますが、Prototype固有クラスやuGUI Viewへ依存しません。

### Presentation

カメラ、描画順、アニメーション、uGUI Viewを置きます。ゲームルールの決定主体にはしません。

### Field / Prototype

Prototypeシーンを組み立てるComposition層です。恒久的なDomain/Gameplayルールをここへ蓄積しません。

## 依存方向

上位のComposition層が具体クラスとUnityアセットを組み合わせ、DomainがUnityやPrototypeを知らないことを重視します。

## Definition / Runtime State / Save DTO

```text
Definition
  CharacterDefinition
  CharacterStatsDefinition
  MeleeAttackDefinition
  DodgeDefinition
  ExperienceTableDefinition
  RewardDefinition
       ↓
Runtime State
  CharacterProgressionState
       ↓ Mapper
Save DTO
  PlayerSaveData / GameSaveData
```

Definitionは静的定義、Runtime Stateはプレイ中に変化する状態、Save DTOは保存形式です。

## Stable Content ID

保存データやコンテンツ間参照には、表示名やAsset名とは独立した安定IDを使用します。

```text
character.player.slime
ability.basic_melee
reward.training_dummy
```

## 起動構造

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

`FieldBootstrap` は最小のエントリーポイントに保ちます。

## CharacterDefinition

```text
CharacterDefinition
  ├ characterId
  ├ prefab
  ├ statsDefinition
  ├ basicMeleeAttackDefinition
  ├ dodgeDefinition
  └ experienceTableDefinition
```

## Combat / Reward境界

```text
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

経験値・ドロップ・進化処理をHealthや攻撃コンポーネントへ直接埋め込みません。RewardServiceは同一Defeatに対する重複付与を防ぐ境界を持ちます。

## Save境界

```text
CharacterProgressionState
  ↕ CharacterProgressionSaveMapper
PlayerSaveData
  ↓
GameSaveData
  ↓
ISaveService
```

ローカル保存、クラウド、Platform保存は `ISaveService` の外側で実装します。

## 意図的に残している移行境界

- `Field/Prototype`: Prototype Composition領域
- `SlimeController`: 既存Prefab互換の薄いマーカー
- `RuntimeShapeFactory`: Prototype専用補助表現
- `Resources`: 少数の起動入口・互換用途
- `PrototypeProjectAssetsAutoRepair`: Editor上の参照修復ツール

## 完了済みの基盤

- Rigidbody2D / Collision Tilemap
- Isometric描画順
- Input Action / Input Context
- Interaction / Combat
- uGUI / Camera / Pause / Dodge
- ScriptableObject Definition
- ApplicationInstaller
- Domain assembly
- CharacterDefinition
- CharacterProgressionState
- ExperienceTable / LevelUpResult
- Save DTO / ISaveService境界
- DamageResult / DefeatContext
- RewardServiceから経験値加算への接続
- EditMode / PlayModeテスト

## 直近の拡張方針

1. Ability / Skill
2. Evolution
3. NPC会話
4. 敵AI
5. クエスト・目的管理
6. 実際のセーブ保存実装

## リアーキテクチャ判断基準

- 同じ変更理由で複数箇所を毎回修正している。
- Platform固有コードがGameplayへ漏れ始めた。
- Resourcesや単一Sceneがコンテンツ量に耐えられない。
- テスト困難性が責務分離不足を示している。
- ScriptableObjectだけでは大量データの整合性管理が難しい。
- 複数機能が同じRuntime Stateを別々に管理し始めた。
