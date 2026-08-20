using System.Collections;
using UnityEngine;
using TwinsDefense.Combat;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// MagBoss, three HP-gated phases that stack additively (thresholds checked
    /// against ArenaEnemy.HealthPercent01, which only ever decreases):
    ///
    /// 100%-band2Threshold HP — Spinning Cross runs alone, on repeat: same
    /// 4-projectile cross-volley spin as SkullBoss, except the per-volley
    /// rotation flips sign (*-1) at the halfway point of the volleyCount
    /// volleys, so the spin winds one way then unwinds back the other.
    ///
    /// band2Threshold-band3Threshold HP — an Aerial Bombing Run triggers
    /// exactly once, the first time this band is reached: the boss flies
    /// straight up off the top of the screen (iframes on, Spinning Cross
    /// paused), then makes hazardCircleCount fast edge-to-edge strafing
    /// passes — alternating entry side — dropping a Reaper-style red hazard
    /// circle (one-shot: it telegraphs once and explodes once, no respawn)
    /// as it crosses each target's X, before flying back down to its
    /// pre-run ground position, dropping iframes, and resuming Spinning
    /// Cross for the rest of the fight.
    ///
    /// Below band3Threshold HP — Spinning Cross stops entirely; the boss
    /// permanently takes even more reduced damage (phase3DamageTakenMultiplier)
    /// for the rest of the fight and never stuns again — it just keeps
    /// chasing the player, on top of and through MagpieCrossLaser casts,
    /// recurring every laserAttackInterval: a horizontal beam descending
    /// from the top of the screen to the player's Y and a vertical beam
    /// sliding in from the right edge to the player's X, both tracking
    /// live and locking together into a cross centered on the player
    /// before exploding.
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
        [Tooltip("Multiplies damage taken while the spin is active (0.2 = takes 20% damage) — also applied permanently once Phase 3 starts, since Spinning Cross stops but the boss still needs a high-defense window while just walking the player down between cross-laser casts.")]
        [SerializeField] private float spinDamageTakenMultiplier = 0.2f;

        [Header("Phase 2 - Aerial Bombing Run (Reaper-style)")]
        [Range(0f, 1f)]
        [SerializeField] private float band2Threshold = 0.5f;
        [Tooltip("How many strafing passes (bombs) the run makes.")]
        [SerializeField] private int hazardCircleCount = 6;
        [Tooltip("How long each dropped bomb telegraphs (grows) before exploding.")]
        [SerializeField] private float hazardGrowDuration = 4f;
        [SerializeField] private float hazardDiameter = 3f;
        [SerializeField] private float hazardExplosionDamage = 20f;
        [Tooltip("Scales the total area bombs can target (2 = twice the area, each screen dimension scaled by sqrt(2)).")]
        [SerializeField] private float hazardSpawnAreaMultiplier = 2f;
        [Tooltip("Seconds to fly straight up off the top of the screen at the start of the run.")]
        [SerializeField] private float ascendDuration = 0.35f;
        [Tooltip("Seconds for one edge-to-edge strafing pass — the bomb drops when the boss crosses the target's X.")]
        [SerializeField] private float divePassDuration = 0.5f;
        [Tooltip("Pause between consecutive strafing passes.")]
        [SerializeField] private float divePassGap = 0.25f;
        [Tooltip("Seconds to fly back down to its pre-run ground position once every bomb has dropped.")]
        [SerializeField] private float descendDuration = 0.6f;
        [Tooltip("How far past the screen edge the boss flies before it's considered fully off-screen.")]
        [SerializeField] private float offScreenMargin = 4f;

        [Header("Phase 3 - Cross Laser (Skull-style)")]
        [Range(0f, 1f)]
        [SerializeField] private float band3Threshold = 0.2f;
        [Tooltip("Seconds between cross-laser casts, once unlocked.")]
        [SerializeField] private float laserAttackInterval = 5f;
        [SerializeField] private float laserDamage = 25f;
        [SerializeField] private float laserTrackDuration = 3f;
        [SerializeField] private float laserLockDuration = 0.5f;
        [SerializeField] private float laserWidth = 0.4f;
        [SerializeField] private Color laserColor = new Color(1f, 0.1f, 0.1f, 0.85f);
        [Tooltip("Multiplies damage taken permanently once Phase 3 starts (0.1 = takes 10% damage) — set lower than spinDamageTakenMultiplier since the boss no longer gets any iframes at all in this phase and keeps chasing the player through cross-laser casts instead of standing stunned.")]
        [SerializeField] private float phase3DamageTakenMultiplier = 0.1f;

        private ArenaEnemy arenaEnemy;
        private Transform player;
        private bool hasStartedBombingRun;
        private bool hasStartedLaser;
        private bool bombingRunActive;

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
            if (!hasStartedBombingRun && arenaEnemy.HealthPercent01 <= band2Threshold)
            {
                hasStartedBombingRun = true;
                StartCoroutine(AerialBombingRunAttack());
            }

            if (!hasStartedLaser && arenaEnemy.HealthPercent01 <= band3Threshold)
            {
                hasStartedLaser = true;
                // Spinning Cross stops for good once Phase 3 starts (see SpinCrossLoop) — grant an even
                // higher permanent defense instead, since the boss no longer earns any window via the spin.
                arenaEnemy.SetDamageTakenMultiplier(phase3DamageTakenMultiplier);
                StartCoroutine(LaserLoop());
            }
        }

        private IEnumerator SpinCrossLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(attackInterval);

                // Phase 3 drops Spinning Cross entirely — the boss just walks the player
                // down between cross-laser casts instead (see SpinCrossAttack's doc).
                if (hasStartedLaser) continue;

                // Keeps firing even mid-Bombing Run — the boss is stunned by SpinCrossAttack either
                // way, so this never fights the flight coroutine for control of transform.position;
                // it just keeps the cross volleys raining from wherever the boss currently is in its
                // ascend/strafe/descend path instead of going silent until it lands.
                yield return StartCoroutine(SpinCrossAttack());
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

        /// <summary>volleyCount 4-way cross volleys, spinning by spinDegreesPerShot per volley — the rotation direction flips (*-1) once the halfway volley is reached, so the spin winds one way then unwinds back. Unlike the other two attacks, this one does NOT grant iframes — the boss stays stunned (can't move) and damageable while spinning, just takes greatly reduced damage (see spinDamageTakenMultiplier).</summary>
        private IEnumerator SpinCrossAttack()
        {
            // High-defense window instead of iframes — the boss stays stunned (can't move) and
            // stays damageable while spinning, just takes greatly reduced damage.
            arenaEnemy.SetDamageTakenMultiplier(spinDamageTakenMultiplier);

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

            arenaEnemy.SetDamageTakenMultiplier(1f);
        }

        /// <summary>Flies off-screen (iframes on, Spinning Cross paused), makes hazardCircleCount fast strafing passes each dropping a one-shot red hazard circle (ReaperHazardCircle), then flies back down to its pre-run ground position and drops iframes.</summary>
        private IEnumerator AerialBombingRunAttack()
        {
            bombingRunActive = true;
            UpdateInvulnerability();

            Vector3 groundPosition = transform.position;

            float totalDuration = ascendDuration + hazardCircleCount * (divePassDuration + divePassGap) + descendDuration;
            arenaEnemy.ApplyStun(totalDuration + 0.1f);

            yield return StartCoroutine(FlyOffScreenUp());

            bool fromLeft = true;
            for (int i = 0; i < hazardCircleCount; i++)
            {
                Vector2 targetPosition = ReaperHazardCircle.FindSpreadPosition(hazardDiameter, hazardSpawnAreaMultiplier);
                yield return StartCoroutine(DiveBombPass(targetPosition, fromLeft));
                fromLeft = !fromLeft;

                yield return new WaitForSeconds(divePassGap);
            }

            yield return StartCoroutine(FlyBackToGround(groundPosition));

            bombingRunActive = false;
            UpdateInvulnerability();
        }

        private IEnumerator FlyOffScreenUp()
        {
            Bounds bounds = GetScreenBounds();
            Vector3 start = transform.position;
            Vector3 end = new Vector3(start.x, bounds.max.y, start.z);

            float elapsed = 0f;
            while (elapsed < ascendDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, elapsed / ascendDuration);
                yield return null;
            }

            transform.position = end;
        }

        /// <summary>A fast edge-to-edge horizontal pass at the target's Y, dropping a hazard circle at the target position — as if the Magpie is diving in to attack — the instant it crosses the target's X.</summary>
        private IEnumerator DiveBombPass(Vector2 targetPosition, bool fromLeft)
        {
            Bounds bounds = GetScreenBounds();
            float entryX = fromLeft ? bounds.min.x : bounds.max.x;
            float exitX = fromLeft ? bounds.max.x : bounds.min.x;

            Vector3 entry = new Vector3(entryX, targetPosition.y, transform.position.z);
            Vector3 exit = new Vector3(exitX, targetPosition.y, transform.position.z);

            transform.position = entry;

            bool dropped = false;
            float elapsed = 0f;
            while (elapsed < divePassDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / divePassDuration;
                transform.position = Vector3.Lerp(entry, exit, t);

                if (!dropped && t >= 0.5f)
                {
                    dropped = true;
                    ReaperHazardCircle.Spawn(targetPosition, hazardGrowDuration, hazardDiameter, hazardExplosionDamage, Time.time + hazardGrowDuration, hazardSpawnAreaMultiplier, Color.red);
                }

                yield return null;
            }

            transform.position = exit;
        }

        private IEnumerator FlyBackToGround(Vector3 groundPosition)
        {
            Vector3 start = transform.position;

            float elapsed = 0f;
            while (elapsed < descendDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, groundPosition, elapsed / descendDuration);
                yield return null;
            }

            transform.position = groundPosition;
        }

        /// <summary>Tracking cross-laser that locks and explodes — see MagpieCrossLaser. No iframes and no stun: the boss keeps chasing the player through the whole cast, same as it does between casts.</summary>
        private IEnumerator LaserAttack()
        {
            float totalDuration = laserTrackDuration + laserLockDuration;

            if (player != null)
            {
                MagpieCrossLaser.Spawn(player, laserTrackDuration, laserLockDuration, laserDamage, laserWidth, laserColor);
            }

            yield return new WaitForSeconds(totalDuration);
        }

        /// <summary>Stays invulnerable while the Aerial Bombing Run is mid-execution. Spinning Cross and the cross-laser deliberately do not grant iframes.</summary>
        private void UpdateInvulnerability()
        {
            arenaEnemy.SetInvulnerable(bombingRunActive);
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

        /// <summary>World-space bounds of the visible camera area, outset by offScreenMargin on every side so ascend/dive-pass endpoints land fully off-screen.</summary>
        private Bounds GetScreenBounds()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return new Bounds(Vector3.zero, new Vector3(16f + offScreenMargin * 2f, 10f + offScreenMargin * 2f, 0f));
            }

            float screenHeight = cam.orthographicSize * 2f;
            float screenWidth = screenHeight * cam.aspect;
            Vector3 center = cam.transform.position;
            center.z = 0f;

            return new Bounds(center, new Vector3(screenWidth + offScreenMargin * 2f, screenHeight + offScreenMargin * 2f, 0f));
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
