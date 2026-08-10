using UnityEngine;
using TwinsDefense.Enemies;
using TwinsDefense.Combat;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Automatically targets and fires at the nearest enemy in range on a
    /// fixed interval. No manual aiming — this is the player's baseline attack.
    /// </summary>
    public class AutoAttack : MonoBehaviour
    {
        [Header("Targeting")]
        [Tooltip("Placeholder value — final per-character balancing (including Ralph's short range) is tuned later.")]
        [SerializeField] private float attackRange = 5f;

        [Header("Firing")]
        [SerializeField] private float attackFireRate = 1f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float damage = 5f;
        [Tooltip("Origin the projectile spawns from. Defaults to this transform if left unassigned.")]
        [SerializeField] private Transform firePoint;

        [Header("Critical Hit")]
        [Tooltip("Placeholder base value — final per-character/upgrade-card balancing is tuned later.")]
        [Range(0f, 1f)]
        [SerializeField] private float critChance = 0.1f;
        [SerializeField] private float critMultiplier = 2f;

        private float attackTimer;

        private void Start()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            if (attackFireRate <= 0f) return;

            attackTimer += Time.deltaTime;

            if (attackTimer >= 1f / attackFireRate)
            {
                attackTimer = 0f;
                Attack();
            }
        }

        private void Attack()
        {
            ArenaEnemy target = FindNearestEnemyInRange();
            if (target == null || projectilePrefab == null) return;

            Vector2 direction = ((Vector2)target.transform.position - (Vector2)firePoint.position).normalized;

            bool isCrit = Random.value < critChance;
            float finalDamage = isCrit ? damage * critMultiplier : damage;

            GameObject instance = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile projectile = instance.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.Launch(direction, finalDamage, projectileSpeed, isCrit);
            }
        }

        private ArenaEnemy FindNearestEnemyInRange()
        {
            ArenaEnemy nearest = null;
            float nearestSqrDistance = attackRange * attackRange;

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
