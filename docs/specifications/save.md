# セーブ仕様

## 保存境界

```text
Runtime State
  ↕ Mapper
Save DTO
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

## ローカル保存

Prototypeでは `JsonFileSaveService` を使用し、`Application.persistentDataPath/save.json` へJSONで1スロット保存します。

書き込み時は完成したJSONを一度 `save.json.tmp` へ出力してから本体ファイルへ反映し、書き込み途中のJSONを本体パスへ直接残さないようにします。

起動時は `PrototypeSaveSession` がSaveを読み込み、Runtime Stateへ復元します。Saveファイルが存在しない場合は新規状態で開始します。JSON破損、未対応Version、現在のPlayer Definitionと異なるCharacter IDなどで復元できない場合は、既存Saveの上書きを防ぐため新規状態で起動し、その起動中の保存を無効化します。

`PrototypeLocalSaveCoordinator` はComposition RootでRuntime Stateを `GameSaveData` へ集約し、次のタイミングで保存します。

- Runtime構築完了直後
- `Time.unscaledTime` 基準で15秒ごと
- Application Pause時
- Application Quit時

Gameplay Featureは保存先や保存タイミングを直接知りません。

## Save Version

現在のSave Versionは `3` です。

`GameSaveDataMigrator` は次の順序で旧Versionを現在Versionへ移行します。

- Version 1 → 2: `artProgress` を追加
- Version 2 → 3: Ability Loadout、Quest進捗、World状態を追加

対応範囲外のVersionは拒否します。欠落したCollectionはMigration後に空Collectionへ正規化します。

## Player成長状態

### Art進捗

Save Version 2から `PlayerSaveData` にキャラクター単位のArt進捗を保持します。

```text
artProgress[]
  artId
  masteryPoints
```

Art進捗レコードの存在を習得済みの意味とします。現在ランクと解放済みAbilityは `ArtDefinition` から導出するため保存しません。Art装備枠は設けないため、装備Artも保存しません。

### Skill取得状態

取得済みSkillは `PlayerSaveData.unlockedSkillIds` に `skill.*` IDとして保存します。補正結果やDefinition参照は保存せず、ロード後に再計算します。

### Evolution選択状態

適用済みEvolution Nodeは `PlayerSaveData.unlockedEvolutionNodeIds` に `evolution.*` IDとして保存します。条件評価結果、排他グループ、補正値、Definition参照は保存せず、ロード後にDefinitionから再構築します。

## Ability Loadout

Save Version 3から、ユーザーが編集できる `Action1`〜`Action4` の割当だけを `PlayerSaveData.abilityLoadout` に保存します。`Primary` はCharacter Definition由来の予約枠なので保存しません。

ロード時は現在の `CharacterDefinition` と `CharacterProgressionState` から割当可能なAbilityを再評価します。未習得・削除済みAbility ID、不正なSlot、重複Abilityは無視します。

## Quest進捗

Save Version 3から、Quest単位で次を保存します。

```text
quests[]
  questId
  status
  objectives[]
    objectiveId
    currentCount
```

Required CountはQuest DefinitionをSource of Truthとし、Saveには現在値だけを保持します。ロード時は現在のDefinitionへObjective IDで対応付けし、進捗値を `0..RequiredCount` に補正します。削除済み・未知のQuest IDは無視し、不正なQuest Statusは復元エラーとして扱います。

## World状態

Save Version 3から、フィールド上の一度きりProgression Grantの消費済みIDを `WorldSaveData.consumedProgressionGrantIds` に保存します。

ロード後は消費済みGrantを再配置せず、同一Grantの再取得を防ぎます。Art / Skillそのものの取得状態はPlayer成長状態として別途復元します。

## 今後の拡張

現在は単一ローカルSave Slotです。複数Save Slot、クラウドSave、Steam / コンソール固有保存は、`ISaveService` 境界を維持したまま必要になった段階で追加します。

現在の実装状況と開発優先度は [ロードマップ](../development/roadmap.md) を参照してください。
