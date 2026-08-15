using UnityEngine;
using UnityEngine.InputSystem;
using TwinsDefense.Environment;
using TwinsDefense.Systems;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Free movement for the solo arena run character, using whichever keys
    /// are bound in KeyBindings (rebindable in Settings, WASD by default)
    /// plus the arrow keys as a fixed fallback that's always active. Open
    /// arena, no grid/pathing — the player can move in any direction.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Left unassigned, resolved via GetComponent on first Awake — supplies the idle pose sprite (CharacterMetaData.idleSprite) shown while standing still.")]
        [SerializeField] private PlayerCharacterData characterData;

        [Header("Knockback")]
        [Tooltip("How long movement input is overridden by the knockback velocity after a hit.")]
        [SerializeField] private float knockbackDuration = 0.15f;

        [Header("Arena Bounds")]
        [Tooltip("Keeps the player's sprite from visually overlapping the edge of the background tilemap (ArenaBounds).")]
        [SerializeField] private float boundsMargin = 0.3f;

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

            if (characterData == null)
            {
                characterData = GetComponent<PlayerCharacterData>();
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

            if (keyboard[KeyBindings.Left].isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard[KeyBindings.Right].isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
            if (keyboard[KeyBindings.Down].isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
            if (keyboard[KeyBindings.Up].isPressed || keyboard.upArrowKey.isPressed) y += 1f;

            moveInput = new Vector2(x, y);
            UpdateVisuals();
        }

        /// <summary>Toggles the Animator on while moving, snaps the sprite to the character's idle pose while stopped (instead of leaving it frozen on whatever frame the Animator was disabled on), and flips the sprite to face left/right.</summary>
        private void UpdateVisuals()
        {
            bool isMoving = moveInput != Vector2.zero;

            if (animator != null)
            {
                animator.enabled = isMoving;
            }

            if (!isMoving && spriteRenderer != null && characterData != null && characterData.Current != null && characterData.Current.idleSprite != null)
            {
                spriteRenderer.sprite = characterData.Current.idleSprite;
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

        /// <summary>Clamps the player back inside ArenaBounds after physics has applied this frame's movement, so it can never walk past the background tilemap.</summary>
        private void LateUpdate()
        {
            if (ArenaBounds.Instance == null) return;

            rb.position = ArenaBounds.Instance.Clamp(rb.position, boundsMargin, boundsMargin);
        }

        /// <summary>Overrides movement input with a short burst away from the hit source. Called by PlayerHealth on TakeDamage.</summary>
        public void ApplyKnockback(Vector2 direction, float force)
        {
            knockbackVelocity = direction.normalized * force;
            knockbackTimer = knockbackDuration;
        }
    }
}
