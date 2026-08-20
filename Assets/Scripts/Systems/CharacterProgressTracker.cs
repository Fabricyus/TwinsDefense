using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.Systems
{
    /// <summary>
    /// Tracks progress toward each character tier's unlock condition and
    /// persists it immediately on every report (not save-on-quit only, so a
    /// crash mid-run doesn't lose an unlock). No project save system existed
    /// at the time this was written, so this uses PlayerPrefs + JsonUtility as
    /// a placeholder persistence layer — swap PersistenceKey's storage for the
    /// real save system once one exists.
    /// </summary>
    public class CharacterProgressTracker : MonoBehaviour
    {
        private static CharacterProgressTracker instance;

        /// <summary>Lazily creates the tracker if no instance has been placed in the current scene yet, so report calls from any scene never NullReferenceException.</summary>
        public static CharacterProgressTracker Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(CharacterProgressTracker));
                    instance = go.AddComponent<CharacterProgressTracker>();
                }

                return instance;
            }
        }

        private const string PersistenceKey = "TwinsDefense.CharacterProgress";

        [System.Serializable]
        private class LevelEntry
        {
            public CharacterId character;
            public int highestLevel;
        }

        /// <summary>Highest level tracked per exact (character, tier) pair — e.g. Izzy PopStar separately from Izzy Base, unlike LevelEntry above which lumps every tier of a character together. Used to gate the Rainbow Aura cosmetic to only the specific evolution that actually earned it.</summary>
        [System.Serializable]
        private class TierLevelEntry
        {
            public CharacterId character;
            public int tier;
            public int highestLevel;
        }

        /// <summary>One of the 12 "Flawless Form" challenge achievements (see ChallengeDefinitions) completed — killed the Magpie as this exact character tier without ever breaking that tier's rule.</summary>
        [System.Serializable]
        private class ChallengeEntry
        {
            public CharacterId character;
            public int tier;
        }

        [System.Serializable]
        private class CardPickEntry
        {
            public CharacterId character;
            public string cardId;
            public int count;
        }

        [System.Serializable]
        private class BossKillEntry
        {
            public CharacterId character;
            public int bossLevel;
            public int characterTier;
        }

        [System.Serializable]
        private class SpecialCardPickEntry
        {
            public CharacterId character;
            public int count;
        }

        [System.Serializable]
        private class SaveData
        {
            public List<LevelEntry> levels = new List<LevelEntry>();
            public List<TierLevelEntry> tierLevels = new List<TierLevelEntry>();
            public List<ChallengeEntry> completedChallenges = new List<ChallengeEntry>();
            public List<CardPickEntry> cardPicks = new List<CardPickEntry>();
            public List<BossKillEntry> bossKills = new List<BossKillEntry>();
            public List<SpecialCardPickEntry> specialCardPicks = new List<SpecialCardPickEntry>();
        }

        private SaveData data;

        /// <summary>Lazily loads on first access instead of relying solely on Awake() — AddComponent doesn't reliably run Awake() synchronously when Instance creates this outside Play mode (e.g. an Editor menu item), which left data null and NullReferenceException'd every accessor.</summary>
        private SaveData Data => data ??= LoadData();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            data = LoadData();
        }

        public bool IsUnlocked(CharacterMetaData meta)
        {
            if (meta == null || meta.unlockCondition == null) return true;

            CharacterUnlockCondition condition = meta.unlockCondition;

            switch (condition.type)
            {
                case UnlockConditionType.None:
                    return true;

                case UnlockConditionType.ReachLevelFirstTime:
                {
                    LevelEntry entry = Data.levels.Find(e => e.character == meta.characterId);
                    return entry != null && entry.highestLevel >= condition.requiredLevel;
                }

                case UnlockConditionType.AccumulateCardPicks:
                {
                    CardPickEntry entry = Data.cardPicks.Find(e => e.character == meta.characterId && e.cardId == condition.requiredCardId);
                    return entry != null && entry.count >= condition.requiredCount;
                }

                case UnlockConditionType.KillBossAtTier:
                    return Data.bossKills.Exists(e => e.character == meta.characterId
                        && e.bossLevel == condition.requiredBossLevel
                        && e.characterTier == condition.requiredCharacterTier);

                case UnlockConditionType.AccumulateSpecialCardPicks:
                {
                    SpecialCardPickEntry entry = Data.specialCardPicks.Find(e => e.character == meta.characterId);
                    return entry != null && entry.count >= condition.requiredCount;
                }

                default:
                    return false;
            }
        }

        /// <summary>Highest level ever reached playing this character (any tier), 0 if never played — used by the Achievements panel to show live progress toward ReachLevelFirstTime unlocks and the 3 Rainbow Aura achievement rows (the achievement itself stays character-wide).</summary>
        public int GetHighestLevel(CharacterId character)
        {
            LevelEntry entry = Data.levels.Find(e => e.character == character);
            return entry != null ? entry.highestLevel : 0;
        }

        /// <summary>Highest level ever reached playing this EXACT character tier (e.g. Izzy PopStar specifically), 0 if never played at this tier — used to gate where the Rainbow Aura cosmetic actually shows (only the evolution that earned it), separate from the character-wide achievement tracked by GetHighestLevel.</summary>
        public int GetHighestLevelForTier(CharacterId character, int tier)
        {
            TierLevelEntry entry = Data.tierLevels.Find(e => e.character == character && e.tier == tier);
            return entry != null ? entry.highestLevel : 0;
        }

