using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Enemies;
using TwinsDefense.VFX;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Rainbow Nova Exclusive card (see CardEffectType.RainbowNova,
    /// PlayerStats.hasRainbowNova) — unlocked by defeating the secret Mega
    /// Magpie at Level 100 (CampaignProgress.MegaMagpieKilled). While active,
    /// damages every ArenaEnemy.Active enemy in the arena on a fixed interval —
    /// not gated by range, this is a full-arena pulse, not a local AoE. Damage
    /// scales off the player's own damage stat so it keeps pace with the build
    /// instead of falling off late-run. Cosmetic-only ExplosionVFX burst at the
    /// player's position each pulse, cycling through the rainbow hue wheel to
    /// match the Mega Magpie's own aura that unlocked this card.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class RainbowNovaController : MonoBehaviour
    {
        [Tooltip("Seconds between pulses.")]
        [SerializeField] private float novaInterval = 4f;
        [Tooltip("Multiplier on the player's own damage stat — each pulse hits every active enemy for stats.damage * this.")]
        [SerializeField] private float novaDamageMultiplier = 5f;
        [SerializeField] private float novaVisualRadius = 9f;
        [Tooltip("Hue travelled per pulse (0-1) — keeps consecutive pulses visibly different colors instead of repeating the same one.")]
        [SerializeField] private float hueStepPerPulse = 0.17f;

        private PlayerStats stats;
        private float timer;
        private float hue;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            if (!stats.hasRainbowNova)
            {
                timer = 0f;
                return;
            }

            timer += Time.deltaTime;
            if (timer < novaInterval) return;

            timer = 0f;
            Detonate();
        }

        private void Detonate()
        {
            Color novaColor = Color.HSVToRGB(hue, 1f, 1f);
            hue = (hue + hueStepPerPulse) % 1f;

            float damage = stats.damage * novaDamageMultiplier;

            // Snapshot first — TakeDamage can destroy enemies (mutating ArenaEnemy.Active) mid-iteration.
            List<ArenaEnemy> targets = new List<ArenaEnemy>(ArenaEnemy.Active);
            foreach (ArenaEnemy enemy in targets)
            {
                if (enemy == null) continue;
                enemy.TakeDamage(damage, isCrit: true, popupColor: novaColor);
            }

            ExplosionVFX.Spawn(transform.position, novaVisualRadius, novaColor, particleCount: 48);
        }
    }
}
