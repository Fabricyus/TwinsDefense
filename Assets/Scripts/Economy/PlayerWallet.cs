using System;
using UnityEngine;
using TwinsDefense.Systems;

namespace TwinsDefense.Economy
{
    /// <summary>
    /// Persistent meta-currency (Coins) earned from each run's Game Over
    /// summary total and later spent on character upgrades in Character
    /// Selection. Backed directly by PlayerPrefs since no project save system
    /// exists yet — same placeholder-persistence rationale as
    /// CharacterProgressTracker. Scoped per save profile via SaveProfileManager.
    /// </summary>
    public static class PlayerWallet
    {
        public const string PersistenceBaseKey = "TwinsDefense.TotalCoins";
        private static string TotalCoinsKey => SaveProfileManager.ScopedKey(PersistenceBaseKey);

        public static int TotalCoins => PlayerPrefs.GetInt(TotalCoinsKey, 0);

        /// <summary>Raised whenever TotalCoins changes, so UI can refresh without polling.</summary>
        public static event Action<int> OnCoinsChanged;

        /// <summary>Called once per run when the Game Over summary total is revealed.</summary>
        public static void AddCoins(int amount)
        {
            if (amount <= 0) return;

            PlayerPrefs.SetInt(TotalCoinsKey, TotalCoins + amount);
            PlayerPrefs.Save();
            OnCoinsChanged?.Invoke(TotalCoins);
        }

        /// <summary>Called when the player spends Coins on a character star upgrade.</summary>
        public static void SpendCoins(int amount)
        {
            if (amount <= 0) return;

            PlayerPrefs.SetInt(TotalCoinsKey, Mathf.Max(0, TotalCoins - amount));
            PlayerPrefs.Save();
            OnCoinsChanged?.Invoke(TotalCoins);
        }
    }
}
