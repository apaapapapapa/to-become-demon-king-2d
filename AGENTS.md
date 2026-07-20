# AGENTS.md

## 目的

このファイルは、CodexなどのAIエージェントが `to-become-demon-king-2d` を変更するときの共通ルールを定義します。

このリポジトリでは、Unity実装とKnowledge Baseを同じプロダクトとして扱います。コードだけ、またはドキュメントだけを無関係に変更せず、変更の意味に応じて両方を同期してください。

## 最初に読む場所

| 作業 | 主な参照先 |
| --- | --- |
| ゲームの方向性を変える | `docs/game/vision.md` |
| アーキテクチャを変える | `docs/design/architecture.md` |
| 実装方式・技術規約を変える | `docs/design/technical-design.md` |
| 入力を変える | `docs/specifications/input.md` |
| 戦闘を変える | `docs/specifications/combat.md` |
| Artを変える | `docs/specifications/art.md` |
| Interactionを変える | `docs/specifications/interaction.md` |
| 成長を変える | `docs/specifications/progression.md` |
| Saveを変える | `docs/specifications/save.md` |
| ストーリーを追加・変更する | `docs/story/` |
| 世界設定を追加・変更する | `docs/world/` |
| モンスターを追加する | `docs/database/monsters/` |
| 進化関係を追加する | `docs/database/evolutions/` |
| アイテムを追加する | `docs/database/items/` |
| アーツを追加する | `docs/database/arts/` |
| スキルを追加する | `docs/database/skills/` |
| 長期的な設計判断を行う | `docs/decisions/` |
| 開発優先順位を変える | `docs/development/roadmap.md` |

## Source of Truth

### コード・Unityアセットが正

- 実際にコンパイルされるC#コード
- Scene / Prefab / Input Actions
- ScriptableObjectに保存する静的Definition・Runtime設定値
- Package / Project Settings
- 自動テスト

Markdownへ同じ数値を複製して二重管理しないでください。

### docsが正

- ゲームビジョン
- 世界観と物語の意図
- 仕様の意味と制約
- アーキテクチャ上の責務境界
- 採用・不採用の設計判断
- ロードマップ
- モンスターや進化などの人間向け索引

### 実装とdocsを同期する情報

- Input Action / Binding
- Combatルール
- Interactionルール
- Save仕様
- 成長・Art・Skill・Evolutionのデータ構造
- Scene遷移
- UI状態遷移
- Platform依存境界
- Stable Content IDの命名規則

## Domain / Definition / Runtime State / Save DTO

### Domain

`DemonKing.Domain` はUnity非依存の純C#領域です。

例:

- `CharacterProgressionState`
- 将来の `ArtProgressState`
- `ExperienceTable` / `LevelUpResult`
- `GameSaveData` / `PlayerSaveData`
- Stable Content ID関連

Unity Scene、`GameObject`、MonoBehaviour、ScriptableObjectなどのUnity依存型をDomainへ持ち込みません。

`DamageRequest` / `DamageResult` / `DefeatContext` は `UnityEngine.GameObject` を参照するため、Domainではなく `Gameplay/Combat` に置きます。

### Definition

ScriptableObjectは静的なコンテンツ定義・バランス値・アセット参照を保持します。

例:

- `CharacterDefinition`
- `CharacterStatsDefinition`
- 将来の `ArtDefinition`
- `MeleeAttackDefinition`
- `DodgeDefinition`
- `ExperienceTableDefinition`
- `RewardDefinition`

### Runtime State

プレイ中に変化するレベル、経験値、Art熟練度、Skill解放、Evolution解放などはRuntime Stateで保持します。DefinitionのScriptableObjectを書き換えて保存状態として使用しないでください。

### Save DTO

保存用データはRuntime Stateから分離し、Mapperを経由して変換します。保存先の具体実装は `ISaveService` の外側に置きます。

## Stable Content ID

Character、Ability、Art、Reward、将来のSkill・Evolution Nodeなど、保存データやコンテンツ間参照に使うIDは表示名やAsset名から独立させます。

例:

```text
character.player.slime
ability.basic_melee
art.magic.example
reward.training_dummy
```

一度Save Dataへ使用したIDは、単純な表示名変更やAsset移動で変更しないでください。

## 変更時の基本手順

