# 成長仕様

## 現在の状態

経験値加算とレベル更新まで実装済みです。SkillとEvolutionは未実装です。

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
- Skill / Evolutionの人間向け索引: `docs/database/`

## 今後

1. Ability / Skill
2. Skill解放ルール
3. Evolution Node / Tree
4. Evolution条件と実行処理
5. 成長UI
