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
        public const int CurrentVersion = 2;

        public int version = CurrentVersion;
        public PlayerSaveData player = new PlayerSaveData();
    }

    /// <summary>
    /// プレイヤー成長状態を永続化するDTOです。
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
}
