---
title: スライム進化系列
contentId: evolution.slime.lineage
relatedContentIds:
  - character.player.slime
  - art.magic.fire
  - skill.combat.predatory_instinct
  - evolution.slime.apex_predator
  - evolution.slime.archmage
---

# スライム進化系列

## 概要

プレイヤースライムが、捕食を軸にした肉体成長か、Artを軸にした魔力成長かを選ぶ進化系列です。

## 進化関係

```text
初期形態
  ├ 捕食・直接戦闘を重視する系統
  │   └ 捕食系の上位形態
  └ 魔法Artの習得・熟練を重視する系統
      └ 魔術系の上位形態
```

Evolution Node ID、前提Node、必要Skill・Art、排他グループなどのRuntime条件はUnityの `EvolutionDefinition` を正とし、このページでは重複管理しません。

## 分岐の意味

### 捕食系

魔物としての捕食と直接戦闘を重ね、身体能力へ成長を集中する経路です。

### 魔術系

外部へ魔力を放つArtを習得・熟練し、術者としての器を形成する経路です。

具体的な条件、排他、適用、Save、UIのルールは [Evolution仕様](../../specifications/evolution.md) を参照してください。

## 関連コンテンツ

<ContentRelations />
