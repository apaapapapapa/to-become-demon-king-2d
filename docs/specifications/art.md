# Art仕様

- Status: Foundation Implemented / Planned Content Page Registered

## 用語と責務

- Ability: キャラクターが実行する行動。習得状態や熟練度を知らない。
- Art: 攻撃魔法や特殊剣攻撃など、習得して熟練する能動技能。1つ以上のAbilityを段階的に解放する。
- Skill: 能力値、コスト、習得条件、Art成長などへ作用する受動的な成長要素。実行可能な行動そのものは表さない。
- Evolution: キャラクターの形態や成長経路を変える、不可逆または排他的な選択。

能動行動は、生得AbilityまたはArtから解放されるAbilityとして表現します。Skillが能動行動を直接実行・所有する設計にはしません。

## 全体フロー

```text
訓練 / 報酬 / アイテム / Evolution
  ↓ Art習得要求
ArtProgressionService
  ├ ArtDefinition
  └ ArtProgressState
  ↓ ランクで解放済みのAbility
AbilityController
  ↓ Ability実行
IAbilityExecutor
  ↓
AbilityEffectResolved
  ↓ 1 Executionにつき最大1回
ArtProgressionService.AwardMastery
```

ArtはAbilityの実行を置き換えません。プレイヤー入力とAIは、Artの取得元に関係なく既存の `AbilityController` から同じAbilityを実行します。

## ArtDefinition

`ArtDefinition` は次を持つ静的なScriptableObjectとして実装します。

- `art.*` 形式のStable Content ID
- 表示名、説明、アイコン、分類
- 熟練ランクごとの累積Mastery Point閾値
- 1つ以上の `ArtAbilityUnlockEntry`

`ArtAbilityUnlockEntry` は次を持ちます。

- 対象の `AbilityDefinition` またはStable Ability ID
- 解放に必要なArtランク
- 効果成立時に加算するMastery Point

具体的な閾値や加算量はUnityのDefinitionをSource of Truthとし、Knowledge Baseへ複製しません。

### 整合性制約

- Art IDは `art.*`、Ability IDは `ability.*` とする。
- ランク1の累積閾値は0とし、Art習得直後から少なくとも1つのAbilityを利用可能にする。
- ランク閾値は昇順、Mastery Point加算量は正数とする。
- 同じArt内でAbility IDを重複させない。
- 1つのAbilityが所属できるArtは最大1つとする。
- 生得、敵専用、NPC専用などのAbilityはArtに所属しなくてもよい。

## ArtProgressState

`ArtProgressState` はUnity非依存のDomain Runtime Stateとして、キャラクターごとに次を保持します。

```text
ArtId
MasteryPoints
```

Art進捗レコードの存在を習得済みの意味とします。習得直後は0ポイント・ランク1です。

現在ランクと解放済みAbilityは `ArtDefinition` の閾値から導出します。ランク、解放Ability ID、Definition参照はRuntime StateやSave DTOへ重複保存しません。

Mastery Pointとランクは減少しません。一度到達したランクで解放されたAbilityは維持します。

## 習得境界

Artの習得は汎用の `ArtProgressionService` を経由します。訓練、報酬、アイテム、Evolutionなどの取得元は共通の習得要求を送ります。

取得元固有の条件や消費は取得元側で判定します。Art側はDefinitionの存在、未習得かどうか、IDの妥当性を検証し、同じArtへの重複習得を冪等に扱います。

SkillはArt習得の条件や補正として参照できますが、能動行動そのものはArtとして登録します。

## Ability付与

習得済みArtについて、現在ランク以下の `ArtAbilityUnlockEntry` を解放済みとみなします。習得時とランクアップ時に、新しく解放されたAbilityをキャラクターの `AbilityController` へ冪等に付与します。

解放済みAbilityは常時利用可能とし、Art装備枠は設けません。どのAbilityを操作へ割り当てるか、選択UIをどう構成するかは入力・UI側の別タスクです。

基本近接攻撃 `ability.basic_melee` は生得Abilityのままとし、Artへ移しません。

## 熟練度加算

Abilityの実行要求が受理されただけではMastery Pointを加算しません。命中、回復、バフ・デバフ付与など、少なくとも1つの実効果が成立した場合に加算します。

共通効果解決通知 `AbilityEffectResolved` は次を含みます。

```text
ExecutionId
User / ActorId
AbilityId
EffectKind
WasApplied
```

`ArtProgressionService` はAbility IDから所属Artを逆引きし、`WasApplied` が真であるExecutionへMastery Pointを付与します。Artに所属しないAbilityは対象外です。

範囲攻撃、多段攻撃、継続効果で複数の効果結果が発生しても、同じ使用者と `ExecutionId` の組み合わせへは1回だけ加算します。Executor、`DamageRequest`、回復やバフの効果処理はArt進捗を直接変更しません。

## Save方針

`PlayerSaveData` はArt進捗の一覧を保持します。

```text
artProgress[]
  artId
  masteryPoints
```

Artはキャラクター単位で保存し、アカウント共通状態にはしません。装備Art、現在ランク、解放Ability IDは保存しません。

Save Version 2でArt進捗を追加しました。既存のSave Version 1は `GameSaveDataMigrator` がArt進捗を空一覧として補います。

## 現在の実装範囲

- `ArtDefinition` / `ArtAbilityUnlockEntry` / `ArtMasteryTable`
- `ArtProgressState` と `CharacterProgressionState` のArt進捗
- `ArtProgressionService` / `ArtProgressionController` による汎用習得
- 習得時・ランクアップ時・Save復元時の冪等なAbility付与
- Execution ID付き `AbilityEffectResolved`
- 近接ダメージの効果成立通知と、1 Executionにつき1回の熟練度加算
- Save DTO Version 2とVersion 1からのMigration
- `DamageTags.Art` と旧 `DamageTags.Skill` の互換Alias

`IArtMasteryModifierSource` によるSkillとEvolutionの熟練ポイント補正まで接続済みです。正式Runtime Artコンテンツ、Art固有の入力割当・UI、訓練や報酬など具体的な習得元は未実装です。火炎魔法はKnowledge Base上の計画ページだけを登録しています。回復、バフ、デバフなども通知型は利用できますが、各Executorからの効果成立通知は今後接続します。

## 関連仕様

- [Ability仕様](./ability.md)
- [成長仕様](./progression.md)
- [Skill仕様](./skill.md)
- [戦闘仕様](./combat.md)
- [セーブ仕様](./save.md)
- [Art一覧](../database/arts/)
