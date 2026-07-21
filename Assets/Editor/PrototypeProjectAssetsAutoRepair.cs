using System.Linq;
using DemonKing.Field.Prototype;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.AI.Configuration;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Combat.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEditor;
using UnityEngine;

namespace DemonKing.EditorTools
{
    /// <summary>
    /// PrototypeProjectAssetsの参照状態を検証し、明示的なメニュー操作時だけ既知の主要参照を修復します。
    /// Editor起動時にSerialized Fieldを自動上書きせず、Runtime / CompositionのSource of Truthはアセット本体に置きます。
    /// </summary>
    [InitializeOnLoad]
    internal static class PrototypeProjectAssetsAutoRepair
    {
        private const string ProjectAssetsPath = "Assets/Resources/Settings/PrototypeProjectAssets.asset";
        private const string ApplicationSettingsPath = "Assets/Resources/Settings/PrototypeApplicationSettings.asset";
        private const string PlayerCharacterPath = "Assets/Resources/Settings/Gameplay/PlayerCharacter.asset";
        private const string TrainingScenarioPath = "Assets/Resources/Settings/Gameplay/TrainingScenario.asset";

        private const string PlayerPrefabPath = "Assets/Resources/Prefabs/Characters/PrototypeSlime.prefab";
        private const string PlayerCharacterStatsPath = "Assets/Resources/Settings/Gameplay/PlayerCharacterStats.asset";
        private const string PlayerMeleeAttackPath = "Assets/Resources/Settings/Gameplay/PlayerMeleeAttack.asset";
        private const string FireMagicArtPath = "Assets/Resources/Settings/Gameplay/FireMagicArt.asset";
        private const string PredatoryInstinctSkillPath = "Assets/Resources/Settings/Gameplay/PredatoryInstinctSkill.asset";
        private const string PredatorSlimeEvolutionPath = "Assets/Resources/Settings/Gameplay/PredatorSlimeEvolution.asset";
        private const string ArcaneSlimeEvolutionPath = "Assets/Resources/Settings/Gameplay/ArcaneSlimeEvolution.asset";
        private const string ApexPredatorSlimeEvolutionPath = "Assets/Resources/Settings/Gameplay/ApexPredatorSlimeEvolution.asset";
        private const string ArchmageSlimeEvolutionPath = "Assets/Resources/Settings/Gameplay/ArchmageSlimeEvolution.asset";
        private const string PlayerDodgePath = "Assets/Resources/Settings/Gameplay/PlayerDodge.asset";
        private const string PlayerExperienceTablePath = "Assets/Resources/Settings/Gameplay/PlayerExperienceTable.asset";

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
            EditorApplication.delayCall += ValidateOnEditorLoad;
        }

