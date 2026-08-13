using UnityEngine;
using TwinsDefense.Economy;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Trigger zone around the player that pulls any IAttractable pickup
    /// (coins, exp, ...) in once it enters range. Expects a Collider2D
    /// (isTrigger) on this GameObject sized to the magnet radius.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PickupMagnet : MonoBehaviour
    {
        [Tooltip("Pickups fly toward this transform. Defaults to this GameObject if left unassigned.")]
        [SerializeField] private Transform attractTarget;

        private CircleCollider2D magnetCollider;
        private PlayerStats stats;

        private void Awake()
        {
            if (attractTarget == null)
            {
                attractTarget = transform;
            }

            magnetCollider = GetComponent<CircleCollider2D>();
            stats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            // Keeps the trigger radius in sync so a Magnet Pull card takes effect immediately.
            if (magnetCollider != null)
            {
                magnetCollider.radius = stats.pickupRadius;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out IAttractable attractable))
            {
                attractable.Attract(attractTarget);
            }
        }
    }
}
