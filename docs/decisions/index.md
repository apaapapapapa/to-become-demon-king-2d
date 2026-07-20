# Architecture Decision Records

長期的に影響する設計判断の理由と履歴をADRとして記録します。現在の設計は `docs/design/`、ADRの命名・配置規則は [ドキュメント規約](../development/documentation-rules.md) を参照してください。

## 一覧

| ADR | Status | 内容 |
| --- | --- | --- |
| [ADR-0001](./ADR-0001-monorepo-knowledge-base.md) | Accepted | Unity実装とKnowledge Baseを同一リポジトリで管理する |
| [ADR-0002](./ADR-0002-ability-art-skill-boundaries.md) | Accepted | Ability・Art・Skillの責務を分離する |
| [ADR-0003](./ADR-0003-evolution-nodes-and-exclusive-paths.md) | Accepted | Evolutionを不可逆・排他的なNode選択として管理する |
