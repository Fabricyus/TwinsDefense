using UnityEngine;
using TwinsDefense.Economy;

namespace TwinsDefense.Systems
{
    [System.Serializable]
    public class SaveProfileSlot
    {
        public bool exists;
        public string profileName = string.Empty;
    }

    /// <summary>
    /// Owns the 3 save-profile slots shown on the "Save" (profile list) screen, and which one
    /// is currently active. Every other PlayerPrefs-backed system (CharacterProgressTracker,
    /// CharacterStarUpgrades, PlayerWallet, CampaignProgress) scopes its own persistence key
    /// through <see cref="ScopedKey"/> so each save profile has fully independent progress.
    /// Backed directly by PlayerPrefs — same placeholder-persistence rationale as those systems
    /// (no project save system exists yet).
    /// </summary>
    public static class SaveProfileManager
    {
        public const int SlotCount = 3;
        public const int MaxNameLength = 16;

        private const string ProfilesKey = "TwinsDefense.SaveProfiles";

        [System.Serializable]
        private class SaveProfileListData
        {
            public SaveProfileSlot[] slots;

            public SaveProfileListData()
            {
                slots = new SaveProfileSlot[SlotCount];
                for (int i = 0; i < SlotCount; i++)
                {
                    slots[i] = new SaveProfileSlot();
                }
            }
        }

        private const string SeedImportedKey = "TwinsDefense.SaveSeedImported";
        private const string SeedFileName = "save_seed.json";

        private static SaveProfileListData cachedList;

        private static SaveProfileListData List
        {
            get
            {
                if (cachedList == null)
                {
                    cachedList = LoadList();
                    TryImportSeedOnFirstRun();
                }

                return cachedList;
            }
        }

        /// <summary>-1 means no profile has been selected yet this session (e.g. app just launched, still on the Save screen). Resets to -1 every app launch by design — the player always picks a save on the way in.</summary>
        public static int ActiveProfileIndex { get; private set; } = -1;

        public static SaveProfileSlot GetSlot(int index) => List.slots[index];

        /// <summary>Creates (or renames) the save at this slot and persists it immediately. Does not select it — the player still has to click the slot to make it active.</summary>
        public static void CreateProfile(int index, string profileName)
        {
            profileName = (profileName ?? string.Empty).Trim();
            if (profileName.Length > MaxNameLength)
            {
                profileName = profileName.Substring(0, MaxNameLength);
            }

            List.slots[index].exists = true;
            List.slots[index].profileName = profileName;
            SaveList();
        }

        /// <summary>Wipes the save slot itself plus every namespaced progress key that belongs to it (coins, character progress, star upgrades, campaign milestones).</summary>
        public static void DeleteProfile(int index)
        {
            List.slots[index].exists = false;
            List.slots[index].profileName = string.Empty;
            SaveList();

            PlayerPrefs.DeleteKey(BuildKey(CharacterProgressTracker.PersistenceBaseKey, index));
            PlayerPrefs.DeleteKey(BuildKey(CharacterStarUpgrades.PersistenceBaseKey, index));
            PlayerPrefs.DeleteKey(BuildKey(PlayerWallet.PersistenceBaseKey, index));
            PlayerPrefs.DeleteKey(BuildKey(CampaignProgress.Level20UnlockedBaseKey, index));
            PlayerPrefs.DeleteKey(BuildKey(CampaignProgress.Level30UnlockedBaseKey, index));
            PlayerPrefs.DeleteKey(BuildKey(CampaignProgress.SkullKillCountBaseKey, index));
            PlayerPrefs.DeleteKey(BuildKey(CampaignProgress.GameCompletedBaseKey, index));
            PlayerPrefs.DeleteKey(BuildKey(CampaignProgress.MegaMagpieKilledBaseKey, index));
            PlayerPrefs.Save();

            if (ActiveProfileIndex == index)
            {
                ActiveProfileIndex = -1;
            }
        }

        /// <summary>Called right before leaving the Save screen for Character Selection — every namespaced system self-heals its cache against this the next time it's read (see CharacterProgressTracker/CharacterStarUpgrades's Data accessor), so no explicit reload plumbing is needed here.</summary>
        public static void SetActiveProfile(int index)
        {
            ActiveProfileIndex = index;
        }

