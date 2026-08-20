using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Enemies;
using TwinsDefense.Systems;
using TwinsDefense.VFX;

namespace TwinsDefense.Combat
{
    /// <summary>
    /// Bundles all of a character's on-hit passive procs (ThunderStrike/Stun/
    /// Slow/Chain/ExplodeOnKill) into one value so Projectile.Launch doesn't
    /// need one parameter per passive. Left at its default (all-zero) for
    /// characters with no on-hit passive — every roll below is gated by its
    /// own chance being greater than zero.
    /// </summary>
    public struct OnHitPassiveEffects
    {
        public float thunderChancePercent;
        public float thunderBonusDamage;
        public GameObject thunderFxPrefab;
        public Color thunderStrikeColor;

        public float stunChancePercent;
        public float stunDurationSeconds;

        public float slowChancePercent;
        public float slowMagnitudePercent;
        public float slowDurationSeconds;

        public float chainChancePercent;

        public float explodeOnKillChancePercent;
        public Color explodeOnKillColor;

        /// <summary>Holy Strike card — flat chance, independent of the equipped character, to proc Paladin Ralph's holyFx. See AutoAttack.ResolveOnHitPassives.</summary>
        public float holyStrikeChancePercent;
        public float holyStrikeBonusDamage;
        public GameObject holyStrikeFxPrefab;
        public Color holyStrikeColor;

        /// <summary>Static Strike card — flat chance, independent of the equipped character, to proc Court Reader's thunderFx.</summary>
        public float staticStrikeChancePercent;
        public float staticStrikeBonusDamage;
        public GameObject staticStrikeFxPrefab;
        public Color staticStrikeColor;

        /// <summary>Dark Fork card — on hitting an enemy, forks the projectile into two children angled +/-45 degrees off its heading. See Projectile.TrySplitOnHit.</summary>
        public bool projectileSplitOnHit;
    }

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
        [Tooltip("Max distance from the hit enemy a ChainOnHit proc will search for its next target.")]
        [SerializeField] private float chainRadius = 4f;
        [Tooltip("Base AoE radius for an ExplodeOnKill proc, before scaling by the player's current Area of Effect.")]
        [SerializeField] private float explodeOnKillRadius = 3f;
        [Tooltip("Chance for each enemy killed by an ExplodeOnKill splash (not the original direct kill) to drop one bonus exp crystal — roughly 1 in 3 by default, since splash kills otherwise grant no XP at all.")]
        [Range(0f, 1f)]
        [SerializeField] private float explodeOnKillBonusExpChance = 1f / 3f;

