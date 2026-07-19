# ゲームデータベース

ゲーム内コンテンツの人間向け索引です。

- [モンスター](./monsters/)
- [進化テーブル](./evolutions/)
- [アイテム](./items/)
- [スキル](./skills/)

## Source of Truth

Runtimeの静的数値はUnityのScriptableObject Definitionを正とします。Knowledge Baseには安定ID、表示名、役割、特徴、関連コンテンツ、世界観上の位置づけ、参照するDefinitionを記載します。

HPや攻撃力などの全数値をMarkdownへコピーして二重管理しません。

コンテンツ量が増え、一覧生成や整合性検証が必要になった段階で、YAML / JSON等の構造化データをSingle Source of Truthにする方式をADRで検討します。
