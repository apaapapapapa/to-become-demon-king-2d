# Dialogue仕様

## 会話コンテンツ

会話本文は `DialogueDefinition` に保持します。Definitionは安定ID、話者、発言列を持ち、会話の進行位置や表示状態は保持しません。

## 会話進行

`LinearDialogueSequence` が発言列の進行位置を管理します。

- Interactごとに次の有効な発言へ進む。
- 空行は読み飛ばす。
- 最終発言の次のInteractで会話を終了する。
- 終了後に進行位置をリセットし、次回は先頭から開始する。

## 表示

`DialogueLog` は現在表示する1件だけを保持し、履歴を保持しません。`DialogueLogView` は現在の発言を表示し、会話終了時にパネルを非表示にします。

現在のPrototypeでは会話表示中もGameplay入力を継続します。

## Prototype NPC

見習い魔術師の会話コンテンツは `ApprenticeMageDialogue.asset` を使用します。会話完了時のProgression GrantやQuest通知、Interact時の訓練対象SpawnはDialogue内部の責務ではなく、Compositionで接続します。

Feature間の接続方向は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。
