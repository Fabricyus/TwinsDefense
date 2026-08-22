namespace TwinsDefense.Systems
{
    /// <summary>
    /// Snapshot of a single save's progress, shipped as Assets/StreamingAssets/save_seed.json
    /// (see SaveSeedExporter for how it's generated). SaveProfileManager imports this into
    /// Slot 1 the very first time the game runs on a machine with no saves yet, so every
    /// build/machine starts with this baseline instead of an empty slot.
    /// </summary>
    [System.Serializable]
    public class SaveSeedData
    {
        public string profileName = "Save1";
        public string characterProgressJson = string.Empty;
        public string characterStarUpgradesJson = string.Empty;
        public int totalCoins;
        public bool level20Unlocked;
        public bool level30Unlocked;
        public int skullKillCount;
        public bool gameCompleted;
        public bool megaMagpieKilled;
    }
}
