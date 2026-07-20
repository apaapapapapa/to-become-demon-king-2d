# アーキテクチャ

## 目的

この文書は、レイヤー責務、依存方向、Composition、状態モデル、Platform境界、意図的な移行境界を定義します。

個別Feature間の接続規則は [Feature間の責務境界](./feature-boundaries.md)、Unity固有の実装方式は [技術設計](./technical-design.md) を参照してください。

## 基本原則

- Domain、Core、Gameplay、Presentation、Compositionを分離する。
- プレイ中に変化する状態とScriptableObject Definitionを分離する。
- Runtime StateとSave DTOを分離する。
- Platform固有処理をGameplayへ直接持ち込まない。
- UI表示とゲーム状態管理を分離する。
- Bootstrapを肥大化させない。
- 必要性が確認できるまで過剰な抽象化を導入しない。

## レイヤー

### Domain

`DemonKing.Domain` はUnity非依存の純C#領域です。状態、値、保存DTOなど、Unity Objectを必要としないルールを置きます。

`UnityEngine`、Scene、`GameObject`、`MonoBehaviour`、`ScriptableObject`をDomainへ持ち込みません。

### Core

アプリケーション基盤とFeature横断の共通処理を置きます。保存抽象、入力抽象、共通Math等を含みます。

### Gameplay

Unity上で動くゲームルールとキャラクター挙動を置きます。

GameplayはDomain / Coreを利用できますが、Prototype固有クラスや具体的なuGUI Viewへ依存しません。

Unity Objectを参照する `DamageRequest` / `DamageResult` / `DefeatContext` のような型はDomainへ移さず、Gameplay側のUnity依存境界として扱います。

### Presentation

カメラ、描画順、アニメーション、uGUI Viewを置きます。Presentationはゲームルールの決定主体になりません。

### Field / Prototype

Prototypeシーンを組み立てるComposition層です。具体クラスとUnityアセットを接続しますが、恒久的なDomain / Gameplayルールを蓄積しません。

## 依存方向

Compositionが具体実装を組み合わせ、内側のレイヤーが外側の具体実装を知らない方向を維持します。

```text
Field / Prototype (Composition)
  ↓
Presentation / Gameplay
  ↓
Core / Domain
```

Gameplay Feature同士の接続が必要な場合は、共通契約、イベント境界、またはCompositionを使用します。具体的な接続方向は [Feature間の責務境界](./feature-boundaries.md) を参照してください。

## Definition / Runtime State / Save DTO

```text
Definition
  ↓ 構築・参照
Runtime State
  ↓ Mapper
Save DTO
```

- Definition: ScriptableObjectによる静的コンテンツ定義、バランス値、Asset参照。
- Runtime State: プレイ中に変化する状態。
- Save DTO: Runtime Stateと分離した保存形式。

DefinitionをRuntime Stateや保存状態として書き換えません。Save形式の詳細は [セーブ仕様](../specifications/save.md) を参照してください。

## Composition Root

起動入口は薄く保ち、具体的な構築順序とFeature間配線をInstaller / Builder / Coordinatorへ委譲します。

現在のUnity起動フローは [技術設計](./technical-design.md) を参照してください。

## Platform境界

Steamや将来のコンソールSDKをGameplayから直接呼び出しません。

保存先、実績、クラウド、ユーザー識別等は抽象境界の外側へ置きます。保存処理の境界は `ISaveService` を使用します。

## 意図的に残している移行境界

- `Field/Prototype`: Prototype Composition領域
- `SlimeController`: 既存Prefab互換の薄いマーカー
- `RuntimeShapeFactory`: Prototype専用補助表現
- `Resources`: 少数の起動入口・互換用途
- `PrototypeProjectAssetsAutoRepair`: Editor上の参照修復ツール

## リアーキテクチャ判断基準

- 同じ変更理由で複数箇所を毎回修正している。
- Platform固有コードがGameplayへ漏れ始めた。
- Resourcesや単一Sceneがコンテンツ量に耐えられない。
- テスト困難性が責務分離不足を示している。
- ScriptableObjectだけでは大量データの整合性管理が難しい。
- 複数Featureが同じRuntime Stateを別々に管理し始めた。

現在の実装状況と開発優先度は [ロードマップ](../development/roadmap.md) を参照してください。
