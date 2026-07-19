namespace DemonKing.Domain.Progression
{
    /// <summary>
    /// 1回の経験値加算によるレベルと累積経験値の変化を表します。
    /// </summary>
    public readonly struct LevelUpResult
    {
        public LevelUpResult(
            long requestedExperience,
            long appliedExperience,
            int previousLevel,
            int currentLevel,
            long previousExperience,
            long currentExperience,
            bool reachedMaxLevel)
        {
            RequestedExperience = requestedExperience;
            AppliedExperience = appliedExperience;
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
            PreviousExperience = previousExperience;
            CurrentExperience = currentExperience;
            ReachedMaxLevel = reachedMaxLevel;
        }

        public long RequestedExperience { get; }
        public long AppliedExperience { get; }
        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
        public long PreviousExperience { get; }
        public long CurrentExperience { get; }
        public int LevelsGained => CurrentLevel > PreviousLevel
            ? CurrentLevel - PreviousLevel
            : 0;
        public bool DidLevelUp => LevelsGained > 0;
        public bool ReachedMaxLevel { get; }
    }
}
