# UIフォント管理

uGUIの日本語表示は、OSにインストールされたフォントへ依存せず、プロジェクト内の `Font` アセットを使用します。

標準フォントは Google Fonts の `DotGothic16-Regular.ttf` です。

Unity Editorでプロジェクトを開いた際、`PrototypeProjectAssetsAutoRepair` がフォントの存在を確認します。未導入の場合は `JapaneseUiFontInstaller` が公式Google Fontsリポジトリから次のファイルを取得し、このフォルダへ配置します。

- `DotGothic16-Regular.ttf`
- `OFL_DotGothic16.txt`

手動で再実行する場合は、Unityメニューの次を使用します。

`Demon King > Project > Install Japanese UI Font`

導入後は `PrototypeProjectAssets.uiFont` にFontアセットが設定され、`GameHudView` などのuGUI表示へ渡されます。

フォント本体とライセンスファイルは、初回導入後にGitへコミットしてチーム内・CI・ビルド環境で同じアセットを使用する運用を推奨します。
