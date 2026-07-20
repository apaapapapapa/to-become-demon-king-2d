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

現在のPrototype NPCは固定発言を `DialogueLog` へ追加し、画面左下の会話ログへ直近4件を表示します。ログ表示中もGameplay入力は継続します。

これはFeature境界確認用の最小実装であり、選択肢、会話分岐、ページ送り、話者立ち絵を持つ完成版の会話システムではありません。

## 今後

- Interaction候補の優先順位
- UIプロンプト
- 分岐を持つDialogue開始
- 扉・宝箱
- 調査ポイント
- Interaction中のInput Context切り替え

`PlayerInteractor` はDialogueを知らず、NPCが `DialogueLog` へ発言を追加します。完成版の会話システムを追加するときもInteractionとDialogueの責務を分離します。
