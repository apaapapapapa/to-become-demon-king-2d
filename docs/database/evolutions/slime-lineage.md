---
title: スライム進化系列
contentId: evolution.slime.lineage
contentType: evolution
status: Prototype Selection and Visual Forms Implemented
relatedContentIds:
  - character.player.slime
  - art.magic.fire
  - skill.combat.predatory_instinct
  - evolution.slime.apex_predator
  - evolution.slime.archmage
---

# スライム進化系列

- Status: Prototype Selection and Visual Forms Implemented
- Evolution Series ID: `evolution.slime.lineage`
- Base Character ID: `character.player.slime`

## 概要

プレイヤースライムが、捕食を軸にした肉体成長か、Artを軸にした魔力成長かを選ぶ最初の排他的な進化系列です。

## 進化関係

```text
character.player.slime
  ├ 捕食経験と戦闘系Skill -> evolution.slime.predator
  │                         -> evolution.slime.apex_predator
  └ 魔法Artの習得・熟練   -> evolution.slime.arcane
                            -> evolution.slime.archmage
```

## 分岐条件

### 捕食系 `evolution.slime.predator`

- 条件の意味: 魔物としての捕食と直接戦闘を重ね、身体能力へ成長を集中する
- 関連Skill: `skill.combat.predatory_instinct`
- 必要な行動: 必要レベルへ到達し、関連Skillを取得する

### 魔術系 `evolution.slime.arcane`

- 条件の意味: 外部へ魔力を放つArtを習得・熟練し、術者としての器を形成する
- 関連Art: `art.magic.fire`
- 必要な行動: 必要レベルへ到達し、関連Artを規定ランクまで熟練する

## 排他・再進化ルール

同じ進化段階の分岐は段階別の排他グループに所属します。Tier 1で片方を選ぶともう片方は取得できず、上位Nodeは選択したTier 1を前提とします。選択済みNodeを取り消すRuntime APIはありません。

## Runtimeデータ

4 NodeのEvolution Definition、条件評価、排他選択、Save復元、Gameplay補正、選択UI、専用形態表示は実装済みです。具体的な閾値、変化量はUnity DefinitionをSource of Truthとします。

捕食系は暖色の棘を持つ専用Sprite Sheetと角状エフェクト、魔術系は青紫の魔術意匠を持つ専用Sprite Sheetと周回する魔力光で区別します。両系統の取得条件はPrototype内の訓練で満たせます。

## 関連リンク

- [プレイヤースライム](../monsters/player-slime.md)
- [火炎魔法](../arts/fire-magic.md)
- [捕食者の本能](../skills/predatory-instinct.md)
- [覇王捕食スライム](./apex-predator-slime.md)
- [大魔導スライム](./archmage-slime.md)

## Stable Content IDでの関連

<ContentRelations content-id="evolution.slime.lineage" />
