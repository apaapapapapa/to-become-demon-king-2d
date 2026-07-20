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

見習い魔術師へInteract入力するたび、訓練用スライムが撃破済みなら同じ位置へ再生成し、生存中ならHPを全回復します。会話の進行状態に関係なく、パネルを閉じる入力でも復活処理を行います。

これはFeature境界確認用の最小実装であり、選択肢、会話分岐、話者立ち絵を持つ完成版の会話システムではありません。

## 今後

- Interaction候補の優先順位
- UIプロンプト
- 分岐を持つDialogue開始
- 扉・宝箱
- 調査ポイント
- Interaction中のInput Context切り替え

`PlayerInteractor` はDialogueやCombatを知りません。NPCが会話位置とInteraction通知を管理し、PrototypeのCompositionが通知をスライム再生成へ接続します。完成版の会話システムを追加するときもInteraction、Dialogue、Combatの責務を分離します。
