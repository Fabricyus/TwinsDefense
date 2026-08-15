using UnityEngine;
using TwinsDefense.Enemies;
using TwinsDefense.Combat;
using TwinsDefense.VFX;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Fires the Star Upgrade's Star Projectile(s) (see PlayerStats.starProjectileCount,
    /// granted at 3+/5+ purchased stars) on a fixed cooldown, fully independent of
    /// AutoAttack's fire-rate loop — this is a passive utility proc, not another shot
    /// in the normal volley. Built entirely at runtime (sprite + trail + collider),
    /// no prefab needed.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class StarProjectileLauncher : MonoBehaviour
    {
        [Tooltip("Origin the Star Projectile spawns from. Defaults to this transform if left unassigned.")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float cooldownSeconds = 5f;
        [Tooltip("Angle in degrees between adjacent Star Projectiles when starProjectileCount > 1.")]
        [SerializeField] private float spreadAngle = 25f;
        [Tooltip("Star Projectile deals roughly the player's damage divided by this.")]
        [SerializeField] private float damageDivisor = 3f;
        [SerializeField] private float projectileScale = 0.5f;
        [SerializeField] private Sprite starSprite;
        [SerializeField] private Color trailColor = new Color(1f, 0.92f, 0.4f, 0.9f);

        private PlayerStats stats;
        private float cooldownTimer;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
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
            if (stats.starProjectileCount <= 0f) return;

            cooldownTimer += Time.deltaTime;
            if (cooldownTimer < cooldownSeconds) return;

            cooldownTimer = 0f;
            FireStarProjectiles();
        }

        private void FireStarProjectiles()
        {
            ArenaEnemy target = FindNearestEnemyInRange();
            if (target == null) return;

            Vector2 direction = ((Vector2)target.transform.position - (Vector2)firePoint.position).normalized;
            int count = Mathf.Max(1, Mathf.RoundToInt(stats.starProjectileCount));
            float damage = Mathf.Max(0.1f, stats.damage / damageDivisor);

            for (int i = 0; i < count; i++)
            {
                float angleOffset = count == 1 ? 0f : (i - (count - 1) / 2f) * spreadAngle;
                Vector2 fireDirection = angleOffset == 0f ? direction : RotateDegrees(direction, angleOffset);
                SpawnStarProjectile(fireDirection, damage);
            }
        }

        private void SpawnStarProjectile(Vector2 direction, float damage)
        {
            GameObject instance = new GameObject("StarProjectile");
            instance.transform.position = firePoint.position;
            instance.transform.localScale = Vector3.one * projectileScale;

            SpriteRenderer spriteRenderer = instance.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = starSprite;
            spriteRenderer.sortingOrder = 5;

            CircleCollider2D collider = instance.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            StarProjectile starProjectile = instance.AddComponent<StarProjectile>();
            starProjectile.Launch(direction, damage);

            ProjectileTrailVFX trail = instance.AddComponent<ProjectileTrailVFX>();
            trail.Configure(trailColor);
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
                float sqrDistance = ((Vector2)enemy.transform.position - (Vector2)firePoint.position).sqrMagnitude;

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
