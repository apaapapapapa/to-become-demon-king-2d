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

- Status: Runtime Acquisition Implemented
- Skill ID: `skill.combat.predatory_instinct`
- Unity Definition: `Assets/Resources/Settings/Gameplay/PredatoryInstinctSkill.asset`

## 概要

相手を捕食対象として捉えることで、使用者が与えるダメージを高める受動Skillです。実行可能な攻撃を追加せず、生得AbilityとArt由来Abilityの双方へ作用します。

## 効果の意味

- 対象: 使用者がAbilityで与えるダメージ
- 方式: 対象Abilityを限定しない常時補正
- 制約: Skill自体は入力、Ability付与、攻撃発生を行わない

具体的な補正値はUnity DefinitionをSource of Truthとします。

## 習得

Prototypeでは訓練用ダミーの初回撃破報酬から取得します。報酬側の `ProgressionGrantDefinition` が汎用 `ProgressionAcquisitionService` へ要求し、ダミーやCombat処理はSkill状態を直接変更しません。

## Art / Evolutionとの関係

- Art: 火炎魔法を含む攻撃Abilityにも、別途制限しない限り作用する
- Evolution: [スライム進化系列](../evolutions/slime-lineage.md)の捕食系分岐と関連づける候補

## 関連リンク

- [プレイヤースライム](../monsters/player-slime.md)
- [Skill仕様](../../specifications/skill.md)
- [覇王捕食スライム](../evolutions/apex-predator-slime.md)

## Stable Content IDでの関連

<ContentRelations content-id="skill.combat.predatory_instinct" />
