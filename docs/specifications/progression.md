# 成長仕様

## CharacterProgressionState

`CharacterProgressionState` はキャラクター単位の可変成長状態を保持します。

```text
CharacterDefinitionId
Level
CurrentExperience
UnlockedSkillIds
UnlockedEvolutionNodeIds
ArtProgressStates
  ArtId
  MasteryPoints
```

ScriptableObject Definitionをプレイ中の可変状態として書き換えません。

Art、Skill、Evolution固有のルールはそれぞれ [Art仕様](./art.md)、[Skill仕様](./skill.md)、[Evolution仕様](./evolution.md) を参照してください。

## Experience

`ExperienceTable` はレベルごとの累積必要経験値を表し、`CharacterProgressionState.GainExperience` が経験値とレベルを同時に更新します。

`LevelUpResult` が1回の経験値加算による変化を表します。Unity側では `ExperienceTableDefinition` がDomainの `ExperienceTable` を構築します。

具体的な経験値テーブルはUnity Definitionを正とします。

## Progression取得境界

`ProgressionAcquisitionService` は複数の取得元からArt習得とSkill解放を調停します。訓練、報酬、アイテム等の取得元固有条件をProgression Serviceへ持ち込みません。

`ProgressionGrantDefinition` は取得元が付与するArt / Skillの組み合わせだけを定義します。取得条件、配置、Interaction方法は保持しません。

フィールド上の一度きり取得物は次の経路を使用します。

```text
PlayerInteractor
  -> IInteractable
  -> ProgressionGrantInteractable
  -> ProgressionAcquisitionService
  -> ArtProgressionController / SkillProgressionController
  -> CharacterProgressionState
```

`ProgressionGrantInteractable` は正常なInteractionを一度処理すると消費済みとなり、同じRuntime中は再取得できません。すでに同じArt / Skillを取得済みの場合も一度きり取得物として消費します。

Prototypeでは `PrototypeProjectAssets` のProgression Pickup設定をCompositionのSource of Truthとします。個別の取得物、付与対象、表示情報、Stable Content IDはRuntime Definitionを正とし、この仕様書へ複製しません。

フィールド取得物の消費状態は `ProgressionGrantConsumptionState` が保持します。Save Version 3では消費済みGrantのStable IDを `WorldSaveData.consumedProgressionGrantIds` へ保存し、ロード後は取得済みGrantを再配置しません。Art / Skillそのものの取得状態は `CharacterProgressionState` と対応するSave DTOで別途保存・復元します。

具体的な保存形式とMigrationは [セーブ仕様](./save.md) を参照してください。個別Art / Skillのプレイヤー向け情報は [Art一覧](../database/arts/) / [Skill一覧](../database/skills/) を参照してください。

Combat / Rewardから成長状態へ接続する方向は [Feature間の責務境界](../design/feature-boundaries.md#ability--combat--reward--progression) を参照してください。

## Save

成長状態の保存対象とDTO構造は [セーブ仕様](./save.md) を参照してください。

現在の実装状況と今後の対象は [ロードマップ](../development/roadmap.md) を参照してください。
