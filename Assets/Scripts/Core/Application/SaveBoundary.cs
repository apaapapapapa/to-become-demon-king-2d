using System;
using System.Collections.Generic;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Save;

namespace DemonKing.Core.Application
{
    /// <summary>
    /// 保存先やシリアライズ方式をGameplayから分離する契約です。
    /// ファイル保存、クラウド保存、プラットフォーム保存はこの契約の外側で実装します。
    /// </summary>
    public interface ISaveService
    {
        bool TryLoad(out GameSaveData saveData);

        void Save(GameSaveData saveData);
    }

    /// <summary>
    /// 実行時の成長状態と保存DTOの相互変換を一か所へ集約します。
    /// </summary>
    public static class CharacterProgressionSaveMapper
    {
        public static PlayerSaveData ToSaveData(CharacterProgressionState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return new PlayerSaveData
            {
                characterDefinitionId = state.CharacterDefinitionId,
                level = state.Level,
                currentExperience = state.CurrentExperience,
                unlockedSkillIds = new List<string>(state.UnlockedSkillIds),
                unlockedEvolutionNodeIds = new List<string>(state.UnlockedEvolutionNodeIds)
            };
        }

        public static CharacterProgressionState FromSaveData(PlayerSaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            return CharacterProgressionState.Restore(
                saveData.characterDefinitionId,
                saveData.level,
                saveData.currentExperience,
                saveData.unlockedSkillIds,
                saveData.unlockedEvolutionNodeIds);
        }
    }
}
