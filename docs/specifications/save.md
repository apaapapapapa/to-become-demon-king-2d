# セーブ仕様

## 保存境界

```text
CharacterProgressionState
  ↕ CharacterProgressionSaveMapper
PlayerSaveData
  ↓
GameSaveData
  ↓
ISaveService
  ↓
Storage
```

- Runtime StateをSave DTOとして直接使用しない。
- Save DTOをGameplay実行状態として使用しない。
- 変換ロジックはMapperへ集約する。
- 保存先の具体実装は `ISaveService` の外側へ置く。
- Save Dataからコンテンツを参照するときはStable Content IDを使用する。

レイヤー上の責務は [アーキテクチャ](../design/architecture.md#definition--runtime-state--save-dto) を参照してください。

## Art進捗

Save Version 2から `PlayerSaveData` にキャラクター単位のArt進捗を保持します。

```text
artProgress[]
  artId
  masteryPoints
```

Art進捗レコードの存在を習得済みの意味とします。現在ランクと解放済みAbilityは `ArtDefinition` から導出するため保存しません。Art装備枠は設けないため、装備Artも保存しません。

`GameSaveDataMigrator` がVersion 1のデータへ空の `artProgress` を補い、Version 2へ更新します。対応範囲外のVersionは拒否します。

## Skill取得状態

取得済みSkillは `PlayerSaveData.unlockedSkillIds` に `skill.*` IDとして保存します。補正結果やDefinition参照は保存せず、ロード後に再計算します。

## Evolution選択状態

適用済みEvolution Nodeは `PlayerSaveData.unlockedEvolutionNodeIds` に `evolution.*` IDとして保存します。条件評価結果、排他グループ、補正値、Definition参照は保存せず、ロード後にDefinitionから再構築します。

現在の保存実装状況と今後の対象は [ロードマップ](../development/roadmap.md) を参照してください。
