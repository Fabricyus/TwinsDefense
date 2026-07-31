using System;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Runtime behaviour for a spawned enemy. All stats come from the linked
    /// EnemyData asset; this component must never hardcode enemy numbers.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private EnemyData data;

        [Header("Path")]
        [Tooltip("Ordered waypoints defining the fixed path this enemy follows.")]
        public Transform[] waypoints;

        private float currentHealth;

        /// <summary>Raised when this enemy is defeated, so systems like the Gem economy can react without a direct reference.</summary>
        public event Action OnEnemyDefeated;

        private void Start()
        {
            if (data == null)
            {
                Debug.LogWarning($"{name}: Enemy has no EnemyData assigned.", this);
                return;
            }

            currentHealth = data.maxHealth;
        }

        /// <summary>
        /// TODO: implement real path-following logic once phase 1's path is
        /// built in the scene — move this enemy along `waypoints` at
        /// data.moveSpeed, and deal data.damageToBase when the final waypoint
        /// is reached.
        /// </summary>
        public void MoveAlongPath()
        {
        }

        /// <summary>Applies damage to this enemy, defeating it once health reaches zero.</summary>
        public void TakeDamage(float amount)
        {
            currentHealth -= amount;

            if (currentHealth <= 0f)
            {
                // TODO: play death VFX, grant data.gemReward to the player's Gem economy,
                // and despawn/pool this enemy.
                OnEnemyDefeated?.Invoke();
            }
        }
    }
}
