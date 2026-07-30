using UnityEngine;

namespace TwinsDefense.Data
{
    /// <summary>
    /// Runtime behaviour for a placed tower. All stats come from the linked
    /// TowerData asset; this component must never hardcode tower numbers.
    /// </summary>
    public class Tower : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private TowerData data;

        [Header("Combat")]
        [Tooltip("Origin used for future projectile spawning/aiming. Defaults to this transform if left unassigned.")]
        [SerializeField] private Transform shootPoint;

        private void Start()
        {
            if (data == null)
            {
                Debug.LogWarning($"{name}: Tower has no TowerData assigned.", this);
                return;
            }

            if (shootPoint == null)
            {
                shootPoint = transform;
            }
        }

        /// <summary>
        /// TODO: implement real targeting/attack logic once the enemy and wave
        /// systems exist — find the closest/first enemy within data.range,
        /// fire from shootPoint, and apply data.effectType via data.effectStats.
        /// </summary>
        public void Attack()
        {
        }

        private void OnDrawGizmosSelected()
        {
            if (data == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, data.range);
        }
    }
}
