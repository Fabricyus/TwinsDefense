using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.VFX;
using TwinsDefense.Economy;
using TwinsDefense.Player;
using TwinsDefense.Progression;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Runtime behaviour for an arena-mode enemy: chases the player directly
    /// with simple steering (no NavMesh/waypoints) and can be damaged by the
    /// player's projectiles. Separate from the legacy waypoint-based Enemy.cs
    /// used by the Tower Defense scenes, which this does not modify or replace.
    /// </summary>
    public class ArenaEnemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 10f;
        [SerializeField] private float moveSpeed = 2f;
        [Tooltip("Multiplies maxHealth based on the player's level at spawn — a flattened curve (e.g. 1.0x at level 0 up to ~2.2x at level 30) keeps trash HP from snowballing, unlike bosses which can scale much harder.")]
        [SerializeField] private AnimationCurve hpMultiplierByLevel = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(10f, 1.3f), new Keyframe(20f, 1.6f), new Keyframe(30f, 2.2f));

        [Header("Hit Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Swapped in on hit, replacing the sprite's own colors with solid white for the flash. Tinting .color toward white does nothing when the sprite's base colors are already near-white.")]
        [SerializeField] private Material flashMaterial;
        [SerializeField] private float flashDuration = 0.08f;
        [SerializeField] private Vector3 damagePopupOffset = new Vector3(0f, 0.4f, 0f);

        [Header("Coin Drop")]
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private int minCoinDrop = 1;
        [SerializeField] private int maxCoinDrop = 3;
        [SerializeField] private float coinScatterRadius = 0.4f;

        [Header("Exp Drop")]
        [SerializeField] private GameObject expPrefab;
        [SerializeField] private int minExpDrop = 1;
        [SerializeField] private int maxExpDrop = 2;
        [SerializeField] private float expScatterRadius = 0.4f;

        [Header("Contact Damage")]
        [SerializeField] private float contactDamage = 5f;
        [Tooltip("Multiplies contactDamage based on the player's level at spawn — kept much flatter than hpMultiplierByLevel, so late-run trash threatens by surrounding the player, not by one hit taking half their bar.")]
        [SerializeField] private AnimationCurve damageMultiplierByLevel = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(10f, 1.15f), new Keyframe(20f, 1.4f), new Keyframe(30f, 1.8f));
        [SerializeField] private float contactDamageInterval = 1f;
        [Tooltip("Brief circle flash spawned under the enemy each time contact damage actually lands.")]
        [SerializeField] private GameObject attackCirclePrefab;
        [SerializeField] private Vector3 attackCircleOffset = new Vector3(0f, -0.15f, 0f);
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private float knockbackDuration = 0.15f;

        [Header("Movement")]
        [Tooltip("If true, locks onto the player's position once at spawn and walks that fixed straight line forever, instead of continuously chasing the player like most enemies.")]
        [SerializeField] private bool moveInStraightLine = false;

        /// <summary>Exposed so companion scripts (e.g. BombEnemy) can reuse the same contact-hit VFX instead of duplicating the reference.</summary>
        public GameObject AttackCirclePrefab => attackCirclePrefab;
        public Vector3 AttackCircleOffset => attackCircleOffset;

        /// <summary>Current health as a 0-1 fraction of this spawn's effective max (maxHealth + level scaling) — used by bosses to gate phase transitions.</summary>
        public float HealthPercent01 => effectiveMaxHealth > 0f ? currentHealth / effectiveMaxHealth : 0f;

        /// <summary>This spawn's effective max HP — used by ExplodeOnKill to scale explosion damage to 100% of the dying enemy's own health.</summary>
        public float EffectiveMaxHealth => effectiveMaxHealth;

        private float currentHealth;
        private float effectiveMaxHealth;
        private float effectiveContactDamage;
        private bool isInvulnerable;
        private Transform player;
        private Vector2 lockedDirection;
        private Material baseMaterial;
        private Coroutine flashRoutine;
        private float contactDamageTimer;
        private Vector2 knockbackVelocity;
        private float knockbackTimer;
        private float slowMultiplier = 1f;
        private float slowTimer;
        private float stunTimer;
        private SlowTrailVFX slowTrail;
        private float speedMultiplier = 1f;
        private float damageTakenMultiplier = 1f;
        private bool isDead;

        /// <summary>All currently active arena enemies, used by AutoAttack for nearest-target queries.</summary>
        public static readonly HashSet<ArenaEnemy> Active = new HashSet<ArenaEnemy>();

        /// <summary>Raised when this enemy's health reaches zero.</summary>
        public event Action OnEnemyDefeated;

        /// <summary>Raised whenever currentHealth changes (HealthPercent01, 0-1), so UI like boss HP bars can refresh without polling.</summary>
        public event Action<float> OnHealthChanged;

        /// <summary>Raised whenever SetInvulnerable actually changes the invulnerability state (not on redundant same-value calls) — used by BossShieldIconUI to show/hide a shield icon while a boss is in iframes.</summary>
        public event Action<bool> OnInvulnerabilityChanged;

        /// <summary>True while TakeDamage is a no-op due to SetInvulnerable(true) — e.g. a boss mid-cast on an attack that grants iframes.</summary>
        public bool IsInvulnerable => isInvulnerable;

        private void Awake()
        {
            int level = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : 0;
            effectiveMaxHealth = maxHealth * Mathf.Max(0f, hpMultiplierByLevel.Evaluate(level));
            currentHealth = effectiveMaxHealth;
            effectiveContactDamage = contactDamage * Mathf.Max(0f, damageMultiplierByLevel.Evaluate(level));
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }

            if (moveInStraightLine && player != null)
            {
                lockedDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                baseMaterial = spriteRenderer.sharedMaterial;
            }
        }

        private void OnEnable()
        {
            Active.Add(this);
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        private void Update()
        {
            if (player == null) return;

            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.deltaTime;
                transform.position += (Vector3)(knockbackVelocity * Time.deltaTime);
                FaceTarget();
                return;
            }

            if (stunTimer > 0f)
            {
                stunTimer -= Time.deltaTime;
                FaceTarget();
                return;
            }

            if (slowTimer > 0f)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0f)
                {
                    slowMultiplier = 1f;

                    if (slowTrail != null)
                    {
                        slowTrail.StopAndFade();
                        slowTrail = null;
                    }
                }
            }

            Vector2 direction = moveInStraightLine ? lockedDirection : ((Vector2)player.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * moveSpeed * slowMultiplier * speedMultiplier * Time.deltaTime);

            FaceTarget();
        }

        /// <summary>Stuns this enemy for the given duration (refreshes rather than stacks if already stunned).</summary>
        public void ApplyStun(float duration)
        {
            stunTimer = Mathf.Max(stunTimer, duration);
        }

        /// <summary>Slows this enemy's move speed by percent (0-100) for duration seconds. Keeps the strongest slow active instead of stacking multiple procs. Spawns (or extends) an icy trail VFX for as long as the slow is active.</summary>
        public void ApplySlow(float percent, float duration)
        {
            float multiplier = 1f - Mathf.Clamp(percent, 0f, 100f) / 100f;
            slowMultiplier = Mathf.Min(slowMultiplier, multiplier);
            slowTimer = Mathf.Max(slowTimer, duration);

            if (slowTrail == null)
            {
                slowTrail = SlowTrailVFX.Attach(transform);
            }
        }

        /// <summary>Flips the sprite horizontally so the enemy faces the player, using a simple X-position comparison instead of sprite-facing math.</summary>
        private void FaceTarget()
        {
            if (spriteRenderer == null) return;

            if (moveInStraightLine)
            {
                if (lockedDirection.x != 0f)
                {
                    spriteRenderer.flipX = lockedDirection.x < 0f;
                }
                return;
            }

            spriteRenderer.flipX = transform.position.x > player.position.x;
        }

        /// <summary>Ticks contact damage into the player's actual hurtbox (not the wider pickup-magnet trigger) while overlapping it.</summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            if (player == null || !other.TryGetComponent(out PlayerHurtbox hurtbox)) return;

            contactDamageTimer += Time.deltaTime;

            if (contactDamageTimer >= contactDamageInterval)
            {
                contactDamageTimer = 0f;
                hurtbox.Health.TakeDamage(effectiveContactDamage, transform.position);
                SpawnAttackCircle();
                ApplyKnockback();
            }
        }

        private void SpawnAttackCircle()
        {
            if (attackCirclePrefab == null) return;

            Instantiate(attackCirclePrefab, transform.position + attackCircleOffset, Quaternion.identity);
        }

        /// <summary>Bounces this enemy back away from the player after landing a contact hit, instead of stacking on top of them.</summary>
        private void ApplyKnockback()
        {
            Vector2 pushDirection = (Vector2)transform.position - (Vector2)player.position;
            if (pushDirection.sqrMagnitude < 0.0001f)
            {
                pushDirection = Vector2.up;
            }

            knockbackVelocity = pushDirection.normalized * knockbackForce;
            knockbackTimer = knockbackDuration;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerHurtbox _))
            {
                contactDamageTimer = 0f;
            }
        }

        /// <summary>While true, TakeDamage is a no-op — used by boss phases (e.g. ReaperEnemy's hazard-field phase) that shouldn't be interruptible.</summary>
