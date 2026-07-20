# スキル一覧

Skillは、能力値、Abilityのコストや性能、Artの習得条件・熟練成長などへ作用する受動的な成長要素です。攻撃魔法や特殊剣攻撃などの能動技能は [Art](../arts/) として管理します。

- `skill.*` 形式のStable Content IDを持たせる。
- Runtimeの補正値はUnity側のDefinitionを正とする。
- Knowledge Baseには効果の意味、対象、制約、Art・他Skill・Evolutionとの関係を記載する。
- Skill自体を実行可能な行動として定義しない。
- 戦闘補正は [Combat仕様](../../specifications/combat.md)、Art成長への作用は [Art仕様](../../specifications/art.md) と矛盾させない。

現在、Skillシステムは未実装であり、正式なSkillデータは未登録です。実装済みのArt基盤へ作用する受動補正として、ロードマップの次段階で実装します。
