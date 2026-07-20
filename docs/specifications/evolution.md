# Evolution仕様

- Status: Prototype Selection UI and Visual Forms Implemented

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
- 形態表示用の色、表示倍率、演出種別、専用2フレームSprite Sheet

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

現在は与ダメージ、Abilityクールダウン、Art熟練ポイントへ作用できます。

`EvolutionAppearanceProfile` は形態の色、表示倍率、演出種別、専用Sprite Sheetを保持します。`PrototypeSlimeEvolutionPresenter` が `EvolutionApplied` を購読し、`PrototypeSlimeSpriteAnimator` が2フレームを分割して待機・移動表示へ使用します。捕食系の角状エフェクト、魔術系の追従光も重ねます。ロード済みSaveでは、初期化時に最後に取得した既知Nodeの外見を復元します。

外見プロファイルは静的Definitionであり、現在の形態IDや生成済みSpriteをSaveへ保存しません。専用Sprite Sheetは4つの実装済みNodeへ登録済みです。将来PrefabやAnimator Controllerへ移行しても、Evolution条件とSave形式へ影響させません。

## 選択UI

Gameplay中に `V` またはGamepadのRight ShoulderでEvolutionメニューを開きます。

- 全Nodeと現在の評価状態を表示する。
- 不足しているレベル、Skill、Artランク、前提Node、排他選択を一覧表示する。
- UI Context中は時間を停止し、移動やAbility入力を受け付けない。
- Submit時に条件を再評価し、成功時だけ進化してGameplayへ戻る。
- 条件不足時は状態を変更せず、メニューを維持する。
- Cancel時は進化せずGameplayへ戻る。

`EvolutionSelectionController` が開閉、選択、確定要求を管理し、`EvolutionMenuView` は表示だけを担当します。

## Save方針

選択済みNodeは既存の `PlayerSaveData.unlockedEvolutionNodeIds` にキャラクター単位で保存します。補正後の値、条件評価結果、排他グループID、Definition参照は保存せず、ロード後にDefinitionから再計算します。

Evolution Node ID欄はSave Version 1から存在するため、今回の実装でSave Versionは変更しません。未知の保存IDは状態として維持しますが、条件や補正には利用しません。

## 現在のコンテンツ

- `evolution.slime.predator`: 戦闘系Skillを条件とする捕食系分岐
- `evolution.slime.arcane`: 魔法Artの熟練を条件とする魔術系分岐
- `evolution.slime.apex_predator`: 捕食系Tier 1を前提とする上位分岐
- `evolution.slime.archmage`: 魔術系Tier 1と火炎魔法の高ランクを前提とする上位分岐
- 排他グループ: `evolution-group.slime.tier1`、`evolution-group.slime.tier2`

4 NodeのDefinition、条件・補正、選択UI、専用2フレームアートと演出を登録済みです。捕食者の本能は訓練用ダミーの撃破報酬、火炎魔法は見習い魔術師との訓練会話から取得できるため、両系統ともPrototype内で条件を満たせます。

## 関連仕様

- [成長仕様](./progression.md)
- [Skill仕様](./skill.md)
- [Art仕様](./art.md)
- [セーブ仕様](./save.md)
- [スライム進化系列](../database/evolutions/slime-lineage.md)
