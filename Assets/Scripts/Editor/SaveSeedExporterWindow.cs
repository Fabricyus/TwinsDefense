using System.IO;
using UnityEditor;
using UnityEngine;
using TwinsDefense.Systems;

namespace TwinsDefense.EditorTools
{
    /// <summary>
    /// One-off/rerunnable tool: snapshots whatever save data currently sits in the UNSCOPED
    /// legacy PlayerPrefs keys (i.e. read with SaveProfileManager.ActiveProfileIndex == -1,
    /// which is exactly the single global save the game used before the multi-profile system
    /// existed) into Assets/StreamingAssets/save_seed.json. SaveProfileManager imports this
    /// file into Slot 1 automatically the very first time the game ever runs on a machine with
    /// no saves yet — see SaveProfileManager.TryImportSeedOnFirstRun — so every build/machine
    /// starts with this baseline progress instead of an empty Slot 1. Re-run this any time you
    /// want a future build to ship with more recent progress as its starting seed.
    /// </summary>
    public class SaveSeedExporterWindow : EditorWindow
    {
        private const string SeedFileName = "save_seed.json";

        private string profileName = "Save1";

        [MenuItem("Tools/TwinsDefense/Export Legacy Save As Seed...")]
        private static void Open()
        {
            SaveSeedExporterWindow window = GetWindow<SaveSeedExporterWindow>(true, "Export Legacy Save As Seed");
            window.minSize = new Vector2(420, 160);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Snapshots the current UNSCOPED legacy PlayerPrefs save (the single global save " +
                "from before Slots 1-3 existed) into Assets/StreamingAssets/save_seed.json. Any " +
                "future build will auto-import this into Slot 1 the first time it runs on a machine " +
                "with no saves yet.",
                MessageType.Info);

            EditorGUILayout.Space();
            profileName = EditorGUILayout.TextField("Seed profile name", profileName);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(profileName)))
            {
                if (GUILayout.Button("Export"))
                {
                    Export(profileName.Trim());
                    Close();
                }
            }
        }

        private static void Export(string profileName)
        {
            var seed = new SaveSeedData
            {
                profileName = profileName,
                characterProgressJson = PlayerPrefs.GetString(CharacterProgressTracker.PersistenceBaseKey, string.Empty),
                characterStarUpgradesJson = PlayerPrefs.GetString(CharacterStarUpgrades.PersistenceBaseKey, string.Empty),
                totalCoins = PlayerPrefs.GetInt(TwinsDefense.Economy.PlayerWallet.PersistenceBaseKey, 0),
                level20Unlocked = PlayerPrefs.GetInt(CampaignProgress.Level20UnlockedBaseKey, 0) == 1,
                level30Unlocked = PlayerPrefs.GetInt(CampaignProgress.Level30UnlockedBaseKey, 0) == 1,
                skullKillCount = PlayerPrefs.GetInt(CampaignProgress.SkullKillCountBaseKey, 0),
                gameCompleted = PlayerPrefs.GetInt(CampaignProgress.GameCompletedBaseKey, 0) == 1,
                megaMagpieKilled = PlayerPrefs.GetInt(CampaignProgress.MegaMagpieKilledBaseKey, 0) == 1,
            };

            string dir = Path.Combine(Application.dataPath, "StreamingAssets");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = Path.Combine(dir, SeedFileName);
            File.WriteAllText(path, JsonUtility.ToJson(seed, true));

            AssetDatabase.Refresh();
            Debug.Log($"SaveSeedExporter: wrote seed for profile '{seed.profileName}' to Assets/StreamingAssets/{SeedFileName}");
        }
    }
}
