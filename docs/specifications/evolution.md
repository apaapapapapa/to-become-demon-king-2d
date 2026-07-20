# Evolution仕様

Evolutionは形態や成長経路を変える不可逆または排他的な選択です。Ability / Art / Skillとの概念境界は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。

## EvolutionDefinition

`EvolutionDefinition` は静的なScriptableObjectとして次を保持します。

- `evolution.*` 形式のNode ID
- 表示情報
- 対象となる `character.*` ID
- `evolution-group.*` 形式の排他グループID
- レベル、Skill、Artランク、前提Node等の条件
- 選択後に作用する数値補正
- 形態表示用プロファイル

具体的な条件値、補正値、表示設定はUnity Definitionを正とします。

## 条件評価

`EvolutionProgressionService.Evaluate` は、Definitionの存在確認後に次を評価します。

1. 対象Character ID
2. レベル
3. 取得済みSkill
4. 習得済みArtの現在ランク
5. 前提Evolution Node
6. 同じ排他グループで選択済みのNode

条件はANDで扱い、不成立理由を `EvolutionRequirementFailure` の一覧として返します。

登録時には前提Node Definitionの欠落と循環参照を拒否します。Save復元時に同じ排他グループの既知Nodeが複数選択されている場合も不正状態として扱います。

## 適用と不可逆性

`Evolve` は実行時に条件を再評価し、すべて成立した場合だけNode IDをRuntime Stateへ追加します。

- 同じNodeの再取得は拒否する。
- 同じ排他グループの別Nodeは拒否する。
- 選択済みNodeを削除・巻き戻すRuntime APIは設けない。
- Definitionを実行時状態として書き換えない。
- 適用成功時だけ `EvolutionApplied` を通知する。

## Gameplay補正

選択済みNodeの常時補正は `EvolutionProgressionController` が汎用Modifier Sourceとして公開します。補正利用側はEvolution取得状態を直接参照しません。

共通接続方向は [Feature間の責務境界](../design/feature-boundaries.md#受動modifier) を参照してください。

## 形態表示

`EvolutionAppearanceProfile` は形態表示用の静的情報を保持します。Presentationは `EvolutionApplied` または復元済みNodeから外見を反映し、条件評価や進捗状態を変更しません。

個別Evolutionの意味と関連コンテンツは [進化テーブル](../database/evolutions/) を参照してください。

## 選択UI

- 全Nodeと現在の評価状態を表示する。
- 不足条件を一覧表示する。
- UI Context中はGameplay入力を受け付けない。
- Submit時に条件を再評価し、成功時だけ適用する。
- 条件不足時は状態を変更しない。
- Cancel時は適用しない。

具体的なInput Bindingは [入力仕様](./input.md) を参照してください。

## Save

Evolution選択状態の保存形式は [セーブ仕様](./save.md) を参照してください。

現在の実装状況と今後の対象は [ロードマップ](../development/roadmap.md) を参照してください。
