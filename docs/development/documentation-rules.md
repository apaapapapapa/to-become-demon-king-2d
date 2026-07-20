# ドキュメント規約

## 原則

同じ情報を複数の文書やRuntimeソースへ複製しません。情報には必ず1つの所有場所を決め、他の場所から参照したい場合はリンクまたは自動生成結果を使用します。

例外はADRです。ADRは判断当時のContext / Decision / Consequencesを履歴として固定します。現在の仕様や実装状態はADRではなく、下表の所有文書を正とします。

## 情報の所有場所

| 情報 | 所有場所 |
| --- | --- |
| ゲームの目的、体験、ゲームプレイの柱 | `docs/game/vision.md` |
| 現在の実装状況、次の開発優先度、将来候補 | `docs/development/roadmap.md` |
| ドキュメント配置、Source of Truth、命名、リンク規約 | この文書 |
| レイヤー責務、依存方向、Composition、Platform境界 | `docs/design/architecture.md` |
| Unityの基準環境、起動方式、Scene、UI、Resources、Editor、テスト方式 | `docs/design/technical-design.md` |
| Ability / Art / Skill / Evolution等の概念境界、Feature間の接続規則 | `docs/design/feature-boundaries.md` |
| 個別Featureのゲーム上・Runtime上の振る舞い | `docs/specifications/` の各仕様書 |
| 物語固有の設定 | `docs/story/` |
| 世界固有の設定 | `docs/world/` |
| コンテンツ固有の役割、意味、関係 | `docs/database/` の各コンテンツページ |
| 長期的な設計判断の理由と履歴 | `docs/decisions/` |
| AIが読むべき実装・仕様・テストへの索引 | `docs/ai/context-map.md` |
| AIエージェント固有の作業手順 | ルートの `AGENTS.md` |
| リリース運用 | `docs/development/release.md` |

## 配置

```text
docs/
  ai/               AI向け参照索引
  game/             ゲームビジョン
  design/           アーキテクチャ、技術設計、Feature間境界
  specifications/   個別Featureの振る舞い
  story/            ストーリー、キャラクター、会話の物語情報
  world/            場所、勢力、歴史、世界固有ルール
  database/         コンテンツの人間向け索引
  development/      ロードマップ、リリース、ドキュメント運用
  decisions/        ADR
  templates/        コンテンツ文書、ADRのテンプレート
```

`docs/` 直下へ新しいMarkdownを追加せず、上表の所有責務に合う場所へ配置します。

## Source of Truth

| 情報 | Source of Truth |
| --- | --- |
| コンパイルされる処理 | C#コード |
| Scene / Prefab / Input Actions | Unityアセット |
| 静的Definition、Stable Content ID、表示名、バランス値、Asset参照 | ScriptableObject Definition |
| Prototype専用Actor ID等、Definitionを持たないRuntime識別子 | 対応するC#コード |
| プレイ中の可変状態 | Domain / Runtime State |
| 保存形式 | Save DTO |
| ゲームビジョン、物語、世界設定、仕様の意味、設計判断 | 対応するKnowledge Base所有文書 |
| 自動検証内容 | テストコード |

Runtimeソースに存在するID、表示名、実装有無、数値、参照関係をMarkdownへ複製しません。Knowledge BaseにはRuntimeに存在しない意味、制約、世界観、ゲームプレイ上の役割を記載します。

## Runtime-backedコンテンツ

Monster、Art、Skill、EvolutionのうちRuntime実装を持つページは、frontmatterの `runtimeSource` でSource of Truthを指します。

```yaml
---
runtimeSource: Assets/Resources/Settings/Gameplay/FireMagicArt.asset
---
```

VitePress Data Loaderはビルド時にRuntimeソースを直接読み、次を自動解決します。

- Stable Content ID
- Content Type（`database/` 配下の配置から判定）
- `displayName` を持つDefinitionの表示名
- Runtime Sourceの種別（Unity Definition / Runtime Code）
- CharacterDefinitionやEvolutionDefinition等から読み取れるRuntime上の関連

Runtimeソースに `displayName` がないMonster等だけ、Knowledge Base表示用の `title` をfrontmatterへ記載できます。これはRuntimeに同じ表示名フィールドが存在しない場合に限ります。

Runtime-backedページへ `contentId`、`contentType`、`status`、Runtimeと同じ `title` を重複記載しません。Sourceの削除、ID不整合、型不整合をData Loaderが検出した場合はVitePressビルドを失敗させます。

Knowledge Baseだけに存在する概念ページは、`runtimeSource` の代わりに `contentId` と `title` を持てます。例として、複数Runtime Nodeをまとめる進化系列ページが該当します。

## コンテンツ間リンク

`relatedContentIds` はRuntimeから導出できない、Knowledge Base上の意味的な関連だけに使用します。

同じ関連を両ページへ重複記載しません。どちらか一方で1回だけ宣言し、VitePress Data Loaderが双方向の関連として生成します。

CharacterDefinitionの参照やEvolutionの前提Skill / Art / Nodeなど、Runtimeソースから取得できる関連はfrontmatterへ書きません。Data LoaderがRuntimeソースから関連グラフを生成し、Knowledge Base上に対応ページが存在する関係だけを表示します。

Stable Content IDは表示名やAsset名から独立させます。一度Save Dataやコンテンツ間参照へ使用したIDは、表示名変更やAsset移動だけを理由に変更しません。

## 状態と将来計画

プロジェクト全体の「実装済み」「次に実装する」「将来候補」は `roadmap.md` だけに記載します。

仕様書や設計書では、そのFeatureを理解するために必要な現在の振る舞いは記載できますが、「現在の実装範囲」一覧や「今後」一覧を持ちません。必要な場合はロードマップへリンクします。

コンテンツ一覧では手入力のRuntime Statusを持たず、Data Loaderが解決したSource種別を表示します。実装状況の詳細はRuntimeソースとロードマップを確認します。

## リンク

別の所有文書にある説明を引用・要約して再掲せず、その説明へリンクします。

- Feature間の責務境界を仕様書で再説明しない。`feature-boundaries.md` へリンクする。
- Save形式をArt / Skill / Evolution仕様へ再掲しない。`save.md` へリンクする。
- Input Bindingを各Feature仕様へ再掲しない。`input.md` へリンクする。
- 実装状況を設計書や仕様書へ再掲しない。`roadmap.md` へリンクする。
- Source of Truth表をREADMEや各カテゴリページへ再掲しない。この文書へリンクする。

## 命名

- Markdownファイル名は原則 `kebab-case.md`。
- ADRは `ADR-0001-title.md` 形式。
- 1ファイル1責務を基本とする。

## 変更同期

Runtime-backedコンテンツのID・表示名・参照を変更した場合、VitePress側の機械的メタデータ更新は不要です。ビルド時にRuntimeソースから再取得します。

仕様の意味や世界観などKnowledge Baseが所有する情報を変更した場合は、その情報を所有する文書だけを更新します。参照元はリンク先が変わらない限り更新しません。

長期的な設計判断を変更する場合は、現在の所有文書を更新し、判断理由をADRへ記録します。
