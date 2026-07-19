# To Become Demon King 2D

『To Become Demon King 2D』は、アイソメトリック2D／2.5Dのピクセルアート表現を採用するUnity製RPGです。Steamを第一ターゲットとし、将来のコンソール移植を妨げない構造で開発します。

現在は、コンテンツを増やすための基礎アーキテクチャ、最小プレイ可能ループ、成長システム実装前のDomain／Save／Combat境界まで整備済みです。

## 現在のプレイ可能ループ

1. アイソメトリックTilemap上を移動する
2. Collision Tilemapによる物理境界に衝突する
3. NPCへ近づいてInteractする
4. 訓練用スライムへAttackする
5. HPを減らして対象を倒す
6. Dodgeで短時間の回避移動を行う
7. PauseでGameplay入力を止め、uGUIのPause画面へ切り替える
8. カメラがプレイヤーを追従する

## 操作

| 操作 | キーボード | ゲームパッド |
| --- | --- | --- |
| 移動 | WASD / 矢印キー | Left Stick |
| 攻撃 | J | Button West |
| 調べる・話す | E | Button South |
| 回避 | Left Shift | Button East |
| ポーズ | Escape | Start |

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

## 現在のアーキテクチャ概要

```text
DemonKing.Domain
  ├ Progression Runtime State
  ├ Save DTO
  ├ Combat Result / Defeat Context
  └ Stable Content ID

DemonKing.Runtime
  ├ Core
  ├ Gameplay
  ├ Presentation
  └ Field / Prototype Composition
```

起動経路:

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

詳細は [Knowledge Baseのアーキテクチャ](docs/design/architecture.md) を参照してください。

## Definition / Runtime State / Save

静的なキャラクター定義と、プレイ中に変化する状態、保存形式を分離しています。

```text
CharacterDefinition
  ├ characterId
  ├ prefab
  ├ statsDefinition
  ├ basicMeleeAttackDefinition
  └ dodgeDefinition
       ↓
CharacterProgressionState
  ├ level
  ├ currentExperience
  ├ unlockedSkillIds
  └ unlockedEvolutionNodeIds
       ↓ Mapper
PlayerSaveData / GameSaveData
```

保存先の具体実装は `ISaveService` 境界の外側へ追加します。

## Combat境界

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
  ↓ 将来
Reward / Experience / Drop
```

経験値やドロップをHealthや攻撃コンポーネントへ直接埋め込まず、後続のReward処理へ接続する方針です。

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
  development/      ロードマップ・開発運用
  decisions/        ADR
  templates/        ドキュメントテンプレート
```

トップページは `docs/index.md` です。

AIエージェントが開発する際の共通ルールはルートの `AGENTS.md` に定義しています。

### VitePress

```bash
npm install
npm run docs:dev
npm run docs:build
npm run docs:preview
```

VitePressの設定は `docs/.vitepress/config.mts` です。

## Source of Truth

- Runtimeの静的数値・Asset参照: UnityのScriptableObject / Asset
- プレイ中に変化する状態: Domain Runtime State
- 保存形式: Save DTO
- ゲームビジョン・世界観・物語意図: Knowledge Base
- 仕様の意味・制約・設計判断: Knowledge Base

Runtime数値をMarkdownへ大量に複製して二重管理しません。

## テスト

現在の主なテスト対象:

- Y座標による描画順
- Healthの死亡処理
- `DamageRequest` / `DamageResult` / `DefeatContext`
- `CharacterProgressionState`
- Save DTOとの相互変換
- `CharacterDefinition` の必須参照
- CameraFollow2D
- Gameplay / UI / Disabled Input Context
- Dodge
- Pause / Resume

## 直近の開発方針

1. 経験値テーブル
2. Reward Service
3. `DefeatContext` から経験値加算までを接続
4. Ability / Skill
5. Evolution
6. NPC・会話
7. 敵AI
8. クエスト・目的管理
9. 最小の縦切りループ完成

長期的な設計判断は `docs/decisions/` にADRとして残します。
