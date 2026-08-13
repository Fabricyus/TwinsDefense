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
        private class SaveData
        {
            public List<LevelEntry> levels = new List<LevelEntry>();
            public List<CardPickEntry> cardPicks = new List<CardPickEntry>();
            public List<BossKillEntry> bossKills = new List<BossKillEntry>();
        }

        private SaveData data;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
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
                    LevelEntry entry = data.levels.Find(e => e.character == meta.characterId);
                    return entry != null && entry.highestLevel >= condition.requiredLevel;
                }

                case UnlockConditionType.AccumulateCardPicks:
                {
                    CardPickEntry entry = data.cardPicks.Find(e => e.character == meta.characterId && e.cardId == condition.requiredCardId);
                    return entry != null && entry.count >= condition.requiredCount;
                }

                case UnlockConditionType.KillBossAtTier:
                    return data.bossKills.Exists(e => e.character == meta.characterId
                        && e.bossLevel == condition.requiredBossLevel
                        && e.characterTier == condition.requiredCharacterTier);

                default:
                    return false;
            }
        }

        public void ReportLevelReached(CharacterId character, int level)
        {
            LevelEntry entry = data.levels.Find(e => e.character == character);

            if (entry == null)
            {
                data.levels.Add(new LevelEntry { character = character, highestLevel = level });
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
            CardPickEntry entry = data.cardPicks.Find(e => e.character == character && e.cardId == cardId);

            if (entry == null)
            {
                data.cardPicks.Add(new CardPickEntry { character = character, cardId = cardId, count = 1 });
            }
            else
            {
                entry.count++;
            }

            Save();
        }

        public void ReportBossKilled(CharacterId character, int bossLevel, int characterTierPlayed)
        {
            bool alreadyAchieved = data.bossKills.Exists(e => e.character == character
                && e.bossLevel == bossLevel
                && e.characterTier == characterTierPlayed);

            if (alreadyAchieved) return;

            data.bossKills.Add(new BossKillEntry { character = character, bossLevel = bossLevel, characterTier = characterTierPlayed });
            Save();
        }

        private void Load()
        {
            string json = PlayerPrefs.GetString(PersistenceKey, string.Empty);
            data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
        }

        private void Save()
        {
            PlayerPrefs.SetString(PersistenceKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
