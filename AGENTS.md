# AGENTS.md

## 目的

このファイルは、CodexなどのAIエージェントが `to-become-demon-king-2d` を変更するときに最初に読む共通ルールと参照ルートです。

Unity実装とKnowledge Baseを同じプロダクトとして扱い、変更の意味に応じてコード、Unityアセット、テスト、仕様を同期してください。

## AI Reading Strategy

1. この `AGENTS.md` を読む。
2. `docs/ai/context-map.md` から変更対象Featureを特定する。
3. Context Mapの `Spec` と `Code` を先に読む。
4. UIやFeature間配線を変更するときだけ `Integration` / 関連Featureを追加で読む。
5. 影響範囲が広い場合は必要な範囲を広げるが、リポジトリ全体を事前に読むことを前提にしない。

Context Mapは索引でありSource of Truthではありません。

## Core Rules

- `DemonKing.Domain` へ `UnityEngine`、Scene、`GameObject`、`MonoBehaviour`、`ScriptableObject`を持ち込まない。
- GameplayはDomain / Coreを利用できるが、Prototype固有クラスや具体的なuGUI Viewへ依存しない。
- `Field/Prototype` はCompositionと試作用コンテンツに限定し、恒久的なDomain / Gameplayルールを蓄積しない。
- ScriptableObject Definition、Runtime State、Save DTOを分離し、Definitionを可変状態や保存状態として書き換えない。
- Combatへ経験値、ドロップ、Art / Skill取得、Evolution処理を直接埋め込まない。
- Feature間連携は必要に応じてCompositionや共通イベント境界で接続し、Dialogue、Combat、Quest、Reward等を具体実装同士で直接結合しない。
- SteamやコンソールSDKをGameplayから直接呼ばず、Platform依存処理は専用境界の外側へ置く。
- Stable Content IDは表示名やAsset名から独立させ、Save Dataやコンテンツ参照に使用したIDを単純な名称変更・Asset移動で変更しない。
- Abilityは実行可能な行動、Artは能動技能、Skillは受動成長、Evolutionは形態・成長経路の選択として分離する。
- 実装状況は現在のコードとUnityアセットで確認する。未実装を「実装済み」と書かず、古い計画文書だけを根拠に実装済み機能を「未実装」と断定しない。

詳細な責務境界は `docs/design/architecture.md`、Dialogue / Quest / Spawnの拡張境界は `docs/design/extension-foundations.md` を参照してください。

## Source of Truth

| 情報 | Source of Truth |
| --- | --- |
| コンパイルされる実装 | C#コード |
| Scene / Prefab / Input | Unityアセット |
| 静的Definition・バランス値・Asset参照 | ScriptableObject |
| プレイ中の可変状態 | Runtime State / Domain |
| 保存形式 | Save DTO |
| ゲームビジョン・世界観・物語意図 | `docs/` |
| 仕様の意味・責務境界・設計判断 | `docs/specifications/`, `docs/design/`, `docs/decisions/` |
| 自動検証対象 | テストコード |

Runtime数値をMarkdownへ大量に複製しません。Input、Combat、Interaction、Save、成長系データ構造、UI状態遷移、Platform境界、Stable Content ID規則を変更した場合は関連docsも同期してください。

## Architecture Boundaries

- Domain: Unity非依存の状態・値・Save DTO。
- Definition: ScriptableObjectによる静的コンテンツ定義。
- Runtime State: プレイ中に変化する状態。
- Save DTO: Runtime Stateと分離しMapper経由で変換する保存形式。
- 保存先の具体実装は `ISaveService` の外側に置く。
- Unity Objectを参照する `DamageRequest` / `DamageResult` / `DefeatContext` 等はGameplay境界に置く。
- 同一DefeatへのReward重複付与を許可しない現在の境界を維持する。

## Task Routing

| 作業 | 最初に確認する場所 |
| --- | --- |
| Input / Interaction / Dialogue / Combat / Ability / Art / Skill / Evolution / Progression / Reward / Save / Pause / Dodge / Quest / Spawning / Enemy AI | `docs/ai/context-map.md` の該当Feature |
| ゲームの方向性 | `docs/game/vision.md` |
| アーキテクチャ変更 | `docs/design/architecture.md` |
| 実装方式・技術規約 | `docs/design/technical-design.md` |
| 長期的な設計判断 | `docs/decisions/` |
| 開発優先順位 | `docs/development/roadmap.md` |
| ストーリー / 世界設定 | `docs/story/` / `docs/world/` |
| コンテンツ索引 | `docs/database/` |

## Change Workflow

1. Context Mapから関連仕様・主要コード・テストを特定する。
2. 現在のコードとUnityアセットを確認する。
3. 既存の責務境界を壊さず実装する。
4. Runtime変更では関連するDomain / EditMode / PlayModeテストを確認・追加する。
5. 仕様や設計意図が変わった場合は同じPRでdocsを更新する。
6. 長期的な設計判断ならADRを追加する。
7. 実装状況の記述は現在の実装に合わせる。

1つの機能変更は、可能な限りRuntime実装、Unityアセット／設定、テスト、関連仕様、必要なADRを同じPRへ含めます。

## Stable Content ID / Naming

代表例:

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
- C#コメントは日本語で記述し、「なぜ」「Unity固有制約」「移行境界」を優先する。
- 古い設計を説明するコメントを残さない。

## Tests

AIが実装対象からテストを検索しやすい命名を優先します。

- 単一クラスが主対象なら原則 `<ClassName>Tests.cs` / `<ClassName>Tests`。
- Unity実行モードを明示する既存テストは `<ClassName>PlayModeTests` 等も可。
- 統合テストは `DialogueInteractionFlowTests` のようにFeatureとFlowが分かる名前にする。
- 無関係な責務のテストを1クラスへ追加し続けない。
- Unity依存が不要なルールはDomain / EditMode側の高速なテストを優先する。

追加前にContext Mapの `Tests` と対象クラス名で既存テストを確認してください。

## Documentation

- `docs/ai/`: AI向けの軽量な参照索引。詳細仕様を複製しない。
- `docs/game/`: ゲームビジョン。
- `docs/design/`: アーキテクチャ・技術設計。
- `docs/specifications/`: 実装と同期する機能仕様。
- `docs/story/`, `docs/world/`: 物語・世界設定。
- `docs/database/`: コンテンツの人間向け索引。
- `docs/development/`: ロードマップ・運用。
- `docs/decisions/`: ADR。

新しいMarkdownを `docs/` 直下へ無秩序に追加しないでください。AI専用の内部索引だけを追加した場合、VitePress公開ナビゲーションへの追加は必須ではありません。

## 詳細ドキュメント

- `docs/design/architecture.md`
- `docs/design/technical-design.md`
- `docs/design/extension-foundations.md`
- `docs/specifications/`
- `docs/development/roadmap.md`
- `docs/development/release.md`
- `docs/decisions/`