        /// <summary>Reads this save's achievement completion percent without actually selecting it — briefly points ActiveProfileIndex at it (so AchievementRegistry's CharacterProgressTracker/CampaignProgress reads resolve to the right PlayerPrefs keys), then restores whatever was active before. Lets the Save screen show every slot's own progress up front, before the player picks one.</summary>
        public static int PeekAchievementPercent(int index)
        {
            int previous = ActiveProfileIndex;
            ActiveProfileIndex = index;
            int percent = AchievementRegistry.GetCompletionPercent();
            ActiveProfileIndex = previous;
            return percent;
        }

        /// <summary>Scopes a base PlayerPrefs key to the currently active save profile. Falls back to the bare base key when no profile is active yet, so reading progress before the Save screen ever runs (e.g. opening Arena Run directly for testing) still behaves like before this system existed.</summary>
        public static string ScopedKey(string baseKey)
        {
            return ActiveProfileIndex >= 0 ? BuildKey(baseKey, ActiveProfileIndex) : baseKey;
        }

        private static string BuildKey(string baseKey, int index)
        {
            return $"{baseKey}.Save{index}";
        }

        /// <summary>
        /// Runs once ever, the very first time the save list is read on a given machine/install
        /// (guarded by SeedImportedKey so it never re-triggers, even if Slot 1 is later deleted).
        /// If Assets/StreamingAssets/save_seed.json shipped with this build (see SaveSeedExporter)
        /// and Slot 1 is still empty, imports it there — lets every fresh install of a build start
        /// with a baseline save instead of 3 empty slots.
        /// </summary>
        private static void TryImportSeedOnFirstRun()
        {
            if (PlayerPrefs.GetInt(SeedImportedKey, 0) == 1) return;

            PlayerPrefs.SetInt(SeedImportedKey, 1); // mark first, so a bad/missing seed file can't retry forever
            PlayerPrefs.Save();

            if (cachedList.slots[0].exists) return;

            string path = System.IO.Path.Combine(Application.streamingAssetsPath, SeedFileName);
            if (!System.IO.File.Exists(path)) return;

            SaveSeedData seed;
            try
            {
                seed = JsonUtility.FromJson<SaveSeedData>(System.IO.File.ReadAllText(path));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"SaveProfileManager: failed to parse {SeedFileName}, skipping seed import. {e.Message}");
                return;
            }

            if (seed == null) return;

            PlayerPrefs.SetString(BuildKey(CharacterProgressTracker.PersistenceBaseKey, 0), seed.characterProgressJson);
            PlayerPrefs.SetString(BuildKey(CharacterStarUpgrades.PersistenceBaseKey, 0), seed.characterStarUpgradesJson);
            PlayerPrefs.SetInt(BuildKey(PlayerWallet.PersistenceBaseKey, 0), seed.totalCoins);
            PlayerPrefs.SetInt(BuildKey(CampaignProgress.Level20UnlockedBaseKey, 0), seed.level20Unlocked ? 1 : 0);
            PlayerPrefs.SetInt(BuildKey(CampaignProgress.Level30UnlockedBaseKey, 0), seed.level30Unlocked ? 1 : 0);
            PlayerPrefs.SetInt(BuildKey(CampaignProgress.SkullKillCountBaseKey, 0), seed.skullKillCount);
            PlayerPrefs.SetInt(BuildKey(CampaignProgress.GameCompletedBaseKey, 0), seed.gameCompleted ? 1 : 0);
            PlayerPrefs.SetInt(BuildKey(CampaignProgress.MegaMagpieKilledBaseKey, 0), seed.megaMagpieKilled ? 1 : 0);

            cachedList.slots[0].exists = true;
            cachedList.slots[0].profileName = string.IsNullOrEmpty(seed.profileName) ? "Save1" : seed.profileName;
            SaveList();
        }

        private static SaveProfileListData LoadList()
        {
            string json = PlayerPrefs.GetString(ProfilesKey, string.Empty);
            return string.IsNullOrEmpty(json) ? new SaveProfileListData() : JsonUtility.FromJson<SaveProfileListData>(json);
        }

        private static void SaveList()
        {
            PlayerPrefs.SetString(ProfilesKey, JsonUtility.ToJson(cachedList));
            PlayerPrefs.Save();
        }
    }
}
