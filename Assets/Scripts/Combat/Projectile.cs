using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Enemies;

namespace TwinsDefense.Combat
{
    /// <summary>
    /// Straight-line projectile fired by the player's AutoAttack. Travels in
    /// a fixed direction (no homing), applies damage to each ArenaEnemy it
    /// collides with (once per enemy), and self-destroys once its pierce
    /// budget is spent or after lifetime.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;
        [Tooltip("Degrees per second spun on the Z axis while isRotatingProjectile is true.")]
        [SerializeField] private float rotationSpeed = 360f;

        private Vector2 direction;
        private float speed;
        private float damage;
        private bool isCrit;
        private bool isRotatingProjectile;
        private float areaOfEffectScale;
        private float procChancePercent;
        private float procBonusDamage;
        private GameObject procFxPrefab;
        private int remainingPierces;
        private float lifeTimer;
        private Vector3 baseScale;
        private readonly HashSet<ArenaEnemy> hitEnemies = new HashSet<ArenaEnemy>();

        private void Awake()
        {
            // Kinematic so it never reacts to physics/gravity, but still raises
            // trigger events against the enemies' (non-rigidbody) colliders.
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            baseScale = transform.localScale;
        }

        /// <summary>
        /// Assigns this projectile's travel direction, damage and speed right after Instantiate.
        /// </summary>
        /// <param name="pierceCount">Extra enemies this projectile can hit after its first, before being destroyed.</param>
        /// <param name="scaleMultiplier">Multiplies the prefab's own scale — how the Area of Effect card grows the projectile's visual/hit size.</param>
        /// <param name="procChancePercent">Chance (0-100), rolled independently per enemy hit, to add procBonusDamage and spawn procFxPrefab (character's ThunderStrikeOnHit passive).</param>
        /// <param name="procBonusDamage">Bonus damage added to a hit that procs — already computed as player damage * the passive's multiplier.</param>
        public void Launch(Vector2 direction, float damage, float speed, bool isCrit = false, int pierceCount = 0, float scaleMultiplier = 1f, bool isRotatingProjectile = false, float procChancePercent = 0f, float procBonusDamage = 0f, GameObject procFxPrefab = null)
        {
            this.direction = direction.normalized;
            this.damage = damage;
            this.speed = speed;
            this.isCrit = isCrit;
            this.isRotatingProjectile = isRotatingProjectile;
            this.areaOfEffectScale = scaleMultiplier;
            this.procChancePercent = procChancePercent;
            this.procBonusDamage = procBonusDamage;
            this.procFxPrefab = procFxPrefab;
            remainingPierces = pierceCount;

            transform.rotation = Quaternion.Euler(0f, 0, -90 + Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg);
            transform.localScale = baseScale * Mathf.Max(0.01f, scaleMultiplier);
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;

            if (lifeTimer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            ApplySpin();
        }

        /// <summary>Adds a constant Z-axis spin on top of the facing rotation set in Launch, while isRotatingProjectile is true.</summary>
        private void ApplySpin()
        {
            if (!isRotatingProjectile) return;

            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ArenaEnemy enemy = other.GetComponent<ArenaEnemy>();
            if (enemy == null || hitEnemies.Contains(enemy)) return;

            enemy.TakeDamage(damage, isCrit);
            hitEnemies.Add(enemy);

            // Rolled independently per enemy hit rather than once at Launch, so Pierce shots
            // get an independent chance to proc against each enemy they pass through.
            if (procFxPrefab != null && procChancePercent > 0f && Random.value * 100f < procChancePercent)
            {
                GameObject fxInstance = Instantiate(procFxPrefab, enemy.transform.position, Quaternion.identity);
                ProcAreaDamage areaDamage = fxInstance.GetComponent<ProcAreaDamage>();

                // Crit (yellow) AoE hit around the impact point — scales with the player's current Area of Effect.
                areaDamage?.Detonate(procBonusDamage, true, areaOfEffectScale);
            }

            if (remainingPierces <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                remainingPierces--;
            }
        }
    }
}
