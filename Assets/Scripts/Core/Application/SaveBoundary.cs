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
    /// Runtime State一式から保存時点のGameSaveDataを生成する境界です。
    /// 保存タイミングを管理する側はFeatureごとのDTO組立を知りません。
    /// </summary>
    public interface IGameSaveSnapshotProvider
    {
        GameSaveData CreateSnapshot();
    }

    /// <summary>
    /// Character / Playerの実行時成長状態とPlayerSaveDataの相互変換を一か所へ集約します。
    /// Game全体のSave Snapshot生成は担当しません。
    /// </summary>
    public static class CharacterProgressionSaveMapper
    {
        public static PlayerSaveData ToSaveData(CharacterProgressionState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var artProgress = new List<ArtProgressSaveData>();
            foreach (ArtProgressState progressState in state.ArtProgressStates)
            {
                artProgress.Add(new ArtProgressSaveData
                {
                    artId = progressState.ArtId,
                    masteryPoints = progressState.MasteryPoints
                });
            }

            return new PlayerSaveData
            {
                characterDefinitionId = state.CharacterDefinitionId,
                level = state.Level,
                currentExperience = state.CurrentExperience,
                artProgress = artProgress,
                unlockedSkillIds = new List<string>(state.UnlockedSkillIds),
                unlockedEvolutionNodeIds = new List<string>(state.UnlockedEvolutionNodeIds),
                abilityLoadout = new AbilityLoadoutSaveData()
            };
        }

        public static CharacterProgressionState FromSaveData(PlayerSaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            var artProgress = new List<ArtProgressState>();
            if (saveData.artProgress != null)
            {
                foreach (ArtProgressSaveData progressSaveData in saveData.artProgress)
                {
                    if (progressSaveData == null)
                    {
                        throw new ArgumentException(
                            "保存されたArt進捗にnullを含めることはできません。",
                            nameof(saveData));
                    }

                    artProgress.Add(ArtProgressState.Restore(
                        progressSaveData.artId,
                        progressSaveData.masteryPoints));
                }
            }

            return CharacterProgressionState.Restore(
                saveData.characterDefinitionId,
                saveData.level,
                saveData.currentExperience,
                saveData.unlockedSkillIds,
                saveData.unlockedEvolutionNodeIds,
                artProgress);
        }
    }

    /// <summary>
    /// Save DTOのVersion差分だけを扱い、Runtime Stateへの変換と分離します。
    /// </summary>
    public static class GameSaveDataMigrator
    {
        private const int FirstSupportedVersion = 1;

        public static GameSaveData MigrateToCurrent(GameSaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            if (saveData.version < FirstSupportedVersion ||
                saveData.version > GameSaveData.CurrentVersion)
            {
                throw new NotSupportedException(
                    $"対応していないSave Versionです: {saveData.version}");
            }

            saveData.player ??= new PlayerSaveData();

            if (saveData.version == 1)
            {
                // Version 1にはArt進捗フィールドが存在しないため、混在値を引き継ぎません。
                saveData.player.artProgress = new List<ArtProgressSaveData>();
                saveData.version = 2;
            }

            if (saveData.version == 2)
            {
                // Version 2にはLoadout / Quest / World状態が存在しません。
                saveData.player.abilityLoadout = new AbilityLoadoutSaveData();
                saveData.quests = new List<QuestProgressSaveData>();
                saveData.world = new WorldSaveData();
                saveData.version = 3;
            }

            NormalizeCollections(saveData);
            return saveData;
        }

        private static void NormalizeCollections(GameSaveData saveData)
        {
            PlayerSaveData player = saveData.player;
            player.artProgress ??= new List<ArtProgressSaveData>();
            player.unlockedSkillIds ??= new List<string>();
            player.unlockedEvolutionNodeIds ??= new List<string>();
            player.abilityLoadout ??= new AbilityLoadoutSaveData();
            player.abilityLoadout.slots ??= new List<AbilitySlotSaveData>();

            saveData.quests ??= new List<QuestProgressSaveData>();
            foreach (QuestProgressSaveData quest in saveData.quests)
            {
                if (quest != null)
                {
                    quest.objectives ??= new List<ObjectiveProgressSaveData>();
                }
            }

            saveData.world ??= new WorldSaveData();
            saveData.world.consumedProgressionGrantIds ??= new List<string>();
        }
    }
}
