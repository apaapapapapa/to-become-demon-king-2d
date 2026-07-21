---
runtimeSource: Assets/Resources/Settings/Gameplay/ArcaneBoltArt.asset
relatedContentIds:
  - skill.magic.mana_flow
---

<RuntimeContentHeader />

## 開発者向け参照

解放Ability、熟練ランク、Mastery Point等のRuntimeデータは上記Runtime Sourceを正とします。Artの習得・熟練・Ability解放ルールは [Art仕様](../../specifications/art.md)、具体的なInput Bindingは [入力仕様](../../specifications/input.md) を参照してください。

Prototypeではフィールド上の「古びた魔導書」へInteractionすることで、共通 `ProgressionGrantDefinition` / `ProgressionAcquisitionService` 境界から習得します。

## 関連コンテンツ

<ContentRelations />
