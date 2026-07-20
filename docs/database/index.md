# ゲームデータベース

ゲーム内コンテンツの人間向け索引です。

- [モンスター](./monsters/)
- [進化テーブル](./evolutions/)
- [アイテム](./items/)
- [アーツ](./arts/)
- [スキル](./skills/)

## Source of Truth

Runtimeの静的数値はUnityのScriptableObject Definitionを正とします。Knowledge Baseには安定ID、表示名、役割、特徴、関連コンテンツ、世界観上の位置づけ、参照するDefinitionを記載します。

HPや攻撃力などの全数値をMarkdownへコピーして二重管理しません。

一覧とページ間参照はMarkdown frontmatterから生成します。コンテンツ量がさらに増え、Unity Definitionとの双方向生成が必要になった段階で、YAML / JSON等の構造化データをSingle Source of Truthにする方式をADRで検討します。

## Stable Content ID一覧

次の一覧は各コンテンツページのfrontmatterからVitePress Data Loaderで自動生成します。ビルド時にID重複、存在しない関連ID、片方向だけの関連を検出します。

<ContentCatalog />
