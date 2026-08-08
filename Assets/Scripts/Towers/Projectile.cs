using UnityEngine;
using TwinsDefense.Enemies;

namespace TwinsDefense.Towers
{
    /// <summary>
    /// Travels from a tower's shootPoint toward the enemy it was fired at,
    /// applying its carried damage on impact. Self-destroys on hit, on losing
    /// its target, or after maxLifetime as a safety net against orphaned shots.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float maxLifetime = 3f;

        private Enemy target;
        private float damage;
        private float lifeTimer;

        /// <summary>Assigns this projectile's target and damage right after Instantiate.</summary>
        public void Launch(Enemy target, float damage)
        {
            this.target = target;
            this.damage = damage;
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;

            if (lifeTimer >= maxLifetime || target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 toTarget = target.transform.position - transform.position;
            float step = speed * Time.deltaTime;

            if (toTarget.magnitude <= step)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            Vector3 direction = toTarget.normalized;
            transform.position += direction * step;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
    }
}
