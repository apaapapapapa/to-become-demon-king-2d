---
title: 捕食者の本能
contentId: skill.combat.predatory_instinct
contentType: skill
status: Runtime Acquisition Implemented
relatedContentIds:
  - character.player.slime
  - art.magic.fire
  - evolution.slime.lineage
  - evolution.slime.apex_predator
---

# 捕食者の本能

Unity Definition: `Assets/Resources/Settings/Gameplay/PredatoryInstinctSkill.asset`

## 概要

相手を捕食対象として捉えることで、使用者の攻撃性能を高める受動Skillです。実行可能な攻撃そのものは追加しません。

## 効果の意味

対象Abilityを限定しない常時補正として、捕食を軸にした戦闘成長を表現します。

具体的な補正方式とRuntimeルールは [Skill仕様](../../specifications/skill.md) を参照してください。

## 習得

Prototypeでは訓練用ダミーの撃破報酬を通じて取得します。RewardとProgressionの接続方向は [Feature間の責務境界](../../design/feature-boundaries.md#ability--combat--reward--progression) を参照してください。

## 関連リンク

- [プレイヤースライム](../monsters/player-slime.md)
- [スライム進化系列](../evolutions/slime-lineage.md)
- [覇王捕食スライム](../evolutions/apex-predator-slime.md)

## Stable Content IDでの関連

<ContentRelations content-id="skill.combat.predatory_instinct" />