1. 関連するKnowledge Base文書を読む。
2. 現在のコードとUnityアセットを確認する。
3. 既存の責務境界を壊さず実装する。
4. 仕様や設計意図が変わった場合は同じPRでdocsを更新する。
5. 長期的な設計判断ならADRを追加する。
6. Runtimeコード変更では関連するDomain / EditMode / PlayModeテストを確認・追加する。
7. 実装していない機能をドキュメント上で「実装済み」と書かない。

## ドキュメント配置ルール

```text
docs/
  game/             ゲームビジョン・ゲームループ
  design/           アーキテクチャ・技術設計
  specifications/   実装と同期する機能仕様
  story/            ストーリー・キャラクター・クエスト
  world/            場所・勢力・世界設定
  database/         モンスター・進化・アイテム・アーツ・スキルの索引
  development/      ロードマップ・運用・AI開発ルール
  decisions/        ADR
  templates/        新規ドキュメントのテンプレート
```

新しいMarkdownを `docs/` 直下へ無秩序に追加しないでください。

## 命名

- Markdownファイル名は原則 `kebab-case.md`。
- 固有IDが必要なデータは、表示名ではなく安定した英数字IDを用意する。
- ADRは `ADR-0001-title.md` 形式。
- 1ファイル1責務を基本とする。

## モンスター・進化・アイテム・アーツ・スキル

コンテンツ数が少ない間は `docs/database/` を人間向けKnowledge Baseとして使用します。

Runtime値はScriptableObjectを正とし、MarkdownへHPや攻撃力などの全数値をコピーしないでください。必要な場合は「役割」「特徴」「進化条件の意味」「安定ID」「参照するDefinition」などを記載します。

Abilityは実行可能な行動、Artは1つ以上のAbilityを習得・熟練する能動技能、Skillは受動的な成長要素として分離します。攻撃魔法や特殊剣攻撃などの能動技能をSkillとして登録しないでください。

データ数が増え、一覧生成や整合性検証が必要になった段階で `game-data/` のYAML / JSONなどをSingle Source of Truthとし、UnityとVitePress双方へ生成する方式をADRで検討します。先行して独自データ生成基盤を作らないでください。

## コメント

C#コメントは日本語で記述します。

コメントはコードを読めば分かる処理手順ではなく、次を優先します。

- なぜこの責務がここにあるか
- なぜ別の実装を採用しなかったか
- Prototype専用か恒久機能か
- 将来削除・移行する境界か
- Unity固有の制約や注意点

古い設計を説明するコメントを残さないでください。

## Prototype境界

`Field/Prototype`、`SlimeController`、`RuntimeShapeFactory` などには移行境界が残っています。

純粋な状態・保存DTOは `Domain`、アプリケーション基盤は `Core`、Unity上のゲームルールは `Gameplay`、表示は `Presentation` へ置き、`Field/Prototype` はCompositionと試作用コンテンツに限定します。

## CombatとReward

Combatコンポーネントへ経験値・ドロップ・進化処理を直接埋め込まないでください。

`DamageRequest` / `DamageResult` / `DefeatContext` は `Gameplay/Combat` のUnity依存境界として扱います。`DamageResult` / `DefeatContext` から `RewardService` へ接続し、経験値加算などを処理します。同じDefeatに対する重複報酬を許可しない現在の境界を維持します。

## Platform対応

Steamや将来のコンソールSDKをGameplayコードから直接呼び出さないでください。

保存先、実績、クラウド、ユーザー識別などのPlatform依存機能は専用境界の外側へ置きます。保存処理は `ISaveService` を経由します。

## PRの単位

可能な限り、1つの機能変更を次の単位で同じPRへ含めます。

```text
Domain / Runtime実装
+ Unityアセット／設定
+ テスト
+ 関連仕様
+ 必要ならADR
```

ドキュメント更新だけのPRも許可しますが、その場合は実装変更がないことを明確にします。

## VitePress

Knowledge BaseのNode依存関係は、ルートのsemantic-release用 `package.json` と分離して `docs/package.json` で管理します。

```bash
cd docs
npm install
npm run dev
npm run build
npm run preview
```

VitePressのナビゲーションや新しい主要カテゴリを追加した場合は `docs/.vitepress/config.mts` も更新してください。
