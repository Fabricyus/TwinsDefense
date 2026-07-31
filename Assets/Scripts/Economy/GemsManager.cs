using System;
using UnityEngine;

namespace TwinsDefense.Economy
{
    /// <summary>
    /// Tracks the player's single in-run currency (Gems) used to summon and
    /// upgrade towers. One instance lives in the gameplay scene.
    /// </summary>
    public class GemsManager : MonoBehaviour
    {
        public static GemsManager Instance { get; private set; }

        [Header("Economy")]
        [SerializeField] private int startingGems = 100;

        public int CurrentGems { get; private set; }

        /// <summary>Raised whenever CurrentGems changes, so UI can refresh without polling.</summary>
        public event Action<int> OnGemsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CurrentGems = startingGems;
        }

        private void Start()
        {
            OnGemsChanged?.Invoke(CurrentGems);
        }

        public bool HasEnough(int cost) => CurrentGems >= cost;

        public void Spend(int cost)
        {
            if (cost <= 0) return;

            CurrentGems -= cost;
            OnGemsChanged?.Invoke(CurrentGems);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;

            CurrentGems += amount;
            OnGemsChanged?.Invoke(CurrentGems);
        }
    }
}
