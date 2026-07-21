# Feature間の責務境界

## 目的

この文書は、複数Featureにまたがる概念の意味と接続方向を定義します。各Feature内部の振る舞いは `docs/specifications/` を参照してください。

## Ability / Art / Skill / Evolution

| 概念 | 責務 |
| --- | --- |
| Ability | キャラクターが実行できる行動。取得経路や成長状態を知らない。 |
| Art | 1つ以上のAbilityを習得・熟練によって段階解放する能動技能。 |
| Skill | Ability性能やArt成長等へ作用する受動的な成長要素。 |
| Evolution | 形態や成長経路を変える不可逆または排他的な選択。 |

この分離を採用した理由は [ADR-0002](../decisions/ADR-0002-ability-art-skill-boundaries.md) と [ADR-0003](../decisions/ADR-0003-evolution-nodes-and-exclusive-paths.md) を参照してください。

## Ability / Combat / Reward / Progression

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
  ↓ 撃破時
DefeatContext
  ↓
RewardService
  ↓
CharacterProgressionState / ProgressionAcquisitionService
```

- Ability実行系はArt、Skill、Evolutionの取得状態を直接変更しません。
- Combatは経験値、ドロップ、Art / Skill取得、Evolution処理を直接実行しません。
- Rewardは `DefeatContext` を境界としてCombat外で適用します。
- 同一Defeatに対するRewardの重複付与を許可しません。

個別の振る舞いは [Ability仕様](../specifications/ability.md)、[戦闘仕様](../specifications/combat.md)、[成長仕様](../specifications/progression.md) を参照してください。

## Art熟練とAbility効果

```text
AbilityController
  ↓
IAbilityExecutor
  ↓ 実効果成立
AbilityEffectResolved
  ↓ Execution単位で重複排除
ArtProgressionService
  ↓
ArtProgressState
```

ExecutorやCombat効果はArt進捗を直接変更しません。Art側がAbility IDとExecution IDを使って熟練度を更新します。

## 受動Modifier

SkillとEvolutionのGameplay補正は、取得元を限定しないModifier Source契約として公開します。

```text
Skill / Evolution
  ↓
NumericModifier / Modifier Source
  ↓
Combat / Ability / Art
```

補正利用側はSkillやEvolutionの取得状態を直接参照しません。将来の装備やバフも同じ境界へ追加できます。

## Interaction / Dialogue

```text
Interact Input
  ↓
PlayerInteractor
  ↓
IInteractable
  ↓
PrototypeNpcInteractable
  ↓
LinearDialogueSequence
  ↓
DialogueLog
  ↓
DialogueLogView
```

`PlayerInteractor` はDialogueやNPC固有処理を知りません。Dialogueの振る舞いは [Dialogue仕様](../specifications/dialogue.md)、汎用Interactionは [Interaction仕様](../specifications/interaction.md) を参照してください。

## Gameplay Event / Quest

```text
Combat / Dialogue / Exploration
  ↓
GameplayEventHub
  ↓ GameplayEvent
QuestProgressionService
  ↓
QuestProgressState
```

CombatやDialogueはQuest固有処理を直接呼びません。QuestはGameplay Eventを入力境界として進捗を更新します。

詳細は [Quest仕様](../specifications/quest.md) を参照してください。

## Spawning / Prototype Composition

Spawn対象固有の生成と、再利用・再生成の判断を分離します。

```text
Concrete Factory
  ↓ spawn delegate
SpawnLifecycle<T>
  ├ canRestore
  └ restore
```

Prototypeの訓練エリアは `PrototypeGameplayFeatureInstaller` が具体オブジェクトとサービスを組み合わせ、複数Featureにまたがる流れを次のComposition境界へ分割します。

- `TrainingQuestFlowController`: NPC Interactから訓練対象のSpawn / Restoreを要求し、Quest状態に応じたDialogue選択、Quest受注・報告完了、Completion Grantを調停する。
- `TrainingDummyEventBridge`: 現在の訓練対象のDefeatをGameplay Eventへ変換し、撃破個体を `SpawnLifecycle<T>` のCurrentから外す。Quest状態やDialogueは参照しない。
- 撃破Reward: Combat構成側で `PrototypeGameplayFeatureInstaller` が訓練対象と `RewardService` を接続し、Quest Flowへ持ち込まない。

各Gameplay Featureは互いの具体実装へ依存せず、Feature横断の接続はGameplay EventまたはPrototype Compositionへ閉じ込めます。

詳細は [Spawning仕様](../specifications/spawning.md) を参照してください。
