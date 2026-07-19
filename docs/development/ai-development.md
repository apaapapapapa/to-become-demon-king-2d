# AI開発ガイド

## 基本方針

AIエージェントは、コードとKnowledge Baseを同じリポジトリ内の開発対象として扱います。ルートの `AGENTS.md` が最優先の共通ガイドです。

## 最初に行うこと

1. `AGENTS.md` を読む。
2. 作業対象に対応するKnowledge Baseを読む。
3. 現在のコードとUnityアセットを確認する。
4. Source of Truthを判断する。

## 変更例

### モンスター追加

```text
docs/database/monsters/
docs/database/evolutions/
Character / Monster Definition
Prefab
Gameplay
Tests
```

Runtime数値をMarkdownへコピーしません。

### Combat変更

```text
Domain Combat
Gameplay Combat
Tests
docs/specifications/combat.md
必要なら docs/design/
```

### アーキテクチャ変更

```text
実装
docs/design/architecture.md
docs/design/technical-design.md
docs/decisions/ADR-xxxx-*.md
```

## Context設計

巨大な1ファイルへ全情報を集めず、作業内容に応じて必要なファイルだけを参照できる構造を維持します。

## 禁止事項

- 実装確認なしで「実装済み」と書く。
- docsだけを根拠にRuntime数値を断定する。
- ScriptableObjectの値をMarkdownへ大量コピーする。
- GameplayロジックをPrototype固有クラスへ安易に追加する。
- Runtime StateをScriptableObject Definitionへ保存する。
- RewardやExperienceをHealthへ直接埋め込む。
- 長期的な設計変更を理由なしで行う。

## 将来の構造化データ

モンスター、アイテム、Skill、Evolutionが大量になった場合は、YAML / JSON等からUnityとVitePress双方へ情報を供給する構成を検討できます。

必要性が確認される前に独自データ生成基盤を導入せず、導入時はADRを追加します。
