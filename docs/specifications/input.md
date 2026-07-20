# 入力仕様

## Gameplay Action Map

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move | WASD / Arrow Keys | Left Stick |
| Attack | J | Button West |
| Interact | E | Button South |
| Dodge | Left Shift | Button East |
| Evolution | V | Right Shoulder |
| Pause | Escape | Start |

## UI Action Map

- Navigate
- Submit
- Cancel
- Pause

## Input Context

`PlayerInputReader` は次を排他的に切り替えます。

- `Gameplay`: Gameplay Mapのみ有効
- `UI`: UI Mapのみ有効
- `Disabled`: すべて無効

GameplayとUIを同時に有効化しません。Pause開始時はUI、Resume時はGameplayへ戻します。

Evolutionメニューを開くとUI Contextへ切り替えて時間を停止し、確定またはキャンセルで直前のGameplay ContextとTime Scaleへ戻します。別のモーダルUIがUI Contextを所有している間はPause画面を重ねません。

将来の会話・メニュー・カットシーンでも、個別コンポーネントの場当たり的なEnable/DisableではなくInput Contextで制御します。

Attack入力は `PlayerAbilityInput` が `ability.basic_melee` の実行要求へ変換します。入力コンポーネントは使用可否やダメージ処理を行いません。
