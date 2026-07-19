# リリース運用

## 目的

このプロジェクトはsemantic-releaseを使用し、`main` のコミット履歴からSemantic Versioningの次バージョンを判定します。

自動化の対象は次のとおりです。

- `vX.Y.Z` 形式のGitタグ作成
- コミット履歴からのリリースノート生成
- GitHub Releaseの作成
- 関連Issue／Pull Requestへのリリース通知

npmパッケージの公開、Unity Playerのビルド、`ProjectVersion.txt` の変更、CHANGELOGの自動コミットは行いません。

## バージョン判定

コミットとPull RequestのタイトルにはConventional Commitsを使用します。

| 形式 | リリース | 例 |
| --- | --- | --- |
| `fix:` | Patch | `fix: ダメージ計算の丸め誤差を修正` |
| `perf:` | Patch | `perf: Tilemap描画の負荷を削減` |
| `feat:` | Minor | `feat: スキル装備画面を追加` |
| `BREAKING CHANGE:` フッター | Major | 互換性のない保存形式変更 |
| `docs:` / `test:` / `chore:` / `ci:` / `refactor:` | なし | 文書、テスト、保守作業 |

Squash mergeを使う場合は、最終的に `main` へ入るPull Requestタイトルをこの形式にします。規約外のコミットはリリース判定から無視されます。

## 自動リリースの流れ

1. Pull Requestを `main` へマージする
2. `.github/workflows/release.yml` が完全なGit履歴とタグを取得する
3. semantic-releaseが直前の `vX.Y.Z` タグ以降を解析する
4. リリース対象があればタグとGitHub Releaseを作成する
5. 対象変更がなければ何も公開せず正常終了する

手動で再実行する場合は、GitHub Actionsの `Release` ワークフローから `Run workflow` を選択します。

## 初回リリース

導入時点では既存のバージョンタグがありません。設定を `main` へマージした後の初回実行では、既存履歴内のリリース対象コミットを基に初回バージョンが決まります。現在の履歴では `feat:` が存在するため、最初のリリースは `v1.0.0` になる想定です。

過去に別経路で公開したバージョンがある場合は、ワークフローを動かす前に、そのリリースを含むコミットへ同じバージョンの `vX.Y.Z` タグを付けて履歴を合わせます。

## ローカル確認

対応Node.jsは `^22.14.0` または `24.10.0` 以上です。

```text
npm ci
npm run release:dry-run
```

dry-runでもGitHubへの接続確認を行うため、通常はPull Request上のActions実行結果と設定ファイルの検証を利用します。ローカルで完全なdry-runを行う場合は、対象リポジトリを読み書きできる一時トークンを環境変数 `GITHUB_TOKEN` に設定してください。トークンはファイルやコマンド履歴へ保存しません。

## 失敗時の扱い

- 同じコミットに対して再実行する場合は、まず既存タグとGitHub Releaseの有無を確認する
- 公開済みタグは削除せず、修正コミットを追加して新しいPatch Releaseを作成する
- リリース権限エラーでは、Workflowの `contents`、`issues`、`pull-requests` 権限を確認する
- npm公開は設定していないため、`NPM_TOKEN` は不要
