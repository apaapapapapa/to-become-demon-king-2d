# 入力仕様

## 目的

キーボードとゲームパッドを論理Actionとして扱い、GameplayとUIの入力を分離します。

## Gameplay Action Map

| Action | Keyboard | Gamepad | 用途 |
| --- | --- | --- | --- |
| Move | WASD / Arrow Keys | Left Stick | 移動 |
| Attack | J | Button West | 攻撃 |
| Interact | E | Button South | 話す・調べる |
| Dodge | Left Shift | Button East | 回避 |
| Pause | Escape | Start | ポーズ |

## UI Action Map

| Action | 用途 |
| --- | --- |
| Navigate | UI選択移動 |
| Submit | 決定 |
| Cancel | 戻る・閉じる |
| Pause | ポーズ解除 |

## Input Context

`PlayerInputReader` は次の3状態を排他的に管理します。

- `Gameplay`: Gameplay Action Mapのみ有効
- `UI`: UI Action Mapのみ有効
- `Disabled`: すべて無効

GameplayとUIを同時に有効化しません。

## Pause

Pause開始時に `UI` へ切り替えます。Resume時に `Gameplay` へ戻します。

## 将来の拡張

- 会話中の入力制御
- メニュー階層
- キーリバインド
- 複数ローカルプレイヤー

これらを追加する場合も、GameplayコードでKeyboardやGamepadを直接ポーリングしません。
