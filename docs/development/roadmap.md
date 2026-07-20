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
- 受動Skill Definition / 取得 / Save接続
- 与ダメージ / Abilityクールダウン / Art熟練ポイント補正
- Stable Content ID相互リンク / VitePress Data Loader一覧
- EditMode / PlayModeテスト
- VitePress Knowledge Base基盤

## 直近の開発フェーズ

1. Evolution
2. NPC会話
3. 敵AI
4. クエスト・目的管理
5. Art / Skillの取得・入力・UIと正式Runtimeコンテンツ
6. 縦切りゲームループ完成

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

1. 新しいコンテンツ実装時に各ページを追加し、実装Statusを同期する。
2. 敵モンスターと上位Art・Skill・EvolutionをStable Content IDで相互リンクする。
3. コンテンツfrontmatterとVitePress Data Loaderの整合性検証を維持する。
4. ScriptableObjectだけでは整合性管理が難しくなった段階で構造化データ化をADRで検討する。

技術基盤を先回りして増やすのではなく、プレイ可能なゲームループを優先します。
