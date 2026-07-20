# 成長仕様

## 現在の状態

経験値加算、レベル更新、Ability基盤まで実装済みです。Artは設計済み・未実装、SkillとEvolutionは未実装です。

Abilityは実行可能な行動、Artは複数Abilityを習得・熟練する能動技能、Skillは受動的な成長要素、Evolutionは形態・成長経路を変える不可逆または排他的な選択です。Art、Skill、Evolutionの判定をAbility実行処理へ埋め込みません。

## Runtime State

`CharacterProgressionState` が次を保持します。

```text
CharacterDefinitionId
Level
CurrentExperience
UnlockedSkillIds
UnlockedEvolutionNodeIds
```

ScriptableObject Definitionをプレイ中に書き換えません。

Art実装後は、キャラクター単位のArt進捗を追加します。

```text
ArtProgress[]
  ArtId
  MasteryPoints
```

Art進捗レコードの存在が習得済みを表します。現在ランクと解放済みAbilityは `ArtDefinition` から導出し、Runtime StateやSave DTOへ重複保存しません。

## Art

Artは `art.*` のStable Content IDを持ち、1つ以上のAbilityを熟練ランクで段階解放します。Art習得時は0ポイント・ランク1とし、ランク1のAbilityを常時利用可能にします。

熟練度はAbilityの実行成功ではなく、命中、回復、バフ付与などの効果成立を受けて加算します。範囲、多段、継続効果で複数結果が発生しても、同じ使用者とExecution IDには1回だけ加算します。

Artの習得と熟練度加算は汎用の成長サービスが担当し、Ability、Executor、Combat効果は進捗を直接変更しません。詳細は [Art仕様](./art.md) を参照してください。

## Skill

Skillは能力値、Abilityのコストや性能、Art習得条件、Art成長などへ作用する受動的な成長要素です。Skill自体を実行可能な行動として扱いません。

Skillをきっかけに能動行動を利用可能にする場合も、その行動はArtまたは生得Abilityとして定義します。

## Experience

`ExperienceTable` はレベルごとの累積必要経験値を表し、`CharacterProgressionState.GainExperience` が経験値とレベルを同時に更新します。

`LevelUpResult` が1回の経験値加算による変化を表します。

Unity側では `ExperienceTableDefinition` がDomainのExperienceTableを構築します。

## Reward接続

```text
DefeatContext
  ↓
RewardService
  ↓ RewardDefinition
CharacterProgressionState.GainExperience
```

現在は訓練用ダミー撃破から経験値加算まで接続済みです。同一Defeatへの重複報酬は付与しません。

## Source of Truth

- 静的キャラクター定義: `CharacterDefinition`
- 経験値テーブル: `ExperienceTableDefinition`
- プレイ中の状態: `CharacterProgressionState`
- 保存形式: `PlayerSaveData` / `GameSaveData`
- Art / Skill / Evolutionの人間向け索引: `docs/database/`

## 今後

1. Art Definition、Art進捗、Save DTOとVersion Migration
2. 汎用Art習得、熟練ランクによるAbility付与
3. 効果成立通知とExecution単位の熟練度加算
4. 受動Skill Definitionと補正接続
5. Evolution Node / Treeと条件・実行処理
6. 成長UI
