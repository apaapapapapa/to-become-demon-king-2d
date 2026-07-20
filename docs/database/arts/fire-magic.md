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

- Status: Runtime Implemented
- Art ID: `art.magic.fire`
- Unity Definition: `Assets/Resources/Settings/Gameplay/FireMagicArt.asset`

## 概要

火を生み、遠距離攻撃と範囲制圧へ発展する攻撃魔法Artです。近接中心の初期スライムへ異なる間合いの選択肢を与えます。

## 習得

- 主な習得経路: 見習い魔術師との訓練会話を最後まで進める
- 条件の意味: 魔力を外部へ放出する制御を身につけること
- 関連Evolution: `evolution.slime.arcane` はこのArtの熟練ランクを条件として参照する

## Ability解放

| 解放段階 | Ability ID | 役割 |
| --- | --- | --- |
| 習得時 | `ability.magic.fire_bolt` | 単体への遠距離攻撃 |

火炎弾は `K` / Gamepad Button Northで使用し、命中したExecutionごとに熟練します。上位ランク用の追加Abilityは今後のコンテンツタスクです。Definitionの閾値、威力、クールダウンはUnityをSource of Truthとします。

## Skillとの関係

Art熟練補正を持つSkillの対象になり得ます。Skillは火炎Abilityそのものを所有・実行しません。

## 世界観

生来の器官ではなく、魔力操作を反復して身につける技法です。術式の流派と教習者はストーリー実装時に確定します。

## 関連リンク

- [プレイヤースライム](../monsters/player-slime.md)
- [スライム進化系列](../evolutions/slime-lineage.md)
- [大魔導スライム](../evolutions/archmage-slime.md)

## Stable Content IDでの関連

<ContentRelations content-id="art.magic.fire" />
