using TMPro;
using UnityEngine;
using TwinsDefense.Systems;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Refreshes every achievement row's text and color (green once complete)
    /// whenever the Achievements panel is opened. The achievement list itself
    /// lives in AchievementRegistry (shared with AchievementUnlockTracker's
    /// Arena Run popup queue, so the two never drift out of sync). Progress
    /// is read live from CampaignProgress and CharacterProgressTracker — both
    /// PlayerPrefs-backed, so this works from the Main Menu with no active
    /// run. labels[] must be wired in the same order as AchievementRegistry.All.
    /// </summary>
    public class AchievementsPanelController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] labels;
        [SerializeField] private TextMeshProUGUI completionPercentLabel;
        [SerializeField] private Color incompleteColor = new Color(0.92f, 0.88f, 0.8f, 1f);
        [SerializeField] private Color completeColor = new Color(0.45f, 0.85f, 0.4f, 1f);

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            AchievementDef[] achievements = AchievementRegistry.All;

            if (completionPercentLabel != null && achievements.Length > 0)
            {
                completionPercentLabel.text = $"{AchievementRegistry.GetCompletionPercent()}%";
            }

            if (labels == null) return;

            for (int i = 0; i < achievements.Length && i < labels.Length; i++)
            {
                if (labels[i] == null) continue;

                int current = Mathf.Min(achievements[i].getCurrent(), achievements[i].required);
                bool complete = current >= achievements[i].required;

                // Secret achievements hide how to unlock them until they're actually
                // unlocked — the progress fraction still shows, just not the condition.
                string description = achievements[i].isSecret && !complete ? "???" : achievements[i].description;

                labels[i].text = $"{description} ({current}/{achievements[i].required})";
                labels[i].color = complete ? completeColor : incompleteColor;
            }
        }
    }
}
