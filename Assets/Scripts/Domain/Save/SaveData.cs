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
        public const int CurrentVersion = 1;

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
        public List<string> unlockedSkillIds = new List<string>();
        public List<string> unlockedEvolutionNodeIds = new List<string>();
    }
}
