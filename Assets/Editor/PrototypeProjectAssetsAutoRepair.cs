using System.Linq;
using DemonKing.Field.Prototype;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.AI.Configuration;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Characters.Configuration;
using DemonKing.Gameplay.Combat.Configuration;
using DemonKing.Gameplay.Dialogue.Configuration;
using DemonKing.Gameplay.Progression.Configuration;
using DemonKing.Gameplay.Quests.Configuration;
using DemonKing.Gameplay.Rewards.Configuration;
using UnityEditor;
using UnityEngine;

namespace DemonKing.EditorTools
{
    /// <summary>
    /// PrototypeProjectAssetsに集約した参照を、Editor上の実アセットから再解決する保守用ツールです。
    /// 参照切れやImport設定の不整合を復旧する目的で使用し、Runtimeの通常動作が毎回の自動修復へ依存しないことを前提とします。
    /// </summary>
    [InitializeOnLoad]
    internal static class PrototypeProjectAssetsAutoRepair
    {
        private const string FontInstallAttemptSessionKey = "DemonKing.FontInstallAttempted";
        private const string ProjectAssetsPath = "Assets/Resources/Settings/PrototypeProjectAssets.asset";
        private const string ApplicationSettingsPath = "Assets/Resources/Settings/PrototypeApplicationSettings.asset";
        private const string PlayerCharacterPath = "Assets/Resources/Settings/Gameplay/PlayerCharacter.asset";
        private const string PlayerPrefabPath = "Assets/Resources/Prefabs/Characters/PrototypeSlime.prefab";
        private const string PlayerCharacterStatsPath = "Assets/Resources/Settings/Gameplay/PlayerCharacterStats.asset";
        private const string PlayerMeleeAttackPath = "Assets/Resources/Settings/Gameplay/PlayerMeleeAttack.asset";
        private const string TrainingSlimeAiPath = "Assets/Resources/Settings/Gameplay/TrainingSlimeAi.asset";
        private const string FireMagicArtPath = "Assets/Resources/Settings/Gameplay/FireMagicArt.asset";
        private const string FireMagicTrainingGrantPath = "Assets/Resources/Settings/Gameplay/FireMagicTrainingGrant.asset";
        private const string ApprenticeMageDialoguePath = "Assets/Resources/Settings/Gameplay/ApprenticeMageDialogue.asset";
        private const string ApprenticeMageActiveDialoguePath = "Assets/Resources/Settings/Gameplay/ApprenticeMageActiveDialogue.asset";
        private const string ApprenticeMageTurnInDialoguePath = "Assets/Resources/Settings/Gameplay/ApprenticeMageTurnInDialogue.asset";
        private const string ApprenticeMageCompletedDialoguePath = "Assets/Resources/Settings/Gameplay/ApprenticeMageCompletedDialogue.asset";
        private const string FirstTrainingQuestPath = "Assets/Resources/Settings/Gameplay/FirstTrainingQuest.asset";
        private const string PredatoryInstinctSkillPath = "Assets/Resources/Settings/Gameplay/PredatoryInstinctSkill.asset";
        private const string PredatorSlimeEvolutionPath = "Assets/Resources/Settings/Gameplay/PredatorSlimeEvolution.asset";
        private const string ArcaneSlimeEvolutionPath = "Assets/Resources/Settings/Gameplay/ArcaneSlimeEvolution.asset";
        private const string ApexPredatorSlimeEvolutionPath = "Assets/Resources/Settings/Gameplay/ApexPredatorSlimeEvolution.asset";
        private const string ArchmageSlimeEvolutionPath = "Assets/Resources/Settings/Gameplay/ArchmageSlimeEvolution.asset";
        private const string PlayerDodgePath = "Assets/Resources/Settings/Gameplay/PlayerDodge.asset";
        private const string PlayerExperienceTablePath = "Assets/Resources/Settings/Gameplay/PlayerExperienceTable.asset";
        private const string TrainingDummyRewardPath = "Assets/Resources/Settings/Gameplay/TrainingDummyReward.asset";
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
            CharacterDefinition playerCharacter = Load<CharacterDefinition>(PlayerCharacterPath);
            bool characterDefinitionChanged = RepairPlayerCharacterDefinition(playerCharacter);

            changed |= AssignIfDifferent(serializedObject, "applicationSettings", Load<PrototypeApplicationSettings>(ApplicationSettingsPath));
            changed |= AssignIfDifferent(serializedObject, "playerCharacter", playerCharacter);
            changed |= AssignIfDifferent(serializedObject, "trainingSlimeAi", Load<EnemyAiDefinition>(TrainingSlimeAiPath));
            changed |= AssignIfDifferent(serializedObject, "apprenticeMageDialogue", Load<DialogueDefinition>(ApprenticeMageDialoguePath));
            changed |= AssignIfDifferent(serializedObject, "apprenticeMageActiveDialogue", Load<DialogueDefinition>(ApprenticeMageActiveDialoguePath));
            changed |= AssignIfDifferent(serializedObject, "apprenticeMageTurnInDialogue", Load<DialogueDefinition>(ApprenticeMageTurnInDialoguePath));
            changed |= AssignIfDifferent(serializedObject, "apprenticeMageCompletedDialogue", Load<DialogueDefinition>(ApprenticeMageCompletedDialoguePath));
            changed |= AssignArrayIfDifferent(serializedObject, "questDefinitions", Load<QuestDefinition>(FirstTrainingQuestPath));
            changed |= AssignIfDifferent(serializedObject, "trainingDummyReward", Load<RewardDefinition>(TrainingDummyRewardPath));
            changed |= AssignIfDifferent(serializedObject, "fireMagicTrainingGrant", Load<ProgressionGrantDefinition>(FireMagicTrainingGrantPath));
            changed |= AssignIfDifferent(serializedObject, "uiFont", Load<Font>(JapaneseUiFontInstaller.FontAssetPath, logIfMissing: forceLog));
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
                Debug.Log("PrototypeProjectAssetsとCharacterDefinitionの参照切れを自動修復しました。");
            }
            else if (forceLog)
            {
                Debug.Log("PrototypeProjectAssetsの参照は正常です。");
            }
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
            changed |= AssignIfDifferent(serializedObject, "statsDefinition", Load<CharacterStatsDefinition>(PlayerCharacterStatsPath));
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
            changed |= AssignIfDifferent(serializedObject, "dodgeDefinition", Load<DodgeDefinition>(PlayerDodgePath));
            changed |= AssignIfDifferent(serializedObject, "experienceTableDefinition", Load<ExperienceTableDefinition>(PlayerExperienceTablePath));

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

        /// <summary>
        /// 指定画像をSpriteとして再解決し、必要な場合だけTextureImporterをSprite / Singleへ補正して再インポートします。
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
