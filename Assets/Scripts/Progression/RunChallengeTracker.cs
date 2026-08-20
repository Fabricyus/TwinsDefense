using UnityEngine;

namespace TwinsDefense.Progression
{
    /// <summary>
    /// In-run compliance state for the 12 "Flawless Form" challenge
    /// achievements (see ChallengeDefinitions) — one instance lives in the
    /// Arena Run scene, reset fresh every run, same lifecycle as RunStats.
    /// PlayerHealth and LevelUpCardsUI report into this as the run happens;
    /// EnemySpawner reads it once, at the Magpie kill, to decide whether this
    /// run's specific challenge (for whichever character/tier is currently
    /// selected) was actually completed.
    /// </summary>
    public class RunChallengeTracker : MonoBehaviour
    {
        public static RunChallengeTracker Instance { get; private set; }

        public bool TookDamage { get; private set; }
        public bool PickedForbiddenCard { get; private set; }
        public bool PickedNonMiddleOption { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RegisterDamageTaken()
        {
            TookDamage = true;
        }

        public void RegisterForbiddenCardPicked()
        {
            PickedForbiddenCard = true;
        }

        public void RegisterNonMiddleOptionPicked()
        {
            PickedNonMiddleOption = true;
        }

        /// <summary>Whether this run's compliance state so far satisfies the given challenge's rule.</summary>
        public bool SatisfiesRule(ChallengeDefinition definition)
        {
            switch (definition.ruleType)
            {
                case ChallengeRuleType.Flawless:
                    return !TookDamage;
                case ChallengeRuleType.ForbiddenCards:
                    return !PickedForbiddenCard;
                case ChallengeRuleType.AlwaysMiddleOption:
                    return !PickedNonMiddleOption;
                default:
                    return false;
            }
        }
    }
}
