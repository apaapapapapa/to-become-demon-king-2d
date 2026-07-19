# インタラクション仕様

## 現在の範囲

プレイヤーが近距離の `IInteractable` を持つ対象へInteractionを実行できる最小構造です。

```text
Interact入力
  ↓
PlayerInteractor
  ↓
IInteractable
  ↓
NPC / 扉 / 宝箱 / 調査対象
```

## 責務

### PlayerInteractor

- Interact入力を受ける。
- 対象候補を検出する。
- 適切な `IInteractable` を呼び出す。

### IInteractable

Interaction可能な対象の契約です。

プレイヤー側へNPC、扉、宝箱などの固有ロジックを書きません。

## 現在のPrototype

試作NPCとのInteractionは、Feature境界が機能することを確認するための最小実装です。完成版の会話システムではありません。

## 今後の拡張

- Interaction候補の優先順位
- UIプロンプト
- 会話開始
- 扉・宝箱
- 調査ポイント
- Interaction中のInput Context切り替え

会話システムを追加する際は、InteractionとDialogueの責務を分離します。
