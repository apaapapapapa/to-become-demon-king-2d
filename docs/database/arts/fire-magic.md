---
title: 火炎魔法
contentId: art.magic.fire
contentType: art
status: Planned / Runtime Not Registered
relatedContentIds:
  - character.player.slime
  - skill.combat.predatory_instinct
  - evolution.slime.lineage
---

# 火炎魔法

- Status: Planned / Runtime Not Registered
- Art ID: `art.magic.fire`
- Unity Definition: 未作成

## 概要

火を生み、遠距離攻撃と範囲制圧へ発展する攻撃魔法Artです。近接中心の初期スライムへ異なる間合いの選択肢を与えます。

## 習得

- 主な習得経路: 魔術訓練または関連アイテム（未実装）
- 条件の意味: 魔力を外部へ放出する制御を身につけること
- 関連Evolution: `evolution.slime.arcane` はこのArtの熟練を条件候補とする

## Ability解放

| 解放段階 | Ability ID | 役割 |
| --- | --- | --- |
| 習得時 | `ability.magic.fire_bolt` | 単体への遠距離攻撃 |
| 熟練後 | `ability.magic.flame_burst` | 範囲を制圧する派生攻撃 |

Ability Definition、熟練ランク閾値、威力、クールダウン、コストは未作成です。実装時はUnity DefinitionをSource of Truthとします。

## Skillとの関係

Art熟練補正を持つSkillの対象になり得ます。Skillは火炎Abilityそのものを所有・実行しません。

## 世界観

生来の器官ではなく、魔力操作を反復して身につける技法です。術式の流派と教習者はストーリー実装時に確定します。

## 関連リンク

- [プレイヤースライム](../monsters/player-slime.md)
- [スライム進化系列](../evolutions/slime-lineage.md)

## Stable Content IDでの関連

<ContentRelations content-id="art.magic.fire" />
