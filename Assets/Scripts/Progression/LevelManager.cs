using System;
using UnityEngine;

namespace TwinsDefense.Progression
{
    /// <summary>
    /// Tracks the player's in-run level and XP slider. Each Exp pickup adds a
    /// fixed percentage to the slider; reaching 100% pauses the game and opens
    /// the upgrade card selection. Card resolution (picking a card, advancing
    /// the level, resetting the slider) is not implemented yet.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Up")]
        [SerializeField] private GameObject cardsPanel;
        [Range(0f, 1f)]
        [SerializeField] private float expPerPickup = 0.01f;

        public int CurrentLevel { get; private set; }
        public float CurrentExp { get; private set; }

        /// <summary>Raised whenever CurrentLevel changes, so UI can refresh without polling.</summary>
        public event Action<int> OnLevelChanged;

        /// <summary>Raised whenever CurrentExp changes (0..1), so the slider can refresh without polling.</summary>
        public event Action<float> OnExpChanged;

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
            OnLevelChanged?.Invoke(CurrentLevel);
            OnExpChanged?.Invoke(CurrentExp);
        }

        /// <summary>Adds one pickup's worth of XP, triggering a level-up once the slider is full.</summary>
        public void AddExp()
        {
            if (CurrentExp >= 1f) return;

            CurrentExp = Mathf.Min(CurrentExp + expPerPickup, 1f);

            // Repeated float addition (e.g. 100x 0.01) drifts just short of 1f,
            // so snap to exactly full once within a hair of the cap.
            if (CurrentExp >= 1f - 0.0001f)
            {
                CurrentExp = 1f;
            }

            OnExpChanged?.Invoke(CurrentExp);

            if (CurrentExp >= 1f)
            {
                TriggerLevelUp();
            }
        }

        private void TriggerLevelUp()
        {
            Time.timeScale = 0f;

            if (cardsPanel != null)
            {
                cardsPanel.SetActive(true);
            }
        }
    }
}
