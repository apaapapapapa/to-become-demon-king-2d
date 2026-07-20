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

## 概要

プレイヤースライムが、捕食を軸にした肉体成長か、Artを軸にした魔力成長かを選ぶ進化系列です。

## 進化関係

```text
character.player.slime
  ├ 捕食経験と戦闘系Skill -> evolution.slime.predator
  │                         -> evolution.slime.apex_predator
  └ 魔法Artの習得・熟練   -> evolution.slime.arcane
                            -> evolution.slime.archmage
```

## 分岐の意味

### 捕食系

魔物としての捕食と直接戦闘を重ね、身体能力へ成長を集中する経路です。[捕食者の本能](../skills/predatory-instinct.md) と関連します。

### 魔術系

外部へ魔力を放つArtを習得・熟練し、術者としての器を形成する経路です。[火炎魔法](../arts/fire-magic.md) と関連します。

具体的な条件、排他、適用、Save、UIのルールは [Evolution仕様](../../specifications/evolution.md) を参照してください。

## 関連リンク

- [プレイヤースライム](../monsters/player-slime.md)
- [火炎魔法](../arts/fire-magic.md)
- [捕食者の本能](../skills/predatory-instinct.md)
- [覇王捕食スライム](./apex-predator-slime.md)
- [大魔導スライム](./archmage-slime.md)

## Stable Content IDでの関連

<ContentRelations content-id="evolution.slime.lineage" />
