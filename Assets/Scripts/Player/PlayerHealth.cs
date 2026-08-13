using System;
using System.Collections;
using UnityEngine;
using TwinsDefense.Systems;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Tracks the player's current HP against PlayerStats.maxHP. Incoming
    /// damage is mitigated by PlayerStats.defense (flat reduction, floor of
    /// 1) and triggers a brief invincibility window (PlayerStats.iFrameDuration).
    /// Regenerates PlayerStats.hpRegen per second while alive and not topped up.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Hit Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Swapped in on hit, replacing the sprite's own colors with solid white for the flash. Optional — skipped if left unassigned.")]
        [SerializeField] private Material flashMaterial;
        [SerializeField] private float flashDuration = 0.08f;

        [Header("Knockback & Camera Shake")]
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private float shakeDuration = 0.15f;
        [SerializeField] private float shakeMagnitude = 0.15f;
        [Tooltip("Left unassigned, resolved via FindAnyObjectByType on first hit.")]
        [SerializeField] private CameraFollow cameraFollow;

        private PlayerStats stats;
        private PlayerController playerController;
        private Material baseMaterial;
        private Coroutine flashRoutine;
        private float regenAccumulator;
        private float invincibleTimer;

        public float CurrentHP { get; private set; }
        public bool IsInvincible { get; private set; }
        public bool IsDead => CurrentHP <= 0f;

        /// <summary>Raised whenever CurrentHP changes, so UI can refresh without polling. Args: (current, max).</summary>
        public event Action<float, float> OnHealthChanged;

        /// <summary>Raised once when CurrentHP first reaches zero.</summary>
        public event Action OnPlayerDied;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();

            playerController = GetComponent<PlayerController>();

            if (cameraFollow == null)
            {
                cameraFollow = FindAnyObjectByType<CameraFollow>();
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

        private void Start()
        {
            // Runs after every component's Awake, so PlayerCharacterData has
            // already applied the selected character's base maxHP by now.
            CurrentHP = stats.maxHP;
            OnHealthChanged?.Invoke(CurrentHP, stats.maxHP);
        }

        private void Update()
        {
            if (IsInvincible)
            {
                invincibleTimer -= Time.deltaTime;
                if (invincibleTimer <= 0f)
                {
                    IsInvincible = false;
                }
            }

            if (IsDead || stats.hpRegen <= 0f || CurrentHP >= stats.maxHP) return;

            regenAccumulator += Time.deltaTime;
            if (regenAccumulator >= 1f)
            {
                regenAccumulator -= 1f;
                Heal(stats.hpRegen);
            }
        }

        /// <summary>Applies incoming damage, mitigated by Defense and gated by the current i-frame window. sourcePosition drives the knockback direction (pushed away from it).</summary>
        public void TakeDamage(float amount, Vector2 sourcePosition)
        {
            if (IsDead || IsInvincible || amount <= 0f) return;

            float mitigatedDamage = Mathf.Max(1f, amount - stats.defense);
            CurrentHP = Mathf.Max(0f, CurrentHP - mitigatedDamage);

            invincibleTimer = stats.iFrameDuration;
            IsInvincible = stats.iFrameDuration > 0f;

            TriggerHitFlash();
            ApplyKnockbackAndShake(sourcePosition);
            OnHealthChanged?.Invoke(CurrentHP, stats.maxHP);

            if (CurrentHP <= 0f)
            {
                OnPlayerDied?.Invoke();
            }
        }

        private void ApplyKnockbackAndShake(Vector2 sourcePosition)
        {
            Vector2 knockbackDirection = (Vector2)transform.position - sourcePosition;
            if (knockbackDirection.sqrMagnitude < 0.0001f)
            {
                knockbackDirection = Vector2.up;
            }

            playerController?.ApplyKnockback(knockbackDirection, knockbackForce);
            cameraFollow?.Shake(shakeDuration, shakeMagnitude);
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead) return;

            CurrentHP = Mathf.Min(stats.maxHP, CurrentHP + amount);
            OnHealthChanged?.Invoke(CurrentHP, stats.maxHP);
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
