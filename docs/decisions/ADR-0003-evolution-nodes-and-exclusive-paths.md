# ADR-0003: Evolutionを不可逆・排他的なNode選択として管理する

- Status: Accepted
- Date: 2026-07-20

## Context

Evolutionはキャラクターの形態や成長経路を変える選択です。条件、分岐、選択結果、Gameplay補正をAbilityやCharacter Prefabへ直接埋め込むと、保存、UI、敵やNPCへの再利用、排他性の検証が困難になります。

また、現在は分岐ごとの完成版SpriteやPrefabがなく、見た目の差し替えを条件・保存基盤と同時に固定できません。

## Decision

- Evolutionの選択肢を `evolution.*` IDを持つ静的なNode Definitionとして表現する。
- 同時に選択できないNodeは `evolution-group.*` IDで同じ排他グループへ所属させる。
- 条件評価はレベル、Skill、Artランク、前提Node、排他グループを共通サービスで行う。
- 条件不成立理由を一覧で返し、UIや取得元が個別条件を再実装しない。
- 選択結果は `CharacterProgressionState` とSave DTOへNode IDだけを保存し、Definition参照や補正結果を保存しない。
- 選択済みNodeを削除するRuntime APIは設けず、同じ排他グループの別Node取得を拒否する。
- Gameplayへの常時効果はSkillと共通のModifier Source境界へ公開する。
- 視覚的な形態変更は `EvolutionApplied` 通知の外側へ分離し、専用アセット確定後にPresentationで実装する。

## Consequences

- 分岐条件と排他性をプレイヤー、敵、NPCで共通評価できる。
- SaveはStable Node IDだけで維持でき、条件や補正値変更と分離できる。
- SkillとEvolutionの補正を同じ規則で合成できる。
- 見た目の形態変更は別タスクとして残るが、選択・保存・Gameplay効果を先に検証できる。
- 排他グループ変更は既存Saveの意味を変えるため、公開後はMigrationを伴う設計変更として扱う必要がある。

## Rejected Alternatives

### Character Prefabだけで進化状態を表す

条件、排他性、保存状態をSceneやPrefab参照へ依存させ、Domainで評価できなくなるため採用しません。

### 条件成立時に自動進化する

排他的な分岐でプレイヤーの選択機会を失い、物語やUIから実行タイミングを制御できないため採用しません。

### 進化後の補正値をSaveへ保存する

Definition更新との二重管理になり、どのNodeから生じた効果か追跡できなくなるため採用しません。

## Reconsider When

- Evolutionを巻き戻す明確なゲームデザインが採用された。
- 複数キャラクター間でEvolution Treeを共有する必要が生じた。
- Character Definitionそのものの差し替えが、見た目だけでなく基礎能力や装備制約にも必要になった。