public void SetInvulnerable(bool invulnerable)
        {
            if (isInvulnerable == invulnerable) return;

            isInvulnerable = invulnerable;
            OnInvulnerabilityChanged?.Invoke(isInvulnerable);
        }

        /// <summary>Multiplies this enemy's base move speed (compounds with slow) — used by boss phases that permanently speed up (e.g. SkullBoss's enrage phase).</summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>Multiplies incoming damage before it's applied — used by boss phases that harden defense (e.g. SkullBoss's enrage phase, where 0.5 = takes half damage).</summary>
        public void SetDamageTakenMultiplier(float multiplier)
        {
            damageTakenMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>Applies damage to this enemy, defeating it once health reaches zero. grantsExp false skips the XP drop (only coins) — used for ExplodeOnKill splash kills, which shouldn't reward XP the same as a direct kill.</summary>
public void TakeDamage(float amount, bool isCrit = false, bool grantsExp = true)
        {
            // isDead guards against a second TakeDamage call landing in the same frame as the
            // killing blow (e.g. two projectiles overlapping this enemy at once) — Destroy(gameObject)
            // only takes effect at end of frame, so without this a boss could fire its on-death
            // report (coins, XP, campaign/achievement counters) twice for one kill.
            if (isInvulnerable || isDead) return;

            float appliedDamage = amount * damageTakenMultiplier;
            currentHealth -= appliedDamage;
            TriggerHitFlash();
            DamagePopupSpawner.Spawn(transform.position + damagePopupOffset, appliedDamage, isCrit);
            OnHealthChanged?.Invoke(HealthPercent01);

            if (currentHealth <= 0f)
            {
                isDead = true;
                OnEnemyDefeated?.Invoke();
                RunStats.Instance?.RegisterKill();
                DeathSmokeVFX.Spawn(transform.position);
                DropCoins();

                if (grantsExp)
                {
                    DropExp();
                }

                Destroy(gameObject);
            }
        }

        /// <summary>Instantly defeats this enemy without dropping coins/XP — used to clear the arena when a boss spawns, so the fight is boss-vs-player only.</summary>
public void HitKill()
        {
            if (isDead) return;

            isDead = true;
            currentHealth = 0f;
            OnEnemyDefeated?.Invoke();
            RunStats.Instance?.RegisterKill();
            DeathSmokeVFX.Spawn(transform.position);
            Destroy(gameObject);
        }

        private void DropCoins()
        {
            if (coinPrefab == null) return;

            int coinCount = UnityEngine.Random.Range(minCoinDrop, maxCoinDrop + 1);

            for (int i = 0; i < coinCount; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * coinScatterRadius;
                Instantiate(coinPrefab, transform.position + (Vector3)offset, Quaternion.identity);
            }
        }

        private void DropExp()
        {
            if (expPrefab == null) return;

            int expCount = UnityEngine.Random.Range(minExpDrop, maxExpDrop + 1);

            for (int i = 0; i < expCount; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * expScatterRadius;
                Instantiate(expPrefab, transform.position + (Vector3)offset, Quaternion.identity);
            }
        }

        private void TriggerHitFlash()
        {
            if (spriteRenderer == null || flashMaterial == null) return;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            spriteRenderer.material = flashMaterial;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.material = baseMaterial;
            flashRoutine = null;
        }
    }
}
