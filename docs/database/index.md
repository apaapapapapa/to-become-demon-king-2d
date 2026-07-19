# ゲームデータベース

このセクションは、ゲーム内コンテンツの人間向け索引を管理します。

- [モンスター](./monsters/)
- [進化テーブル](./evolutions/)
- [アイテム](./items/)
- [スキル](./skills/)

## Source of Truth

当面、Runtimeの数値はUnityのScriptableObjectを正とします。

Knowledge Baseには、次の情報を中心に記載します。

- 安定したID
- 表示名
- 役割・特徴
- 関連する進化・スキル・アイテム
- 世界観上の位置づけ
- 参照するUnityアセット

HP、攻撃力などの全数値をMarkdownへコピーして二重管理しないでください。

コンテンツ数が増え、一覧生成や整合性検証が必要になった段階で、YAML / JSON等の構造化データをSingle Source of Truthにする方式をADRで検討します。
