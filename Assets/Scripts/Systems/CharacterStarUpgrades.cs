using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Economy;

namespace TwinsDefense.Systems
{
    /// <summary>
    /// Persists each character tier's purchased Attack Star count (0-5),
    /// spending Coins from PlayerWallet. Star costs rise per purchase: 1000,
    /// 2500, 7500, 15000, 50000. Every character starts at zero stars. Same
    /// placeholder PlayerPrefs+JSON persistence as CharacterProgressTracker.
    /// </summary>
    public class CharacterStarUpgrades : MonoBehaviour
    {
        private static CharacterStarUpgrades instance;

        public static CharacterStarUpgrades Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(CharacterStarUpgrades));
                    instance = go.AddComponent<CharacterStarUpgrades>();
                }

                return instance;
            }
        }

        /// <summary>Coin cost of each star, in purchase order (index 0 = first star bought, ... index 4 = fifth/final star).</summary>
        private static readonly int[] StarCosts = { 1000, 2500, 7500, 15000, 50000 };

        public static int MaxStars => StarCosts.Length;

        public const string PersistenceBaseKey = "TwinsDefense.CharacterStarUpgrades";
        private static string PersistenceKey => SaveProfileManager.ScopedKey(PersistenceBaseKey);

        [System.Serializable]
        private class StarEntry
        {
            public string slotId;
            public int stars;
        }

        [System.Serializable]
        private class SaveData
        {
            public List<StarEntry> entries = new List<StarEntry>();
        }

        private SaveData data;
        private int loadedForProfileIndex = int.MinValue;

        /// <summary>Self-heals against SaveProfileManager.ActiveProfileIndex changing after this was first loaded — this singleton can be created before the player has picked a save profile.</summary>
        private SaveData Data
        {
            get
            {
                if (data == null || loadedForProfileIndex != SaveProfileManager.ActiveProfileIndex)
                {
                    Load();
                    loadedForProfileIndex = SaveProfileManager.ActiveProfileIndex;
                }

                return data;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public int GetStars(string slotId)
        {
            StarEntry entry = Data.entries.Find(e => e.slotId == slotId);
            return entry != null ? entry.stars : 0;
        }

        /// <summary>Coin cost of this slot's next star, or -1 once all stars are purchased.</summary>
        public int GetNextStarCost(string slotId)
        {
            int current = GetStars(slotId);
            return current < StarCosts.Length ? StarCosts[current] : -1;
        }

        /// <summary>Spends Coins and adds one star if the slot isn't already maxed and the player can afford it. Returns whether the purchase happened.</summary>
        public bool TryPurchaseStar(string slotId)
        {
            int cost = GetNextStarCost(slotId);
            if (cost < 0 || PlayerWallet.TotalCoins < cost) return false;

            PlayerWallet.SpendCoins(cost);

            StarEntry entry = Data.entries.Find(e => e.slotId == slotId);
            if (entry == null)
            {
                entry = new StarEntry { slotId = slotId, stars = 0 };
                Data.entries.Add(entry);
            }

            entry.stars++;
            Save();
            return true;
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
