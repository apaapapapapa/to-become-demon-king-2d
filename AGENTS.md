# AGENTS.md

## 目的

このファイルは、CodexなどのAIエージェントが `to-become-demon-king-2d` を変更するときの共通ルールを定義します。

このリポジトリでは、Unity実装とKnowledge Baseを同じプロダクトとして扱います。コードだけ、またはドキュメントだけを無関係に変更せず、変更の意味に応じて両方を同期してください。

## 最初に読む場所

作業内容に応じて、実装前に次を確認してください。

| 作業 | 主な参照先 |
| --- | --- |
| ゲームの方向性を変える | `docs/game/vision.md` |
| アーキテクチャを変える | `docs/design/architecture.md` |
| 実装方式・技術規約を変える | `docs/design/technical-design.md` |
| 入力を変える | `docs/specifications/input.md` |
| 戦闘を変える | `docs/specifications/combat.md` |
| Interactionを変える | `docs/specifications/interaction.md` |
| ストーリーを追加・変更する | `docs/story/` |
| 世界設定を追加・変更する | `docs/world/` |
| モンスターを追加する | `docs/database/monsters/` |
| 進化関係を追加する | `docs/database/evolutions/` |
| アイテムを追加する | `docs/database/items/` |
| スキルを追加する | `docs/database/skills/` |
| 長期的な設計判断を行う | `docs/decisions/` |
| 開発優先順位を変える | `docs/development/roadmap.md` |

## Source of Truth

情報の種類ごとに正を分けます。

### コード・Unityアセットが正

次は実装側を正とします。

- 実際にコンパイルされるC#コード
- Scene / Prefab / Input Actions
- ScriptableObjectに保存するRuntime値
- Package / Project Settings
- 自動テスト

Markdownへ同じ数値を複製して二重管理しないでください。

### docsが正

次はKnowledge Baseを正とします。

- ゲームビジョン
- 世界観と物語の意図
- 仕様の意味と制約
- アーキテクチャ上の責務境界
- 採用・不採用の設計判断
- ロードマップ
- モンスターや進化などの人間向け索引

### 実装とdocsを同期する情報

次の変更では、コードと関連ドキュメントの両方を確認してください。

- 入力Action / Binding
- Combatルール
- Interactionルール
- Save仕様
- Scene遷移
- UI状態遷移
- Platform依存境界
- モンスターやスキルのRuntimeデータ構造

## 変更時の基本手順

1. 関連するKnowledge Base文書を読む。
2. 現在のコードとUnityアセットを確認する。
3. 既存の責務境界を壊さず実装する。
4. 仕様や設計意図が変わった場合は同じPRでdocsを更新する。
5. 長期的な設計判断ならADRを追加する。
6. Runtimeコード変更では関連するEditMode / PlayModeテストを確認・追加する。
7. 実装していない機能をドキュメント上で「実装済み」と書かない。

## ドキュメント配置ルール

```text
docs/
  game/             ゲームビジョン・ゲームループ
  design/           アーキテクチャ・技術設計
  specifications/   実装と同期する機能仕様
  story/            ストーリー・キャラクター・クエスト
  world/            場所・勢力・世界設定
  database/         モンスター・進化・アイテム・スキルの索引
  development/      ロードマップ・運用・AI開発ルール
  decisions/        ADR
  templates/        新規ドキュメントのテンプレート
```

新しいMarkdownをルート直下へ無秩序に追加しないでください。

## 命名

- Markdownファイル名は原則 `kebab-case.md`。
- 固有IDが必要なデータは、表示名ではなく安定した英数字IDを用意する。
- ADRは `ADR-0001-title.md` 形式。
- 1ファイル1責務を基本とする。

## モンスター・進化・アイテム・スキル

コンテンツ数が少ない間は `docs/database/` を人間向けKnowledge Baseとして使用します。

Runtime値はScriptableObjectを正とし、MarkdownへHPや攻撃力などの全数値をコピーしないでください。必要な場合は「役割」「特徴」「進化条件の意味」「参照するScriptableObject」などを記載します。

データ数が増え、一覧生成や整合性検証が必要になった段階で `game-data/` のYAML / JSONなどをSingle Source of Truthとし、UnityとVitePress双方へ生成する方式をADRで検討します。先行して独自データ生成基盤を作らないでください。

## コメント

C#コメントは日本語で記述します。

コメントは「コードを読めば分かる処理手順」ではなく、次を優先します。

- なぜこの責務がここにあるか
- なぜ別の実装を採用しなかったか
- Prototype専用か恒久機能か
- 将来削除・移行する境界か
- Unity固有の制約や注意点

古い設計を説明するコメントを残さないでください。

## Prototype境界

`Field/Prototype`、`SlimeController`、`RuntimeShapeFactory` などには移行境界が残っています。

新しい恒久GameplayロジックをPrototype固有クラスへ追加し続けないでください。恒久ロジックは `Core` / `Gameplay` / `Presentation` へ置き、`Field/Prototype` はCompositionと試作用コンテンツに限定します。

## Platform対応

Steamや将来のコンソールSDKをGameplayコードから直接呼び出さないでください。

セーブ、実績、クラウド、ユーザー識別などのPlatform依存機能を追加するときは、まず `docs/design/architecture.md` と関連ADRを更新し、境界を定義します。

## PRの単位

可能な限り、1つの機能変更を次の単位で同じPRへ含めます。

```text
Runtime実装
+ Unityアセット／設定
+ テスト
+ 関連仕様
+ 必要ならADR
```

ドキュメント更新だけのPRも許可しますが、その場合は実装変更がないことを明確にします。

## VitePress

Knowledge Baseは `docs/` をVitePressルートとして構築します。

```bash
npm install
npm run docs:dev
npm run docs:build
npm run docs:preview
```

VitePressのナビゲーションや新しい主要カテゴリを追加した場合は `docs/.vitepress/config.mts` も更新してください。
