using System;
using DemonKing.Domain.Progression;
using UnityEngine;

namespace DemonKing.Gameplay.Progression.Configuration
{
    /// <summary>
    /// レベルごとの累積必要経験値と、最大レベル時の余剰経験値方針を定義します。
    /// 実行時のレベルや現在経験値は保持しません。
    /// </summary>
    [CreateAssetMenu(
        fileName = "ExperienceTable",
        menuName = "Demon King/Gameplay/Experience Table")]
    public sealed class ExperienceTableDefinition : ScriptableObject
    {
        [SerializeField] private long[] cumulativeExperienceByLevel = { 0, 5, 15, 30, 50 };
        [SerializeField] private bool keepOverflowAtMaxLevel = true;

        public int MaxLevel => cumulativeExperienceByLevel == null
            ? 0
            : cumulativeExperienceByLevel.Length;
        public bool KeepOverflowAtMaxLevel => keepOverflowAtMaxLevel;

        public bool IsConfigured
        {
            get
            {
                try
                {
                    CreateRuntimeTable();
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }
        }

        public ExperienceTable CreateRuntimeTable()
        {
            return new ExperienceTable(
                cumulativeExperienceByLevel,
                keepOverflowAtMaxLevel);
        }
    }
}
