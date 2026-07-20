# To Become Demon King 2D

『To Become Demon King 2D』は、アイソメトリック2D／2.5Dのピクセルアート表現を採用するUnity製RPGです。Steamを第一ターゲットとし、将来のコンソール移植を妨げない構造で開発します。

現在は、今後コンテンツを増やすための基礎アーキテクチャと最小プレイ可能ループまで実装済みです。

## 現在のプレイ可能ループ

1. アイソメトリックTilemap上を移動する
2. Collision Tilemapによる物理境界に衝突する
3. NPCへ近づいてInteractする
4. 訓練用スライムへAttackする
5. HPを減らして対象を倒す
6. Dodgeで短時間の回避移動を行う
7. PauseでGameplay入力を止め、uGUIのPause画面へ切り替える
8. 撃破報酬として経験値を獲得し、成長状態を更新する
9. カメラがプレイヤーを追従する

## 操作

| 操作 | キーボード | ゲームパッド |
| --- | --- | --- |
| 移動 | WASD / 矢印キー | Left Stick |
| 攻撃 | J | Westボタン |
| 調べる・話す | E | Southボタン |
| 回避 | Left Shift | Eastボタン |
| ポーズ | Escape | Startボタン |

Pause中はUI用Input Action Mapへ切り替わります。

## 技術基盤

- Unity 6（現在の基準Editorは `6000.5.4f1`）
- C#
- Universal Render Pipeline（URP）
- Isometric Tilemap
- Unity Input System
- Canvas（uGUI）
- Rigidbody2D / TilemapCollider2D
- ScriptableObjectによる静的Definition管理
- Unity非依存の `DemonKing.Domain`
- Unity Runtimeの `DemonKing.Runtime`
- Unity Test Framework
- VitePressによるKnowledge Base

## 現在のランタイム構成

```text
Prototype.unity
  ↓
FieldBootstrap
  ↓
PrototypeProjectAssets
  ↓
PrototypeApplicationInstaller
  ├ PrototypeApplicationSettings
  ├ PrototypeWorldBuilder
  ├ GamePauseController
  └ PrototypeUiInstaller
```

`FieldBootstrap` は設定値や具体的な初期化順序を持たず、起動処理を `PrototypeApplicationInstaller` へ委譲します。

## Definition / Runtime State / Save

静的なコンテンツ定義と、プレイ中に変化する状態、保存形式を分離しています。

```text
CharacterDefinition
  ↓
CharacterProgressionState
  ├ Level
  ├ CurrentExperience
  ├ UnlockedSkillIds
  └ UnlockedEvolutionNodeIds
  ↓ Mapper
PlayerSaveData / GameSaveData
  ↓
ISaveService
```

## Combat / Reward

```text
PlayerMeleeAttack
  ↓
DamageRequest
  ↓
IDamageable / Health
  ↓
DamageResult
  ↓
DefeatContext
  ↓
RewardService
  ↓
CharacterProgressionState.GainExperience
```

経験値、ドロップ、進化処理をHealthや攻撃コンポーネントへ直接埋め込まない方針です。

## Knowledge Base

ゲームに関する設計書、仕様書、ストーリー、世界設定、モンスター、進化、アイテム、スキル、開発判断を `docs/` で一元管理します。

```text
docs/
  game/             ゲームビジョン
  design/           アーキテクチャ・技術設計
  specifications/   機能仕様
  story/            ストーリー・キャラクター・クエスト
  world/            世界設定
  database/         モンスター・進化・アイテム・スキル
  development/      ロードマップ・リリース・開発運用
  decisions/        ADR
  templates/        ドキュメントテンプレート
```

Knowledge Baseのトップは `docs/index.md`、AIエージェント向けの共通ルールはルートの `AGENTS.md` です。

### VitePress

VitePressのNode依存関係は、ルートのsemantic-release用Node環境と分離して `docs/package.json` で管理します。

```bash
cd docs
npm install
npm run dev
npm run build
npm run preview
```

設定は `docs/.vitepress/config.mts` にあります。

## Source of Truth

- 静的なRuntime設定値・Asset参照: UnityのScriptableObject Definition
- プレイ中に変化する状態: `DemonKing.Domain` のRuntime State
- 保存形式: Save DTO
- ゲームビジョン・世界観・物語意図: Knowledge Base
- 仕様の意味・制約・設計判断: Knowledge Base

Runtime数値をMarkdownへ大量に複製して二重管理しません。

## テスト

現在の主な自動テスト対象:

- 描画順
- Health / Combat結果
- CharacterProgressionState
- ExperienceTable / LevelUpResult
- RewardServiceによる経験値付与と重複防止
- Save DTO相互変換
- CharacterDefinition
- CameraFollow2D
- Input Context
- Dodge
- Pause / Resume

## 開発フェーズ

P0〜P2に加え、Definition、Runtime State、Save DTO、Combat境界、経験値／撃破報酬の最小経路まで完了しています。

次はAbility実行基盤、Skill、Evolution、NPC・会話、敵AI、クエストを段階的に追加します。

## リリース

`main` へのマージ時にsemantic-releaseを実行し、Conventional Commitsから `vX.Y.Z` タグ、`CHANGELOG.md`、Unity Playerのバージョン、GitHub Releaseを自動更新します。npm公開やUnity Playerの自動ビルドは行いません。

- `fix:` / `perf:`: Patch Release
- `feat:`: Minor Release
- `BREAKING CHANGE:`: Major Release
- `docs:` / `test:` / `chore:` / `ci:` / `refactor:`: リリースなし

Squash merge時はPull RequestタイトルをConventional Commits形式にします。詳しい運用は [リリース運用](docs/development/release.md) を参照してください。

## 主要ドキュメント

- [Knowledge Base](docs/index.md)
- [ゲームビジョン](docs/game/vision.md)
- [アーキテクチャ](docs/design/architecture.md)
- [技術設計](docs/design/technical-design.md)
- [仕様書一覧](docs/specifications/)
- [ロードマップ](docs/development/roadmap.md)
- [リリース運用](docs/development/release.md)
- [AI開発ルール](AGENTS.md)