        [MenuItem("Demon King/Prototype/Validate Project Assets References")]
        private static void ValidateFromMenu()
        {
            ValidateProjectAssets(forceLog: true);
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

        private static void ValidateOnEditorLoad()
        {
            ValidateProjectAssets(forceLog: false);
        }

        private static void ValidateProjectAssets(bool forceLog)
        {
            PrototypeProjectAssets projectAssets =
                AssetDatabase.LoadAssetAtPath<PrototypeProjectAssets>(ProjectAssetsPath);
            if (projectAssets == null)
            {
                Debug.LogError($"PrototypeProjectAssetsが見つかりません: {ProjectAssetsPath}");
                return;
            }

            if (!projectAssets.IsConfigured)
            {
                Debug.LogWarning(
                    "PrototypeProjectAssetsの必須参照またはTrainingScenarioDefinitionに不足があります。" +
                    "必要な場合は Demon King/Prototype/Repair Project Assets References を明示的に実行してください。",
                    projectAssets);
                return;
            }

            if (forceLog)
            {
                Debug.Log("PrototypeProjectAssetsの参照は正常です。", projectAssets);
            }
        }

        private static void Repair(bool forceLog)
        {
            JapaneseUiFontInstaller.EnsureInstalled(forceLog: forceLog);

            PrototypeProjectAssets projectAssets =
                AssetDatabase.LoadAssetAtPath<PrototypeProjectAssets>(ProjectAssetsPath);
            if (projectAssets == null)
            {
                Debug.LogError($"PrototypeProjectAssetsが見つかりません: {ProjectAssetsPath}");
                return;
            }

            SerializedObject serializedObject = new(projectAssets);
            bool changed = false;
            CharacterDefinition playerCharacter = Load<CharacterDefinition>(PlayerCharacterPath);
            bool characterDefinitionChanged = RepairPlayerCharacterDefinition(playerCharacter);

            changed |= AssignIfDifferent(
                serializedObject,
                "applicationSettings",
                Load<PrototypeApplicationSettings>(ApplicationSettingsPath));
            changed |= AssignIfDifferent(serializedObject, "playerCharacter", playerCharacter);
            changed |= AssignIfDifferent(
                serializedObject,
                "trainingScenario",
                Load<TrainingScenarioDefinition>(TrainingScenarioPath));
            changed |= AssignIfDifferent(
                serializedObject,
                "uiFont",
                Load<Font>(JapaneseUiFontInstaller.FontAssetPath, logIfMissing: forceLog));
            changed |= AssignIfDifferent(serializedObject, "cottagePrefab", Load<GameObject>(CottagePrefabPath));
            changed |= AssignIfDifferent(serializedObject, "treePrefab", Load<GameObject>(TreePrefabPath));
            changed |= AssignIfDifferent(serializedObject, "lamppostPrefab", Load<GameObject>(LamppostPrefabPath));
            changed |= AssignIfDifferent(serializedObject, "cottageSprite", LoadSprite(CottageSpritePath));
            changed |= AssignIfDifferent(serializedObject, "treeSprite", LoadSprite(TreeSpritePath));
            changed |= AssignIfDifferent(serializedObject, "lamppostSprite", LoadSprite(LamppostSpritePath));
            changed |= AssignIfDifferent(serializedObject, "grassTileSprite", LoadSprite(GrassSpritePath));
            changed |= AssignIfDifferent(serializedObject, "pathTileSprite", LoadSprite(PathSpritePath));

            if (changed || characterDefinitionChanged)
            {
                if (changed)
                {
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(projectAssets);
                }

                AssetDatabase.SaveAssets();
                Debug.Log("PrototypeProjectAssetsとCharacterDefinitionの主要参照を手動修復しました。");
            }
            else if (forceLog)
            {
                Debug.Log("PrototypeProjectAssetsの修復対象はありませんでした。");
            }

            ValidateProjectAssets(forceLog: false);
        }

        private static bool RepairPlayerCharacterDefinition(CharacterDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            SerializedObject serializedObject = new(definition);
            bool changed = false;
            changed |= AssignIfDifferent(serializedObject, "prefab", Load<GameObject>(PlayerPrefabPath));
            changed |= AssignIfDifferent(
                serializedObject,
                "statsDefinition",
                Load<CharacterStatsDefinition>(PlayerCharacterStatsPath));
            changed |= AssignArrayIfDifferent(
                serializedObject,
                "abilityDefinitions",
                Load<MeleeAttackDefinition>(PlayerMeleeAttackPath));
            changed |= AssignArrayIfDifferent(
                serializedObject,
                "artDefinitions",
                Load<ArtDefinition>(FireMagicArtPath));
            changed |= AssignArrayIfDifferent(
                serializedObject,
                "skillDefinitions",
                Load<SkillDefinition>(PredatoryInstinctSkillPath));
            changed |= AssignArrayIfDifferent(
                serializedObject,
                "evolutionDefinitions",
                Load<EvolutionDefinition>(PredatorSlimeEvolutionPath),
                Load<EvolutionDefinition>(ArcaneSlimeEvolutionPath),
                Load<EvolutionDefinition>(ApexPredatorSlimeEvolutionPath),
                Load<EvolutionDefinition>(ArchmageSlimeEvolutionPath));
            changed |= AssignIfDifferent(
                serializedObject,
                "dodgeDefinition",
                Load<DodgeDefinition>(PlayerDodgePath));
            changed |= AssignIfDifferent(
                serializedObject,
                "experienceTableDefinition",
                Load<ExperienceTableDefinition>(PlayerExperienceTablePath));

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
            }

            return changed;
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
                Debug.LogError($"対象アセットにSerializedProperty '{propertyName}' が見つかりません。");
                return false;
            }

            if (value == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool AssignArrayIfDifferent(
            SerializedObject serializedObject,
            string propertyName,
            params Object[] values)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                Debug.LogError($"対象アセットに配列SerializedProperty '{propertyName}' が見つかりません。");
                return false;
            }

            Object[] validValues = values.Where(value => value != null).ToArray();
            bool isSame = property.arraySize == validValues.Length;
            for (int index = 0; isSame && index < validValues.Length; index++)
            {
                isSame = property.GetArrayElementAtIndex(index).objectReferenceValue == validValues[index];
            }

            if (isSame)
            {
                return false;
            }

            property.arraySize = validValues.Length;
            for (int index = 0; index < validValues.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = validValues[index];
            }

            return true;
        }
    }
}
