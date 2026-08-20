using UnityEditor;
using UnityEngine;
using TwinsDefense.Progression;
using TwinsDefense.Systems;

namespace TwinsDefense.EditorTools
{
    /// <summary>
    /// Editor-only shortcut to mark every "Flawless Form" challenge (see
    /// ChallengeDefinitions) as completed without having to actually clear all
    /// 12 Magpie runs — every Exclusive card (Assets/Data/Cards/Exclusive,
    /// gated by CardData.requiredChallengeTier) becomes draftable on its own
    /// restricted character tier as soon as this runs. Works with the game
    /// closed — same PlayerPrefs+JSON persistence as DebugCoinsMenu's PlayerWallet.
    /// </summary>
    public static class DebugChallengesMenu
    {
        [MenuItem("Tools/TwinsDefense/Debug/Unlock All Exclusive Cards (Complete All Challenges)")]
        public static void UnlockAllExclusiveCards()
        {
            foreach (ChallengeDefinition challenge in ChallengeDefinitions.All)
            {
                CharacterProgressTracker.Instance.ReportChallengeCompleted(challenge.character, challenge.tier);
            }

            Debug.Log($"DebugChallengesMenu: marked all {ChallengeDefinitions.All.Length} Flawless Form challenges as completed — every Exclusive card is now draftable on its own character tier.");

            CleanupTrackerInstance();
        }

        /// <summary>CharacterProgressTracker.Instance lazily spawns a runtime GameObject to hold the singleton — data is already persisted to PlayerPrefs by the time this runs, so remove it instead of leaving a stray object behind in whichever scene happens to be open.</summary>
        private static void CleanupTrackerInstance()
        {
            CharacterProgressTracker tracker = Object.FindFirstObjectByType<CharacterProgressTracker>();
            if (tracker != null)
            {
                Object.DestroyImmediate(tracker.gameObject);
            }
        }
    }
}
