# Field Transition仕様

## 目的

複数FieldをStable IDで遷移できるようにし、Sceneの寿命とGame Sessionの進行状態を分離します。

Field遷移ではScene名やBuild IndexをGameplayへ公開せず、要求・Save・復元は常に `FieldLocation(fieldId, entryPointId)` を使用します。

## Field構成

P0では次の3 FieldをCatalogへ登録します。

```text
Prologue Field（New Game初期Field）
  fieldId: field.prologue.deep_forest
  scene: Prototype
  default entry: entry.prologue.birth
  display name: 深森のねぐら
  Part 1時点ではField出口なし

Training Field（既存Save互換Field）
  fieldId: field.prototype.training_ground
  scene: Prototype
  default entry: entry.default
  return entry: entry.from_forest_gate
  exit -> field.prototype.forest_gate:entry.from_training_ground

Forest Gate Field
  fieldId: field.prototype.forest_gate
  scene: PrototypeForestGate
  entry: entry.from_training_ground
  exit -> field.prototype.training_ground:entry.from_forest_gate
```

`PrototypeFieldCatalog` のInitial Fieldは `field.prologue.deep_forest:entry.prologue.birth` です。New GameはこのLocationから開始します。既存Saveが保持する `field.prototype.training_ground` / `field.prototype.forest_gate` は引き続き同じStable IDで解決します。

Prologue Fieldは育ての親、食料探索、森の幼獣、Story進行を持ちます。Training Fieldは訓練NPC / 訓練スライム / Progression Pickupを持ち、Forest Gate Fieldはそれらを持ちません。いずれも同じ `PrototypeFieldComposer` / Installer列を利用し、Field追加のためにWorld Builderをコピーしません。

PrototypeのWorldはRuntime Composition型です。Field Sceneは `SceneManager.CreateScene` で独立Sceneとして生成し、Scene Asset / Build IndexをSaveやField CatalogのSource of Truthにはしません。Prologue FieldとTraining Fieldは同じScene名 `Prototype` を使いますが、Stable Field IDとField Definitionは別に保持します。

## 遷移要求

Field出口は `PrototypeFieldTransitionInteractable` として `IInteractable` を実装し、Application境界へ次だけを要求します。

```text
FieldLocation destination
```

出口自身は次を知りません。

- Scene Load方式
- Save Slot
- Game Session State
- Player再生成方法
- UI再構築方法

`PrototypeFieldTransitionService` がStable Field LocationをCatalogで検証してからScene切替を開始します。

## 寿命境界

### Game Sessionより長く保持する状態

Field Sceneを跨いで同一インスタンスまたは同一Runtime Stateを保持します。

- `CharacterProgressionState`
  - Level / Experience
  - Art進捗
  - Skill取得
  - Evolution取得
- `QuestProgressionService`
- `StoryProgressState`
- `StoryProgressionService`
- `GameplayEventHub`
- `ProgressionGrantConsumptionState`
- `DialogueLog`
- `PrototypeLocalSaveCoordinator`
- 選択済み `ISaveService`

### Fieldごとに再構築する状態

Sceneに属するRuntime参照はField切替ごとに破棄・再構築します。

- World Root
- Player GameObject
- Player上のUnity Component
- Field固有NPC / Enemy / Pickup / Scenario Controller
- Field固有Reward Service
- Camera Follow接続
- Field UI Root

Ability LoadoutはPlayer ComponentがScene寿命のため、遷移前に `AbilityLoadoutSaveData` へ一時Snapshotし、新Playerの `AbilityLoadoutController` へ再適用します。

## 遷移順序

```text
Field Exit Interaction
  -> PrototypeFieldTransitionService
  -> destination FieldLocationをCatalogで検証
  -> Player Input停止
  -> PrototypeGameSession.PrepareForFieldTransition
     |- Ability Loadoutを退避
     `- 現在FieldをSave
  -> destination SceneをActive化
  -> PrototypeGameSession.EnterField
     |- Field Definition / Entry Point解決
     |- World / Player再構築
     |- Session Progression / Quest / Story / Grant Stateを再接続
     |- Ability Loadout再適用
     `- Save Snapshot Providerを新Player / Field Locationへ再Bind
  -> PrototypeApplicationFieldBinder
     |- Pause / Modal / Evolutionを新Player Inputへ再Bind
     |- Loadout Selectionを新Playerへ再Bind
     `- HUD / Modal UIを新Field Sceneへ再生成
  -> 旧Field SceneをUnload
  -> Player Input再開
```

同一遷移の多重実行は `IsTransitioning` で拒否します。Modal UIが開いている間も遷移を開始しません。

## Save / Continue

World LocationはSave Version 4で導入され、現行Save Version 5でも同じ形式を維持します。

```text
world
  currentFieldId
  entryPointId
```

遷移前に旧Fieldを一度保存し、遷移後のRuntime再Bind完了時に新Field Locationを即時保存します。

Continue / Load Gameでは `PrototypeSaveSession` が保存済み `FieldLocation` を読み、`PrototypeFieldCatalog` でDefinitionとEntry Pointを解決します。

- `field.prologue.deep_forest` は `Prototype` Sceneの `entry.prologue.birth` から復元します。
- `field.prototype.training_ground` は既存Training Fieldとして復元します。
- `field.prototype.forest_gate` は `PrototypeForestGate` Sceneの `entry.from_training_ground` から復元します。

未知のField ID / Entry Point IDはInitial Fieldである `field.prologue.deep_forest` のDefault Entry Point `entry.prologue.birth` へFallbackします。

## Scene境界

`PrototypeFieldSceneRuntime` はActive Field Scene内だけからCamera / Grid等を解決します。遷移中に旧Sceneと新Sceneが一時的に同時Loadされても、旧FieldのGridやCameraを新Field Compositionが誤利用しないようにします。

Application Rootは `DontDestroyOnLoad` とし、Scene切替では破棄しません。

## テスト

EditModeでは次を検証します。

- 3 FieldをStable Field / Entry Point IDで解決できる
- Prologue FieldがInitial Fieldである
- Training Field / Forest Gate Fieldの出口が相互のEntry Pointを指す
- 既存Training / Forest GateのStable IDを引き続き解決できる
- Scene依存Playerを外している間もSave SnapshotがLoadoutを保持する
- 新PlayerへBind後は新Field LocationをSaveする

PlayModeでは次を検証します。

- New GameがPrologue FieldのBirth Entry Pointから開始する
- Training Field -> Forest Gate Field -> Training Fieldの往復
- Level / Art / Skill / Evolution / Loadout / Quest / StoryがField遷移後も維持される
- Field遷移後のSaveが新しいField / Entry Pointを持つ
- 保存済み各Fieldから起動した場合に対応するEntry PointへPlayerが復元される
