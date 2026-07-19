# 技術設計

## この文書の役割

この文書は、現在の実装を基準にした技術設計と開発規約を定義します。

- ゲームとして何を目指すか: `GAME_DIRECTION.md`
- 依存方向、構成責務、P0〜P2の整備履歴: `ARCHITECTURE.md`
- 現在の技術仕様と実装上の判断基準: 本書

過去の初期案ではなく、現在の `main` の実装を基準に記述します。

## 基準環境

現在のプロジェクト基準は次のとおりです。

- Unity Editor: `6000.5.4f1`
- C#
- Universal Render Pipeline（URP）
- Unity Input System
- 2D Tilemap
- 2D Animation関連パッケージ
- Canvas（uGUI）
- Unity Test Framework

Unity非依存の成長状態と保存DTOは `DemonKing.Domain.asmdef`、Unity上のRuntimeコードは `DemonKing.Runtime.asmdef` にまとめています。

## アーキテクチャ原則

1. ゲームプレイコードをSteamやコンソール固有APIへ直接依存させない。
2. 入力はデバイス固有キーではなくInput Actionとして扱う。
3. Gameplay / UI / Disabledの入力コンテキストを明示的に切り替える。
4. 通常移動、Dodge、Combat、Interaction、Pause、UI表示の責務を分離する。
5. ゲームバランス値はPrefabやMonoBehaviourへ重複保持せずScriptableObjectを正とする。
6. UI表示ロジックとゲーム状態管理を分離する。
7. 描画順計算を各オブジェクトへ重複実装しない。
8. Bootstrapは設定値やゲームロジックを持たず、Composition Rootとして起動処理を委譲する。
9. 必要になる前に大規模DIコンテナや独自フレームワークを導入しない。
10. 大規模化の必要性が確認できるまでは、Runtime assemblyを過剰に分割しない。

## 現在の主要フォルダー構成

```text
Assets/
  Art/
    External/
    World/
  Fonts/
  Resources/
    Input/
      PlayerControls.inputactions
    Prefabs/
      Characters/
      World/
    Settings/
      PrototypeProjectAssets.asset
      PrototypeApplicationSettings.asset
      Gameplay/
        PlayerCharacter.asset
        PlayerCharacterStats.asset
        PlayerMeleeAttack.asset
        PlayerDodge.asset
  Scenes/
    Prototype/
      Prototype.unity
  Scripts/
    Domain/
      Progression/
      Save/
      DemonKing.Domain.asmdef
    Core/
      Application/
      Input/
      Math/
    Gameplay/
      Characters/
        Configuration/
      Combat/
        Configuration/
      Interaction/
    Presentation/
      Camera/
      Characters/
      Rendering/
      UI/
    Field/
      Prototype/
        Configuration/
    World/
    DemonKing.Runtime.asmdef
    FieldBootstrap.cs
    SlimeController.cs
  Tests/
    EditMode/
    PlayMode/
  Editor/
```

`Field/Prototype` は現在の試作シーンを組み立てる外側のComposition層です。恒久的なGameplayルールは `Gameplay`、入力やアプリケーション状態など汎用的な基盤は `Core`、表示は `Presentation` に置きます。

## 起動フロー

現在の起動フローは次のとおりです。

```text
Prototype.unity
  ↓
FieldBootstrap
  ↓ Resources.Load
PrototypeProjectAssets
  ↓
PrototypeApplicationInstaller
  ├ PrototypeApplicationSettings
  ├ PrototypeSceneConfigurator
  ├ PrototypeSortingConfigurator
  ├ PrototypeWorldBuilder
  │   ├ TerrainBuilder
  │   ├ CollisionMapBuilder
  │   ├ PrototypeWorldPrefabFactory
  │   ├ ArchitectureBuilder
  │   ├ NatureBuilder
  │   ├ AtmosphereBuilder
  │   ├ PrototypeGameplayFeatureInstaller
  │   ├ PrototypePlayerSpawner
  │   └ PrototypeCameraInstaller
  ├ GamePauseController
  └ PrototypeUiInstaller
```

`FieldBootstrap` は `PrototypeProjectAssets` を解決して `PrototypeApplicationInstaller` へ委譲するだけの最小エントリーポイントです。

Spawn位置、フィールド範囲、Pause時TimeScaleなどの値は `PrototypeApplicationSettings` に保持します。

## シーンとTilemap

正規の試作シーンは次です。

```text
Assets/Scenes/Prototype/Prototype.unity
```

GridはIsometricレイアウトを使用し、基本レイヤーを次のように分離します。

