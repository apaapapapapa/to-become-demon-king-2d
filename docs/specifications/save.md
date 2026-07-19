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

## 今後

- ローカル保存実装
- 自動保存タイミング
- Save Slot
- Version Migration
- Steam Cloud
- コンソール向け保存
