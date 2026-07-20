# ロードマップ

## 完了済み基盤

- Scene / Build Settings統一
- Rigidbody2D / Collision Tilemap
- Isometric描画順
- Input Action / Input Context
- Interaction / Combat
- uGUI / Camera / Pause / Dodge
- ScriptableObject Definition
- ApplicationInstaller
- `DemonKing.Domain`
- CharacterDefinition / Stable Content ID
- CharacterProgressionState
- ExperienceTable / LevelUpResult
- Save DTO / ISaveService境界
- DamageResult / DefeatContext
- RewardServiceから経験値加算への接続
- Ability Definition / Runtime State / Controller / Executor
- `ability.basic_melee` の共通実行化
- Art Definition / Art進捗 / Save DTO Version 2
- Version 1から空のArt進捗へのMigration
- 汎用Art習得 / 熟練ランクによるAbility付与
- Ability Execution ID / 効果成立通知 / 1実行1回の熟練度加算
- EditMode / PlayModeテスト
- VitePress Knowledge Base基盤

## 直近の開発フェーズ

1. 受動Skill Definitionと補正接続
2. Evolution
3. NPC会話
4. 敵AI
5. クエスト・目的管理
6. Art入力・UIと正式コンテンツ
7. 縦切りゲームループ完成

## P3候補

必要性が発生した時点で着手します。

- `ISaveService` のローカル保存実装
- Steam機能とPlatform層
- クラウドセーブ
- 将来のコンソール向けPlatform実装
- Addressables / 非同期ロード
- Scene分割・ストリーミング
- パフォーマンス予算

## Knowledge Base側の次段階

1. モンスター・Art・Skill・Evolutionページを追加する。
2. Stable Content IDで相互リンクする。
3. 必要に応じてVitePress Data Loaderで一覧を自動生成する。
4. ScriptableObjectだけでは整合性管理が難しくなった段階で構造化データ化をADRで検討する。

技術基盤を先回りして増やすのではなく、プレイ可能なゲームループを優先します。
