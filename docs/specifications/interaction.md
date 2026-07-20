# インタラクション仕様

## 対象探索と実行

```text
Interact入力
  ↓
PlayerInteractor
  ↓
IInteractable
```

`PlayerInteractor` は近傍のInteract可能対象を探索し、最も近い対象へ `Interact` を委譲します。

- 対象固有の会話、Combat、Quest、報酬処理を持たない。
- `CanInteract` が偽の対象は選択しない。
- 同一のInteractableを複数Collider経由で重複評価しない。

NPC会話の進行は [Dialogue仕様](./dialogue.md)、訓練対象の再生成は [Spawning仕様](./spawning.md) を参照してください。

Interactionから他Featureへ接続する場合の依存方向は [Feature間の責務境界](../design/feature-boundaries.md#interaction--dialogue) を参照してください。
