# ADR-0002: Ability・Art・Skillの責務を分離する

- Status: Accepted
- Date: 2026-07-20

## Context

Ability基盤は、プレイヤーとAIが同じ方法で基本近接攻撃などの行動を実行するために導入しました。一方、従来の文書ではSkillを「Abilityの獲得・強化」と定義しており、攻撃魔法や特殊剣攻撃などの能動技能と、能力値補正などの受動成長が同じ概念へ集まり始めていました。

能動技能には、習得、熟練、派生行動の段階解放が必要です。これらをAbilityまたはSkillへ直接持たせると、実行処理がプレイヤーの成長状態に依存し、敵やNPCとの共通利用が難しくなります。

## Decision

- Abilityを、取得経路を知らない実行可能な行動とする。
- Artを、`art.*` IDを持ち、1つ以上のAbilityを熟練ランクで段階解放する能動技能とする。
- Skillを、能力値、コスト、条件、Art成長などへ作用する受動成長とする。
- Evolutionを、形態や排他的な成長経路を変える選択として維持する。
- 生得・敵専用・NPC専用のAbilityはArtへ所属しなくてもよい。
- Artの習得と熟練度はキャラクター単位のRuntime StateおよびSave DTOで管理し、Definitionを書き換えない。
- Abilityの効果成立を通知し、Art側がExecution単位で熟練度を加算する。AbilityやCombat効果は成長状態を直接変更しない。
- 習得済みArtから解放されたAbilityは常時利用可能とし、Art装備枠は導入しない。

## Consequences

- 攻撃魔法や特殊剣攻撃をArtとして一貫して索引・保存できる。
- 同じAbility実行基盤をプレイヤー、敵、NPCで維持できる。
- Skillの受動補正とArtの能動行動を別々に拡張できる。
- Art進捗、Abilityとの対応、効果成立通知、Save Migrationの実装が必要になる。
- 複数Abilityを含むArtでは、ArtとAbilityのStable IDをそれぞれ管理する必要がある。
- 現在の `DamageTags.Skill` は能動攻撃を表す名前として不適切になり、互換性を保った移行が必要になる。

## Rejected Alternatives

### Skillが能動行動も所有する

受動成長と実行可能な行動の境界が曖昧になり、Skill取得状態がAbility実行系へ漏れやすいため採用しません。

### ArtをAbilityの表示上の分類だけにする

複数Abilityの段階解放やArt単位の熟練度を独立して保存できないため採用しません。

### Art習得時に全Abilityを解放する

熟練による派生行動の解放という成長を表現できないため採用しません。

### Art装備枠を導入する

現段階では入力やUIの要件が未確定であり、習得・熟練の基盤と同時に制限を導入する必要がないため採用しません。

## Reconsider When

- 常時利用可能なAbility数が入力やUIの許容量を超えた。
- 1つのAbilityを複数Artで共有する明確なゲームデザイン要件が生じた。
- 熟練度の減少、Art忘却、キャラクター間共有が必要になった。