        private Vector2 direction;
        private float speed;
        private float damage;
        private bool isCrit;
        private bool isRotatingProjectile;
        private float areaOfEffectScale;
        private OnHitPassiveEffects onHitPassives;
        private int remainingPierces;
        private float lifeTimer;
        private Vector3 baseScale;
        private SpriteRenderer spriteRenderer;
        private float baseAlpha = 1f;
        private readonly HashSet<ArenaEnemy> hitEnemies = new HashSet<ArenaEnemy>();
        private GameObject sourcePrefab;
        private bool hasSplitOnHit;

private void Awake()
        {
            // Kinematic so it never reacts to physics/gravity, but still raises
            // trigger events against the enemies' (non-rigidbody) colliders.
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            baseScale = transform.localScale;

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

        /// <summary>Scales the sprite's own alpha by the player's opacity preference, so a projectile with a semi-transparent base color (if any) isn't clipped to full opacity.</summary>
        private void ApplyOpacity(float opacity)
        {
            if (spriteRenderer == null) return;

            Color color = spriteRenderer.color;
            color.a = baseAlpha * opacity;
            spriteRenderer.color = color;
        }


        /// <summary>
        /// Assigns this projectile's travel direction, damage and speed right after Instantiate.
        /// </summary>
        /// <param name="pierceCount">Extra enemies this projectile can hit after its first, before being destroyed.</param>
        /// <param name="scaleMultiplier">Multiplies the prefab's own scale — how the Area of Effect card grows the projectile's visual/hit size.</param>
        /// <param name="onHitPassives">The firing character's on-hit passives (ThunderStrike/Stun/Slow/Chain), rolled independently per enemy hit.</param>
        /// <param name="sourcePrefab">The prefab this instance was spawned from — kept only so Dark Fork's split-on-hit proc can spawn clean children from the original prefab instead of cloning this (already-scaled, already-hit) live instance. Left null for callers that never grant the split proc (e.g. enemy/boss projectiles).</param>
        public void Launch(Vector2 direction, float damage, float speed, bool isCrit = false, int pierceCount = 0, float scaleMultiplier = 1f, bool isRotatingProjectile = false, OnHitPassiveEffects onHitPassives = default, GameObject sourcePrefab = null)
        {
            this.direction = direction.normalized;
            this.damage = damage;
            this.speed = speed;
            this.isCrit = isCrit;
            this.isRotatingProjectile = isRotatingProjectile;
            this.areaOfEffectScale = scaleMultiplier;
            this.onHitPassives = onHitPassives;
            this.sourcePrefab = sourcePrefab;
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

            // Read before RollOnHitPassives so a proc that also touches this enemy (e.g. a stun
            // that no longer matters once it's dead) can't affect the read — HealthPercent01 only
            // reflects the TakeDamage call above at this point.
            bool killedByThisHit = enemy.HealthPercent01 <= 0f;

            RollOnHitPassives(enemy);

            if (killedByThisHit)
            {
                RollExplodeOnKill(enemy);
            }

            TrySplitOnHit();

            if (remainingPierces <= 0)
            {
                Destroy(gameObject);
                return;
            }

            remainingPierces--;

            // Only a piercing hit can chain — retargets this same flying projectile at the
            // nearest un-hit enemy instead of letting it continue straight, so the chain is a
            // visible redirect (and can chain again from there) rather than an instant extra hit.
            TryChainRedirect(enemy);
        }

        /// <summary>
        /// Rolled independently per enemy hit rather than once at Launch, so Pierce shots
        /// get an independent chance to proc against each enemy they pass through.
        /// </summary>
        private void RollOnHitPassives(ArenaEnemy enemy)
        {
            if (onHitPassives.thunderFxPrefab != null && onHitPassives.thunderChancePercent > 0f && Random.value * 100f < onHitPassives.thunderChancePercent)
            {
                GameObject fxInstance = Instantiate(onHitPassives.thunderFxPrefab, enemy.transform.position, Quaternion.identity);
                ProcAreaDamage areaDamage = fxInstance.GetComponent<ProcAreaDamage>();

                // Guaranteed-hit AoE around the impact point, styled as a crit popup (size/motion) but tinted to
                // this character's own strike color instead of the default crit gold — scales with the player's
                // current Area of Effect.
                areaDamage?.Detonate(onHitPassives.thunderBonusDamage, true, areaOfEffectScale, onHitPassives.thunderStrikeColor);
            }

            if (onHitPassives.stunChancePercent > 0f && Random.value * 100f < onHitPassives.stunChancePercent)
            {
                enemy.ApplyStun(onHitPassives.stunDurationSeconds);
            }

            if (onHitPassives.slowChancePercent > 0f && Random.value * 100f < onHitPassives.slowChancePercent)
            {
                enemy.ApplySlow(onHitPassives.slowMagnitudePercent, onHitPassives.slowDurationSeconds);
            }

            if (onHitPassives.holyStrikeFxPrefab != null && onHitPassives.holyStrikeChancePercent > 0f && Random.value * 100f < onHitPassives.holyStrikeChancePercent)
            {
                GameObject holyFxInstance = Instantiate(onHitPassives.holyStrikeFxPrefab, enemy.transform.position, Quaternion.identity);
                holyFxInstance.GetComponent<ProcAreaDamage>()?.Detonate(onHitPassives.holyStrikeBonusDamage, true, areaOfEffectScale, onHitPassives.holyStrikeColor);
            }

            if (onHitPassives.staticStrikeFxPrefab != null && onHitPassives.staticStrikeChancePercent > 0f && Random.value * 100f < onHitPassives.staticStrikeChancePercent)
            {
                GameObject staticFxInstance = Instantiate(onHitPassives.staticStrikeFxPrefab, enemy.transform.position, Quaternion.identity);
                staticFxInstance.GetComponent<ProcAreaDamage>()?.Detonate(onHitPassives.staticStrikeBonusDamage, true, areaOfEffectScale, onHitPassives.staticStrikeColor);
            }
        }

        /// <summary>Dark Fork's on-hit proc: forks this projectile into two children angled +/-45 degrees off its current heading, each carrying the same damage/pierce/passives (minus the split flag itself, so a fork can't fork again). Fires at most once per projectile no matter how many enemies it goes on to pierce.</summary>
        private void TrySplitOnHit()
        {
            if (!onHitPassives.projectileSplitOnHit || hasSplitOnHit) return;

            hasSplitOnHit = true;

            SpawnFork(RotateDegrees(direction, 45f));
            SpawnFork(RotateDegrees(direction, -45f));
        }

        /// <summary>Spawns a fresh child projectile from sourcePrefab (falling back to this GameObject if unset) rather than cloning this live instance — avoids double-baking the current Area of Effect scale and inheriting this instance's already-hit enemy set.</summary>
        private void SpawnFork(Vector2 forkDirection)
        {
            GameObject prefabToSpawn = sourcePrefab != null ? sourcePrefab : gameObject;
            GameObject instance = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);

            if (!instance.TryGetComponent(out Projectile fork)) return;

            OnHitPassiveEffects forkPassives = onHitPassives;
            forkPassives.projectileSplitOnHit = false;

            fork.Launch(forkDirection, damage, speed, isCrit, remainingPierces, areaOfEffectScale, isRotatingProjectile, forkPassives, sourcePrefab);
        }

