using System.Collections;
using UnityEngine;
using TwinsDefense.Combat;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Reaper boss, two phases:
    /// Phase 1 — chases the player (handled by ArenaEnemy) and periodically stops
    /// to fire a rapid series of 8-directional projectile rings (first shot due
    /// north, each next +45 degrees).
    /// Phase 2 — triggered once, the first time health drops to/below
    /// phase2HealthThreshold: the boss freezes and becomes invulnerable while a
    /// spread of growing red hazard circles (ReaperHazardCircle) telegraph AoE
    /// damage across the screen for phase2Duration seconds, then it resumes
    /// Phase 1 permanently until it dies.
    /// </summary>
    [RequireComponent(typeof(ArenaEnemy))]
    public class ReaperBoss : MonoBehaviour
    {
        [Header("Phase 1 - Ring Burst")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private float projectileDamage = 8f;
        [Tooltip("Seconds spent chasing the player between each ring-burst attack.")]
        [SerializeField] private float attackInterval = 4f;
        [Tooltip("How many 8-projectile rings are fired per attack cycle.")]
        [SerializeField] private int volleyCount = 10;
        [Tooltip("Seconds between each ring within a burst.")]
        [SerializeField] private float volleyInterval = 0.15f;

        [Header("Phase 2 - Hazard Field")]
        [Tooltip("Health fraction (0-1) at which Phase 2 triggers, once, the first time it's reached.")]
        [Range(0f, 1f)]
        [SerializeField] private float phase2HealthThreshold = 0.3f;
        [SerializeField] private float phase2Duration = 12f;
        [SerializeField] private int hazardCircleCount = 6;
        [SerializeField] private float hazardGrowDuration = 4f;
        [SerializeField] private float hazardDiameter = 3f;
        [SerializeField] private float hazardExplosionDamage = 20f;
        [Tooltip("Scales the total area circles can spawn across (2 = twice the area, each screen dimension scaled by sqrt(2)).")]
        [SerializeField] private float hazardSpawnAreaMultiplier = 2f;

        private ArenaEnemy arenaEnemy;
        private float attackTimer;
        private bool isAttacking;
        private bool hasEnteredPhase2;
        private bool inPhase2;

        private void Awake()
        {
            arenaEnemy = GetComponent<ArenaEnemy>();
        }

        private void Update()
        {
            if (inPhase2 || isAttacking) return;

            if (!hasEnteredPhase2 && arenaEnemy.HealthPercent01 <= phase2HealthThreshold)
            {
                hasEnteredPhase2 = true;
                StartCoroutine(Phase2Sequence());
                return;
            }

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                StartCoroutine(Phase1AttackCycle());
            }
        }

        private IEnumerator Phase1AttackCycle()
        {
            isAttacking = true;
            arenaEnemy.ApplyStun(volleyCount * volleyInterval + 0.1f);

            for (int i = 0; i < volleyCount; i++)
            {
                FireRing();
                yield return new WaitForSeconds(volleyInterval);
            }

            isAttacking = false;
        }

        /// <summary>8 projectiles spread evenly around a full circle, starting due north.</summary>
        private void FireRing()
        {
            if (projectilePrefab == null) return;

            for (int i = 0; i < 8; i++)
            {
                Vector2 direction = RotateDegrees(Vector2.up, i * 45f);
                GameObject instance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                if (instance.TryGetComponent(out EnemyProjectile projectile))
                {
                    projectile.Launch(direction, projectileDamage, projectileSpeed);
                }
            }
        }

        private IEnumerator Phase2Sequence()
        {
            inPhase2 = true;
            arenaEnemy.SetInvulnerable(true);
            arenaEnemy.ApplyStun(phase2Duration);

            float endTime = Time.time + phase2Duration;
            for (int i = 0; i < hazardCircleCount; i++)
            {
                Vector2 position = ReaperHazardCircle.FindSpreadPosition(hazardDiameter, hazardSpawnAreaMultiplier);
                ReaperHazardCircle.Spawn(position, hazardGrowDuration, hazardDiameter, hazardExplosionDamage, endTime, hazardSpawnAreaMultiplier);
            }

            yield return new WaitForSeconds(phase2Duration);

            arenaEnemy.SetInvulnerable(false);
            inPhase2 = false;
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
