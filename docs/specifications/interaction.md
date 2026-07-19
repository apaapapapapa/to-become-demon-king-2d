# インタラクション仕様

## 現在のフロー

```text
Interact入力
  ↓
PlayerInteractor
  ↓
IInteractable
  ↓
NPC / 扉 / 宝箱 / 調査対象
```

`PlayerInteractor` は対象固有ロジックを知りません。

現在のPrototype NPCはFeature境界確認用の最小実装であり、完成版の会話システムではありません。

## 今後

- Interaction候補の優先順位
- UIプロンプト
- Dialogue開始
- 扉・宝箱
- 調査ポイント
- Interaction中のInput Context切り替え

会話システムを追加するときはInteractionとDialogueの責務を分離します。
