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

## Art進捗の将来形式

Art実装後は、`PlayerSaveData` にキャラクター単位のArt進捗を追加します。

```text
artProgress[]
  artId           // art.*
  masteryPoints   // 累積値
```

Art進捗レコードの存在を習得済みの意味とします。現在ランクと解放済みAbilityは `ArtDefinition` から導出するため保存しません。Art装備枠は設けないため、装備Artも保存しません。

Art進捗追加時はSave Versionを更新し、Version 1のデータには空の `artProgress` を補います。未知のArt IDをどう扱うかなど、一般的な欠損コンテンツ方針はVersion Migration実装時に確定します。

## 今後

- ローカル保存実装
- 自動保存タイミング
- Save Slot
- Art進捗DTOとVersion 1からのMigration
- 一般的なVersion Migration基盤
- Steam Cloud
- コンソール向け保存
