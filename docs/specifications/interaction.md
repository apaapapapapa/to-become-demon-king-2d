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
  ↓ NPC発言
DialogueLog
  ↓
DialogueLogView
```

`PlayerInteractor` は対象固有ロジックを知りません。

現在のPrototype NPCは複数の発言を持ち、Interact入力のたびに次の発言へ進みます。`DialogueLog` は現在の1件だけを保持し、`DialogueLogView` が画面左下へ表示します。最終発言を表示した後、もう一度Interact入力するとパネルを閉じ、次回の会話は先頭から再開します。表示中もGameplay入力は継続します。

これはFeature境界確認用の最小実装であり、選択肢、会話分岐、話者立ち絵を持つ完成版の会話システムではありません。

## 今後

- Interaction候補の優先順位
- UIプロンプト
- 分岐を持つDialogue開始
- 扉・宝箱
- 調査ポイント
- Interaction中のInput Context切り替え

`PlayerInteractor` はDialogueを知らず、NPCが会話位置を管理して `DialogueLog` の表示中発言を更新します。完成版の会話システムを追加するときもInteractionとDialogueの責務を分離します。
