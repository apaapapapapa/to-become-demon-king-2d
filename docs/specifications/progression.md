# 成長仕様

## 現在の状態

成長システム本体のゲームルールは未完成ですが、成長状態を安全に追加するためのDomain境界は実装済みです。

## Runtime State

プレイ中に変化する成長状態は `CharacterProgressionState` が保持します。

```text
CharacterProgressionState
  ├ CharacterDefinitionId
  ├ Level
  ├ CurrentExperience
  ├ UnlockedSkillIds
  └ UnlockedEvolutionNodeIds
```

ScriptableObjectのDefinitionをプレイ中に書き換えて成長状態として使用しません。

## Character Definitionとの関係

```text
CharacterDefinition
  ↓ 安定ID・静的定義
CharacterProgressionState
  ↓ プレイ中に変化
Save DTO
```

`CharacterDefinitionId` は表示名やAsset名ではなく安定Content IDを使用します。

## 現在未実装

- 経験値テーブル
- レベルアップ条件
- レベルアップ時の能力値成長
- Reward Service
- Skill解放条件
- Evolution解放条件
- 実際の進化処理

## 直近の実装順

1. 経験値テーブルを定義する。
2. Reward Serviceを追加する。
3. `DefeatContext` から経験値報酬へ接続する。
4. `CharacterProgressionState` へ経験値を加算する。
5. Level更新ルールを追加する。
6. Skill / Evolutionへ拡張する。

## Source of Truth

- 静的なキャラクター設定: `CharacterDefinition` と関連ScriptableObject
- プレイ中の成長状態: `CharacterProgressionState`
- 保存形式: `PlayerSaveData` / `GameSaveData`
- 進化・Skillの人間向け索引: `docs/database/`