```text
Grid
  ├ Ground
  ├ Collision
  ├ Props
  └ Foreground
```

- `Ground`: 地面表示
- `Collision`: 物理衝突用。通常は非表示
- `Props`: ワールド中の小物や必要な表示要素
- `Foreground`: プレイヤーより手前へ被せる前景

表示データと衝突データは分離します。

Editorメニュー `Demon King > Prototype > Create Isometric Scene` は試作シーンの基礎構造を再生成するためのツールです。既存の `Prototype.unity` を作り直す用途があるため、シーンを手作業で編集した後に実行する場合は差分を確認してください。

## Tileアセット

Ground表示はインポート済みSpriteを正とします。

```text
Imported Sprite
  ↓
PrototypeProjectAssets
  ↓
PrototypeRuntimeTileFactory
  ↓
Unity Tile
  ↓
Tilemap
```

`PrototypeRuntimeTileFactory` はTexture2DやSpriteそのものを生成しません。実行時に必要な `Tile` オブジェクトだけを生成します。

Collision用Tileは表示Spriteを持たず、Grid colliderとして使用します。

## RuntimeShapeFactoryの扱い

`RuntimeShapeFactory` は現在も一部で使用しますが、主要アートの代替ではありません。

用途は次に限定します。

- プロトタイプ専用の軽量な装飾
- 柵、ランドマークなど未アセット化の補助表示
- 雰囲気確認用の簡易演出

地形、プレイヤー、校舎、木、街灯など、継続的に利用する主要要素はプロジェクト管理アセットやPrefabを正とします。

コンテンツ制作が進んだ要素から、RuntimeShapeFactory利用箇所を静的アセットまたはPrefabへ置き換えます。

## 描画順

アイソメトリック空間の動的オブジェクトは、Y座標を基準に描画順を決定します。

基本計算は `WorldSortOrder` に集約します。

```text
sortingOrder = -round(worldY * precision) + offset
```

複数SpriteRendererを持つキャラクターなどは `GroupYSorter` を使用し、子Sprite間の相対的な順序を保ったままグループ全体をYソートします。

原則としてYソート対象は `World` Sorting Layerを使用します。

オブジェクトごとに独自のYソート式を実装しません。

## 入力

Input Actionsは `Assets/Resources/Input/PlayerControls.inputactions` で管理します。

### Gameplay Action Map

```text
Gameplay
  Move       Vector2
  Attack     Button
  Interact   Button
  Dodge      Button
  Pause      Button
```

現在の主なバインド:

```text
Move
  Keyboard: WASD / Arrow Keys
  Gamepad: Left Stick

Attack
  Keyboard: J
  Gamepad: Button West

Interact
  Keyboard: E
  Gamepad: Button South

Dodge
  Keyboard: Left Shift
  Gamepad: Button East

Pause
  Keyboard: Escape
  Gamepad: Start
```

### UI Action Map

```text
UI
  Navigate   Vector2
  Submit     Button
  Cancel     Button
  Pause      Button
```

現在の主なバインド:

```text
Navigate
  Keyboard: WASD / Arrow Keys
  Gamepad: Left Stick / D-Pad

Submit
  Keyboard: Enter / Space
  Gamepad: Button South

Cancel
  Keyboard: Escape
  Gamepad: Button East

Pause
  Gamepad: Start
```

`PlayerInputReader` がAction Assetの実行時インスタンスを所有し、次のコンテキストを排他的に切り替えます。

```text
Gameplay
UI
Disabled
```

ゲームプレイスクリプトで `Keyboard.current` や特定キーを直接参照しません。

## プレイヤー通常移動

通常移動は `CharacterMotor2D` が担当します。

```text
PlayerInputReader
  ↓
MoveInputReader
  ↓
CharacterMotor2D
  ↓
Rigidbody2D.MovePosition
```

- 入力取得はUpdate
- 物理移動はFixedUpdate
- 入力Vector2はInput側で最大長1へ制限
- 重力は使用しない
- 回転は固定
- 移動速度は `CharacterStatsDefinition` から取得

通常移動を一時停止する必要があるDodgeなどは、`CharacterMotor2D.SetMovementLocked` を利用します。

## Dodge

Dodgeは `CharacterDodge2D` が担当します。

```text
PlayerInputReader.DodgePressed
  ↓
CharacterDodge2D
  ├ DodgeDefinition
  ├ CharacterMotor2D.SetMovementLocked(true)
  └ Rigidbody2D.MovePosition
```

