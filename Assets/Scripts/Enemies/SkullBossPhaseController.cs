using System.Collections;
using UnityEngine;
using TwinsDefense.Combat;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// SkullBoss, three HP-gated phases (thresholds checked against
    /// ArenaEnemy.HealthPercent01, which only ever decreases):
    ///
    /// 100%-50% HP — only the Phase 1 pattern runs, on repeat: the boss stops
    /// moving, takes greatly reduced damage (see spinDamageTakenMultiplier) but
    /// stays hittable throughout, then fires a rapid series of 4-projectile
    /// cross volleys (N/E/S/W), each volley rotated 1 degree further clockwise
    /// than the last for a spiral/spin effect, then resumes chasing.
    ///
    /// 50%-20% HP — alternates Phase 1 with Phase 2: a red laser (this is the
    /// only attack that grants iframes) that tracks the player for 3s, locks
    /// for 0.5s, then explodes (SkullLaserBeam).
    ///
    /// Below 20% HP (once, permanently) — enrage: +30% move speed, takes half
    /// damage (double defense), and Phase 1 + Phase 2 now run concurrently on
    /// independent loops instead of alternating.
    /// </summary>
    [RequireComponent(typeof(ArenaEnemy))]
    public class SkullBossPhaseController : MonoBehaviour
    {
        private enum Band
        {
            Phase1Only,
            Alternating,
            DualEnrage
        }

        [Header("Shared Attack Cadence")]
        [Tooltip("Seconds spent chasing the player between attacks (Phase 1-Only and Alternating bands).")]
        [SerializeField] private float attackInterval = 4f;

        [Header("Phase 1 - Spinning Cross Volleys")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 6f;
        [SerializeField] private float projectileDamage = 10f;
        [Tooltip("How many 4-projectile cross volleys fire in a row — every volley is always a 4-way cross, never a lone shot.")]
        [SerializeField] private int volleyCount = 12;
        [Tooltip("Seconds between each volley — kept small so the accumulating rotation reads as a spin.")]
        [SerializeField] private float spinBurstInterval = 0.03f;
        [Tooltip("Degrees the whole 4-way cross rotates clockwise from one volley to the next.")]
        [SerializeField] private float spinDegreesPerShot = 1f;
        [Tooltip("Multiplies damage taken while the spin is active — high defense window, but unlike the laser this does NOT grant iframes: the boss stays hittable throughout. 0.2 = takes 20% damage.")]
        [SerializeField] private float spinDamageTakenMultiplier = 0.2f;

        [Header("Phase 2 - Tracking Laser")]
        [SerializeField] private float laserDamage = 25f;
        [SerializeField] private float laserTrackDuration = 3f;
        [SerializeField] private float laserLockDuration = 0.5f;
        [SerializeField] private float laserWidth = 0.4f;
        [SerializeField] private Color laserColor = new Color(1f, 0.1f, 0.1f, 0.85f);
        [Tooltip("Seconds between laser casts while in the DualEnrage band (independent from attackInterval, which paces Phase 1 there).")]
        [SerializeField] private float enrageLaserInterval = 5f;

        [Header("Phase Thresholds")]
        [Range(0f, 1f)]
        [SerializeField] private float band2Threshold = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float band3Threshold = 0.2f;

        [Header("Phase 3 - Enrage")]
        [SerializeField] private float enrageSpeedMultiplier = 1.3f;
        [Tooltip("0.5 = takes half damage, i.e. double defense.")]
        [SerializeField] private float enrageDamageTakenMultiplier = 0.5f;

        private ArenaEnemy arenaEnemy;
        private Transform player;
        private Band currentBand = Band.Phase1Only;
        private bool hasEnteredBand3;
        private bool isAttacking;
        private bool phase2Turn;
        private float attackTimer;
        private float baselineDamageTakenMultiplier = 1f;

        private void Awake()
        {
            arenaEnemy = GetComponent<ArenaEnemy>();
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        private void Update()
        {
            if (!hasEnteredBand3 && arenaEnemy.HealthPercent01 <= band3Threshold)
            {
                EnterBand3();
            }
            else if (currentBand == Band.Phase1Only && arenaEnemy.HealthPercent01 <= band2Threshold)
            {
                currentBand = Band.Alternating;
            }

            if (currentBand == Band.DualEnrage || isAttacking) return;

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                StartCoroutine(RunNextAttack());
            }
        }

        private IEnumerator RunNextAttack()
        {
            isAttacking = true;

            if (currentBand == Band.Alternating && phase2Turn)
            {
                yield return StartCoroutine(Phase2LaserAttack());
            }
            else
            {
                yield return StartCoroutine(Phase1CrossSpinAttack());
            }

            if (currentBand == Band.Alternating)
            {
                phase2Turn = !phase2Turn;
            }

            isAttacking = false;
        }

        private void EnterBand3()
        {
            hasEnteredBand3 = true;
            currentBand = Band.DualEnrage;
            baselineDamageTakenMultiplier = enrageDamageTakenMultiplier;

            arenaEnemy.SetSpeedMultiplier(enrageSpeedMultiplier);
            arenaEnemy.SetDamageTakenMultiplier(baselineDamageTakenMultiplier);

            StartCoroutine(Band3Phase1Loop());
            StartCoroutine(Band3Phase2Loop());
        }

        private IEnumerator Band3Phase1Loop()
        {
            while (true)
            {
                yield return StartCoroutine(Phase1CrossSpinAttack());
                yield return new WaitForSeconds(attackInterval);
            }
        }

        private IEnumerator Band3Phase2Loop()
        {
            while (true)
            {
                yield return StartCoroutine(Phase2LaserAttack());
                yield return new WaitForSeconds(enrageLaserInterval);
            }
        }

        /// <summary>
        /// volleyCount volleys, each always a 4-way cross (N/E/S/W relative to that
        /// volley's base angle) fired simultaneously — never a lone shot. The first
        /// volley's base angle is 0 (due north), and each following volley's base angle
        /// advances by spinDegreesPerShot, so the whole cross rotates a little further
        /// clockwise every volley, spinning the pattern across the arena.
        /// </summary>
        private IEnumerator Phase1CrossSpinAttack()
        {
            // High-defense window instead of iframes — the spin is meant to be free to hit, just tanky.
            arenaEnemy.SetDamageTakenMultiplier(baselineDamageTakenMultiplier * spinDamageTakenMultiplier);

            arenaEnemy.ApplyStun(volleyCount * spinBurstInterval + 0.1f);

            float baseAngle = 0f;
            for (int i = 0; i < volleyCount; i++)
            {
                FireCross(baseAngle);
                baseAngle += spinDegreesPerShot;
                yield return new WaitForSeconds(spinBurstInterval);
            }

            arenaEnemy.SetDamageTakenMultiplier(baselineDamageTakenMultiplier);
        }

        /// <summary>Fires 4 simultaneous projectiles 90 degrees apart, starting at baseAngle clockwise from north.</summary>
        private void FireCross(float baseAngle)
        {
            for (int i = 0; i < 4; i++)
            {
                FireProjectile(RotateDegrees(Vector2.up, -(baseAngle + 90f * i)));
            }
        }

        /// <summary>Tracking laser that locks and explodes — see SkullLaserBeam.</summary>
        private IEnumerator Phase2LaserAttack()
        {
            // Iframes live here now — only the laser cast is untouchable, not the spin.
            arenaEnemy.SetInvulnerable(true);

            float totalDuration = laserTrackDuration + laserLockDuration;
            arenaEnemy.ApplyStun(totalDuration + 0.1f);

            if (player != null)
            {
                SkullLaserBeam.Spawn(transform, player, laserTrackDuration, laserLockDuration, laserDamage, laserWidth, laserColor);
            }

            yield return new WaitForSeconds(totalDuration);

            arenaEnemy.SetInvulnerable(false);
        }

        private void FireProjectile(Vector2 direction)
        {
            if (projectilePrefab == null) return;

            GameObject instance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            if (instance.TryGetComponent(out EnemyProjectile projectile))
            {
                projectile.Launch(direction, projectileDamage, projectileSpeed);
            }
        }

        private static Vector2 RotateDegrees(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }
    }
}
