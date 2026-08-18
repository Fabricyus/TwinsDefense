using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.Enemies;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Shield icon that appears above a boss while it's currently invulnerable
    /// (iframes) — e.g. SkullBoss/MagBoss mid-laser-cast. Lives on the icon's
    /// own Image, sibling to BossHealthBarUI's Fill inside the same
    /// world-space HealthBarCanvas. Toggles Image.enabled rather than the
    /// GameObject itself so this script's own subscription to the boss's
    /// events never gets torn down by its own visibility change.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BossShieldIconUI : MonoBehaviour
    {
        [SerializeField] private ArenaEnemy boss;

        private Image icon;

        private void Awake()
        {
            icon = GetComponent<Image>();

            if (boss == null)
            {
                boss = GetComponentInParent<ArenaEnemy>();
            }
        }

        private void Start()
        {
            // Start (not OnEnable) so ArenaEnemy.Awake has already run.
            if (boss == null) return;

            boss.OnInvulnerabilityChanged += HandleInvulnerabilityChanged;
            boss.OnEnemyDefeated += HandleBossDefeated;
            HandleInvulnerabilityChanged(boss.IsInvulnerable);
        }

        private void OnDisable()
        {
            if (boss == null) return;

            boss.OnInvulnerabilityChanged -= HandleInvulnerabilityChanged;
            boss.OnEnemyDefeated -= HandleBossDefeated;
        }

        private void HandleInvulnerabilityChanged(bool invulnerable)
        {
            icon.enabled = invulnerable;
        }

        /// <summary>The Canvas dies along with the boss GameObject — this just avoids a dangling event unsubscribe warning by unhooking first.</summary>
        private void HandleBossDefeated()
        {
            if (boss == null) return;

            boss.OnInvulnerabilityChanged -= HandleInvulnerabilityChanged;
            boss.OnEnemyDefeated -= HandleBossDefeated;
        }
    }
}