- 入力中は入力方向へ回避
- 無入力時は最後に移動した方向へ回避
- Dodge中は通常移動を一時ロック
- 回避終了後に通常移動を復帰
- クールダウン中は再Dodge不可

速度、継続時間、クールダウンは `PlayerDodge.asset` を正とします。

## Interaction

Interactionの恒久的な契約は `Gameplay/Interaction` に置きます。

```text
PlayerInputReader.InteractPressed
  ↓
PlayerInteractor
  ↓
IInteractable
  ↓
NPC / Door / Chest / Inspectable
```

`PlayerInteractor` は具体的なNPCやオブジェクト種別へ依存しません。

現在のPrototype NPCは、この契約を確認するための試作実装です。

## Combat

Combatの基本経路は次です。

```text
PlayerInputReader.AttackPressed
  ↓
PlayerMeleeAttack
  ↓
MeleeAttackDefinition
  ↓
Overlap判定
  ↓
IDamageable
  ↓
Health
  ↓
DamageResult / DefeatContext
  ↓
RewardService
  ↓
CharacterProgressionState.GainExperience
```

- `DamageRequest`: 要求ダメージ、攻撃者、Character ID、Ability ID、属性、タグ
- `Health`: 現在HP、生存状態、ダメージ適用、型付き結果／撃破イベント
- `DamageResult`: 実適用量、残りHP、撃破結果
- `DefeatContext`: 一意なDefeat ID、攻撃者、撃破対象、Ability ID、報酬Definition ID
- `MeleeAttackDefinition`: Ability ID、ダメージ、属性、攻撃半径、攻撃距離
- `PlayerMeleeAttack`: 入力と近接攻撃判定の接続
- `RewardService`: 撃破結果、攻撃者ID、報酬ID、重複付与を検証して経験値を反映

敵AI、報酬付与、死亡演出は `Health` に入れません。現在の訓練用ダミーは `DefeatContext` を通知し、`RewardService` がプレイヤーの `CharacterProgressionState` へ経験値を反映します。

## Pause

Pause状態は `GamePauseController` が管理します。

Pause時:

```text
Time.timeScale = pausedTimeScale
PlayerInputContext = UI
PauseStateChanged(true)
```

Resume時:

```text
Time.timeScale = Pause前の値
PlayerInputContext = Gameplay
PauseStateChanged(false)
```

`PauseMenuView` はPause状態の表示だけを担当し、TimeScaleやInput Contextを直接変更しません。

## UI

本番UI基盤はCanvas（uGUI）です。

現在のルート:

```text
UI Root
  ├ Canvas
  ├ CanvasScaler
  ├ GraphicRaycaster
  ├ GameHudView
  └ PauseMenuView
```

UIはプレイヤーPrefabのライフサイクルから分離します。

日本語フォントは `PrototypeProjectAssets.UiFont` から注入します。OSフォントへの依存は避けます。

標準フォントの導入ツール:

```text
Demon King > Project > Install Japanese UI Font
```

## ScriptableObjectによる設定管理

現在の設定アセットは次です。

```text
PrototypeProjectAssets.asset
  -> アセット参照の集約

PlayerCharacter.asset
  -> characterId
  -> Player Prefab
  -> Stats / Melee Attack / Dodge / Experience Table Definition

PrototypeApplicationSettings.asset
  -> playerSpawnPosition
  -> playableTileRadius
  -> pausedTimeScale

PlayerCharacterStats.asset
  -> moveSpeed
  -> maxHealth

PlayerMeleeAttack.asset
  -> abilityId
  -> damage
  -> damageType
  -> attackRadius
  -> attackDistance

PlayerDodge.asset
  -> dodgeSpeed
  -> duration
  -> cooldown

PlayerExperienceTable.asset
  -> レベルごとの累積必要経験値
  -> 最大レベル時の余剰経験値保持方針

TrainingDummyReward.asset
  -> rewardId
  -> experience
```

MonoBehaviourやPrefabへ同じバランス値を重複保持しない方針です。

プレイ中に変化するレベル、現在経験値、解放済みスキル／進化ノードはScriptableObjectへ保存せず、`CharacterProgressionState` が保持します。経験値加算時は `ExperienceTable` からレベルを再計算し、`LevelUpResult` で加算前後と複数レベル上昇を通知します。永続化時は `CharacterProgressionSaveMapper` を通して `PlayerSaveData` へ変換します。

保存データの現在バージョンは `GameSaveData.CurrentVersion` を正とします。現段階の `ISaveService` は保存先を分離する契約のみで、ファイルやクラウドへの具体保存はまだ実装しません。

