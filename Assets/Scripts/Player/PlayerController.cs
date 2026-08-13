using UnityEngine;
using UnityEngine.InputSystem;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Free WASD/arrow-key movement for the solo arena run character.
    /// Open arena, no grid/pathing — the player can move in any direction.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Knockback")]
        [Tooltip("How long movement input is overridden by the knockback velocity after a hit.")]
        [SerializeField] private float knockbackDuration = 0.15f;

        private Rigidbody2D rb;
        private PlayerStats stats;
        private Vector2 moveInput;
        private Vector2 knockbackVelocity;
        private float knockbackTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            stats = GetComponent<PlayerStats>();

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                moveInput = Vector2.zero;
                UpdateVisuals();
                return;
            }

            float x = 0f;
            float y = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;

            moveInput = new Vector2(x, y);
            UpdateVisuals();
        }

        /// <summary>Toggles the Animator on while moving and flips the sprite to face left/right.</summary>
        private void UpdateVisuals()
        {
            if (animator != null)
            {
                animator.enabled = moveInput != Vector2.zero;
            }

            if (spriteRenderer != null)
            {
                if (moveInput.x < 0f)
                {
                    spriteRenderer.flipX = true;
                }
                else if (moveInput.x > 0f)
                {
                    spriteRenderer.flipX = false;
                }
            }
        }

        private void FixedUpdate()
        {
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.fixedDeltaTime;
                rb.linearVelocity = knockbackVelocity;
                return;
            }

            rb.linearVelocity = moveInput.normalized * stats.moveSpeed;
        }

        /// <summary>Overrides movement input with a short burst away from the hit source. Called by PlayerHealth on TakeDamage.</summary>
        public void ApplyKnockback(Vector2 direction, float force)
        {
            knockbackVelocity = direction.normalized * force;
            knockbackTimer = knockbackDuration;
        }
    }
}
