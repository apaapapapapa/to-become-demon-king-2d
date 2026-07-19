# リリース運用

## 目的

このプロジェクトはsemantic-releaseを使用し、`main` のコミット履歴からSemantic Versioningの次バージョンを判定します。

自動化の対象:

- `vX.Y.Z` 形式のGitタグ作成
- コミット履歴からのリリースノート生成
- GitHub Releaseの作成
- 関連Issue／Pull Requestへのリリース通知

npmパッケージの公開、Unity Playerのビルド、`ProjectVersion.txt` の変更、CHANGELOGの自動コミットは行いません。

## バージョン判定

コミットとPull RequestのタイトルにはConventional Commitsを使用します。

| 形式 | リリース |
| --- | --- |
| `fix:` / `perf:` | Patch |
| `feat:` | Minor |
| `BREAKING CHANGE:` | Major |
| `docs:` / `test:` / `chore:` / `ci:` / `refactor:` | なし |

Squash mergeを使う場合は、最終的に `main` へ入るPull Requestタイトルをこの形式にします。

## 自動リリース

1. Pull Requestを `main` へマージする。
2. Release WorkflowがGit履歴とタグを取得する。
3. semantic-releaseが直前のタグ以降を解析する。
4. 対象変更があればタグとGitHub Releaseを作成する。
5. リリース対象がなければ何も公開せず終了する。

## ローカル確認

対応Node.jsはルート `package.json` の `engines` を正とします。

```text
npm ci
npm run release:dry-run
```

## 失敗時

- 再実行前に既存タグとGitHub Releaseを確認する。
- 公開済みタグは削除せず、修正コミットから新しいPatch Releaseを作成する。
- 権限エラーではWorkflowの権限設定を確認する。
- npm公開は設定していないため `NPM_TOKEN` は不要。
