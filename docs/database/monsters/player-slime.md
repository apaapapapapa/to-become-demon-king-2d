---
title: プレイヤースライム
contentId: character.player.slime
contentType: monster
status: Prototype Implemented
relatedContentIds:
  - character.training_dummy
  - art.magic.fire
  - skill.combat.predatory_instinct
  - evolution.slime.lineage
  - evolution.slime.apex_predator
  - evolution.slime.archmage
---

# プレイヤースライム

- Status: Prototype Implemented
- Character ID: `character.player.slime`
- Unity Definition: `Assets/Resources/Settings/Gameplay/PlayerCharacter.asset`

## 概要

魔王を目指して成長する主人公の初期形態です。小さく単純な身体から始まり、Art、Skill、Evolutionの選択によって戦い方と形態を分岐させます。

## ゲームプレイ上の役割

- 出現場所: はじまりの草原
- 戦闘上の特徴: 生得Abilityによる近距離戦闘
- プレイヤーに要求する行動: 探索、訓練、成長経路の選択

## 世界観

現時点では最弱に近い魔物ですが、捕食、学習、進化によって魔王へ至る可能性を持ちます。詳細な出自はストーリー設計と同期して確定します。

## 進化

- 進化元: なし
- 進化先: 捕食・魔術のTier 1と、各系統の上位Node（Definition、選択UI、専用外見を実装済み）
- 関連進化ページ: [スライム進化系列](../evolutions/slime-lineage.md)

## Skill

- [捕食者の本能](../skills/predatory-instinct.md): 訓練用ダミーの初回撃破報酬から取得

## Art / Ability

- 生得Ability: `ability.basic_melee`
- 習得可能Art: [火炎魔法](../arts/fire-magic.md)（見習い魔術師の訓練から取得）

## 実装参照

Runtime数値はUnity側のCharacter、Stats、Ability Definitionを参照し、このページへ複製しません。現在の見た目とフィールド生成はPrototype境界です。

## Stable Content IDでの関連

<ContentRelations content-id="character.player.slime" />
