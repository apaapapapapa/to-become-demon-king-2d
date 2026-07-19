# ドキュメント規約

## 目的

Knowledge Baseを、実装と乖離した古い資料の置き場にしないためのルールです。

## 配置

```text
docs/
  game/             ゲームビジョン
  design/           アーキテクチャ・技術設計
  specifications/   実装と同期する仕様
  story/            ストーリー・キャラクター・クエスト
  world/            世界設定
  database/         モンスター・進化・アイテム・スキル
  development/      開発運用
  decisions/        ADR
  templates/        テンプレート
```

ルート直下へ新しいMarkdownを無秩序に追加しません。

## ファイル名

原則 `kebab-case.md` を使用します。

ADRだけは次の形式です。

```text
ADR-0001-title.md
```

## 1ファイル1責務

大きな文書へ異なる目的の情報を詰め込みません。

例:

- アーキテクチャと戦闘ルールを同じ文書にしない。
- モンスター一覧と個別モンスター詳細を必要に応じて分ける。
- 世界設定とストーリーイベントを区別する。

## Source of Truth

### Unity側を正にする情報

- ScriptableObjectのRuntime数値
- Input Action AssetのBinding
- Scene / Prefab構成
- Package / Project Settings

### docsを正にする情報

- ゲームビジョン
- 世界観
- 物語の意図
- 仕様の意味と制約
- 設計判断
- 開発方針

同じRuntime数値をMarkdownへコピーして二重管理しません。

## ステータス

未確定情報には状態を明記します。

例:

```text
状態: Draft
状態: Proposed
状態: Accepted
状態: Deprecated
```

「未実装」「Prototypeのみ」「完成版仕様」を区別してください。

## リンク

関連情報は相互リンクします。

例:

```text
Monster
  <-> Evolution
  <-> Skill
  <-> Story Character
  <-> Unity Asset
```

孤立したページを増やさないことを重視します。

## 変更の同期

仕様が変わるコード変更では、同じPRで関連ドキュメントを確認します。

ただし、コメントやMarkdownを変更する必要がない場合に機械的な更新を強制しません。実装と説明の意味が変わるかを判断します。

## 履歴

「なぜ採用したか」が将来重要になる判断はADRへ記録します。

古い設計を本文へ延々と残すのではなく、現在の設計を本文に書き、重要な意思決定履歴をADRへ分離します。
