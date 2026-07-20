# 成長仕様

## 現在の状態

経験値加算、レベル更新、Ability基盤、Artの習得・熟練・Ability付与・Save境界、受動Skill、Evolution Nodeの条件評価・排他選択・補正接続まで実装済みです。

Abilityは実行可能な行動、Artは複数Abilityを習得・熟練する能動技能、Skillは受動的な成長要素、Evolutionは形態・成長経路を変える不可逆または排他的な選択です。Art、Skill、Evolutionの判定をAbility実行処理へ埋め込みません。

## Runtime State

`CharacterProgressionState` が次を保持します。

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

ScriptableObject Definitionをプレイ中に書き換えません。

Art進捗レコードの存在が習得済みを表します。現在ランクと解放済みAbilityは `ArtDefinition` から導出し、Runtime StateやSave DTOへ重複保存しません。

## Art

Artは `art.*` のStable Content IDを持ち、1つ以上のAbilityを熟練ランクで段階解放します。Art習得時は0ポイント・ランク1とし、ランク1のAbilityを常時利用可能にします。

熟練度はAbilityの実行成功ではなく、命中、回復、バフ付与などの効果成立を受けて加算します。範囲、多段、継続効果で複数結果が発生しても、同じ使用者とExecution IDには1回だけ加算します。

Artの習得と熟練度加算は汎用の成長サービスが担当し、Ability、Executor、Combat効果は進捗を直接変更しません。詳細は [Art仕様](./art.md) を参照してください。

## Skill

Skillは能力値、Abilityのコストや性能、Art習得条件、Art成長などへ作用する受動的な成長要素です。Skill自体を実行可能な行動として扱いません。

Skillをきっかけに能動行動を利用可能にする場合も、その行動はArtまたは生得Abilityとして定義します。

`SkillProgressionService` が `skill.*` Definitionの存在を検証し、`CharacterProgressionState.UnlockedSkillIds` へ冪等に取得状態を追加します。現在は汎用補正契約を介して、与ダメージ、Abilityクールダウン、Art熟練ポイントへ接続済みです。詳細は [Skill仕様](./skill.md) を参照してください。

## Evolution

Evolutionは `evolution.*` Nodeとして定義し、レベル、Skill、Artランク、前提Node、排他グループを共通サービスで評価します。適用済みNode IDだけをRuntime Stateへ不可逆に追加し、同じ排他グループの別Nodeを拒否します。

選択済みNodeのGameplay補正はSkillと同じ汎用Modifier Sourceへ公開します。進化選択UIとPrototype形態表示は実装済みで、本番用の専用アートはPresentation内の後続タスクです。詳細は [Evolution仕様](./evolution.md) を参照してください。

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

1. Skill、Art、Evolutionの具体的な取得元・選択UI
2. Evolution専用の見た目・演出と上位Node
3. Abilityコスト、能力値、習得条件への追加補正
4. 回復、バフ、デバフなどの効果成立通知
