---
title: スライム進化系列
contentId: evolution.slime.lineage
contentType: evolution
status: Runtime Nodes Registered / Visual Forms Not Implemented
relatedContentIds:
  - character.player.slime
  - art.magic.fire
  - skill.combat.predatory_instinct
---

# スライム進化系列

- Status: Runtime Nodes Registered / Visual Forms Not Implemented
- Evolution Series ID: `evolution.slime.lineage`
- Base Character ID: `character.player.slime`

## 概要

プレイヤースライムが、捕食を軸にした肉体成長か、Artを軸にした魔力成長かを選ぶ最初の排他的な進化系列です。

## 進化関係

```text
character.player.slime
  ├ 捕食経験と戦闘系Skill -> evolution.slime.predator
  └ 魔法Artの習得・熟練   -> evolution.slime.arcane
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

同じ進化段階の2分岐は `evolution-group.slime.tier1` に所属し、片方を選択するともう片方は取得できません。選択済みNodeを取り消すRuntime APIはありません。再進化や上位形態は未設計です。

## Runtimeデータ

両分岐のEvolution Definition、条件評価、排他選択、Save復元、Gameplay補正は実装済みです。具体的な閾値や変化量はUnity DefinitionをSource of Truthとします。

専用Sprite、Prefab、Animator、進化演出、選択UIは未実装です。魔術系分岐が参照する火炎魔法ArtもRuntime未登録のため、現在のPrototypeでは魔術系条件を満たせません。

## 関連リンク

- [プレイヤースライム](../monsters/player-slime.md)
- [火炎魔法](../arts/fire-magic.md)
- [捕食者の本能](../skills/predatory-instinct.md)

## Stable Content IDでの関連

<ContentRelations content-id="evolution.slime.lineage" />
