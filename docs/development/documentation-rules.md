# ドキュメント規約

## 原則

同じ情報を複数の文書やRuntimeソースへ複製しません。情報には必ず1つの所有場所を決め、他の場所から参照したい場合はリンクまたは自動生成結果を使用します。

例外はADRです。ADRは判断当時のContext / Decision / Consequencesを履歴として固定します。現在の仕様や実装状態はADRではなく、下表の所有場所を正とします。

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
| プレイヤーがゲーム内UI・図鑑で読むコンテンツ名、概要、図鑑解説、Icon | 対応するScriptableObject Definition |
| コンテンツのStable Content ID、数値、条件、Runtime参照 | 対応するScriptableObject Definition |
| VitePress上の図鑑一覧・Runtimeコンテンツ表示 | `docs/database/` の自動生成ビュー |
| Runtimeに存在しない図鑑補助概念、開発者向け補足 | `docs/database/` の必要最小限のMarkdown |
| 物語の制作情報、ゲーム内Definitionへまだ公開しない長文設定 | `docs/story/` |
| 世界の制作情報、ゲーム内Definitionへまだ公開しない長文設定 | `docs/world/` |
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
  story/            ストーリー制作情報
  world/            世界設定の制作情報
  database/         Runtime Definitionを表示するWeb図鑑と補助ページ
  development/      ロードマップ、リリース、ドキュメント運用
  decisions/        ADR
  templates/        補助文書、ADRのテンプレート
```

VitePress上では `docs/database/` を「図鑑」と表示します。既存URLの互換性を優先して物理パスは維持します。

`docs/` 直下へ新しいMarkdownを追加せず、上表の所有責務に合う場所へ配置します。

## Source of Truth

| 情報 | Source of Truth |
| --- | --- |
| コンパイルされる処理 | C#コード |
| Scene / Prefab / Input Actions | Unityアセット |
| 静的Definition、Stable Content ID、表示名、図鑑説明、バランス値、Asset参照 | ScriptableObject Definition |
| Prototype専用Actor ID等、Definitionを持たないRuntime識別子 | 対応するC#コード |
| プレイ中の可変状態 | Domain / Runtime State |
| 図鑑の発見済み状態 | 将来のCompendium Runtime State / Save DTO |
| 保存形式 | Save DTO |
| ゲームビジョン、仕様の意味、設計判断 | 対応するKnowledge Base所有文書 |
| ゲーム内公開前の物語・世界設定の制作情報 | `docs/story/` / `docs/world/` |
| 自動検証内容 | テストコード |

Runtimeソースに存在するID、表示名、説明、実装有無、数値、参照関係をMarkdownへ複製しません。

## ゲームコンテンツDefinition

ゲーム内図鑑で参照する静的コンテンツは `IGameContentDefinition` の共通契約へ寄せます。

共通情報は次の意味で使用します。

- `ContentId`: Saveやコンテンツ間参照にも使用するStable Content ID
- `DisplayName`: ゲームUIとWeb図鑑で表示する名称
- `Description`: 一覧、Tooltip等で使用する短い概要
- `EncyclopediaDescription`: ゲーム内図鑑とWeb図鑑で読むプレイヤー向け本文
- `Icon`: UI表示用画像
- `VisibleInEncyclopedia`: 図鑑公開対象かどうか

現在は `CharacterDefinition`、`AbilityDefinition`、`ArtDefinition`、`SkillDefinition`、`EvolutionDefinition` が対象です。

図鑑の「発見済み」「未発見」はプレイヤーごとの可変状態なのでDefinitionへ保存しません。将来のゲーム内図鑑ではStable Content IDのみをCompendium Runtime State / Save DTOへ保持します。

## Runtime-backed図鑑

Runtime実装を持つ個別Markdownページは、frontmatterの `runtimeSource` でSource of Truthを指します。

```yaml
---
runtimeSource: Assets/Resources/Settings/Gameplay/FireMagicArt.asset
---
```

VitePress Data Loaderはビルド時にRuntimeソースを直接読み、次を自動解決します。

- Stable Content ID
- Content Type
- `displayName`
- `description`
- `encyclopediaDescription`
- 図鑑公開可否
- Runtime Sourceの種別
- Character / Ability / Art / Skill / Evolution Definition間の参照

図鑑一覧はMarkdownページの存在を登録条件にしません。`VisibleInEncyclopedia` が有効なRuntime Definitionは、個別Markdownがなくても一覧へ自動表示します。

個別Markdownは、開発者向け参照リンクやRuntimeに存在しない補足が必要な場合だけ作成します。プレイヤー向け説明を本文へ再掲しません。

Runtime-backedページへ `contentId`、`contentType`、`status`、Runtimeと同じ `title`、`description`、`encyclopediaDescription` を重複記載しません。Sourceの削除、ID不整合、型不整合をData Loaderが検出した場合はVitePressビルドを失敗させます。

Runtimeに存在しない概念ページは `runtimeSource` の代わりに `contentId` と `title` を持てます。ゲーム内図鑑へ直接掲載しない補助ページは `visibleInEncyclopedia: false` とします。

## コンテンツ間リンク

`relatedContentIds` はRuntimeから導出できない、Knowledge Base上の意味的な関連だけに使用します。

同じ関連を両ページへ重複記載しません。どちらか一方で1回だけ宣言し、VitePress Data Loaderが双方向の関連として生成します。

CharacterDefinitionの参照、ArtからAbilityへの参照、Evolutionの前提Skill / Art / Nodeなど、Runtimeソースから取得できる関連はfrontmatterへ書きません。Data LoaderがRuntimeソースから関連グラフを生成します。

Stable Content IDは表示名やAsset名から独立させます。一度Save Dataやコンテンツ間参照へ使用したIDは、表示名変更やAsset移動だけを理由に変更しません。

## 状態と将来計画

プロジェクト全体の「実装済み」「次に実装する」「将来候補」は `roadmap.md` だけに記載します。

仕様書や設計書では、そのFeatureを理解するために必要な現在の振る舞いは記載できますが、「現在の実装範囲」一覧や「今後」一覧を持ちません。必要な場合はロードマップへリンクします。

図鑑一覧では手入力のRuntime Statusを持ちません。実装状況の詳細はRuntimeソースとロードマップを確認します。

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

Runtime-backedコンテンツのID、表示名、概要、図鑑解説、参照を変更した場合、VitePress側の機械的メタデータ更新は不要です。ビルド時にRuntimeソースから再取得します。

ゲーム内図鑑UIを実装するときも、同じ `IGameContentDefinition` / `GameContentCatalog` を読み取り、図鑑用の説明データを別管理しません。

仕様の意味や設計意図を変更した場合は、その情報を所有する文書だけを更新します。参照元はリンク先が変わらない限り更新しません。

長期的な設計判断を変更する場合は、現在の所有文書を更新し、判断理由をADRへ記録します。
