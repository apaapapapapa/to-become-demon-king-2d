# ロードマップ

## 完了済み基盤

P0〜P2の基礎リファクタリング／リアーキテクチャに加え、成長システム実装前のDomain境界整備まで完了しています。

主な完了項目:

- Scene / Build Settings統一
- Rigidbody2D移動とCollision Tilemap
- Isometric描画順
- Input ActionとInput Context
- Interaction / Combat分離
- Camera Follow分離
- uGUI
- ScriptableObject Definition
- Dodge / Pause
- ApplicationInstaller
- EditMode / PlayModeテスト
- 日本語Font管理
- `DemonKing.Domain` assembly
- `CharacterDefinition`
- `CharacterProgressionState`
- `GameSaveData` / `PlayerSaveData`
- `ISaveService` 境界
- Combatの `DamageRequest` / `DamageResult` / `DefeatContext`
- Knowledge Base / VitePress基盤

## 直近のゲーム開発フェーズ

現在の実装方針では、成長・報酬の接続を先に完成させます。

1. 経験値テーブル
2. Reward Service
3. `DefeatContext` から経験値加算までを接続
4. Ability基盤
5. Skill
6. Evolution
7. NPC・会話
8. 敵AI
9. クエスト・目的管理
10. 縦切りループ完成

順序はゲーム体験の検証結果に応じて変更できます。

## P3候補

必要性が発生した時点で着手します。

- `ISaveService` の具体的なローカル保存実装
- Steam機能とPlatform層
- 将来のコンソール向けPlatform実装
- クラウドセーブ
- Addressables / 非同期ロード
- Scene分割・ストリーミング
- パフォーマンス予算

## Knowledge Base側の次段階

コンテンツ量が増えたら次を検討します。

1. モンスター・Skill・Evolutionページの追加
2. 安定Content IDによる相互リンク
3. VitePress Data Loaderによる一覧自動生成
4. ScriptableObjectだけでは整合性管理が難しくなった場合の構造化データ化

構造化データをSingle Source of Truthへ変更する場合は、UnityとVitePress双方への影響が大きいためADRを作成します。

## 判断基準

技術基盤を先回りして増やすのではなく、プレイ可能なゲームループを優先します。

新しい基盤が必要になった場合は、必要性と採用理由を `decisions/` のADRへ残します。
