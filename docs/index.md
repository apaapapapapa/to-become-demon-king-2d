---
layout: home
title: To Become Demon King Knowledge Base
titleTemplate: false
hero:
  name: To Become Demon King
  text: Game Knowledge Base
  tagline: 設計・仕様・ストーリー・世界設定・ゲームデータ・開発判断を、実装と同じリポジトリで管理します。
  actions:
    - theme: brand
      text: ゲーム方針を見る
      link: /game/vision
    - theme: alt
      text: アーキテクチャを見る
      link: /design/architecture
features:
  - title: Game & Design
    details: ゲームビジョン、アーキテクチャ、技術設計を管理します。
  - title: Specifications
    details: 入力、戦闘、成長、セーブなど実装と同期する仕様を管理します。
  - title: Story & World
    details: ストーリー、キャラクター、クエスト、場所、勢力を管理します。
  - title: Game Database
    details: モンスター、進化、アイテム、スキルの索引を管理します。
  - title: Development
    details: ロードマップ、ドキュメント規約、AI開発ルールを管理します。
  - title: Decisions
    details: 将来の開発者やAIが判断理由を追跡できるようADRを残します。
---

## Knowledge Baseの位置づけ

`docs/` は補足資料ではなく、ゲーム開発に必要な知識を検索・参照するKnowledge Baseです。実装コード、Unityアセット、テスト、ドキュメントを同じGitリポジトリで管理し、関連変更は可能な限り同じPull Requestで更新します。

## Source of Truth

| 情報 | Source of Truth |
| --- | --- |
| ゲームの目的・体験・世界観 | `docs/` |
| 設計意図・アーキテクチャ判断 | `docs/design/` / `docs/decisions/` |
| 実装済み機能の仕様 | コード・Unityアセットと `docs/specifications/` を同期 |
| 静的なコンテンツ定義・バランス値 | UnityのScriptableObject Definition |
| プレイ中に変化する成長状態 | `DemonKing.Domain` のRuntime State |
| 保存形式 | DomainのSave DTO |
| モンスターや進化などの人間向け索引 | `docs/database/` |

Runtime数値をMarkdownへ大量に複製して二重管理しません。ドキュメントには意味、ルール、制約、安定ID、参照先を記載します。

## AI開発

AIエージェントは最初にルートの `AGENTS.md` を参照します。コード変更だけを行って関連仕様を放置しないこと、逆に実装を確認せずドキュメント上だけで「実装済み」と断定しないことを基本ルールとします。
