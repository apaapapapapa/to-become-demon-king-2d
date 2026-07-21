---
runtimeSource: Assets/Resources/Settings/Gameplay/ManaFlowSkill.asset
relatedContentIds:
  - art.magic.arcane_bolt
---

<RuntimeContentHeader />

## 開発者向け参照

具体的な補正方式とRuntimeルールは [Skill仕様](../../specifications/skill.md) を参照してください。魔力循環は特定Abilityを直接所有せず、既存の汎用Modifier境界からAbility Cooldownへ常時補正を適用します。

Prototypeではフィールド上の「魔力結晶」へInteractionすることで、共通 `ProgressionGrantDefinition` / `ProgressionAcquisitionService` 境界から取得します。

## 関連コンテンツ

<ContentRelations />