/// <summary>Whether the "Flawless Form" challenge for this exact character tier (see ChallengeDefinitions) has already been completed.</summary>
        public bool HasCompletedChallenge(CharacterId character, int tier)
        {
            return Data.completedChallenges.Exists(e => e.character == character && e.tier == tier);
        }

        /// <summary>Marks this exact character tier's "Flawless Form" challenge as completed. Safe to call repeatedly — a no-op once already recorded.</summary>
        public void ReportChallengeCompleted(CharacterId character, int tier)
        {
            if (HasCompletedChallenge(character, tier)) return;

            Data.completedChallenges.Add(new ChallengeEntry { character = character, tier = tier });
            Save();
        }


        /// <summary>Special (buff+debuff) cards picked so far while playing this character, 0 if none — used by the Achievements panel to show live progress toward AccumulateSpecialCardPicks unlocks.</summary>
        public int GetSpecialCardPickCount(CharacterId character)
        {
            SpecialCardPickEntry entry = Data.specialCardPicks.Find(e => e.character == character);
            return entry != null ? entry.count : 0;
        }

        /// <summary>Whether this exact boss-level/tier kill has already been reported — used by the Achievements panel to show live progress toward KillBossAtTier unlocks.</summary>
        public bool HasKilledBossAtTier(CharacterId character, int bossLevel, int characterTier)
        {
            return Data.bossKills.Exists(e => e.character == character && e.bossLevel == bossLevel && e.characterTier == characterTier);
        }

        public void ReportLevelReached(CharacterId character, int level)
        {
            LevelEntry entry = Data.levels.Find(e => e.character == character);

            if (entry == null)
            {
                Data.levels.Add(new LevelEntry { character = character, highestLevel = level });
                Save();
                return;
            }

            if (level > entry.highestLevel)
            {
                entry.highestLevel = level;
                Save();
            }
        }

public void ReportLevelReachedForTier(CharacterId character, int tier, int level)
        {
            TierLevelEntry entry = Data.tierLevels.Find(e => e.character == character && e.tier == tier);

            if (entry == null)
            {
                Data.tierLevels.Add(new TierLevelEntry { character = character, tier = tier, highestLevel = level });
                Save();
                return;
            }

            if (level > entry.highestLevel)
            {
                entry.highestLevel = level;
                Save();
            }
        }


        public void ReportCardPicked(CharacterId character, string cardId)
        {
            CardPickEntry entry = Data.cardPicks.Find(e => e.character == character && e.cardId == cardId);

            if (entry == null)
            {
                Data.cardPicks.Add(new CardPickEntry { character = character, cardId = cardId, count = 1 });
            }
            else
            {
                entry.count++;
            }

            Save();
        }

        /// <summary>Counts a pick toward AccumulateSpecialCardPicks — call only when the picked card is special (isSpecial), regardless of which one.</summary>
        public void ReportSpecialCardPicked(CharacterId character)
        {
            SpecialCardPickEntry entry = Data.specialCardPicks.Find(e => e.character == character);

            if (entry == null)
            {
                Data.specialCardPicks.Add(new SpecialCardPickEntry { character = character, count = 1 });
            }
            else
            {
                entry.count++;
            }

            Save();
        }

        public void ReportBossKilled(CharacterId character, int bossLevel, int characterTierPlayed)
        {
            bool alreadyAchieved = Data.bossKills.Exists(e => e.character == character
                && e.bossLevel == bossLevel
                && e.characterTier == characterTierPlayed);

            if (alreadyAchieved) return;

            Data.bossKills.Add(new BossKillEntry { character = character, bossLevel = bossLevel, characterTier = characterTierPlayed });
            Save();
        }

        private static SaveData LoadData()
        {
            string json = PlayerPrefs.GetString(PersistenceKey, string.Empty);
            return string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
        }

        private void Save()
        {
            PlayerPrefs.SetString(PersistenceKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
