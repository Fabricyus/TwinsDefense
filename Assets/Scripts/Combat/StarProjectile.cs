using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Enemies;
using TwinsDefense.Systems;

namespace TwinsDefense.Combat
{
    /// <summary>
    /// Star Upgrade reward (3+ stars): a small, low-damage shot fired
    /// independently of AutoAttack on its own cooldown (see
    /// StarProjectileLauncher). Flies out to a fixed distance, then eases back
    /// toward its launch point — both legs use an "ease in back" curve, which
    /// overshoots backward before accelerating forward, so each leg reads as a
    /// little windup-then-throw rather than a straight glide. Carries its own
    /// fixed pierce budget, independent of the player's Pierce stat.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class StarProjectile : MonoBehaviour
    {
        [SerializeField] private float travelDistance = 4f;
        [SerializeField] private float legDuration = 0.5f;
        [SerializeField] private float spinDegreesPerSecond = 540f;
        [SerializeField] private int pierceBudget = 5;

        private float damage;
        private int remainingPierces;
        private readonly HashSet<ArenaEnemy> hitEnemies = new HashSet<ArenaEnemy>();
        private Vector3 startPos;
        private Vector3 outPos;
        private float legTimer;
        private bool returning;
        private SpriteRenderer spriteRenderer;
        private float baseAlpha = 1f;

private void Awake()
        {
            // Kinematic so it never reacts to physics/gravity, but still raises
            // trigger events against the enemies' (non-rigidbody) colliders.
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                baseAlpha = spriteRenderer.color.a;
                ApplyOpacity(ProjectileOpacitySettings.Value);
            }
        }

private void OnEnable()
        {
            ProjectileOpacitySettings.OnChanged += ApplyOpacity;
        }

        private void OnDisable()
        {
            ProjectileOpacitySettings.OnChanged -= ApplyOpacity;
        }

        private void ApplyOpacity(float opacity)
        {
            if (spriteRenderer == null) return;

            Color color = spriteRenderer.color;
            color.a = baseAlpha * opacity;
            spriteRenderer.color = color;
        }


        public void Launch(Vector2 direction, float damageAmount)
        {
            damage = damageAmount;
            remainingPierces = pierceBudget;
            startPos = transform.position;
            outPos = startPos + (Vector3)(direction.normalized * travelDistance);
            legTimer = 0f;
            returning = false;
        }

        private void Update()
        {
            legTimer += Time.deltaTime;
            float t = Mathf.Clamp01(legTimer / legDuration);
            float eased = EaseInBack(t);

            if (!returning)
            {
                transform.position = Vector3.LerpUnclamped(startPos, outPos, eased);

                if (t >= 1f)
                {
                    returning = true;
                    legTimer = 0f;
                }
            }
            else
            {
                transform.position = Vector3.LerpUnclamped(outPos, startPos, eased);

                if (t >= 1f)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            transform.Rotate(0f, 0f, spinDegreesPerSecond * Time.deltaTime);
        }

        /// <summary>Standard easeInBack: dips slightly negative before t=0.3ish, then accelerates past 1 — gives each leg a little backswing before it commits to the throw.</summary>
        private static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ArenaEnemy enemy = other.GetComponent<ArenaEnemy>();
            if (enemy == null || hitEnemies.Contains(enemy)) return;

            enemy.TakeDamage(damage);
            hitEnemies.Add(enemy);

            if (remainingPierces <= 0)
            {
                Destroy(gameObject);
                return;
            }

            remainingPierces--;
        }
    }
}
