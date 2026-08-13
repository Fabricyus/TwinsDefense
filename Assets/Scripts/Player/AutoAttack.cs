using UnityEngine;
using TwinsDefense.Enemies;
using TwinsDefense.Combat;
using TwinsDefense.Data;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Automatically targets and fires at the nearest enemy in range on a
    /// fixed interval. No manual aiming — this is the player's baseline attack.
    /// Reads its numbers live from PlayerStats so level-up cards take effect
    /// immediately.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class AutoAttack : MonoBehaviour
    {
        [Header("Firing")]
        [Tooltip("Used only if no PlayerCharacterData is found, or its CharacterMetaData has no projectilePrefab assigned.")]
        [SerializeField] private GameObject projectilePrefab;
        [Tooltip("Origin the projectile spawns from. Defaults to this transform if left unassigned.")]
        [SerializeField] private Transform firePoint;
        [Tooltip("Angle in degrees between adjacent projectiles when Extra Projectile cards add more than one shot.")]
        [SerializeField] private float extraProjectileSpreadAngle = 15f;

        private PlayerStats stats;
        private PlayerCharacterData characterData;
        private float attackTimer;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            characterData = GetComponent<PlayerCharacterData>();
        }

        private void Start()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            if (stats.attackFireRate <= 0f) return;

            attackTimer += Time.deltaTime;

            if (attackTimer >= 1f / stats.attackFireRate)
            {
                attackTimer = 0f;
                Attack();
            }
        }

        private void Attack()
        {
            ArenaEnemy target = FindNearestEnemyInRange();
            GameObject prefab = ResolveProjectilePrefab();
            if (target == null || prefab == null) return;

            Vector2 direction = ((Vector2)target.transform.position - (Vector2)firePoint.position).normalized;

            bool isCrit = Random.value < stats.critChance;
            float finalDamage = isCrit ? stats.damage * stats.critDamage : stats.damage;

            // Extra Projectile cards fan additional shots out around the aimed direction
            // instead of retargeting, so more projectiles means wider coverage.
            int totalProjectiles = 1 + Mathf.Max(0, Mathf.RoundToInt(stats.extraProjectileCount));

            for (int i = 0; i < totalProjectiles; i++)
            {
                float angleOffset = (i - (totalProjectiles - 1) / 2f) * extraProjectileSpreadAngle;
                Vector2 fireDirection = angleOffset == 0f ? direction : RotateDegrees(direction, angleOffset);
                FireProjectile(prefab, fireDirection, finalDamage, isCrit);
            }
        }

        /// <summary>Character-specific prefab (set per tier in Character Selection) takes priority over the inspector fallback.</summary>
        private GameObject ResolveProjectilePrefab()
        {
            if (characterData != null && characterData.Current != null && characterData.Current.projectilePrefab != null)
            {
                return characterData.Current.projectilePrefab;
            }

            return projectilePrefab;
        }

        private void FireProjectile(GameObject prefab, Vector2 direction, float damage, bool isCrit)
        {
            GameObject instance = Instantiate(prefab, firePoint.position, Quaternion.identity);
            Projectile projectile = instance.GetComponent<Projectile>();

            if (projectile != null)
            {
                int pierceCount = Mathf.Max(0, Mathf.RoundToInt(stats.pierceCount));
                bool isRotatingProjectile = characterData != null && characterData.Current != null && characterData.Current.isRotatingProjectile;

                CharacterPassiveEffect thunderStrike = characterData != null && characterData.Current != null
                    ? characterData.Current.passiveEffects.Find(e => e.effectType == CharacterPassiveEffectType.ThunderStrikeOnHit)
                    : null;
                float procChancePercent = thunderStrike != null ? thunderStrike.procChancePercent : 0f;
                // Treated as a guaranteed crit: the passive's own multiplier (e.g. 300%) stacks additively with
                // the player's current critDamage stat, so Crit Damage cards make this proc hit harder too.
                float procBonusDamage = thunderStrike != null ? stats.damage * (thunderStrike.damageMultiplier + stats.critDamage) : 0f;
                GameObject procFxPrefab = characterData != null && characterData.Current != null ? characterData.Current.procFxPrefab : null;

                projectile.Launch(direction, damage, stats.projectileSpeed, isCrit, pierceCount, stats.areaOfEffect, isRotatingProjectile, procChancePercent, procBonusDamage, procFxPrefab);
            }
        }

        private static Vector2 RotateDegrees(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }

        private ArenaEnemy FindNearestEnemyInRange()
        {
            ArenaEnemy nearest = null;
            float nearestSqrDistance = stats.attackRange * stats.attackRange;

            foreach (ArenaEnemy enemy in ArenaEnemy.Active)
            {
                float sqrDistance = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;

                if (sqrDistance <= nearestSqrDistance)
                {
                    nearest = enemy;
                    nearestSqrDistance = sqrDistance;
                }
            }

            return nearest;
        }
    }
}
