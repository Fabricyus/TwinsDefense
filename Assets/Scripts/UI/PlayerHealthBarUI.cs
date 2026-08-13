using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.Player;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Floating HP bar above the player's head. Lives on the Fill Image of a
    /// world-space Canvas parented to the Player, so it follows automatically
    /// via the transform hierarchy — no manual position tracking needed.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PlayerHealthBarUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;

        private PlayerStats stats;

        private void Awake()
        {
            if (fillImage == null)
            {
                fillImage = GetComponent<Image>();
            }

            if (health == null)
            {
                health = GetComponentInParent<PlayerHealth>();
            }

            if (health != null)
            {
                stats = health.GetComponent<PlayerStats>();
            }
        }

        private void Start()
        {
            // Start (not OnEnable) so PlayerHealth.Awake has already run.
            if (health == null) return;

            health.OnHealthChanged += HandleHealthChanged;
            float maxHP = stats != null ? stats.maxHP : 100f;
            HandleHealthChanged(health.CurrentHP, maxHP);
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            fillImage.fillAmount = ratio;
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, ratio);
        }
    }
}
