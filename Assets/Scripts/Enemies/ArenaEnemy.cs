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
        [Tooltip("Flat HP added on spawn for each player level (e.g. 2 -> HP = maxHealth + level * 2).")]
        [SerializeField] private float hpPerLevel = 2f;

        [Header("Hit Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        //[SerializeField] private GameObject deathPrefab;
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
        [SerializeField] private float contactDamageInterval = 1f;
        [Tooltip("Brief circle flash spawned under the enemy each time contact damage actually lands.")]
        [SerializeField] private GameObject attackCirclePrefab;
        [SerializeField] private Vector3 attackCircleOffset = new Vector3(0f, -0.15f, 0f);
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private float knockbackDuration = 0.15f;

        private float currentHealth;
        private Transform player;
        private Material baseMaterial;
        private Coroutine flashRoutine;
        private float contactDamageTimer;
        private Vector2 knockbackVelocity;
        private float knockbackTimer;

        /// <summary>All currently active arena enemies, used by AutoAttack for nearest-target queries.</summary>
        public static readonly HashSet<ArenaEnemy> Active = new HashSet<ArenaEnemy>();

        /// <summary>Raised when this enemy's health reaches zero.</summary>
        public event Action OnEnemyDefeated;

        private void Awake()
        {
            int level = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : 0;
            currentHealth = maxHealth + level * hpPerLevel;
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
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

            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

            FaceTarget();
        }

        /// <summary>Flips the sprite horizontally so the enemy faces the player, using a simple X-position comparison instead of sprite-facing math.</summary>
        private void FaceTarget()
        {
            if (spriteRenderer == null) return;

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
                hurtbox.Health.TakeDamage(contactDamage, transform.position);
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

        /// <summary>Applies damage to this enemy, defeating it once health reaches zero.</summary>
        public void TakeDamage(float amount, bool isCrit = false)
        {
            currentHealth -= amount;
            TriggerHitFlash();
            DamagePopupSpawner.Spawn(transform.position + damagePopupOffset, amount, isCrit);

            if (currentHealth <= 0f)
            {
                OnEnemyDefeated?.Invoke();
                RunStats.Instance?.RegisterKill();
                //GameObject tmp = Instantiate(deathPrefab, transform.position + (Vector3)this.gameObject.transform.position, Quaternion.identity);
                //Destroy(tmp, 0.11f);
                DropCoins();
                DropExp();
                Destroy(gameObject);
            }
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
