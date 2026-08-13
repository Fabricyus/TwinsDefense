using UnityEngine;
using TwinsDefense.Combat;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Optional ranged attack for arena enemies (e.g. Sprinter): fires a
    /// straight-line EnemyProjectile at the player's current position on a
    /// fixed interval whenever the player is within range. Movement and
    /// contact damage stay on ArenaEnemy — this only adds the shooting.
    /// </summary>
    public class EnemyRangedAttacker : MonoBehaviour
    {
        [Header("Firing")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireInterval = 2f;
        [SerializeField] private float projectileSpeed = 6f;
        [SerializeField] private float damage = 5f;
        [SerializeField] private float attackRange = 10f;
        [Tooltip("Origin the projectile spawns from. Defaults to this transform if left unassigned.")]
        [SerializeField] private Transform firePoint;

        private Transform player;
        private float fireTimer;

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }

            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            if (player == null || projectilePrefab == null) return;

            fireTimer += Time.deltaTime;
            if (fireTimer < fireInterval) return;

            float sqrDistance = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance > attackRange * attackRange) return;

            fireTimer = 0f;
            Fire();
        }

        private void Fire()
        {
            Vector2 direction = ((Vector2)player.position - (Vector2)firePoint.position).normalized;

            GameObject instance = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            if (instance.TryGetComponent(out EnemyProjectile projectile))
            {
                projectile.Launch(direction, damage, projectileSpeed);
            }
        }
    }
}
