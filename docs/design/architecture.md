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
    ArtMasteryTable
    ArtProgressState
    CharacterProgressionState
  ExperienceTable
  LevelUpResult
Save/
  GameSaveData
  PlayerSaveData
StableContentId
```

`UnityEngine`、Scene、GameObject、MonoBehaviour、ScriptableObjectなどのUnity依存型をDomainへ持ち込みません。

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
  Abilities/
    AbilityController
    AbilityRuntimeState
    IAbilityExecutor
  Characters/
  Combat/
    DamageRequest
    DamageResult
    DefeatContext
  Interaction/
  Progression/
    ArtProgressionController
    ArtProgressionService
    EvolutionProgressionController
    EvolutionProgressionService
    EvolutionSelectionController
    SkillProgressionController
    SkillProgressionService
  Modifiers/
    NumericModifier
    Modifier Source contracts
  Rewards/
```

`DamageRequest`、`DamageResult`、`DefeatContext` は `UnityEngine.GameObject` を参照するため、Unity非依存のDomainではなくGameplay/Combatの責務です。

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
  AbilityDefinition
  ArtDefinition
  EvolutionDefinition
  SkillDefinition
  CharacterDefinition
  CharacterStatsDefinition
  MeleeAttackDefinition
  DodgeDefinition
  ExperienceTableDefinition
  RewardDefinition
       ↓
Runtime State
  AbilityRuntimeState
  ArtProgressState
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
art.magic.example
skill.combat.example
evolution.slime.example
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
  ├ abilityDefinitions[]
  ├ artDefinitions[]
  ├ skillDefinitions[]
  ├ evolutionDefinitions[]
  ├ dodgeDefinition
  └ experienceTableDefinition
```

## Ability / Art / Combat / Reward境界

```text
Player Input / AI
  ↓
AbilityController
  ↓
IAbilityExecutor
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

経験値・ドロップ・進化処理をHealthや攻撃コンポーネントへ直接埋め込みません。RewardServiceは同一Defeatに対する重複付与を防ぐ境界を持ちます。

Abilityは実行可能な行動、Artは1つ以上のAbilityを習得・熟練する能動技能、Skillは受動的な成長要素、Evolutionは形態・成長経路を変える不可逆または排他的な選択として分離します。AbilityControllerとExecutorはArt進捗、Skill取得状態、Evolution条件を知りません。

Artは次の境界で既存Ability基盤へ接続します。

```text
ArtProgressState + ArtDefinition
  ↓ ランクで解放済みのAbility
AbilityController
  ↓ 実行
IAbilityExecutor
  ↓ 効果成立通知
ArtProgressionService
  ↓ Execution単位で重複排除
ArtProgressState.AddMastery
```

Art進捗はDomain、静的なランク閾値とAbility対応はDefinition、習得・熟練度加算・Ability付与の調停はGameplayの責務です。効果処理から成長状態を直接変更しません。

Skillは `CharacterProgressionState.UnlockedSkillIds` と `SkillDefinition` から受動補正を導出します。`SkillProgressionController` が補正を汎用Modifier Source契約として公開し、Ability、Combat、Artは補正取得元がSkill、装備、バフのどれであるかを知りません。

EvolutionはNode Definitionと `UnlockedEvolutionNodeIds` を組み合わせ、レベル、Skill、Artランク、前提Node、排他グループを `EvolutionProgressionService` で評価します。選択済みNodeの永続補正はSkillと同じModifier Source境界へ公開します。

`EvolutionSelectionController` はInput Context、選択位置、確定要求を管理します。uGUIの `EvolutionMenuView` は評価結果を表示するだけで、進捗状態を直接変更しません。形態変更は `EvolutionApplied` 通知の外側にある `PrototypeSlimeEvolutionPresenter` が担当し、Save復元時も選択済みNodeから外見を導出します。

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
- Ability Definition / Runtime State / Controller / Executor
- プレイヤー入力とAIで共有できる基本近接攻撃
- Art Definition / Runtime State / 習得 / 熟練度 / Ability付与
- Ability Execution ID / 効果成立通知
- Save DTO Version 2 / Version 1 Migration
- 受動Skill Definition / 取得 / 汎用補正接続
- Evolution Node Definition / 条件評価 / 排他選択 / 永続補正
- Evolution選択UI / Prototype形態表示・演出
- EditMode / PlayModeテスト

## 直近の拡張方針

1. NPC会話
2. 敵AI
3. クエスト・目的管理
4. Art / Skill入力・UIと正式Runtimeコンテンツ
5. Evolutionの本番用アートと上位Node
6. 実際のセーブ保存実装

## リアーキテクチャ判断基準

- 同じ変更理由で複数箇所を毎回修正している。
- Platform固有コードがGameplayへ漏れ始めた。
- Resourcesや単一Sceneがコンテンツ量に耐えられない。
- テスト困難性が責務分離不足を示している。
- ScriptableObjectだけでは大量データの整合性管理が難しい。
- 複数機能が同じRuntime Stateを別々に管理し始めた。
