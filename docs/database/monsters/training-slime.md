---
title: 訓練用スライム
contentId: character.training_dummy
contentType: monster
status: Prototype Only
relatedContentIds:
  - character.player.slime
---

# 訓練用スライム

- Status: Prototype Only
- Actor ID: `character.training_dummy`
- Reward ID: `reward.training_dummy`
- Runtime Composition: `PrototypeCombatDummyRespawner`

## 概要

基本攻撃、撃破、報酬、Art熟練度などのシステム確認に使う訓練対象です。完成版の正式モンスターとは区別します。

## ゲームプレイ上の役割

- 出現場所: はじまりの草原の訓練場所
- 戦闘上の特徴: 移動や反撃を行わない
- プレイヤーに要求する行動: Abilityを命中させ、撃破と成長接続を確認する

## 復活と報酬

撃破後、見習い魔術師へのInteractionで再生成します。生存中のInteractionでは同じ個体を全回復します。同一撃破に対する報酬は1回だけ付与します。

## 進化・Skill・Art

Prototype専用のため、進化、Skill、Artは持ちません。

## 実装参照

Runtime数値は `PrototypeCombatDummy` と `TrainingDummyReward.asset` を参照します。正式コンテンツ化するときは専用Character DefinitionとAIを追加します。

## Stable Content IDでの関連

<ContentRelations content-id="character.training_dummy" />
