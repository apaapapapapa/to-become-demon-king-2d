# セーブ仕様

## 現在の状態

保存先の具体実装は未着手ですが、Runtime Stateと保存形式を分離する境界は実装済みです。

## 構造

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

## Runtime State

ゲームプレイ中に変化する状態です。Save DTOそのものをGameplayの実行状態として使用しません。

## Save DTO

`GameSaveData` / `PlayerSaveData` は保存形式を表します。

保存データは将来の互換性を考慮し、Runtime Stateとは独立して変更できる構造を維持します。

## Mapper

`CharacterProgressionSaveMapper` がRuntime StateとSave DTOの相互変換を担当します。

変換ロジックをGameplayコンポーネントや保存先実装へ分散させません。

## ISaveService

保存先を抽象化する契約です。

具体的なローカルファイル、Steam Cloud、将来のコンソール保存領域などは、この境界の外側で実装します。

GameplayコードからPlatform固有の保存APIを直接呼び出しません。

## Stable Content ID

Save Dataからキャラクター、Skill、Evolution等を参照する場合は安定IDを使用します。

表示名やAssetファイル名を保存上の識別子として使用しません。

## 現在未実装

- ローカル保存実装
- 自動保存タイミング
- 手動保存UI
- Save Slot
- データVersion Migration
- Steam Cloud
- コンソール向け保存実装

具体実装を開始するときは、保存タイミング、失敗時挙動、Version Migrationをこの仕様へ追加します。
