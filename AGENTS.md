# AGENTS.md

## 目的

このファイルは、CodexなどのAIエージェントが `to-become-demon-king-2d` を変更するときに最初に確認する共通ルールと参照ルートを定義します。

このリポジトリではUnity実装とKnowledge Baseを同じプロダクトとして扱います。変更の意味に応じてコード、Unityアセット、テスト、仕様を同期してください。

## AI Reading Strategy

1. 最初にこの `AGENTS.md` を読む。
2. 次に `docs/ai/context-map.md` から変更対象Featureを特定する。
3. 対象Featureの `Primary specification` と `Primary code` を先に確認する。
4. UI、Composition、Feature間連携を変更する場合だけ、関連するPresentation / Integration先を追加で確認する。
5. 影響範囲が広い変更では必要な関連Featureも確認するが、リポジトリ全体を事前に読むことを前提にしない。

Context Mapは索引でありSource of Truthではありません。仕様や実装判断はリンク先のコード、Unityアセット、仕様書、設計文書を確認してください。

## Core Rules

- `DemonKing.Domain` はUnity非依存の純C#領域とし、`UnityEngine`、Scene、`GameObject`、`MonoBehaviour`、`ScriptableObject`を持ち込まない。
- GameplayはDomain / Coreを利用できるが、Prototype固有クラスや具体的なuGUI Viewへ依存しない。
- `Field/Prototype` はCompositionと試作用コンテンツに限定し、恒久的なDomain / Gameplayルールを蓄積しない。
- ScriptableObject Definition、Runtime State、Save DTOを混同しない。Definitionをプレイ中の可変状態や保存状態として書き換えない。
- Combatコンポーネントへ経験値、ドロップ、Art / Skill取得、Evolution処理を直接埋め込まない。
- Steamや将来のコンソールSDKをGameplayから直接呼び出さない。保存先、実績、クラウド、ユーザー識別などのPlatform依存処理は専用境界の外側へ置く。
- Stable Content IDは表示名やAsset名から独立させ、一度Save Dataやコンテンツ参照に使用したIDを単純な表示名変更・Asset移動で変更しない。
- Abilityは実行可能な行動、Artは1つ以上のAbilityを習得・熟練する能動技能、Skillは受動的な成長要素、Evolutionは形態・成長経路を変える不可逆または排他的な選択として分離する。
- 実装していない機能をドキュメント上で「実装済み」と書かない。逆に、現在の実装を確認せず古い計画文書だけを根拠に未実装と断定しない。

詳細な責務境界は `docs/design/architecture.md`、現在の拡張基盤は `docs/design/extension-foundations.md` を参照してください。

## Source of Truth

### コード・Unityアセットが正

- 実際にコンパイルされるC#コード
- Scene / Prefab / Input Actions
- ScriptableObjectに保存する静的Definition・Runtime設定値・Asset参照
- Package / Project Settings
- 自動テスト

Runtime数値をMarkdownへ大量に複製して二重管理しないでください。

### docsが正

- ゲームビジョン
- 世界観と物語の意図
- 仕様の意味と制約
- アーキテクチャ上の責務境界
- 採用・不採用の設計判断
- ロードマップ
- モンスター、Art、Skill、Evolutionなどの人間向け索引

### 実装とdocsを同期する情報

- Input Action / Binding
- Combat / Interaction / Saveルール
- 成長・Ability・Art・Skill・Evolution・Dialogue・Questのデータ構造と責務境界
- Scene遷移 / UI状態遷移
- Platform依存境界
- Stable Content IDの命名規則

## Architecture Boundaries

### Domain / Definition / Runtime State / Save DTO

- `DemonKing.Domain`: Unity非依存の状態・値・保存DTO等。
- Definition: ScriptableObjectによる静的コンテンツ定義、バランス値、Asset参照。
- Runtime State: プレイ中に変化する状態。
- Save DTO: Runtime Stateと分離した保存形式。Mapperを介して変換する。

保存先の具体実装は `ISaveService` の外側に置きます。

`DamageRequest` / `DamageResult` / `DefeatContext` のようにUnity Objectを参照する型はDomainへ移さず、GameplayのUnity依存境界として扱います。

### Prototype

`Field/Prototype`、`SlimeController`、`RuntimeShapeFactory` などには移行境界があります。純粋な状態・保存DTOはDomain、アプリケーション基盤はCore、Unity上のゲームルールはGameplay、表示はPresentationへ置きます。

Feature間連携は必要に応じてCompositionで接続し、Dialogue、Combat、Quest、Reward等の具体実装同士を直接結合しないでください。

### Combat / Reward

`DamageResult` / `DefeatContext` からReward境界へ接続し、経験値やProgression取得を処理します。同じDefeatに対する重複報酬を許可しない現在の境界を維持します。

### Platform

保存処理は `ISaveService` 等の抽象境界を経由します。Platform SDKの型をDomain / Gameplayへ漏らさないでください。

## Task Routing

まず `docs/ai/context-map.md` の該当Featureを確認してください。

