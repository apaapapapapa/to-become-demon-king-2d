using System;
using System.Collections.Generic;

namespace DemonKing.Domain.Save
{
    /// <summary>
    /// ゲーム全体の保存データ境界です。
    /// Serializerや保存先には依存せず、形式変更に備えてバージョンを保持します。
    /// </summary>
    [Serializable]
    public sealed class GameSaveData
    {
        public const int CurrentVersion = 4;

        public int version = CurrentVersion;
        public PlayerSaveData player = new PlayerSaveData();
        public List<QuestProgressSaveData> quests = new List<QuestProgressSaveData>();
        public WorldSaveData world = new WorldSaveData();
    }

    /// <summary>
    /// プレイヤー成長状態と入力割当を永続化するDTOです。
    /// Unityアセット参照ではなく、変更されないDefinition IDだけを保存します。
    /// </summary>
    [Serializable]
    public sealed class PlayerSaveData
    {
        public string characterDefinitionId = string.Empty;
        public int level = 1;
        public long currentExperience;
        public List<ArtProgressSaveData> artProgress = new List<ArtProgressSaveData>();
        public List<string> unlockedSkillIds = new List<string>();
        public List<string> unlockedEvolutionNodeIds = new List<string>();
        public AbilityLoadoutSaveData abilityLoadout = new AbilityLoadoutSaveData();
    }

    /// <summary>
    /// Artの習得状態と累積熟練ポイントだけを永続化するDTOです。
    /// ランクと解放AbilityはDefinitionから再計算します。
    /// </summary>
    [Serializable]
    public sealed class ArtProgressSaveData
    {
        public string artId = string.Empty;
        public long masteryPoints;
    }

    /// <summary>
    /// プレイヤーが明示的に変更できるAbility Slot割当です。
    /// SlotはCore Input enumへDomain Save DTOを依存させないため整数値で保持します。
    /// </summary>
    [Serializable]
    public sealed class AbilityLoadoutSaveData
    {
        public List<AbilitySlotSaveData> slots = new List<AbilitySlotSaveData>();
    }

    [Serializable]
    public sealed class AbilitySlotSaveData
    {
        public int slot;
        public string abilityId = string.Empty;
    }

    /// <summary>
    /// Quest単位の状態とObjective進捗です。
    /// StatusはDomain Quest enumへのSave DTO依存を避けるため整数値で保持します。
    /// </summary>
    [Serializable]
    public sealed class QuestProgressSaveData
    {
        public string questId = string.Empty;
        public int status;
        public List<ObjectiveProgressSaveData> objectives = new List<ObjectiveProgressSaveData>();
    }

    [Serializable]
    public sealed class ObjectiveProgressSaveData
    {
        public string objectiveId = string.Empty;
        public int currentCount;
    }

    /// <summary>
    /// Field位置と、フィールド上の一度きり取得物などCharacter成長とは別のWorld状態です。
    /// Scene名やBuild IndexではなくStable Field / Entry Point IDを保存します。
    /// </summary>
    [Serializable]
    public sealed class WorldSaveData
    {
        public string currentFieldId = string.Empty;
        public string entryPointId = string.Empty;
        public List<string> consumedProgressionGrantIds = new List<string>();
    }
}