新しい設定値をScriptableObjectへ移すかどうかは、次を目安にします。

- 複数Prefabやシーンから共有する
- デザイナーがコード変更なしで調整する
- キャラクターや装備ごとに差し替える
- セーブデータとは別の静的定義データである

## アセット参照とResources

`Resources.Load` は無制限に使用しません。

現在の主な利用は次です。

- `FieldBootstrap` が `PrototypeProjectAssets` を解決する入口
- `PlayerInputReader` のInput Action Asset互換フォールバック
- uGUI組み込みフォントの最終フォールバック

Prefab、Sprite、Gameplay設定などは `PrototypeProjectAssets` のUnityシリアライズ参照を経由します。

将来Addressablesへ移行する場合も、Gameplay側から直接Addressables APIを呼ばず、アセット供給側の境界を置き換える方針です。

## カメラ

`CameraFollow2D` は任意の `Transform` を追従対象として受け取ります。

プレイヤー固有クラスへ依存しません。

現在は正投影カメラを使用し、試作中はカメラ回転を前提にしません。

Cinemachineは、カメラ演出や複数ターゲットなど明確な必要性が出た段階で検討します。

## ライティング

URPを導入済みで、2D Lightingを利用できる構成です。

ただし現在のPrototype Scene Builderには簡易なDirectional Light placeholderも残っており、最終的な2D Lighting構成は確定していません。

本番ライティングを整備する際は、次を基準にします。

- 視認性を損なわない
- 多数の高負荷ライトを前提にしない
- Steam向けPCだけでなく将来のコンソール性能も考慮する
- 実機計測前に過剰な最適化を行わない

## テスト

現在のテストassembly:

```text
DemonKing.EditMode.Tests
DemonKing.PlayMode.Tests
```

主なテスト:

```text
EditMode
  WorldSortOrderTests
  ProgressionBoundaryTests

PlayMode
  GameplayAndCameraPlayModeTests
  PlayerInputContextPlayModeTests
  DodgeAndPausePlayModeTests
```

テストで優先する対象は、表示の細部よりも変更時に壊れやすいルールと境界です。

今後の拡張候補:

- Interaction対象選択
- CombatのOverlap判定
- Pause中のGameplay入力抑止
- NPC会話状態とInput Context切り替え
- セーブ／ロード境界
- 進化グラフ検証

## Platform移植性

次の機能を追加するときは、ゲームプレイコードから直接プラットフォームSDKを呼びません。

- セーブデータ保存
- 実績
- クラウドセーブ
- プラットフォームユーザー識別
- DLC／権利確認

将来の境界例:

```text
Gameplay / Application
  ↓
ISaveService
IAchievementService
IPlatformUserService
  ↓
Platform Implementation
  ├ Steam
  └ Console
```

実際に機能が必要になるまでは空のPlatform抽象化を増やしません。

## パフォーマンス方針

将来のコンソール移植を考慮し、次を守ります。

- 透明Spriteの過剰な重なりを避ける
- 2D Lightを無制限に増やさない
- アセット数が増えた段階でSprite Atlasを検討する
- 大規模マップを一括ロードする前提にしない
- 高性能な開発PCだけで評価しない
- 最適化はProfilerと実機計測に基づいて行う

Addressables、シーンストリーミング、細分化したasmdefは、規模と計測結果が必要性を示した段階で導入します。

## 現在の技術マイルストーン

P0〜P2で予定していた基礎アーキテクチャ整備は完了しています。

現在確認できる技術基盤:

1. 正規Prototypeシーン
2. Isometric TilemapとCollision Tilemap
3. Rigidbody2Dによる移動
4. 共通Yソート
5. Gameplay / UI / Disabled入力コンテキスト
6. Interaction契約
7. Combat / Health契約
8. Dodge
9. Pause状態管理
10. uGUI HUD / Pause Menu
11. ScriptableObject設定管理
12. EditMode / PlayModeテスト基盤
13. Unity非依存の `DemonKing.Domain`
14. `CharacterDefinition` と実行時成長状態の分離
15. バージョン付きSave DTOと `ISaveService` 契約
16. `DamageRequest` / `DamageResult` / `DefeatContext` によるCombat境界
17. 累積経験値テーブルと `LevelUpResult`
18. `RewardService` による訓練用ダミー撃破から経験値付与までの経路

次の段階では、既存の通常攻撃をAbility実行基盤へ移し、スキルと進化を段階的に追加します。
