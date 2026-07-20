# Skill仕様

SkillはGameplayへ常時作用する受動的な成長要素です。Ability / Art / Evolutionとの概念境界は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。

## SkillDefinition

`SkillDefinition` は静的なScriptableObjectとして次を保持します。

- `skill.*` 形式のStable Content ID
- 表示名、説明、アイコン、分類
- 1つ以上の `GameplayModifierEntry`

各補正は対象、演算方式、値、任意の対象Content IDを持ちます。具体的な補正値はUnity Definitionを正とします。

現在の補正対象は次の3種類です。

| 対象 | 接続先 | 下限 |
| --- | --- | --- |
| 与ダメージ | `DamageRequest` を生成するとき | 0 |
| Abilityクールダウン | `AbilityRuntimeState` へ使用結果を記録するとき | 0秒 |
| Art熟練ポイント | `ArtProgressState` へ加算するとき | 効果成立1回につき1以上 |

補正演算は固定値加算と割合加算を持ち、全取得元の固定値と割合をそれぞれ合算してから適用します。

## Runtime Stateと取得

Skillの取得状態は `CharacterProgressionState.UnlockedSkillIds` が保持します。`SkillProgressionService` がDefinitionの存在とIDを確認し、重複取得を冪等に扱います。

取得元固有の条件と消費は取得元側の責務です。個別Skillの取得経路は [Skill一覧](../database/skills/) の各コンテンツページを参照してください。

## 補正接続

`SkillProgressionController` は取得済みSkillとDefinitionから汎用Modifier Sourceを公開します。補正利用側はSkill取得状態を直接参照しません。

Evolution等との共通接続方向は [Feature間の責務境界](../design/feature-boundaries.md#受動modifier) を参照してください。

Definitionは静的データとして扱い、取得時や補正適用時に書き換えません。SkillはAbilityを直接付与・実行しません。

## Save

Skill取得状態の保存形式は [セーブ仕様](./save.md) を参照してください。

現在の実装状況と今後の対象は [ロードマップ](../development/roadmap.md) を参照してください。
