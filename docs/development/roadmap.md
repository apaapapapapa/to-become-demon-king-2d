# ロードマップ

## 完了済み基盤

P0〜P2の基礎リファクタリング／リアーキテクチャは完了しています。

主な完了項目:

- Scene / Build Settings統一
- Rigidbody2D移動とCollision Tilemap
- Isometric描画順
- Input ActionとInput Context
- Interaction / Combat分離
- Camera Follow分離
- uGUI
- ScriptableObject設定
- Dodge / Pause
- ApplicationInstaller
- EditMode / PlayModeテスト
- 日本語Font管理
- Knowledge Base / VitePress基盤

## 次のゲーム開発フェーズ

優先順位は固定ではありませんが、現在のゲームビジョンから次を推奨します。

1. NPC会話
2. 簡単な目的・クエスト管理
3. 敵AI
4. 報酬・進行変化
5. 最小の縦切りループ完成
6. 成長・進化システムの設計
7. コンテンツ追加

## P3候補

必要性が発生した時点で着手します。

- セーブ機能と保存データ境界
- Steam機能とPlatform層
- 将来のコンソール向けPlatform実装
- Addressables / 非同期ロード
- Scene分割・ストリーミング
- パフォーマンス予算

## 判断基準

技術基盤を先回りして増やすのではなく、プレイ可能なゲームループを優先します。

新しい基盤が必要になった場合は、必要性と採用理由を `decisions/` のADRへ残します。
