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
    details: ゲームビジョン、ゲームループ、設計思想、技術設計を管理します。
  - title: Specifications
    details: 入力、戦闘、Interaction、将来のセーブや会話など、実装と同期する仕様を管理します。
  - title: Story & World
    details: ストーリー、キャラクター、クエスト、場所、勢力などのナラティブ情報を管理します。
  - title: Game Database
    details: モンスター、進化、アイテム、スキルなど、増え続けるゲーム情報の索引を管理します。
  - title: Development
    details: ロードマップ、ドキュメント規約、AI開発ルールを管理します。
  - title: Decisions
    details: 将来の開発者やAIが「なぜそうしたか」を追跡できるようADRを残します。
---

## Knowledge Baseの位置づけ

この `docs/` は、単なる補足資料ではなく、ゲーム開発に必要な知識を検索・参照するためのKnowledge Baseです。

実装コード、Unityアセット、テスト、ドキュメントは同じGitリポジトリで管理し、関連する変更を可能な限り同じPull Requestで更新します。

## 情報の正を分ける

| 情報 | Source of Truth |
| --- | --- |
| ゲームの目的・体験・世界観 | `docs/` |
| 設計意図・アーキテクチャ判断 | `docs/design/` / `docs/decisions/` |
| 実装済み機能の仕様 | コード・Unityアセットと `docs/specifications/` を同期 |
| 静的なコンテンツ定義・バランス値 | UnityのScriptableObject Definition |
| プレイ中に変化する成長状態 | `DemonKing.Domain` のRuntime State |
| 保存形式 | DomainのSave DTO |
| モンスターや進化などの一覧・索引 | 当面は `docs/database/`、規模拡大時に構造化データ化を検討 |

同じ数値をMarkdownとScriptableObjectへ二重に持つことは避けます。ドキュメントには数値そのものより、意味、ルール、制約、安定ID、参照先を記載します。

## AI開発

AIエージェントがこのリポジトリを変更する場合は、最初にルートの `AGENTS.md` を参照します。

特に、コード変更だけを行って関連仕様を放置しないこと、逆にドキュメント上だけで実装済みと断定しないことを基本ルールとします。
