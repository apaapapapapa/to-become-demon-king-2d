using System.Linq;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Combat.Configuration;
using UnityEditor;
using UnityEngine;

namespace DemonKing.EditorTools
{
    /// <summary>
    /// PrototypeProjectAssetsの参照切れをEditor上で自動修復します。
    /// Git経由で追加した画像、Prefab、設定アセット、UIフォントを実際のインポート結果から再解決します。
    /// </summary>
    [InitializeOnLoad]
    internal static class PrototypeProjectAssetsAutoRepair
    {
        private const string FontInstallAttemptSessionKey = "DemonKing.FontInstallAttempted";
        private const string ProjectAssetsPath = "Assets/Resources/Settings/PrototypeProjectAssets.asset";
        private const string PlayerPrefabPath = "Assets/Resources/Prefabs/Characters/PrototypeSlime.prefab";
        private const string PlayerCharacterStatsPath = "Assets/Resources/Settings/Gameplay/PlayerCharacterStats.asset";
        private const string PlayerMeleeAttackPath = "Assets/Resources/Settings/Gameplay/PlayerMeleeAttack.asset";
        private const string CottagePrefabPath = "Assets/Resources/Prefabs/World/PrototypeCottage.prefab";
        private const string TreePrefabPath = "Assets/Resources/Prefabs/World/PrototypeTree.prefab";
        private const string LamppostPrefabPath = "Assets/Resources/Prefabs/World/PrototypeLamppost.prefab";
        private const string CottageSpritePath = "Assets/Art/World/cottage.png";
        private const string TreeSpritePath = "Assets/Art/World/tree.png";
        private const string LamppostSpritePath = "Assets/Art/World/lamppost.png";
        private const string GrassSpritePath = "Assets/Art/External/Kenney/grass_a.png";
        private const string PathSpritePath = "Assets/Art/External/Kenney/grass_b.png";

        static PrototypeProjectAssetsAutoRepair()
        {
            EditorApplication.delayCall += RepairIfNeeded;
        }

        [MenuItem("Demon King/Prototype/Repair Project Assets References")]
        private static void RepairFromMenu()
        {
            RepairNow(forceLog: true);
        }

        internal static void RepairNow(bool forceLog = true)
        {
            Repair(forceLog);
        }

        private static void RepairIfNeeded()
        {
            Repair(forceLog: false);
        }

        private static void Repair(bool forceLog)
        {
            // 自動実行時のネットワーク試行はEditorセッションにつき1回に限定します。
            // 手動修復時は再試行できるため、オフライン起動時にも開発を妨げません。
            bool shouldAttemptFontInstall =
                forceLog ||
                !SessionState.GetBool(FontInstallAttemptSessionKey, false);

            if (shouldAttemptFontInstall)
            {
                SessionState.SetBool(FontInstallAttemptSessionKey, true);
                JapaneseUiFontInstaller.EnsureInstalled(forceLog: forceLog);
            }

            PrototypeProjectAssets projectAssets = AssetDatabase.LoadAssetAtPath<PrototypeProjectAssets>(ProjectAssetsPath);
            if (projectAssets == null)
            {
                Debug.LogError($"PrototypeProjectAssetsが見つかりません: {ProjectAssetsPath}");
                return;
            }

            SerializedObject serializedObject = new(projectAssets);
            bool changed = false;

            changed |= AssignIfDifferent(serializedObject, "playerPrefab", Load<GameObject>(PlayerPrefabPath));
            changed |= AssignIfDifferent(serializedObject, "playerCharacterStats", Load<CharacterStatsDefinition>(PlayerCharacterStatsPath));
            changed |= AssignIfDifferent(serializedObject, "playerMeleeAttack", Load<MeleeAttackDefinition>(PlayerMeleeAttackPath));
            changed |= AssignIfDifferent(serializedObject, "uiFont", Load<Font>(JapaneseUiFontInstaller.FontAssetPath, logIfMissing: forceLog));
            changed |= AssignIfDifferent(serializedObject, "cottagePrefab", Load<GameObject>(CottagePrefabPath));
            changed |= AssignIfDifferent(serializedObject, "treePrefab", Load<GameObject>(TreePrefabPath));
            changed |= AssignIfDifferent(serializedObject, "lamppostPrefab", Load<GameObject>(LamppostPrefabPath));
            changed |= AssignIfDifferent(serializedObject, "cottageSprite", LoadSprite(CottageSpritePath));
            changed |= AssignIfDifferent(serializedObject, "treeSprite", LoadSprite(TreeSpritePath));
            changed |= AssignIfDifferent(serializedObject, "lamppostSprite", LoadSprite(LamppostSpritePath));
            changed |= AssignIfDifferent(serializedObject, "grassTileSprite", LoadSprite(GrassSpritePath));
            changed |= AssignIfDifferent(serializedObject, "pathTileSprite", LoadSprite(PathSpritePath));

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(projectAssets);
                AssetDatabase.SaveAssets();
                Debug.Log("PrototypeProjectAssetsの参照切れを自動修復しました。");
            }
            else if (forceLog)
            {
                Debug.Log("PrototypeProjectAssetsの参照は正常です。");
            }
        }

        private static T Load<T>(string path, bool logIfMissing = true) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null && logIfMissing)
            {
                Debug.LogError($"ProjectAssetsへ割り当てるアセットが見つかりません: {path}");
            }

            return asset;
        }

        /// <summary>
        /// PNGのMain AssetがTexture2Dとして扱われる環境でも、Spriteサブアセットまで探索して取得します。
        /// Sprite設定になっていない場合はTextureImporterを修正して再インポートします。
        /// </summary>
        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = FindImportedSprite(path);
            if (sprite != null)
            {
                return sprite;
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"Sprite画像のTextureImporterが見つかりません: {path}");
                return null;
            }

            bool needsReimport =
                importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single;

            if (needsReimport)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            else
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            sprite = FindImportedSprite(path);
            if (sprite == null)
            {
                Debug.LogError($"画像は存在しますがSpriteとして読み込めません: {path}");
            }

            return sprite;
        }

        private static Sprite FindImportedSprite(string path)
        {
            Sprite direct = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (direct != null)
            {
                return direct;
            }

            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .FirstOrDefault();
        }

        private static bool AssignIfDifferent(
            SerializedObject serializedObject,
            string propertyName,
            Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"PrototypeProjectAssetsにSerializedProperty '{propertyName}' が見つかりません。");
                return false;
            }

            if (value == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }
    }
}
