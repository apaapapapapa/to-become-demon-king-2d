# Art一覧

- Status: System Implemented / Planned Content Registered

Artは、攻撃魔法や特殊剣攻撃など、キャラクターが習得して熟練する能動技能です。1つのArtは1つ以上のAbilityを持ち、熟練ランクに応じて段階的に解放します。

各Artページには次を記載します。

- `art.*` 形式のStable Content ID
- ゲームプレイ上・世界観上の役割
- 習得経路と条件の意味
- 関連する `ability.*` IDと解放段階の意味
- 関連するSkill、Evolution、装備、クエスト
- 参照するUnityの `ArtDefinition`

具体的な熟練閾値、威力、クールダウン、コストはUnity側のDefinitionをSource of Truthとし、この索引へ複製しません。

## 登録ページ

<ContentCatalog content-type="art" />

ArtのDefinition、進捗、習得、熟練度、Ability付与、Save基盤は実装済みです。各ページのStatusでRuntime登録状況を確認し、追加時は [Artテンプレート](../../templates/art.md) と [Art仕様](../../specifications/art.md) を参照してください。
