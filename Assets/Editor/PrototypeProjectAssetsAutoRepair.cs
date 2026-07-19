using DemonKing.Field.Prototype;
using UnityEditor;
using UnityEngine;

namespace DemonKing.EditorTools
{
    /// <summary>
    /// PrototypeProjectAssetsの参照切れをEditor上で自動修復します。
    /// Git経由で追加した画像やPrefabの再インポート時にfileIDが変わっても、AssetDatabaseから実体を再解決します。
    /// </summary>
    [InitializeOnLoad]
    internal static class PrototypeProjectAssetsAutoRepair
    {
        private const string ProjectAssetsPath = "Assets/Resources/Settings/PrototypeProjectAssets.asset";
        private const string PlayerPrefabPath = "Assets/Resources/Prefabs/Characters/PrototypeSlime.prefab";
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
            Repair(forceLog: true);
        }

        private static void RepairIfNeeded()
        {
            Repair(forceLog: false);
        }

        private static void Repair(bool forceLog)
        {
            PrototypeProjectAssets projectAssets = AssetDatabase.LoadAssetAtPath<PrototypeProjectAssets>(ProjectAssetsPath);
            if (projectAssets == null)
            {
                Debug.LogError($"PrototypeProjectAssetsが見つかりません: {ProjectAssetsPath}");
                return;
            }

            SerializedObject serializedObject = new(projectAssets);
            bool changed = false;

            changed |= AssignIfDifferent(serializedObject, "playerPrefab", Load<GameObject>(PlayerPrefabPath));
            changed |= AssignIfDifferent(serializedObject, "cottagePrefab", Load<GameObject>(CottagePrefabPath));
            changed |= AssignIfDifferent(serializedObject, "treePrefab", Load<GameObject>(TreePrefabPath));
            changed |= AssignIfDifferent(serializedObject, "lamppostPrefab", Load<GameObject>(LamppostPrefabPath));
            changed |= AssignIfDifferent(serializedObject, "cottageSprite", Load<Sprite>(CottageSpritePath));
            changed |= AssignIfDifferent(serializedObject, "treeSprite", Load<Sprite>(TreeSpritePath));
            changed |= AssignIfDifferent(serializedObject, "lamppostSprite", Load<Sprite>(LamppostSpritePath));
            changed |= AssignIfDifferent(serializedObject, "grassTileSprite", Load<Sprite>(GrassSpritePath));
            changed |= AssignIfDifferent(serializedObject, "pathTileSprite", Load<Sprite>(PathSpritePath));

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

        private static T Load<T>(string path) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Debug.LogError($"ProjectAssetsへ割り当てるアセットが見つかりません: {path}");
            }

            return asset;
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

            if (property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }
    }
}
