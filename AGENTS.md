# AGENTS.md

## 目的

このファイルは、AIエージェントが `to-become-demon-king-2d` を変更するときの読み順と作業手順だけを定義します。

設計ルールや仕様をここへ複製しません。必ず所有文書を参照してください。

## Reading Strategy

1. この `AGENTS.md` を読む。
2. `docs/ai/context-map.md` から変更対象Featureを特定する。
3. Context Mapの `Spec` と `Code` を先に読む。
4. UIやFeature間配線を変更するときだけ `Integration` と関連Featureを追加で読む。
5. 文書の配置やSource of Truthを判断するときは `docs/development/documentation-rules.md` を読む。
6. レイヤーや依存方向を変更するときは `docs/design/architecture.md` を読む。
7. Feature間の責務や接続方向を変更するときは `docs/design/feature-boundaries.md` を読む。
8. Unity固有の実装方式を変更するときは `docs/design/technical-design.md` を読む。

リポジトリ全体を事前に読むことを前提にせず、必要な範囲だけ段階的に広げます。

## Change Workflow

1. Context Mapから関連仕様・主要コード・テストを特定する。
2. 現在のコードとUnityアセットを確認する。
3. 所有文書に定義された責務境界を維持して実装する。
4. Runtime変更では関連するDomain / EditMode / PlayModeテストを確認・追加する。
5. 仕様や設計意図が変わった場合は、その情報を所有する文書だけを同じPRで更新する。
6. 長期的な設計判断を変更する場合はADRを追加する。
7. 実装状況を記述する場合は現在のコードとUnityアセットを確認し、`docs/development/roadmap.md` を更新する。

## Task Routing

| 作業 | 最初に確認する場所 |
| --- | --- |
| Feature変更 | `docs/ai/context-map.md` の該当Feature |
| ゲームの方向性 | `docs/game/vision.md` |
| 現在の実装状況・優先度 | `docs/development/roadmap.md` |
| アーキテクチャ | `docs/design/architecture.md` |
| Feature間境界 | `docs/design/feature-boundaries.md` |
| Unity実装方式・テスト方式 | `docs/design/technical-design.md` |
| 文書配置・Source of Truth・Stable Content ID | `docs/development/documentation-rules.md` |
| 長期的な判断理由 | `docs/decisions/` |
| ストーリー / 世界設定 | `docs/story/` / `docs/world/` |
| コンテンツ固有情報 | `docs/database/` |
| リリース | `docs/development/release.md` |

Context Mapは索引であり、仕様や設計のSource of Truthではありません。
