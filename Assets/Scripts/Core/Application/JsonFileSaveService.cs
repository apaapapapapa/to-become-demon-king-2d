using System;
using System.IO;
using System.Text;
using DemonKing.Domain.Save;
using UnityEngine;

namespace DemonKing.Core.Application
{
    /// <summary>
    /// UnityのpersistentDataPath配下へGameSaveDataをJSONとして保存するローカル実装です。
    /// Gameplayはこの具体クラスを参照せず、ISaveService経由で利用します。
    /// </summary>
    public sealed class JsonFileSaveService : ISaveService
    {
        public const string DefaultFileName = "save.json";

        private readonly string filePath;

        public JsonFileSaveService(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Saveファイルパスは空にできません。", nameof(filePath));
            }

            this.filePath = Path.GetFullPath(filePath);
        }

        public string FilePath => filePath;

        public static JsonFileSaveService CreateDefault()
        {
            return new JsonFileSaveService(
                Path.Combine(Application.persistentDataPath, DefaultFileName));
        }

        public bool TryLoad(out GameSaveData saveData)
        {
            saveData = null;
            if (!File.Exists(filePath))
            {
                return false;
            }

            string json = File.ReadAllText(filePath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException("Saveファイルが空です。");
            }

            try
            {
                saveData = JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException("Save JSONを読み込めませんでした。", exception);
            }

            if (saveData == null)
            {
                throw new InvalidDataException("Save JSONからGameSaveDataを復元できませんでした。");
            }

            return true;
        }

        public void Save(GameSaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(saveData, prettyPrint: true);
            string temporaryPath = filePath + ".tmp";

            // 完成したJSONを一度別ファイルへ書き、その後で本体へ置き換えます。
            // 書き込み途中のSaveを本体パスへ直接残さないための最小保護です。
            File.WriteAllText(temporaryPath, json, Encoding.UTF8);
            File.Copy(temporaryPath, filePath, overwrite: true);
            File.Delete(temporaryPath);
        }
    }
}
