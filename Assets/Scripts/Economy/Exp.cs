using UnityEngine;
using TwinsDefense.Progression;
using TwinsDefense.Player;

namespace TwinsDefense.Economy
{
    /// <summary>
    /// Pickup dropped by defeated enemies. Sits idle until a PickupMagnet trigger
    /// calls <see cref="Attract"/>, then homes toward the player and adds a
    /// fixed slice of XP to LevelManager on arrival.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Exp : MonoBehaviour, IAttractable
    {
        [SerializeField] private float attractSpeed = 8f;
        [SerializeField] private float collectDistance = 0.25f;

        private Transform target;

        /// <summary>Called by PickupMagnet once this pickup enters the player's range.</summary>
        public void Attract(Transform magnetTarget)
        {
            target = magnetTarget;
        }

        private void Update()
        {
            if (target == null) return;

            transform.position = Vector3.MoveTowards(transform.position, target.position, attractSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.position) <= collectDistance)
            {
                Collect();
            }
        }

        private void Collect()
        {
            float multiplier = 1f;
            if (target != null && target.TryGetComponent(out PlayerStats stats))
            {
                multiplier = stats.xpGainMultiplier;
            }

            LevelManager.Instance?.AddExp(multiplier);
            Destroy(gameObject);
        }
    }
}
