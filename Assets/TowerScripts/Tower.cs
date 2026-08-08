using UnityEngine;
using System;
using TwinsDefense.Enemies;
using TwinsDefense.Towers;


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

        private float damageMultiplier = 1f;
        private float rangeMultiplier = 1f;
        private float fireRateMultiplier = 1f;
        private float attackTimer;

        /// <summary>Read-only access to this tower's data asset, used by upgrade/UI code.</summary>
        public TowerData Data => data;

        /// <summary>Raised whenever any placed tower is clicked, so the upgrade panel can show it.</summary>
        public static event Action<Tower> OnTowerClicked;


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
        /// TODO: apply data.effectType via data.effectStats once subclass VFX exist —
        /// currently every tower deals plain damage, either instantly or via a projectile.
        /// </summary>
public void Attack()
        {
            Enemy target = FindNearestEnemyInRange();

            if (target == null) return;

            float amount = data.damage * damageMultiplier;

            if (data.projectilePrefab != null)
            {
                GameObject instance = Instantiate(data.projectilePrefab, shootPoint.position, Quaternion.identity);
                Projectile projectile = instance.GetComponent<Projectile>();

                if (projectile != null)
                {
                    projectile.Launch(target, amount);
                }
                else
                {
                    target.TakeDamage(amount);
                }
            }
            else
            {
                target.TakeDamage(amount);
            }
        }

private Enemy FindNearestEnemyInRange()
        {
            float effectiveRange = data.range * rangeMultiplier;
            Enemy nearest = null;
            float nearestSqrDistance = effectiveRange * effectiveRange;

            foreach (Enemy enemy in Enemy.Active)
            {
                float sqrDistance = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;

                if (sqrDistance <= nearestSqrDistance)
                {
                    nearest = enemy;
                    nearestSqrDistance = sqrDistance;
                }
            }

            return nearest;
        }

        private void Update()
        {
            if (data == null || !data.IsDamageDealer) return;

            float effectiveFireRate = data.fireRate * fireRateMultiplier;
            if (effectiveFireRate <= 0f) return;

            attackTimer += Time.deltaTime;

            if (attackTimer >= 1f / effectiveFireRate)
            {
                attackTimer = 0f;
                Attack();
            }
        }

        /// <summary>Applies the multipliers for the tower's current star level. Called by TowerStarUpgrade.</summary>
        public void SetStarMultipliers(float damageMult, float rangeMult, float fireRateMult)
        {
            damageMultiplier = damageMult;
            rangeMultiplier = rangeMult;
            fireRateMultiplier = fireRateMult;
        }

        private void OnMouseDown()
        {
            OnTowerClicked?.Invoke(this);
        }


        private void OnDrawGizmosSelected()
        {
            if (data == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, data.range);
        }
    }
}
