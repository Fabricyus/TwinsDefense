using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Systems;

namespace TwinsDefense.Progression
{
    /// <summary>
    /// Snapshots which of AchievementRegistry.All are already complete at the
    /// moment a run starts, so GameOverController can diff against the
    /// end-of-run state and know exactly which achievements were newly
    /// unlocked THIS run — drives the Game Over achievement popup queue. One
    /// instance lives in the Arena Run scene, same reset-per-run lifecycle as
    /// RunStats/RunChallengeTracker.
    /// </summary>
    public class AchievementUnlockTracker : MonoBehaviour
    {
        public static AchievementUnlockTracker Instance { get; private set; }

        private bool[] wasCompleteAtStart;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            AchievementDef[] achievements = AchievementRegistry.All;
            wasCompleteAtStart = new bool[achievements.Length];

            for (int i = 0; i < achievements.Length; i++)
            {
                wasCompleteAtStart[i] = AchievementRegistry.IsComplete(i);
            }
        }

        /// <summary>Every achievement description that went from incomplete to complete since this run started, in AchievementRegistry.All order.</summary>
        public List<string> GetNewlyUnlockedDescriptions()
        {
            List<string> result = new List<string>();
            AchievementDef[] achievements = AchievementRegistry.All;

            for (int i = 0; i < wasCompleteAtStart.Length && i < achievements.Length; i++)
            {
                if (!wasCompleteAtStart[i] && AchievementRegistry.IsComplete(i))
                {
                    result.Add(achievements[i].description);
                }
            }

            return result;
        }
    }
}
