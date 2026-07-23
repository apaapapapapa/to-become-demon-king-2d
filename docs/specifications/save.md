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

Prototypeでは `LocalSaveSlotStore` が3つの固定Slotから具体的な `ISaveService` を解決し、各SlotのGame Save本体には `JsonFileSaveService` を使用します。

```text
Slot 1 -> save.json
Slot 2 -> save-2.json
Slot 3 -> save-3.json
```

Slot 1は既存Prototypeの `Application.persistentDataPath/save.json` をそのまま使用し、既存Saveを移動せず後方互換を維持します。Slot 2 / 3は同じディレクトリの独立ファイルへ保存します。

書き込み時は完成したJSONを一度 `.tmp` へ出力してから本体ファイルへ反映し、書き込み途中のJSONを本体パスへ直接残さないようにします。

起動時はApplication側が選択Slotから `ISaveService` を解決したうえで、`PrototypeGameSession` が次の順序を管理します。

1. `PrototypeFieldCatalog` によるInitial Field Definitionの解決
2. `PrototypeSaveSession` によるSave読込、Version Migration、Player / World / Story基礎Runtime State復元
3. `PrototypeFieldComposer` によるField / Player Runtime構築
4. `PrototypeGameSaveRestorer` によるAbility Loadout / Quest進捗のRuntime適用
5. `PrototypeGameSaveSnapshotProvider` と `PrototypeLocalSaveCoordinator` による保存開始

Saveファイルが存在しない場合はInitial FieldのDefault Entry Point、新規Player状態、Initial Story Chapterで開始します。JSON破損、未対応Version、現在のPlayer Definitionと異なるCharacter IDなどで復元できない場合は、既存Saveの上書きを防ぐため新規状態で起動し、その起動中の保存を無効化します。

保存されたField IDまたはEntry Point IDを現在のField Catalogで解決できない場合はInitial FieldのDefault Entry Pointへ戻します。SceneのBuild Indexや一時的なGameObject参照はSaveへ保存しません。

`PrototypeGameSaveSnapshotProvider` はComposition RootでRuntime Stateを `GameSaveData` へ集約します。`PrototypeLocalSaveCoordinator` はFeatureごとのDTO組立を行わず、次の保存タイミングだけを管理します。

- Runtime構築完了直後
- `Time.unscaledTime` 基準で15秒ごと
- Application Pause時
- Application Quit時
- Field遷移前後の明示Save

Gameplay Featureは保存先や保存タイミングを直接知りません。`PrototypeFieldComposer` と各Field InstallerもSave DTOを知りません。

## Save Slot Metadata

各SlotはGame Save本体とは別に表示用Metadataを `.metadata.json` へ保存します。MetadataはTitle / Continue / Load Gameの表示とSlot判定だけに使用し、Runtime Stateの復元元にはしません。

Metadataには少なくとも次を保持します。

```text
savedAtUtc
playTimeSeconds
level
currentFieldId
```

`LocalSaveSlotStore` はGame Save本体を先に検証し、UIへ次のSlot状態を返します。

- `Empty`: Game Save本体が存在しない
- `Ready`: 現在のMigration対象として読込可能
- `Corrupted`: 空ファイルや不正JSONなどで読込不能
- `UnsupportedVersion`: `GameSaveDataMigrator` の対応範囲外Version

Metadata sidecarが欠落または破損していてもGame Save本体が有効なら `Ready` とします。その場合、最終Save日時はGame Save本体の更新日時、Level / Current FieldはGame Save本体から復元し、累積Play Timeは0秒から再開します。

Play Timeは既存Metadataの累積値へ、そのApplication Session中の `Time.unscaledTime` 経過分を加算します。

## Save Version

現在のSave Versionは `5` です。

`GameSaveDataMigrator` は次の順序で旧Versionを現在Versionへ移行します。

- Version 1 → 2: `artProgress` を追加
- Version 2 → 3: Ability Loadout、Quest進捗、World状態を追加
- Version 3 → 4: Stable Field IDとEntry Point IDをWorld状態へ追加
- Version 4 → 5: Questとは独立したStory状態を追加

Version 3以前のSaveにはField位置がないため、Migration後のField ID / Entry Point IDは空文字列へ正規化し、起動時にInitial FieldのDefault Entry Pointを使用します。

Version 4以前のSaveにはStory状態がないため、Migrationでは空の `StorySaveData` を追加し、Runtime復元時にInitial Chapterを適用します。対応範囲外のVersionは拒否します。欠落したCollectionはMigration後に空Collectionへ正規化します。

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

Quest進捗はStory状態を直接変更しません。Quest完了は共有Gameplay Event境界へ通知し、Story側が独立して反応します。

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

## Story状態

Save Version 5から、Questとは独立して本編進行を保存します。

```text
story
  currentChapterId
  flags[]
  executedEventIds[]
```

- `currentChapterId`: 現在章のStable Story Chapter ID
- `flags`: 成立済みStory Flag ID
- `executedEventIds`: 一度きりStory Eventの実行済みID

Story Event Definition、条件評価結果、Dialogue参照、Event Hub購読状態は保存しません。

`StoryProgressionSaveMapper` は `StoryProgressState` と `StorySaveData` の変換だけを担当します。Story Eventの条件判定やFlag更新は `StoryProgressionService` が担当し、Save Mapperへ持ち込みません。

具体的なStory / Quest責務分離とEvent発火規則は [Story Progression仕様](./story.md) を参照してください。

## Save Slot / New Game / Continue境界

ローカル保存は3 Slotを持ちます。Slot IDから具体的なファイルパスと `ISaveService` を解決する責務は `LocalSaveSlotStore` に置きます。

`ISaveService`、`GameSaveData`、各Mapper、Runtime StateはSlot数に依存させません。GameplayへSlot IDや具体的なファイルパスを持ち込みません。

`FieldBootstrap` はGame Sessionを直接開始せずTitle Screenを起動します。New Game / Continue / Load GameでのSlot選択と開始方法の制御はTitle/Application側で行い、選択後は解決済み `ISaveService` を `PrototypeApplicationInstaller` へ渡します。New Gameだけは `FreshGameSaveService` を通し、既存SaveのRestoreを明示的に抑止します。詳細は [Title Screen仕様](./title-screen.md) を参照してください。

現在の実装状況と開発優先度は [ロードマップ](../development/roadmap.md) を参照してください。
