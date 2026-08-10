using System;
using UnityEngine;

namespace TwinsDefense.Economy
{
    /// <summary>
    /// Tracks the player's run currency (Coins) collected from enemy drops.
    /// One instance lives in the Arena Run scene. Separate from GemsManager,
    /// which is the legacy Tower Defense currency.
    /// </summary>
    public class CoinManager : MonoBehaviour
    {
        public static CoinManager Instance { get; private set; }

        public int CurrentCoins { get; private set; }

        /// <summary>Raised whenever CurrentCoins changes, so UI can refresh without polling.</summary>
        public event Action<int> OnCoinsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            OnCoinsChanged?.Invoke(CurrentCoins);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;

            CurrentCoins += amount;
            OnCoinsChanged?.Invoke(CurrentCoins);
        }
    }
}
