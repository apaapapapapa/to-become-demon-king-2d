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
- 変換ロジックはMapper / Snapshot境界へ集約する。
- 保存先の具体実装は `ISaveService` の外側へ置く。
- Save DataからコンテンツやFieldを参照するときはStable IDを使用する。

レイヤー上の責務は [アーキテクチャ](../design/architecture.md#definition--runtime-state--save-dto) を参照してください。

## ローカル保存

Prototypeでは `JsonFileSaveService` を使用し、`Application.persistentDataPath/save.json` へJSONで1スロット保存します。

書き込み時は完成したJSONを一度 `save.json.tmp` へ出力してから本体ファイルへ反映し、書き込み途中のJSONを本体パスへ直接残さないようにします。

起動時は `PrototypeGameSession` が次の順序を管理します。

1. `PrototypeFieldCatalog` によるInitial Field Definitionの解決
2. `PrototypeSaveSession` によるSave読込、Version Migration、Stable Field Location復元
3. `PrototypeFieldComposer` によるField / Player Runtime構築
4. `PrototypeGameSaveRestorer` によるAbility Loadout / Quest進捗のRuntime適用
5. `PrototypeGameSaveSnapshotProvider` と `PrototypeLocalSaveCoordinator` による保存開始

Saveファイルが存在しない場合はInitial FieldのDefault Entry Pointと新規Player状態で開始します。JSON破損、未対応Version、現在のPlayer Definitionと異なるCharacter IDなどで復元できない場合は、既存Saveの上書きを防ぐため新規状態で起動し、その起動中の保存を無効化します。

保存されたField IDまたはEntry Point IDを現在のField Catalogで解決できない場合はInitial FieldのDefault Entry Pointへ戻します。SceneのBuild Indexや一時的なGameObject参照はSaveへ保存しません。

`PrototypeGameSaveSnapshotProvider` はComposition RootでRuntime Stateを `GameSaveData` へ集約します。`PrototypeLocalSaveCoordinator` はFeatureごとのDTO組立を行わず、次の保存タイミングだけを管理します。

- Runtime構築完了直後
- `Time.unscaledTime` 基準で15秒ごと
- Application Pause時
- Application Quit時

Gameplay Featureは保存先や保存タイミングを直接知りません。`PrototypeFieldComposer` と各Field InstallerもSave DTOやQuest復元処理を知りません。

## Save Version

現在のSave Versionは `4` です。

`GameSaveDataMigrator` は次の順序で旧Versionを現在Versionへ移行します。

- Version 1 → 2: `artProgress` を追加
- Version 2 → 3: Ability Loadout、Quest進捗、World状態を追加
- Version 3 → 4: Stable Field IDとEntry Point IDをWorld状態へ追加

Version 3以前のSaveにはField位置がないため、Migration後のField ID / Entry Point IDは空文字列へ正規化し、起動時にInitial FieldのDefault Entry Pointを使用します。対応範囲外のVersionは拒否します。欠落したCollectionはMigration後に空Collectionへ正規化します。

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

Save Version 4から、現在位置を次のStable IDで保存します。

```text
world
  currentFieldId
  entryPointId
  consumedProgressionGrantIds[]
```

- `currentFieldId`: Field Definitionを識別する変更されないID
- `entryPointId`: Field内のSpawn / Entry Pointを識別する変更されないID
- Scene名、Scene Build Index、Player座標、Transform参照は保存しない

ロード後は消費済みGrantを再配置せず、同一Grantの再取得を防ぎます。Art / Skillそのものの取得状態はPlayer成長状態として別途復元します。

## Save Slot / New Game / Continue境界

現在は単一ローカルSave Slotです。複数Save Slot、New Game / Continue、クラウドSave、Steam / コンソール固有保存を追加する場合、Slotや開始方法に応じた保存先解決はApplication / Platform側で行います。

`ISaveService`、`GameSaveData`、各Mapper、Runtime StateはSlot数に依存させません。GameplayへSlot IDや具体的なファイルパスを持ち込みません。

現在の実装状況と開発優先度は [ロードマップ](../development/roadmap.md) を参照してください。
