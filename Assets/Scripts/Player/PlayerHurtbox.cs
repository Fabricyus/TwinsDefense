using UnityEngine;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Marks the player's actual body-sized hit collider so enemies have
    /// something precise to detect contact against — distinct from the much
    /// wider PickupMagnet trigger on the Player root. Lives on a small child
    /// GameObject with its own Collider2D (isTrigger).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerHurtbox : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;

        public PlayerHealth Health => health;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponentInParent<PlayerHealth>();
            }
        }
    }
}
