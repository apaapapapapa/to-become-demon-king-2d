# 移動仕様

## 座標

フィールド物理は3D Physicsを使用し、座標軸は次の通りです。

```text
X / Y : フィールド平面
Z     : Elevation（高さ）
```

平面移動と高さ移動は独立して扱います。

- `CharacterPlanarMotor`: X/Y平面の通常移動
- `CharacterDodge`: X/Y平面の回避移動
- `CharacterElevationMotor`: Z方向のJump / Fall / Flight

## 高さ状態

`CharacterElevationMotor` は次の3状態を持ちます。

- `Grounded`: 地面または上向きの支持面に接地している
- `Airborne`: Jump後または飛行解除後に落下している
- `Flying`: プレイヤーが上昇・下降を直接操作する

## Jump / Fall

地上または支持面に接地中のみJumpを開始できます。

Jump開始時にZ方向へ初速を与え、以降はElevation専用の落下加速度を適用します。Unity標準重力はY軸方向のため使用しません。

平地ではElevation `0` を最低高度とし、そこへ到達すると `Grounded` へ戻ります。3D Collider上面に着地した場合は上向きの接触面を支持面として扱います。

## Flight

Flight Toggleで飛行状態を切り替えます。

飛行中は次の操作を行えます。

- Jump / Ascend入力を保持: 上昇
- Descend入力を保持: 下降
- Move: 現在高度を維持したままX/Y平面移動

飛行解除時に地面より上にいる場合は `Airborne` へ移行して落下します。

現在の最大飛行高度は `CharacterElevationMotor` の設定値を使用します。

## 有限高さ障害物

建物等はZ方向へ有限の厚みを持つ3D Colliderとして表現します。

ActorのCollider下端が障害物上端を超え、3D Collider同士が重ならない高度では通常のPhysics判定だけで上空通過できます。高さ専用の例外判定は追加しません。

外周等、飛行でも越えさせない領域は十分大きいZ高さを持つHard Boundaryとして扱います。

## 表示

Physics上のElevationは `CharacterElevationPresenter` が2D表示上のYオフセットへ変換します。

Sorting基準はActor RootのフィールドY座標を使用し、Visualの高さオフセットで前後関係を変えません。

## 入力

具体的なキー・Gamepad割り当ては [入力仕様](./input.md) を参照してください。

3D Physicsの軸規約と実装境界は [技術設計](../design/technical-design.md) を参照してください。
