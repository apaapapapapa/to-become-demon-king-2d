using System;
using System.IO;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace DemonKing.EditorTools
{
    /// <summary>
    /// uGUIで使用する日本語フォントをプロジェクト内へ導入します。
    /// OSフォントへの依存をなくし、Steam／将来のコンソールビルドでも同じFontアセットを参照できる状態を作ります。
    /// </summary>
    internal static class JapaneseUiFontInstaller
    {
        internal const string FontAssetPath = "Assets/Fonts/DotGothic16-Regular.ttf";
        private const string LicenseAssetPath = "Assets/Fonts/OFL_DotGothic16.txt";
        private const string FontDownloadUrl = "https://raw.githubusercontent.com/google/fonts/main/ofl/dotgothic16/DotGothic16-Regular.ttf";
        private const string LicenseDownloadUrl = "https://raw.githubusercontent.com/google/fonts/main/ofl/dotgothic16/OFL.txt";

        [MenuItem("Demon King/Project/Install Japanese UI Font")]
        private static void InstallFromMenu()
        {
            bool installed = EnsureInstalled(forceLog: true);
            if (installed)
            {
                PrototypeProjectAssetsAutoRepair.RepairNow(forceLog: true);
            }
        }

        internal static bool EnsureInstalled(bool forceLog = false)
        {
            Font existing = AssetDatabase.LoadAssetAtPath<Font>(FontAssetPath);
            if (existing != null)
            {
                if (forceLog)
                {
                    Debug.Log($"日本語UIフォントは導入済みです: {FontAssetPath}");
                }

                return true;
            }

            try
            {
                string absoluteFontPath = Path.GetFullPath(FontAssetPath);
                string directory = Path.GetDirectoryName(absoluteFontPath);
                if (string.IsNullOrEmpty(directory))
                {
                    Debug.LogError($"フォント配置先ディレクトリを解決できません: {FontAssetPath}");
                    return false;
                }

                Directory.CreateDirectory(directory);

                using HttpClient client = new();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ToBecomeDemonKing2D-UnityEditor");

                byte[] fontBytes = client.GetByteArrayAsync(FontDownloadUrl).GetAwaiter().GetResult();
                File.WriteAllBytes(absoluteFontPath, fontBytes);

                string absoluteLicensePath = Path.GetFullPath(LicenseAssetPath);
                if (!File.Exists(absoluteLicensePath))
                {
                    byte[] licenseBytes = client.GetByteArrayAsync(LicenseDownloadUrl).GetAwaiter().GetResult();
                    File.WriteAllBytes(absoluteLicensePath, licenseBytes);
                }

                AssetDatabase.ImportAsset(FontAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                AssetDatabase.ImportAsset(LicenseAssetPath, ImportAssetOptions.ForceUpdate);

                Font imported = AssetDatabase.LoadAssetAtPath<Font>(FontAssetPath);
                if (imported == null)
                {
                    Debug.LogError($"日本語UIフォントをダウンロードしましたが、UnityのFontアセットとして読み込めません: {FontAssetPath}");
                    return false;
                }

                Debug.Log($"日本語UIフォントをプロジェクトへ導入しました: {FontAssetPath}");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "日本語UIフォントの自動導入に失敗しました。" +
                    "Demon King > Project > Install Japanese UI Font から再実行してください。\n" +
                    exception.Message);
                return false;
            }
        }
    }
}
