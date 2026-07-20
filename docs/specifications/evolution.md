# Evolution仕様

- Status: Node Foundation Implemented / Visual Forms Not Implemented

## 用語と責務

Evolutionは、キャラクターの形態または成長経路を変える不可逆・排他的な選択です。各選択肢を `evolution.*` 形式のStable Content IDを持つEvolution Nodeとして表現します。

EvolutionはAbility実行中に自動判定しません。UI、報酬、物語などの取得元が評価を要求し、条件成立を確認したうえで共通のEvolutionサービスへ実行を要求します。

## EvolutionDefinition

`EvolutionDefinition` は静的なScriptableObjectとして次を保持します。

- `evolution.*` 形式のNode ID
- 表示情報
- 対象となる `character.*` ID
- `evolution-group.*` 形式の排他グループID
- 必要レベル
- 必要な `skill.*` ID
- 必要なArt IDとランク
- 前提となるEvolution Node ID
- 選択後に常時作用する数値補正

同じ段階の分岐は同じ排他グループIDを持ちます。具体的な必要レベル、Artランク、補正値はUnity DefinitionをSource of Truthとします。

## 条件評価

`EvolutionProgressionService.Evaluate` は、Definitionの存在確認後に次をすべて評価します。

1. 対象Character ID
2. レベル
3. 取得済みSkill
4. 習得済みArtの現在ランク
5. 前提Evolution Node
6. 同じ排他グループで選択済みのNode

条件はANDで扱い、不成立理由を `EvolutionRequirementFailure` の一覧として返します。UIは最初の失敗だけでなく、現在不足している全条件を表示できます。

登録時には前提Node Definitionの欠落と循環参照を拒否し、到達不能な成長経路を起動前に検出します。

Save復元時に同じ排他グループの既知Nodeが複数選択されている場合は不正状態として拒否し、補正を重複適用しません。

## 適用と不可逆性

`Evolve` は実行時に条件を再評価し、すべて成立した場合だけ `CharacterProgressionState.UnlockedEvolutionNodeIds` へNode IDを追加します。

- 同じNodeの再取得は拒否する。
- 同じ排他グループの別Nodeは拒否する。
- 選択済みNodeを削除・巻き戻すRuntime APIは設けない。
- DefinitionやSave DTOを実行時状態として書き換えない。
- 適用成功時だけ `EvolutionApplied` を通知する。

## 成長経路の反映

選択済みNodeの常時補正は `EvolutionProgressionController` が汎用Modifier Sourceとして公開します。Ability、Combat、Artは補正の取得元がEvolutionであることを知りません。

```text
UnlockedEvolutionNodeIds + EvolutionDefinition[]
  ↓
EvolutionProgressionService
  ↓ NumericModifier
EvolutionProgressionController
  ├ IOutgoingDamageModifierSource
  ├ IAbilityCooldownModifierSource
  └ IArtMasteryModifierSource
```

現在は与ダメージ、Abilityクールダウン、Art熟練ポイントへ作用できます。専用Sprite、Prefab、Animator、Character Definition差し替えなど、視覚的な形態変更は未実装です。Presentationは将来 `EvolutionApplied` を購読して演出と見た目を変更します。

## Save方針

選択済みNodeは既存の `PlayerSaveData.unlockedEvolutionNodeIds` にキャラクター単位で保存します。補正後の値、条件評価結果、排他グループID、Definition参照は保存せず、ロード後にDefinitionから再計算します。

Evolution Node ID欄はSave Version 1から存在するため、今回の実装でSave Versionは変更しません。未知の保存IDは状態として維持しますが、条件や補正には利用しません。

## 現在のコンテンツ

- `evolution.slime.predator`: 戦闘系Skillを条件とする捕食系分岐
- `evolution.slime.arcane`: 魔法Artの熟練を条件とする魔術系分岐
- 排他グループ: `evolution-group.slime.tier1`

両NodeのDefinitionと条件・補正は登録済みです。正式な習得UI、進化演出、専用外見は未実装です。魔術系分岐が参照する火炎魔法ArtもRuntime未登録のため、現時点のPrototypeでは条件を満たせません。

## 関連仕様

- [成長仕様](./progression.md)
- [Skill仕様](./skill.md)
- [Art仕様](./art.md)
- [セーブ仕様](./save.md)
- [スライム進化系列](../database/evolutions/slime-lineage.md)
