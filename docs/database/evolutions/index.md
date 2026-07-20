# 進化テーブル

キャラクターやモンスターの進化関係と、進化条件の意味を管理します。

## 方針

- 進化元・進化先はStable Content IDで関連付ける。
- 条件の意図とゲーム上の意味を記載する。
- Runtimeの閾値や数値はUnity側のDefinitionを正とする。
- 分岐進化では選択条件と排他関係を明記する。
- モンスター個別ページと相互リンクする。

## 登録ページ

<ContentCatalog content-type="evolution" />

Evolution Node Definition、条件評価、排他選択、Save、永続補正、選択UI、Prototype形態表示の基盤は実装済みです。本番用アートと上位Nodeの状態は各ページのStatusで区別します。
