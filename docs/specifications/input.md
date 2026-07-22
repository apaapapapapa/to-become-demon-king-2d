# 入力仕様

## Gameplay Action Map

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move | WASD / Arrow Keys | Left Stick |
| Primary | J | Button West |
| Action1 | K / 1 | Button North / D-pad Up |
| Action2 | 2 | D-pad Right |
| Action3 | 3 | D-pad Down |
| Action4 | 4 | D-pad Left |
| Loadout | Tab | Right Stick Press |
| Interact | E | Button South |
| Dodge | Left Shift | Button East |
| Jump / Ascend | Space | Left Shoulder |
| Flight Toggle | F | Left Stick Press |
| Descend | Left Ctrl | Left Trigger |
| Evolution | V | Right Shoulder |
| Pause | Escape | Start |

Ability系Input Action名は論理Slotと同じ `Primary` / `Action1` / `Action2` / `Action3` / `Action4` を使用します。`PlayerInputReader` は物理Actionを `AbilitySlotPressed(AbilitySlot)` へ変換するだけとし、具体的なAbility ID、Art ID、基本攻撃という意味は入力層へ持たせません。

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

EvolutionメニューまたはLoadoutメニューを開くとUI Contextへ切り替えて時間を停止し、確定またはキャンセルで直前のGameplay ContextとTime Scaleへ戻します。別のモーダルUIがUI Contextを所有している間はPause画面や別モーダルを重ねません。

将来の会話、メニュー、カットシーンも、個別コンポーネントの場当たり的なEnable / DisableではなくInput Contextで制御します。

## Ability入力とLoadout

`PlayerAbilityInput` は `PlayerInputReader` から受け取った論理 `AbilitySlot` をRuntime `AbilityLoadout` でAbility IDへ解決し、`AbilityController` へ実行要求を渡します。

- `Primary` は基本攻撃等の予約枠で、ユーザー編集対象外です。
- `Action1` から `Action4` は `AbilityLoadoutPolicy` が公開する編集可能Slotです。
- Loadout画面はTab / Right Stick Pressで開きます。
- 上下入力で取得済みArt Ability / Skillを選び、左右入力で割当先Action Slotを選びます。
- Artから解放済みのAbilityだけをAction Slotへ割り当てられます。割当可否は `AbilityLoadoutEligibility` が `CharacterDefinition` と `CharacterProgressionState` から判定します。
- 現在のSkillは受動Modifierとして常時有効なため、Loadout画面には状態確認用として表示しますが入力Slotへは割り当てません。
- 同じAbilityを別Action Slotへ割り当てた場合は、`AbilityLoadout` 自体が重複配置を解消して新しいSlotへ移動します。UI固有ルールにはしません。

Runtime初期Loadoutは保存・実行時の `CharacterProgressionState` を基準に構築します。未習得ArtのAbilityを先回りしてSlotへ割り当てません。Save復元も同じ `AbilityLoadoutPolicy` / `AbilityLoadoutEligibility` を使用し、無効・未習得・削除済み・重複した保存エントリは無視します。新たにArt Abilityを取得した場合はLoadout画面から割り当てます。

入力側はAbilityの使用可否やArt進捗を判定しません。Ability実行との責務境界は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。
