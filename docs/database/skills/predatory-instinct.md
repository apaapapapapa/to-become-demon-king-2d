---
title: 捕食者の本能
contentId: skill.combat.predatory_instinct
contentType: skill
status: Definition Registered / Acquisition Not Connected
relatedContentIds:
  - character.player.slime
  - art.magic.fire
  - evolution.slime.lineage
---

# 捕食者の本能

- Status: Definition Registered / Acquisition Not Connected
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

正式な習得経路と条件は未実装です。将来は捕食行動や撃破実績など、取得元側で条件を判定して汎用 `SkillProgressionService.Unlock` へ要求します。

## Art / Evolutionとの関係

- Art: 火炎魔法を含む攻撃Abilityにも、別途制限しない限り作用する
- Evolution: [スライム進化系列](../evolutions/slime-lineage.md)の捕食系分岐と関連づける候補

## 関連リンク

- [プレイヤースライム](../monsters/player-slime.md)
- [Skill仕様](../../specifications/skill.md)

## Stable Content IDでの関連

<ContentRelations content-id="skill.combat.predatory_instinct" />
