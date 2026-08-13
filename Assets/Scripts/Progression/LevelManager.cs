using System;
using UnityEngine;
using TwinsDefense.Systems;

namespace TwinsDefense.Progression
{
    /// <summary>
    /// Tracks the player's in-run level and XP slider. Each Exp pickup adds a
    /// fixed percentage to the slider; reaching 100% advances the level,
    /// resets the slider, and pauses the game to open the upgrade card
    /// selection.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Up")]
        [SerializeField] private GameObject cardsPanel;
        [Tooltip("XP granted per pickup at level 0, before per-level decay.")]
        [Range(0f, 1f)]
        [SerializeField] private float expPerPickup = 0.01f;
        [Tooltip("Fraction expPerPickup shrinks by for each level gained (e.g. 0.12 = 12% less per level).")]
        [Range(0f, 1f)]
        [SerializeField] private float expPerPickupDecayPerLevel = 0.12f;

        public int CurrentLevel { get; private set; }
        public float CurrentExp { get; private set; }

        /// <summary>Current XP granted per pickup, after applying the per-level decay.</summary>
        public float CurrentExpPerPickup => expPerPickup * Mathf.Pow(1f - expPerPickupDecayPerLevel, CurrentLevel);

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

        /// <summary>Adds one pickup's worth of XP (scaled by the player's XP Gain card, if any), triggering a level-up once the slider is full.</summary>
        public void AddExp(float multiplier = 1f)
        {
            if (CurrentExp >= 1f) return;

            CurrentExp = Mathf.Min(CurrentExp + CurrentExpPerPickup * multiplier, 1f);

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
            CurrentLevel++;
            CharacterProgressTracker.Instance.ReportLevelReached(SelectedRunContext.Instance.SelectedCharacter, CurrentLevel);
            OnLevelChanged?.Invoke(CurrentLevel);

            CurrentExp = 0f;
            OnExpChanged?.Invoke(CurrentExp);

            Time.timeScale = 0f;

            if (cardsPanel != null)
            {
                cardsPanel.SetActive(true);
            }
        }
    }
}
