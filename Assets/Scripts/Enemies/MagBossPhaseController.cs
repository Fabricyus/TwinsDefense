using System.Collections;
using UnityEngine;
using TwinsDefense.Combat;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// MagBoss, three HP-gated phases that stack additively — nothing ever
    /// turns off once it starts (thresholds checked against
    /// ArenaEnemy.HealthPercent01, which only ever decreases):
    ///
    /// 100%-50% HP — Spinning Cross runs alone, on repeat: same 4-projectile
    /// cross-volley spin as SkullBoss, except the per-volley rotation flips
    /// sign (*-1) at the halfway point of the volleyCount volleys, so the
    /// spin winds one way then unwinds back the other.
    ///
    /// 50%-20% HP — the Reaper's red hazard-circle field (ReaperHazardCircle:
    /// growing telegraphed AoE circles that explode and respawn) starts and
    /// runs concurrently alongside the still-looping Spinning Cross, on its
    /// own independent timer.
    ///
    /// Below 20% HP — SkullBoss's tracking laser also joins in, so all three
    /// patterns (Spinning Cross + Hazard Field + Laser) run concurrently on
    /// independent loops for the rest of the fight.
    /// </summary>
    [RequireComponent(typeof(ArenaEnemy))]
    public class MagBossPhaseController : MonoBehaviour
    {
        [Header("Shared")]
        [SerializeField] private GameObject projectilePrefab;

        [Header("Phase 1 - Spinning Cross (mid-flip)")]
        [Tooltip("Seconds spent chasing the player between Spinning Cross attacks.")]
        [SerializeField] private float attackInterval = 4f;
        [SerializeField] private float crossProjectileSpeed = 6f;
        [SerializeField] private float crossProjectileDamage = 10f;
        [Tooltip("How many 4-projectile cross volleys fire per attack.")]
        [SerializeField] private int volleyCount = 12;
        [SerializeField] private float spinInterval = 0.03f;
        [Tooltip("Degrees the cross rotates per volley — the sign flips at the halfway volley, reversing the spin direction.")]
        [SerializeField] private float spinDegreesPerShot = 1f;

        [Header("Phase 2 - Hazard Field (Reaper-style)")]
        [Range(0f, 1f)]
        [SerializeField] private float band2Threshold = 0.5f;
        [Tooltip("Seconds spent chasing the player between Hazard Field activations, once unlocked.")]
        [SerializeField] private float hazardAttackInterval = 8f;
        [SerializeField] private float hazardDuration = 12f;
        [SerializeField] private int hazardCircleCount = 6;
        [SerializeField] private float hazardGrowDuration = 4f;
        [SerializeField] private float hazardDiameter = 3f;
        [SerializeField] private float hazardExplosionDamage = 20f;
        [Tooltip("Scales the total area circles can spawn across (2 = twice the area, each screen dimension scaled by sqrt(2)).")]
        [SerializeField] private float hazardSpawnAreaMultiplier = 2f;

        [Header("Phase 3 - Tracking Laser (Skull-style)")]
        [Range(0f, 1f)]
        [SerializeField] private float band3Threshold = 0.2f;
        [Tooltip("Seconds between laser casts, once unlocked.")]
        [SerializeField] private float laserAttackInterval = 5f;
        [SerializeField] private float laserDamage = 25f;
        [SerializeField] private float laserTrackDuration = 3f;
        [SerializeField] private float laserLockDuration = 0.5f;
        [SerializeField] private float laserWidth = 0.4f;
        [SerializeField] private Color laserColor = new Color(1f, 0.1f, 0.1f, 0.85f);

        private ArenaEnemy arenaEnemy;
        private Transform player;
        private bool hasStartedHazard;
        private bool hasStartedLaser;
        private bool hazardRunning;
        private bool laserRunning;

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

            StartCoroutine(SpinCrossLoop());
        }

        private void Update()
        {
            if (!hasStartedHazard && arenaEnemy.HealthPercent01 <= band2Threshold)
            {
                hasStartedHazard = true;
                StartCoroutine(HazardFieldLoop());
            }

            if (!hasStartedLaser && arenaEnemy.HealthPercent01 <= band3Threshold)
            {
                hasStartedLaser = true;
                StartCoroutine(LaserLoop());
            }
        }

        private IEnumerator SpinCrossLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(attackInterval);
                yield return StartCoroutine(SpinCrossAttack());
            }
        }

        private IEnumerator HazardFieldLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(hazardAttackInterval);
                yield return StartCoroutine(HazardFieldAttack());
            }
        }

        private IEnumerator LaserLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(laserAttackInterval);
                yield return StartCoroutine(LaserAttack());
            }
        }

        /// <summary>volleyCount 4-way cross volleys, spinning by spinDegreesPerShot per volley — the rotation direction flips (*-1) once the halfway volley is reached, so the spin winds one way then unwinds back. Unlike the other two attacks, this one does NOT grant iframes — the boss stays stunned (can't move) but stays damageable while spinning.</summary>
        private IEnumerator SpinCrossAttack()
        {
            arenaEnemy.ApplyStun(volleyCount * spinInterval + 0.1f);

            float baseAngle = 0f;
            float direction = 1f;
            int halfway = volleyCount / 2;

            for (int i = 0; i < volleyCount; i++)
            {
                if (i == halfway)
                {
                    direction *= -1f;
                }

                FireCross(baseAngle);
                baseAngle += spinDegreesPerShot * direction;
                yield return new WaitForSeconds(spinInterval);
            }
        }

        /// <summary>Spreads hazardCircleCount growing red telegraph circles (ReaperHazardCircle) across the arena — each explodes and respawns elsewhere until hazardDuration elapses. Same pattern as ReaperBoss's Phase 2, except here it's a recurring attack instead of a one-time trigger.</summary>
        private IEnumerator HazardFieldAttack()
        {
            hazardRunning = true;
            UpdateInvulnerability();

            arenaEnemy.ApplyStun(hazardDuration + 0.1f);

            float endTime = Time.time + hazardDuration;
            for (int i = 0; i < hazardCircleCount; i++)
            {
                Vector2 position = ReaperHazardCircle.FindSpreadPosition(hazardDiameter, hazardSpawnAreaMultiplier);
                ReaperHazardCircle.Spawn(position, hazardGrowDuration, hazardDiameter, hazardExplosionDamage, endTime, hazardSpawnAreaMultiplier);
            }

            yield return new WaitForSeconds(hazardDuration);

            hazardRunning = false;
            UpdateInvulnerability();
        }

        /// <summary>Tracking laser that locks and explodes — see SkullLaserBeam.</summary>
        private IEnumerator LaserAttack()
        {
            laserRunning = true;
            UpdateInvulnerability();

            float totalDuration = laserTrackDuration + laserLockDuration;
            arenaEnemy.ApplyStun(totalDuration + 0.1f);

            if (player != null)
            {
                SkullLaserBeam.Spawn(transform, player, laserTrackDuration, laserLockDuration, laserDamage, laserWidth, laserColor);
            }

            yield return new WaitForSeconds(totalDuration);

            laserRunning = false;
            UpdateInvulnerability();
        }

        /// <summary>Stays invulnerable as long as Hazard Field or Laser is mid-execution — needed because both can run concurrently once unlocked. Spinning Cross deliberately does not grant iframes.</summary>
        private void UpdateInvulnerability()
        {
            arenaEnemy.SetInvulnerable(hazardRunning || laserRunning);
        }

        private void FireCross(float baseAngle)
        {
            for (int i = 0; i < 4; i++)
            {
                FireProjectile(RotateDegrees(Vector2.up, -(baseAngle + 90f * i)), crossProjectileDamage, crossProjectileSpeed);
            }
        }

        private void FireProjectile(Vector2 direction, float damage, float speed)
        {
            if (projectilePrefab == null) return;

            GameObject instance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            if (instance.TryGetComponent(out EnemyProjectile projectile))
            {
                projectile.Launch(direction, damage, speed);
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
