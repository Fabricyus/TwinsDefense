using UnityEngine;

namespace TwinsDefense.Systems
{
    /// <summary>
    /// Persistent "boss rush" milestone gate, separate from a normal run:
    /// the first time the player ever reaches level 10, the run ends early
    /// as a win and level 20 unlocks. Once unlocked, killing the level-20
    /// boss (the skull) also ends the run as a win, and does so 3 times
    /// before level 30 unlocks — after that, killing the skull no longer
    /// ends the run early, letting the player push on to the real final
    /// boss. Backed directly by PlayerPrefs — same placeholder-persistence
    /// rationale as PlayerWallet/CharacterProgressTracker (no project save
    /// system exists yet).
    /// </summary>
    public static class CampaignProgress
    {
        public const string Level20UnlockedBaseKey = "TwinsDefense.Campaign.Level20Unlocked";
        public const string Level30UnlockedBaseKey = "TwinsDefense.Campaign.Level30Unlocked";
        public const string SkullKillCountBaseKey = "TwinsDefense.Campaign.SkullKillCount";
        public const string GameCompletedBaseKey = "TwinsDefense.Campaign.GameCompleted";
        public const string MegaMagpieKilledBaseKey = "TwinsDefense.Campaign.MegaMagpieKilled";

        private static string Level20UnlockedKey => SaveProfileManager.ScopedKey(Level20UnlockedBaseKey);
        private static string Level30UnlockedKey => SaveProfileManager.ScopedKey(Level30UnlockedBaseKey);
        private static string SkullKillCountKey => SaveProfileManager.ScopedKey(SkullKillCountBaseKey);
        private static string GameCompletedKey => SaveProfileManager.ScopedKey(GameCompletedBaseKey);
        private static string MegaMagpieKilledKey => SaveProfileManager.ScopedKey(MegaMagpieKilledBaseKey);

        /// <summary>How many level-20 boss kills are needed to unlock level 30.</summary>
        public const int SkullKillsRequiredForLevel30 = 3;

        public static bool Level20Unlocked => PlayerPrefs.GetInt(Level20UnlockedKey, 0) == 1;
        public static bool Level30Unlocked => PlayerPrefs.GetInt(Level30UnlockedKey, 0) == 1;
        public static int SkullKillCount => PlayerPrefs.GetInt(SkullKillCountKey, 0);
        public static bool GameCompleted => PlayerPrefs.GetInt(GameCompletedKey, 0) == 1;

        /// <summary>Global, character-independent: has the secret level-100 Mega Magpie ever been killed, with any character. Reached by out-leveling the regular level-30 Magpie fight before killing it — uncollected exp crystals from earlier in the run keep the level climbing even mid-fight — not by a normal bossSpawns-triggered ending, so this never interacts with Level20Unlocked/Level30Unlocked/GameCompleted above.</summary>
        public static bool MegaMagpieKilled => PlayerPrefs.GetInt(MegaMagpieKilledKey, 0) == 1;

        /// <summary>Called once, the first time the player reaches level 10.</summary>
        public static void UnlockLevel20()
        {
            if (Level20Unlocked) return;

            PlayerPrefs.SetInt(Level20UnlockedKey, 1);
            PlayerPrefs.Save();
        }

        /// <summary>Increments the level-20 boss kill counter, unlocking level 30 once it reaches SkullKillsRequiredForLevel30. Safe to keep calling after that — it just stops mattering.</summary>
        public static void ReportSkullKilled()
        {
            if (Level30Unlocked) return;

            int count = SkullKillCount + 1;
            PlayerPrefs.SetInt(SkullKillCountKey, count);

            if (count >= SkullKillsRequiredForLevel30)
            {
                PlayerPrefs.SetInt(Level30UnlockedKey, 1);
            }

            PlayerPrefs.Save();
        }

        /// <summary>Called once, when the final boss (highest level in bossSpawns, today the magpie at level 30) is defeated.</summary>
        public static void ReportGameCompleted()
        {
            if (GameCompleted) return;

            PlayerPrefs.SetInt(GameCompletedKey, 1);
            PlayerPrefs.Save();
        }

        /// <summary>Called once, when the secret level-100 Mega Magpie is defeated.</summary>
        public static void ReportMegaMagpieKilled()
        {
            if (MegaMagpieKilled) return;

            PlayerPrefs.SetInt(MegaMagpieKilledKey, 1);
            PlayerPrefs.Save();
        }
    }
}
