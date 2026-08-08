using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Economy;

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

        [Header("Hit Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Swapped in on hit, replacing the sprite's own colors with solid white for the flash. Tinting .color toward white does nothing when the sprite's base colors are already near-white.")]
        [SerializeField] private Material flashMaterial;
        [SerializeField] private float flashDuration = 0.08f;

        private float currentHealth;
        private int currentWaypointIndex;
        private Material baseMaterial;
        private Coroutine flashRoutine;

        /// <summary>All currently active enemies, used by towers for range queries without per-enemy colliders.</summary>
        public static readonly HashSet<Enemy> Active = new HashSet<Enemy>();

        /// <summary>Raised when this enemy reaches the final waypoint (i.e. reaches the player's base).</summary>
        public event Action OnReachedGoal;


        /// <summary>Raised when this enemy is defeated, so systems like the Gem economy can react without a direct reference.</summary>
        public event Action OnEnemyDefeated;

        private void Start()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                baseMaterial = spriteRenderer.sharedMaterial;
            }

            if (data == null)
            {
                Debug.LogWarning($"{name}: Enemy has no EnemyData assigned.", this);
                return;
            }

            currentHealth = data.maxHealth;
        }

private void OnEnable()
        {
            Active.Add(this);
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        private void Update()
        {
            if (data == null) return;
            MoveAlongPath();
        }


        /// <summary>
        /// TODO: implement real path-following logic once phase 1's path is
        /// built in the scene — move this enemy along `waypoints` at
        /// data.moveSpeed, and deal data.damageToBase when the final waypoint
        /// is reached.
        /// </summary>
public void MoveAlongPath()
        {
            if (waypoints == null || waypoints.Length == 0 || currentWaypointIndex >= waypoints.Length)
            {
                return;
            }

            Transform target = waypoints[currentWaypointIndex];
            float step = data.moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);

            if (Vector3.Distance(transform.position, target.position) <= 0.01f)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    OnReachedGoal?.Invoke();
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>Applies damage to this enemy, defeating it once health reaches zero.</summary>
public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            TriggerHitFlash();

            if (currentHealth <= 0f)
            {
                // TODO: play death VFX once real art/VFX exists.
                if (GemsManager.Instance != null)
                {
                    GemsManager.Instance.Add(data.gemReward);
                }

                OnEnemyDefeated?.Invoke();
                Destroy(gameObject);
            }
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
