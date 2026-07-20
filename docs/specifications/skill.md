# Skill仕様

- Status: Passive Modifier Foundation Implemented

## 用語と責務

Skillは、能力値、Abilityの性能、Art成長などへ常時作用する受動的な成長要素です。`skill.*` 形式のStable Content IDを持ちます。

攻撃魔法や特殊剣攻撃などの能動行動はSkillに含めず、Artまたは生得Abilityとして表現します。Ability、Executor、ArtはSkillの取得状態を直接参照しません。

## SkillDefinition

`SkillDefinition` は静的なScriptableObjectとして次を保持します。

- `skill.*` 形式のStable Content ID
- 表示名、説明、アイコン、分類
- 1つ以上の `SkillModifierEntry`

各補正は対象、演算方式、値、任意の対象Content IDを持ちます。対象IDが空なら同種の全コンテンツ、指定されていればその `ability.*` または `art.*` だけへ作用します。具体的な補正値はUnity DefinitionをSource of Truthとします。

現在の補正対象は次の3種類です。

| 対象 | 接続先 | 下限 |
| --- | --- | --- |
| 与ダメージ | `MeleeAttackExecutor` から生成する `DamageRequest` | 0 |
| Abilityクールダウン | `AbilityRuntimeState` へ使用結果を記録するとき | 0秒 |
| Art熟練ポイント | `ArtProgressState` へ加算するとき | 効果成立1回につき1以上 |

補正演算は固定値加算と割合加算を持ち、全取得元の固定値と割合をそれぞれ合算してから適用します。これによりコンポーネントの走査順で結果が変わりません。

## Runtime Stateと取得

Skillの取得状態は `CharacterProgressionState.UnlockedSkillIds` が保持します。`SkillProgressionService` がDefinitionの存在とIDを確認し、同じSkillへの重複取得を冪等に扱います。

訓練、報酬、アイテム、Evolutionなど取得元固有の条件と消費は取得元側の責務です。取得元は条件成立後に共通の `Unlock` を呼び、Skillや補正適用先へ固有条件を埋め込みません。

## 補正接続

```text
CharacterProgressionState.UnlockedSkillIds
  + SkillDefinition[]
  ↓
SkillProgressionService
  ↓ NumericModifier
SkillProgressionController
  ├ IOutgoingDamageModifierSource
  ├ IAbilityCooldownModifierSource
  └ IArtMasteryModifierSource
       ↓
Combat / Ability / Art
```

補正を利用する側は汎用契約だけを参照します。将来の装備、バフ、Evolution補正は同じ契約を実装でき、Skillへ直接依存する必要はありません。

Definitionは静的データとして扱い、取得時や補正適用時に書き換えません。SkillはAbilityを直接付与・実行しません。

## Save方針

取得済み `skill.*` IDは既存の `PlayerSaveData.unlockedSkillIds` へキャラクター単位で保存します。補正後の値や `SkillDefinition` 参照は保存せず、ロード後に取得済みIDとDefinitionから再計算します。

Skill ID欄はSave Version 1から存在するため、今回の実装でSave Versionは変更しません。Definitionが見つからない保存IDは状態として維持しますが、補正には使用しません。

## 現在の実装範囲

- `SkillDefinition` / `SkillModifierEntry`
- `SkillProgressionService` / `SkillProgressionController`
- Skill取得状態と既存Save DTOの往復
- 与ダメージ、Abilityクールダウン、Art熟練ポイントへの補正接続
- 正式Definition `skill.combat.predatory_instinct`

具体的な習得元、Skill選択UI、能力値・コスト・習得条件など追加対象への補正は未実装です。

## 関連仕様

- [Ability仕様](./ability.md)
- [Art仕様](./art.md)
- [戦闘仕様](./combat.md)
- [成長仕様](./progression.md)
- [セーブ仕様](./save.md)
- [Skill一覧](../database/skills/)
