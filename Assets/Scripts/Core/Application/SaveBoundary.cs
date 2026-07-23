using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Save;
using DemonKing.Domain.Story;

namespace DemonKing.Core.Application
{
    public interface ISaveService
    {
        bool TryLoad(out GameSaveData saveData);
        void Save(GameSaveData saveData);
    }

    public interface IGameSaveSnapshotProvider
    {
        GameSaveData CreateSnapshot();
    }

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
    /// Story Runtime StateとSave DTOの変換だけを担当します。
    /// Story Event条件評価やGameplay Event購読は行いません。
    /// </summary>
    public static class StoryProgressionSaveMapper
    {
        public static StorySaveData ToSaveData(StoryProgressState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return new StorySaveData
            {
                currentChapterId = state.CurrentChapterId,
                flags = state.Flags.OrderBy(value => value, StringComparer.Ordinal).ToList(),
                executedEventIds = state.ExecutedEventIds
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList()
            };
        }

        public static StoryProgressState FromSaveData(
            StorySaveData saveData,
            string defaultChapterId)
        {
            string chapterId = saveData == null || string.IsNullOrWhiteSpace(saveData.currentChapterId)
                ? defaultChapterId
                : saveData.currentChapterId;
            return StoryProgressState.Restore(
                chapterId,
                saveData?.flags,
                saveData?.executedEventIds);
        }
    }

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
                saveData.player.artProgress = new List<ArtProgressSaveData>();
                saveData.version = 2;
            }

            if (saveData.version == 2)
            {
                saveData.player.abilityLoadout = new AbilityLoadoutSaveData();
                saveData.quests = new List<QuestProgressSaveData>();
                saveData.world = new WorldSaveData();
                saveData.version = 3;
            }

            if (saveData.version == 3)
            {
                saveData.world ??= new WorldSaveData();
                saveData.world.currentFieldId = string.Empty;
                saveData.world.entryPointId = string.Empty;
                saveData.version = 4;
            }

            if (saveData.version == 4)
            {
                // Version 4にはQuestと独立したStory Runtime Stateが存在しません。
                saveData.story = new StorySaveData();
                saveData.version = 5;
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
            saveData.world.currentFieldId ??= string.Empty;
            saveData.world.entryPointId ??= string.Empty;
            saveData.world.consumedProgressionGrantIds ??= new List<string>();

            saveData.story ??= new StorySaveData();
            saveData.story.currentChapterId ??= string.Empty;
            saveData.story.flags ??= new List<string>();
            saveData.story.executedEventIds ??= new List<string>();
        }
    }
}