        /// <summary>
        /// Kicks off an ExplodeOnKill chain reaction rooted at the enemy this projectile just
        /// killed. No-op if the proc doesn't roll — once it does, every enemy it goes on to kill
        /// (see TriggerExplosion) explodes too, unconditionally, no re-roll needed.
        /// </summary>
        private void RollExplodeOnKill(ArenaEnemy killedEnemy)
        {
            if (onHitPassives.explodeOnKillChancePercent <= 0f || Random.value * 100f >= onHitPassives.explodeOnKillChancePercent) return;

            TriggerExplosion(killedEnemy, new HashSet<ArenaEnemy> { killedEnemy });
        }

        /// <summary>
        /// AoE-damages every enemy within explodeOnKillRadius of killedEnemy (scaled by the
        /// player's current Area of Effect) for 100% of killedEnemy's own max HP — usually enough
        /// to one-shot same-tier neighbors too. Any enemy this splash kills immediately triggers
        /// its own explosion in turn (recursively), so a single proc can chain through an entire
        /// pack. alreadyExploded is threaded through the recursion so no enemy explodes (or takes
        /// splash damage) more than once in the same chain. Enemies killed by any explosion in the
        /// chain still don't grant direct XP (only coins, same as a direct kill would skip via
        /// grantsExp: false) — instead each one independently rolls explodeOnKillBonusExpChance
        /// (~33%) for a single bonus exp crystal, so a chain can't be farmed as reliably as direct
        /// kills but isn't a total XP dead zone either.
        /// </summary>
        private void TriggerExplosion(ArenaEnemy killedEnemy, HashSet<ArenaEnemy> alreadyExploded)
        {
            Vector2 position = killedEnemy.transform.position;
            float radius = explodeOnKillRadius * areaOfEffectScale;
            float explosionDamage = killedEnemy.EffectiveMaxHealth;

            ExplosionVFX.Spawn(position, radius, onHitPassives.explodeOnKillColor);

            Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);

            foreach (Collider2D hit in hits)
            {
                if (!hit.TryGetComponent(out ArenaEnemy other) || !alreadyExploded.Add(other)) continue;

                other.TakeDamage(explosionDamage, grantsExp: false);

                if (other.HealthPercent01 <= 0f)
                {
                    if (Random.value < explodeOnKillBonusExpChance)
                    {
                        other.DropBonusExpCrystal();
                    }

                    TriggerExplosion(other, alreadyExploded);
                }
            }
        }

        /// <summary>Redirects this projectile's travel direction toward the nearest un-hit enemy in range, so it visibly flies there instead of continuing straight. No-op (projectile keeps its current course) if the chain doesn't proc or no target is found.</summary>
        private void TryChainRedirect(ArenaEnemy justHit)
        {
            if (onHitPassives.chainChancePercent <= 0f || Random.value * 100f >= onHitPassives.chainChancePercent) return;

            ArenaEnemy chainTarget = FindNearestChainTarget(justHit.transform.position);
            if (chainTarget == null) return;

            direction = ((Vector2)chainTarget.transform.position - (Vector2)transform.position).normalized;
            transform.rotation = Quaternion.Euler(0f, 0f, -90 + Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        private static Vector2 RotateDegrees(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }

        /// <summary>Nearest enemy this projectile hasn't already hit, within chainRadius of fromPosition — the ChainOnHit jump target.</summary>
        private ArenaEnemy FindNearestChainTarget(Vector2 fromPosition)
        {
            ArenaEnemy nearest = null;
            float nearestSqrDistance = chainRadius * chainRadius;

            foreach (ArenaEnemy candidate in ArenaEnemy.Active)
            {
                if (hitEnemies.Contains(candidate)) continue;

                float sqrDistance = ((Vector2)candidate.transform.position - fromPosition).sqrMagnitude;
                if (sqrDistance <= nearestSqrDistance)
                {
                    nearest = candidate;
                    nearestSqrDistance = sqrDistance;
                }
            }

            return nearest;
        }
    }
}
