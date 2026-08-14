using UnityEngine;

namespace TwinsDefense.Data
{
    public enum UnlockConditionType
    {
        None,                       // tier 1, always unlocked
        ReachLevelFirstTime,        // e.g. reach level 10 once, with this character
        AccumulateCardPicks,        // e.g. pick a specific card 10x total, across runs, with this character
        KillBossAtTier,             // e.g. kill the level-30 boss while playing a specific tier of this character
        AccumulateSpecialCardPicks  // e.g. pick any 10 special (buff+debuff) cards total, across runs, with this character
    }

    [System.Serializable]
    public class CharacterUnlockCondition
    {
        public UnlockConditionType type;

        [Header("ReachLevelFirstTime")]
        public int requiredLevel;

        [Header("AccumulateCardPicks / AccumulateSpecialCardPicks")]
        [Tooltip("Only used by AccumulateCardPicks — matches CardData.cardId. Ignored by AccumulateSpecialCardPicks, which counts any special card pick.")]
        public string requiredCardId;
        public int requiredCount;

        [Header("KillBossAtTier")]
        public int requiredBossLevel;
        public int requiredCharacterTier; // must be playing this tier when the kill happens
    }
}
