using UnityEngine;
using UnityEngine.InputSystem;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Free WASD/arrow-key movement for the solo arena run character.
    /// Open arena, no grid/pathing — the player can move in any direction.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Visuals")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D rb;
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

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
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }
    }
}
