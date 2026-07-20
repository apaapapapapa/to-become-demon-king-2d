# ADR-0001: Unity実装とKnowledge Baseを同一リポジトリで管理する

- Status: Accepted
- Date: 2026-07-20

## Context

今後、設計書、仕様書、ストーリー、世界設定、モンスター、進化、アイテム、Art、Skillなどの情報が増加します。同時にAIエージェントを利用してUnity実装とドキュメントを継続的に更新します。

ゲーム本体とドキュメントを別リポジトリにすると、仕様変更と実装変更のPRやCommitが分断され、AIが作業時に両方のContextを取得しにくくなります。

## Decision

UnityプロジェクトとKnowledge Baseを同じGitリポジトリで管理します。

```text
repository/
  Assets/        Unity Runtime / Content
  Tests/         Unity Tests
  docs/          VitePress Knowledge Base
  AGENTS.md      AI Development Guide
```

`docs/` はVitePressのサイトルートとして扱います。

## Positive

- 実装と仕様を同じPRで更新できる。
- AIがコードとKnowledge Baseを横断しやすい。
- Commit単位で仕様と実装の対応を追跡できる。
- ADRを実装履歴と同じGit履歴に残せる。

## Negative

- リポジトリ内のファイル数が増える。
- Knowledge Baseが巨大化すると検索対象が広くなる。
- Runtime値をMarkdownへ重複記載すると不整合が起きる。

## Mitigation

- `docs/` を目的別ディレクトリへ分割する。
- `AGENTS.md` でAIの参照先を明示する。
- Runtime数値はScriptableObject Definitionを正とする。
- プレイ中の状態はDomain Runtime Stateを正とする。
- 大量データの構造化は必要になった段階で再検討する。

## Reconsider When

- ドキュメントだけ異なるアクセス権限が必要になった。
- Knowledge Baseを複数ゲームで共有する必要が生じた。
- 独立した編集チームがUnityリポジトリへアクセスすべきでない状態になった。
- リポジトリ規模が開発ツールやCIへ明確な問題を発生させた。
