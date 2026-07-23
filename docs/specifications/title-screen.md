# Title Screen仕様

## 目的

ゲーム起動時にGameplay Runtimeを即座に構築せず、New Game / Continue / Load Gameから開始方法とSave Slotを明示的に決定します。

Title ScreenはApplication層の開始導線です。Gameplay Runtime、Save DTO、Field RuntimeへTitle固有の状態を持ち込みません。

## 起動フロー

```text
Prototype.unity
  -> FieldBootstrap
  -> PrototypeTitleScreenInstaller
  -> PrototypeTitleScreenController
     |- New Game
     |- Continue
     `- Load Game
  -> SaveSlotIdを選択
  -> LocalSaveSlotStore
  -> selected ISaveService
  -> PrototypeApplicationInstaller
  -> PrototypeGameSession
```

Game Sessionの構築経路は開始方法にかかわらず `PrototypeApplicationInstaller` へ一本化します。Title Screen自身はPlayer、Quest、Ability、FieldなどのRuntime Stateを構築しません。

## Main Menu

Main Menuには次を表示します。

1. `NEW GAME`
2. `CONTINUE`
3. `LOAD GAME`
4. `SETTINGS`
5. `QUIT`

`CONTINUE` は `Ready` なSave Slotが1件もない場合は無効表示とし、決定してもGameplayを開始しません。

`SETTINGS` はP0では導線のみを表示します。設定項目の実装は対象外です。

`QUIT` はApplication終了を要求します。

## New Game

New Gameでは3つのSave Slotから `Empty` なSlotを選択します。

- 既存Saveが存在するSlotはP0では上書きしません。
- 空きSlotがない場合はエラーを表示し、Gameplayを開始しません。
- 選択後は `FreshGameSaveService` を通して既存SaveのRestoreを必ず抑止します。
- 最初のSave以降は選択したSlotへ通常どおり保存します。

UI上の空Slot判定だけに依存せず、New GameのLoad境界自体が `TryLoad = false` を保証することで、別Slotや以前のRuntime Stateが混入しないようにします。

## Continue

Continueでは `Ready` なSlotのうち `LastSavedUtc` が最も新しいSaveを開始します。

同一日時の場合はSlot番号が小さい方を優先します。対象がない場合はGameplayを開始しません。

## Load Game

Load Gameでは3 SlotのMetadataを一覧表示し、選択した `Ready` Slotを開始します。

表示対象は次です。

```text
Slot ID
Status
Level
Current Field
Play Time
Last Saved
```

Slot状態ごとの扱い:

- `Empty`: 選択開始不可
- `Ready`: 選択開始可能
- `Corrupted`: エラー表示し、既存ファイルを変更しない
- `UnsupportedVersion`: Save Versionを含むエラーを表示し、既存ファイルを変更しない

破損・未対応Saveを選択しただけではSave処理を開始しません。

## Input

Title Screenは既存 `PlayerInputReader` の `UI` Action Mapを使用します。

- Navigate: 項目・Slot選択
- Submit: 決定
- Cancel: Slot一覧からMain Menuへ戻る

Keyboard / Gamepadの具体的なBindingは [入力仕様](./input.md) をSource of Truthとします。Title専用のInput Actionは追加しません。

## UI実装

P0では既存Prototype Scene上にScreen Space OverlayのuGUIを構築し、Title Screenを表示します。Gameplay Runtimeは開始選択後まで構築しません。

Title Viewは表示だけを担当します。Slot選択、Continue対象の決定、開始可否、エラー判断は `PrototypeTitleScreenController` が担当します。

## Saveとの境界

Save SlotとMetadataの詳細は [セーブ仕様](./save.md) を参照してください。

Title Screenは `SaveSlotId` を選択し、`LocalSaveSlotStore` から解決済み `ISaveService` を取得してApplication Installerへ渡します。`GameSaveData` とGameplay FeatureはSlot数やTitle Screenを知りません。

## テスト

PlayModeでは少なくとも次を検証します。

- New Gameが空Slotを選択しFresh Startを要求する
- Continueが最新の有効Saveを選択する
- Load Gameが破損Slotを開始しない
- Title中はUI Input Contextを使用し、開始時にTitle側入力を停止する

EditModeでは `FreshGameSaveService` が既存SaveをLoadせず、Saveだけを選択済みStorageへ委譲することを検証します。
