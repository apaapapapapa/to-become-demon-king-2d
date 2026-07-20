# Art仕様

Artは1つ以上のAbilityを習得・熟練によって段階解放する能動技能です。Ability / Skill / Evolutionとの概念境界は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。

## ArtDefinition

`ArtDefinition` は次を持つ静的なScriptableObjectです。

- `art.*` 形式のStable Content ID
- 表示名、説明、アイコン、分類
- 熟練ランクごとの累積Mastery Point閾値
- 1つ以上の `ArtAbilityUnlockEntry`

`ArtAbilityUnlockEntry` は対象Abilityと解放に必要なArtランク、効果成立時のMastery Pointを定義します。具体的な閾値や加算量はUnity Definitionを正とします。

### 整合性制約

- Art IDは `art.*`、Ability IDは `ability.*` とする。
- ランク1の累積閾値は0とし、習得直後から少なくとも1つのAbilityを利用可能にする。
- ランク閾値は昇順、Mastery Point加算量は正数とする。
- 同じArt内でAbility IDを重複させない。
- 1つのAbilityが所属できるArtは最大1つとする。
- 生得、敵専用、NPC専用等のAbilityはArtに所属しなくてもよい。

## ArtProgressState

`ArtProgressState` はキャラクターごとに `ArtId` と累積 `MasteryPoints` を保持します。

Art進捗レコードの存在を習得済みの意味とします。現在ランクと解放済みAbilityは `ArtDefinition` から導出し、Runtime Stateへ重複保持しません。

Mastery Pointとランクは減少しません。一度到達したランクで解放されたAbilityは維持します。

## 習得

Artの習得は `ArtProgressionService` を経由します。訓練、報酬、アイテム、Evolution等の取得元は、条件判定後に共通の習得境界へ要求します。

`ProgressionAcquisitionService` は `ProgressionGrantDefinition` を受け取り、Art習得とSkill解放を冪等に調停します。取得元固有の条件や消費をArt側へ持ち込みません。

個別Artの習得経路は [Art一覧](../database/arts/) の各コンテンツページを参照してください。

## Ability解放

習得済みArtについて、現在ランク以下の `ArtAbilityUnlockEntry` を解放済みとみなします。習得時とランクアップ時に、新しく解放されたAbilityを `AbilityController` へ冪等に付与します。

解放済みAbilityは常時利用可能とし、Art装備枠は設けません。

## 熟練度加算

Abilityの実行要求が受理されただけではMastery Pointを加算しません。命中、回復、バフ・デバフ付与等の実効果が成立した場合に加算します。

同じ使用者と `ExecutionId` の組み合わせへは1回だけ加算します。Executorや効果処理はArt進捗を直接変更しません。

Ability効果との接続方向は [Feature間の責務境界](../design/feature-boundaries.md#art熟練とability効果) を参照してください。

## Save

Art進捗の保存形式とMigrationは [セーブ仕様](./save.md) を参照してください。

現在の実装状況と今後の対象は [ロードマップ](../development/roadmap.md) を参照してください。
