using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.Enemies;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Floating HP bar above a boss's head. Lives on the Fill Image of a
    /// world-space Canvas parented to the boss, mirroring
    /// PlayerHealthBarUI's setup — so it follows automatically via the
    /// transform hierarchy, including inheriting the boss's own scale.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BossHealthBarUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private ArenaEnemy boss;
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;

        private void Awake()
        {
            if (fillImage == null)
            {
                fillImage = GetComponent<Image>();
            }

            if (boss == null)
            {
                boss = GetComponentInParent<ArenaEnemy>();
            }
        }

        private void Start()
        {
            // Start (not OnEnable) so ArenaEnemy.Awake has already run.
            if (boss == null) return;

            boss.OnHealthChanged += HandleHealthChanged;
            boss.OnEnemyDefeated += HandleBossDefeated;
            HandleHealthChanged(boss.HealthPercent01);
        }

        private void OnDisable()
        {
            if (boss == null) return;

            boss.OnHealthChanged -= HandleHealthChanged;
            boss.OnEnemyDefeated -= HandleBossDefeated;
        }

        private void HandleHealthChanged(float ratio01)
        {
            fillImage.fillAmount = ratio01;
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, ratio01);
        }

        /// <summary>The Canvas dies along with the boss GameObject — this just avoids a dangling event unsubscribe warning by unhooking first.</summary>
        private void HandleBossDefeated()
        {
            if (boss == null) return;

            boss.OnHealthChanged -= HandleHealthChanged;
            boss.OnEnemyDefeated -= HandleBossDefeated;
        }
    }
}
