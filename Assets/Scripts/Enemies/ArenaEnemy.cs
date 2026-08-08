using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        [Header("Hit Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Swapped in on hit, replacing the sprite's own colors with solid white for the flash. Tinting .color toward white does nothing when the sprite's base colors are already near-white.")]
        [SerializeField] private Material flashMaterial;
        [SerializeField] private float flashDuration = 0.08f;

        private float currentHealth;
        private Transform player;
        private Material baseMaterial;
        private Coroutine flashRoutine;

        /// <summary>All currently active arena enemies, used by AutoAttack for nearest-target queries.</summary>
        public static readonly HashSet<ArenaEnemy> Active = new HashSet<ArenaEnemy>();

        /// <summary>Raised when this enemy's health reaches zero.</summary>
        public event Action OnEnemyDefeated;

        private void Awake()
        {
            currentHealth = maxHealth;
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

        /// <summary>Applies damage to this enemy, defeating it once health reaches zero.</summary>
        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            TriggerHitFlash();

            if (currentHealth <= 0f)
            {
                OnEnemyDefeated?.Invoke();
                Destroy(gameObject);
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