| 作業 | 最初に確認する場所 |
| --- | --- |
| Input | `docs/ai/context-map.md` の Input / `docs/specifications/input.md` |
| Interaction | Context Mapの Interaction / `docs/specifications/interaction.md` |
| Dialogue | Context Mapの Dialogue / `docs/design/extension-foundations.md` |
| Combat | Context Mapの Combat / `docs/specifications/combat.md` |
| Ability | Context Mapの Ability / `docs/specifications/ability.md` |
| Art | Context Mapの Art / `docs/specifications/art.md` |
| Skill | Context Mapの Skill / `docs/specifications/skill.md` |
| Evolution | Context Mapの Evolution / `docs/specifications/evolution.md` |
| Progression | Context Mapの Progression / `docs/specifications/progression.md` |
| Reward | Context Mapの Reward |
| Save | Context Mapの Save / `docs/specifications/save.md` |
| Pause / Dodge | Context Mapの Pause / Dodge / `docs/specifications/input.md` |
| Quest | Context Mapの Quest / `docs/design/extension-foundations.md` |
| Spawn | Context Mapの Spawning / `docs/design/extension-foundations.md` |
| Enemy AI | Context Mapの Enemy AI / `docs/development/roadmap.md` |
| ゲームの方向性 | `docs/game/vision.md` |
| アーキテクチャ変更 | `docs/design/architecture.md` |
| 実装方式・技術規約 | `docs/design/technical-design.md` |
| 長期的な設計判断 | `docs/decisions/` |
| 開発優先順位 | `docs/development/roadmap.md` |
| ストーリー / 世界設定 | `docs/story/` / `docs/world/` |
| コンテンツ索引 | `docs/database/` |

## Change Workflow

1. Context Mapから変更対象の仕様・主要コード・テストを特定する。
2. 現在のコードとUnityアセットを確認する。
3. 既存の責務境界を壊さず実装する。
4. Runtimeコード変更では関連するDomain / EditMode / PlayModeテストを確認・追加する。
5. 仕様や設計意図が変わった場合は同じPRでdocsを更新する。
6. 長期的な設計判断ならADRを追加する。
7. 実装状況の記述は現在のコードとUnityアセットを確認して更新する。

1つの機能変更は、可能な限りRuntime実装、Unityアセット／設定、テスト、関連仕様、必要なADRを同じPRへ含めます。ドキュメント更新だけのPRでは実装変更がないことを明確にしてください。

## Stable Content ID / Naming

Character、Ability、Art、Skill、Evolution、Reward、Dialogue、Quest等の保存データやコンテンツ間参照には表示名から独立した安定IDを使用します。

例:

```text
character.player.slime
ability.basic_melee
art.magic.fire
skill.combat.predatory_instinct
evolution.slime.apex_predator
reward.training_dummy
dialogue.training.apprentice_mage
quest.training.first_defeat
```

- Markdownファイル名は原則 `kebab-case.md`。
- ADRは `ADR-0001-title.md` 形式。
- 1ファイル1責務を基本とする。
- C#コメントは日本語で記述し、処理手順の逐語説明より「なぜこの責務がここにあるか」「Unity固有の制約」「移行境界」を優先する。
- 古い設計を説明するコメントを残さない。

## Tests

AIが実装対象からテストを検索しやすい命名を優先します。

- 単一クラスを主対象にするテストは原則 `<ClassName>Tests.cs` / `<ClassName>Tests` とする。
- Unity実行モードを名前で明示する必要がある既存テストは `<ClassName>PlayModeTests` 等も許可する。
- 複数クラスを統合的に検証する場合は `DialogueInteractionFlowTests`、`EvolutionSelectionFlowTests` のようにFeatureとFlowが分かる名前を使う。
- 無関係な責務のテストを1つのテストクラスへ追加し続けない。
- Unity依存が不要なルールはDomain / EditMode側の高速なテストを優先する。

新しいテストを追加する前に、Context Mapの `Tests` と対象クラス名で既存テストを確認してください。

## Documentation

```text
docs/
  ai/               AI向けの軽量な参照索引
  game/             ゲームビジョン・ゲームループ
  design/           アーキテクチャ・技術設計
  specifications/   実装と同期する機能仕様
  story/            ストーリー・キャラクター・クエスト
  world/            場所・勢力・世界設定
  database/         モンスター・進化・アイテム・アーツ・スキルの索引
  development/      ロードマップ・運用
  decisions/        ADR
  templates/        新規ドキュメントのテンプレート
```

`docs/ai/context-map.md` は索引に限定し、詳細仕様を複製しません。新しいMarkdownを `docs/` 直下へ無秩序に追加しないでください。

VitePressのナビゲーションや主要な公開カテゴリを追加する場合は `docs/.vitepress/config.mts` を更新します。AI専用の内部索引を追加しただけの場合は、公開ナビゲーションへの追加を必須としません。

## 詳細ドキュメント

- ゲームビジョン: `docs/game/vision.md`
- アーキテクチャ: `docs/design/architecture.md`
- 技術設計: `docs/design/technical-design.md`
- Dialogue / Quest / Spawn拡張基盤: `docs/design/extension-foundations.md`
- 機能仕様: `docs/specifications/`
- ロードマップ: `docs/development/roadmap.md`
- 設計判断: `docs/decisions/`
- リリース運用: `docs/development/release.md`
