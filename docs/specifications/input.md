# 入力仕様

## Gameplay Action Map

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move | WASD / Arrow Keys | Left Stick |
| Attack | J | Button West |
| Art | K | Button North |
| Interact | E | Button South |
| Dodge | Left Shift | Button East |
| Jump / Ascend | Space | Left Shoulder |
| Flight Toggle | F | Left Stick Press |
| Descend | Left Ctrl | Left Trigger |
| Evolution | V | Right Shoulder |
| Pause | Escape | Start |

Jump入力は地上ではJump開始に使用します。飛行中は同じ入力を保持して上昇します。Descend入力は飛行中の下降に使用します。

Flight Toggleで飛行状態を切り替えます。飛行解除時に地面より上にいる場合は落下状態へ移行します。

移動状態と高さ方向の具体的な振る舞いは [移動仕様](./movement.md) を参照してください。

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

将来の会話、メニュー、カットシーンも、個別コンポーネントの場当たり的なEnable / DisableではなくInput Contextで制御します。

## Ability入力

Attack入力は `PlayerAbilityInput` が `ability.basic_melee` の実行要求へ変換します。Art入力は現在 `ability.magic.fire_bolt` の実行要求へ変換します。

入力側はAbilityの使用可否やArt進捗を判定しません。Ability実行との責務境界は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。
