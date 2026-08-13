using UnityEngine;
using TwinsDefense.Player;
using TwinsDefense.Progression;

namespace TwinsDefense.Economy
{
    /// <summary>
    /// Pickup dropped by defeated enemies. Sits idle until a PickupMagnet trigger
    /// calls <see cref="Attract"/>, then homes toward the player and adds its
    /// value to CoinManager on arrival.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Coin : MonoBehaviour, IAttractable
    {
        [SerializeField] private int coinValue = 10;
        [SerializeField] private float attractSpeed = 8f;
        [SerializeField] private float collectDistance = 0.25f;

        private Transform target;

        /// <summary>Called by PickupMagnet once this coin enters the player's pickup range.</summary>
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
                multiplier = stats.coinGainMultiplier;
            }

            CoinManager.Instance?.Add(Mathf.RoundToInt(coinValue * multiplier));
            RunStats.Instance?.RegisterCoinCollected();
            Destroy(gameObject);
        }
    }
}
