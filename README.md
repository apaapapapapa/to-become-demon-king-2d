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
8. カメラがプレイヤーを追従する

この段階では、物語・クエスト・敵AI・成長要素などのコンテンツ量よりも、機能追加に耐えられる境界を優先しています。

## 操作

| 操作 | キーボード | ゲームパッド |
| --- | --- | --- |
| 移動 | WASD / 矢印キー | 左スティック |
| 攻撃 | J | Westボタン |
| 調べる・話す | E | Southボタン |
| 回避 | Left Shift | Eastボタン |
| ポーズ | Escape | Startボタン |

Pause中はUI用Input Action Mapへ切り替わります。キーボードではEscapeのCancel入力、ゲームパッドではStartまたはEastボタンのCancel入力でゲームへ復帰できます。

## 技術基盤

- Unity 6（現在の基準Editorは `6000.5.4f1`）
- C#
- Universal Render Pipeline（URP）
- Isometric Tilemap
- Unity Input System
- Canvas（uGUI）
- Rigidbody2D / TilemapCollider2D
- ScriptableObjectによるゲームバランス・起動設定管理
- Unity Test Framework
- Assembly DefinitionによるDomain / Runtime / Test分離

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
  ├ PrototypeSceneConfigurator
  ├ PrototypeSortingConfigurator
  ├ PrototypeWorldBuilder
  │   ├ Terrain / Collision / World Prefab
  │   ├ Prototype Gameplay Features
  │   ├ PrototypePlayerSpawner
  │   └ PrototypeCameraInstaller
  ├ GamePauseController
  └ PrototypeUiInstaller
      ├ GameHudView
      └ PauseMenuView
```

`FieldBootstrap` は設定値や具体的な初期化順序を持たず、起動処理を `PrototypeApplicationInstaller` へ委譲します。

## Input Actionコンテキスト

Input Actionsは用途別に分離しています。

```text
Gameplay
  ├ Move
  ├ Attack
  ├ Interact
  ├ Dodge
  └ Pause

UI
  ├ Navigate
  ├ Submit
  ├ Cancel
  └ Pause
```

`PlayerInputReader` は `Gameplay` / `UI` / `Disabled` を排他的に切り替えます。Pauseや将来の会話・メニューでは、個別のゲームプレイスクリプトを無効化するのではなくInput Contextを切り替える方針です。

## 設定データ

ゲームバランス値とプロトタイプ起動設定はScriptableObjectへ分離しています。

```text
Assets/Resources/Settings/
  PrototypeProjectAssets.asset
  PrototypeApplicationSettings.asset
  Gameplay/
    PlayerCharacter.asset
    PlayerCharacterStats.asset
    PlayerMeleeAttack.asset
    PlayerDodge.asset
    PlayerExperienceTable.asset
    TrainingDummyReward.asset
```

主な責務は次のとおりです。

- `PrototypeProjectAssets`: Prefab、Sprite、Font、各設定アセットへの参照
- `PrototypeApplicationSettings`: Spawn位置、フィールド範囲、Pause時TimeScale
- `CharacterDefinition`: 安定Character ID、Prefab、基礎能力値、通常攻撃、回避、経験値テーブルの集約
- `CharacterStatsDefinition`: 移動速度、最大HP
- `MeleeAttackDefinition`: Ability ID、ダメージ属性、ダメージ、攻撃半径、攻撃距離
- `DodgeDefinition`: 回避速度、継続時間、クールダウン
- `ExperienceTableDefinition`: レベルごとの累積必要経験値と最大レベル時の余剰経験値方針
- `RewardDefinition`: 安定Reward IDと付与経験値

プレイ中に変化する成長状態はUnity非依存の `CharacterProgressionState`、保存形式はバージョン付き `GameSaveData` / `PlayerSaveData` へ分離しています。保存先の具体実装は `ISaveService` を通して後から追加します。

## UIと日本語フォント

本番UI基盤はCanvas（uGUI）です。OSにインストールされたフォントへ依存せず、プロジェクト管理のFontアセットを利用します。

標準フォントはDotGothic16です。未導入時はEditorツールから導入できます。

```text
Demon King > Project > Install Japanese UI Font
```

フォント参照は `PrototypeProjectAssets` から `PrototypeUiInstaller` を経由して各Viewへ渡します。

## テスト

成長状態と保存DTOはUnity非依存の `DemonKing.Domain.asmdef`、Unity上のRuntimeコードは `DemonKing.Runtime.asmdef` にまとめ、EditMode / PlayModeテストを独立assemblyで管理します。

現在の主な自動テスト対象は次です。

- Y座標による描画順計算
- Healthの致死ダメージと死亡イベント
- DamageRequest / DamageResult / DefeatContextのCombat結果
- 成長状態と保存DTOの相互変換
- CharacterDefinitionの必須アセット参照
- 経験値テーブルの境界、複数レベルアップ、最大レベル処理
- 訓練用ダミー撃破からRewardServiceを経由した経験値付与と重複防止
- CameraFollow2Dの追従とZ座標維持
- Gameplay / UI / Disabled入力コンテキスト切り替え
- Dodge開始時のRigidbody2D移動
- Pause / Resume時のTimeScaleとInput Context切り替え

## 開発フェーズ

P0〜P2に加え、成長システム実装前のDefinition、Runtime State、Save DTO、Combat境界整備と、経験値／撃破報酬の最小経路が完了しています。

次は既存の通常攻撃をAbility実行基盤へ移し、スキル、進化、NPC・会話・敵AI・クエストを段階的に追加します。Addressablesや大規模なシーン分割は、コンテンツ量とロード時間が必要性を示した段階で導入します。

## リリース

`main` へのマージ時にsemantic-releaseを実行し、Conventional Commitsから `vX.Y.Z` タグとGitHub Releaseを自動生成します。npm公開やUnity Playerの自動ビルドは行いません。

- `fix:` / `perf:`: Patch Release
- `feat:`: Minor Release
- `BREAKING CHANGE:`: Major Release
- `docs:` / `test:` / `chore:` / `ci:` / `refactor:`: リリースなし

Squash merge時はPull RequestタイトルをConventional Commits形式にします。詳しい運用と初回リリースの扱いは `docs/RELEASE.md` を参照してください。

## ドキュメント

各ドキュメントの役割を分けています。

- `README.md`: 現在のプロジェクト概要、操作、主要な技術基盤
- `docs/GAME_DIRECTION.md`: ゲーム体験、ビジュアル、物語、コンテンツ開発の方針
- `docs/TECHNICAL_DESIGN.md`: 現在の技術設計、実装規約、入力・物理・UI・データ管理の基準
- `docs/ARCHITECTURE.md`: 依存方向、構成責務、完了したP0〜P2の整備履歴と今後の優先順位
- `docs/RELEASE.md`: semantic-releaseのバージョン判定、自動公開、初回リリース、障害対応

実装とドキュメントが食い違う場合は、まず現在のコードとUnityアセットを確認し、その後ドキュメントを更新してください。
