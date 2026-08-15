using System;
using TMPro;
using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Systems;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Refreshes every achievement row's text and color (green once complete)
    /// whenever the Achievements panel is opened. Progress is read live from
    /// CampaignProgress (the 3 boss-rush milestones) and
    /// CharacterProgressTracker (the 9 character-tier unlocks) — both
    /// PlayerPrefs-backed, so this works from the Main Menu with no active
    /// run. labels[] must be wired in the same order as the achievements
    /// list built in Awake.
    /// </summary>
    public class AchievementsPanelController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] labels;
        [SerializeField] private Color incompleteColor = new Color(0.92f, 0.88f, 0.8f, 1f);
        [SerializeField] private Color completeColor = new Color(0.45f, 0.85f, 0.4f, 1f);

        private struct AchievementDef
        {
            public string description;
            public int required;
            public Func<int> getCurrent;

            public AchievementDef(string description, int required, Func<int> getCurrent)
            {
                this.description = description;
                this.required = required;
                this.getCurrent = getCurrent;
            }
        }

        private AchievementDef[] achievements;

        private void Awake()
        {
            achievements = new AchievementDef[]
            {
                new AchievementDef("Unlock Level 11+ by killing the Reaper", 1,
                    () => CampaignProgress.Level20Unlocked ? 1 : 0),
                new AchievementDef("Unlock Level 21+ by killing the Skull", CampaignProgress.SkullKillsRequiredForLevel30,
                    () => CampaignProgress.SkullKillCount),
                new AchievementDef("Complete the game by defeating the Magpie", 1,
                    () => CampaignProgress.GameCompleted ? 1 : 0),

                new AchievementDef("Unlock Izzy Blaze by reaching Level 10 as Izzy", 10,
                    () => CharacterProgressTracker.Instance.GetHighestLevel(CharacterId.Izzy)),
                new AchievementDef("Unlock Izzy Archer by picking 10 special cards as Izzy", 10,
                    () => CharacterProgressTracker.Instance.GetSpecialCardPickCount(CharacterId.Izzy)),
                new AchievementDef("Unlock Izzy PopStar by defeating the Magpie as Izzy Archer", 1,
                    () => CharacterProgressTracker.Instance.HasKilledBossAtTier(CharacterId.Izzy, 30, 3) ? 1 : 0),

                new AchievementDef("Unlock Frost Court by reaching Level 10 as Court", 10,
                    () => CharacterProgressTracker.Instance.GetHighestLevel(CharacterId.Court)),
                new AchievementDef("Unlock Court Reader by picking 10 special cards as Court", 10,
                    () => CharacterProgressTracker.Instance.GetSpecialCardPickCount(CharacterId.Court)),
                new AchievementDef("Unlock Dark Court by defeating the Magpie as Court Reader", 1,
                    () => CharacterProgressTracker.Instance.HasKilledBossAtTier(CharacterId.Court, 30, 3) ? 1 : 0),

                new AchievementDef("Unlock Priest Ralph by reaching Level 10 as Ralph", 10,
                    () => CharacterProgressTracker.Instance.GetHighestLevel(CharacterId.Ralph)),
                new AchievementDef("Unlock Paladin Ralph by picking 10 special cards as Ralph", 10,
                    () => CharacterProgressTracker.Instance.GetSpecialCardPickCount(CharacterId.Ralph)),
                new AchievementDef("Unlock Cute Ralph by defeating the Magpie as Paladin Ralph", 1,
                    () => CharacterProgressTracker.Instance.HasKilledBossAtTier(CharacterId.Ralph, 30, 3) ? 1 : 0),
            };
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (achievements == null || labels == null) return;

            for (int i = 0; i < achievements.Length && i < labels.Length; i++)
            {
                if (labels[i] == null) continue;

                int current = Mathf.Min(achievements[i].getCurrent(), achievements[i].required);
                bool complete = current >= achievements[i].required;

                labels[i].text = $"{achievements[i].description} ({current}/{achievements[i].required})";
                labels[i].color = complete ? completeColor : incompleteColor;
            }
        }
    }
}
