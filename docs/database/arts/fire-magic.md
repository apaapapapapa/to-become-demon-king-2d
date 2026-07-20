---
title: 火炎魔法
contentId: art.magic.fire
contentType: art
status: Runtime Implemented
relatedContentIds:
  - character.player.slime
  - skill.combat.predatory_instinct
  - evolution.slime.lineage
  - evolution.slime.archmage
---

# 火炎魔法

Unity Definition: `Assets/Resources/Settings/Gameplay/FireMagicArt.asset`

## 概要

火を生み、遠距離攻撃と範囲制圧へ発展する攻撃魔法Artです。近接中心の初期スライムへ異なる間合いの選択肢を与えます。

## 習得

見習い魔術師との訓練を通じ、魔力を外部へ放出する制御を身につけるArtとして位置づけます。

## Ability

| Ability ID | 役割 |
| --- | --- |
| `ability.magic.fire_bolt` | 単体への遠距離攻撃 |

Artの習得、熟練、Ability解放のRuntimeルールは [Art仕様](../../specifications/art.md)、具体的なInput Bindingは [入力仕様](../../specifications/input.md) を参照してください。

## 世界観

生来の器官ではなく、魔力操作を反復して身につける技法です。術式の流派と教習者はストーリー実装時に確定します。

## 関連リンク

- [プレイヤースライム](../monsters/player-slime.md)
- [スライム進化系列](../evolutions/slime-lineage.md)
- [大魔導スライム](../evolutions/archmage-slime.md)

## Stable Content IDでの関連

<ContentRelations content-id="art.magic.fire" />
