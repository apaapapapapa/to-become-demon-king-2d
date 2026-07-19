# AI開発ガイド

## 基本方針

AIエージェントは、コードだけでなくKnowledge Baseも同じリポジトリ内の開発対象として扱います。

ルートの `AGENTS.md` が最優先の共通ガイドです。

## AIが最初に行うこと

1. `AGENTS.md` を読む。
2. 作業対象に対応するKnowledge Baseを読む。
3. 現在のコードとUnityアセットを確認する。
4. 実装と文書のどちらがSource of Truthかを判断する。

## 変更の例

### 新しいモンスターを追加する

確認・変更候補:

```text
docs/database/monsters/
docs/database/evolutions/
Unity ScriptableObject
Prefab
Gameplay実装
Tests
```

Runtime数値をMarkdownへコピーする必要はありません。

### Combatルールを変更する

```text
Assets/Scripts/Gameplay/Combat/
Assets/Tests/
docs/specifications/combat.md
必要なら docs/design/
```

### アーキテクチャを変更する

```text
実装
docs/design/architecture.md
docs/design/technical-design.md
docs/decisions/ADR-xxxx-*.md
```

## AIへのContext設計

巨大な1ファイルへ全情報を集めず、作業内容に応じて必要なファイルだけを参照できる構造を維持します。

そのため、モンスター、進化、ストーリーなどはカテゴリ・個別ページへ分割します。

## 禁止事項

- 実装を確認せず「実装済み」とドキュメントへ書く。
- docsだけを根拠にRuntime値を断定する。
- ScriptableObjectの値をMarkdownへ大量コピーする。
- 新しいGameplayロジックをPrototype固有クラスへ安易に追加する。
- 長期的な設計変更を理由なしで行う。
- 関係のないドキュメントを一括で書き換える。

## 将来の構造化データ

モンスター、アイテム、スキル、進化などが大量になった場合は、YAML / JSON等の構造化データからUnityとVitePress双方へ情報を供給する構成を検討できます。

ただし、現時点ではScriptableObjectをRuntime値の正とし、必要性が確認される前にデータ生成基盤を導入しません。

この変更を行う場合はADRを追加してください。
