using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Combat;
using TwinsDefense.VFX;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Fires the Star Upgrade's Star Projectile(s) (see PlayerStats.starProjectileCount,
    /// granted at 3+/5+ purchased stars) on a fixed cooldown, fully independent of
    /// AutoAttack's fire-rate loop — this is a passive utility proc, not another shot
    /// in the normal volley. Built entirely at runtime (sprite + trail + collider),
    /// no prefab needed. No targeting: every shot fires at one of 5 fixed, player-relative
    /// directions (North, Right, Left, South+45, South-45 — a 5-pointed star when all 5
    /// fire together), picked without repeats within the same volley so hitting exactly 5
    /// projectiles always covers the whole star. Star Round's damage/cooldown bonuses
    /// (PlayerStats.starDamageBonusPercent/starCooldownReductionSeconds) only affect this
    /// launcher's own damage/cooldown — never the player's main damage or attackFireRate stat.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class StarProjectileLauncher : MonoBehaviour
    {
        [Tooltip("Origin the Star Projectile spawns from. Defaults to this transform if left unassigned.")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float cooldownSeconds = 5f;
        [Tooltip("Floor the effective cooldown can't drop below, no matter how much Star Round's cooldown reduction stacks.")]
        [SerializeField] private float minCooldownSeconds = 0.1f;
        [Tooltip("Star Projectile deals roughly the player's damage divided by this.")]
        [SerializeField] private float damageDivisor = 3f;
        [SerializeField] private float projectileScale = 0.5f;
        [SerializeField] private Sprite starSprite;
        [SerializeField] private Color trailColor = new Color(1f, 0.92f, 0.4f, 0.9f);

        /// <summary>The 5 fixed, player-relative firing directions — North, Right, Left, South+45 (southeast), South-45 (southwest). Together they trace a 5-pointed star; picked from without repeats per volley (see FireStarProjectiles).</summary>
        private static readonly Vector2[] StarDirections =
        {
            Vector2.up,
            Vector2.right,
            Vector2.left,
            RotateDegrees(Vector2.down, 45f),
            RotateDegrees(Vector2.down, -45f),
        };

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
            float effectiveCooldown = Mathf.Max(minCooldownSeconds, cooldownSeconds - stats.starCooldownReductionSeconds);
            if (cooldownTimer < effectiveCooldown) return;

            cooldownTimer = 0f;
            FireStarProjectiles();
        }

        /// <summary>Draws without replacement from StarDirections so a volley never repeats a direction until every other point has fired once — a volley of exactly 5 always covers the whole star, in shuffled order. Volleys beyond 5 (heavily stacked builds) reshuffle and start drawing again.</summary>
        private void FireStarProjectiles()
        {
            int count = Mathf.Max(1, Mathf.RoundToInt(stats.starProjectileCount));
            float damage = Mathf.Max(0.1f, stats.damage / damageDivisor) * (1f + stats.starDamageBonusPercent / 100f);

            List<Vector2> pool = new List<Vector2>(StarDirections);

            for (int i = 0; i < count; i++)
            {
                if (pool.Count == 0)
                {
                    pool.AddRange(StarDirections);
                }

                int index = Random.Range(0, pool.Count);
                Vector2 direction = pool[index];
                pool.RemoveAt(index);

                SpawnStarProjectile(direction, damage);
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
    }
}
