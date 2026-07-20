# セーブ仕様

## 現在の状態

保存先の具体実装は未着手ですが、Runtime Stateと保存形式を分離する境界は実装済みです。

```text
CharacterProgressionState
  ↕ CharacterProgressionSaveMapper
PlayerSaveData
  ↓
GameSaveData
  ↓
ISaveService
  ↓ 将来
Local / Cloud / Platform Storage
```

## 方針

- Runtime StateをSave DTOとして直接使用しない。
- Save DTOをGameplay実行状態として使用しない。
- 変換ロジックはMapperへ集約する。
- 保存先は `ISaveService` の外側へ置く。
- Save Dataからコンテンツを参照するときはStable Content IDを使用する。

## Art進捗

Save Version 2から、`PlayerSaveData` にキャラクター単位のArt進捗を保持します。

```text
artProgress[]
  artId           // art.*
  masteryPoints   // 累積値
```

Art進捗レコードの存在を習得済みの意味とします。現在ランクと解放済みAbilityは `ArtDefinition` から導出するため保存しません。Art装備枠は設けないため、装備Artも保存しません。

`GameSaveDataMigrator` がVersion 1のデータへ空の `artProgress` を補い、Version 2へ更新します。現在より新しいVersionやVersion 1より古いデータは拒否します。

## 今後

- ローカル保存実装
- 自動保存タイミング
- Save Slot
- 一般的なVersion Migration基盤
- 未知・削除済みコンテンツIDの扱い
- Steam Cloud
- コンソール向け保存
