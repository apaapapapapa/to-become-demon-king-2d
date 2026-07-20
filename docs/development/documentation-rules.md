# ドキュメント規約

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

`docs/` 直下へ新しいMarkdownを無秩序に追加しません。

## 命名

- 原則 `kebab-case.md`
- ADRは `ADR-0001-title.md`
- 1ファイル1責務

## Source of Truth

Unity側を正にする情報:

- ScriptableObject DefinitionのRuntime数値
- Input Action Binding
- Scene / Prefab
- Package / Project Settings

Domain側を正にする情報:

- プレイ中に変化するRuntime Stateのルール
- Save DTO

Knowledge Baseを正にする情報:

- ゲームビジョン
- 世界観・物語意図
- 仕様の意味と制約
- 設計判断
- 開発方針

## 状態

未確定情報には `Draft`、`Proposed`、`Accepted`、`Deprecated` などの状態を明記します。

## リンク

関連情報は相互リンクします。Monster、Evolution、Skill、Story、Unity Definitionを孤立させません。

## 変更同期

仕様の意味が変わる実装変更では同じPRで関連ドキュメントを確認します。機械的な更新は強制せず、実装と説明が乖離する場合に更新します。

## 履歴

現在の設計は本文へ、重要な意思決定履歴はADRへ記録します。
