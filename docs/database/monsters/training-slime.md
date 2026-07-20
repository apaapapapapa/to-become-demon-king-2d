---
title: 訓練用スライム
contentId: character.training_dummy
contentType: monster
status: Prototype Only
relatedContentIds:
  - character.player.slime
---

# 訓練用スライム

## 概要

基本攻撃、撃破、報酬、Quest等のシステム確認に使うPrototype専用の訓練対象です。完成版の正式モンスターとは区別します。

## ゲームプレイ上の役割

- 出現場所: はじまりの草原の訓練場所
- 戦闘上の特徴: 移動や反撃を行わない
- プレイヤーに要求する行動: Abilityを命中させ、撃破する

再生成・復元の振る舞いは [Spawning仕様](../../specifications/spawning.md)、撃破と報酬の境界は [Feature間の責務境界](../../design/feature-boundaries.md#ability--combat--reward--progression) を参照してください。

## Stable Content IDでの関連

<ContentRelations content-id="character.training_dummy" />
