using UnityEngine;
using TwinsDefense.Enemies;

namespace TwinsDefense.Combat
{
    /// <summary>
    /// Straight-line projectile fired by the player's AutoAttack. Travels in
    /// a fixed direction (no homing), applies damage to the first ArenaEnemy
    /// it collides with, and self-destroys on hit or after lifetime.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;

        private Vector2 direction;
        private float speed;
        private float damage;
        private bool isCrit;
        private float lifeTimer;

        private void Awake()
        {
            // Kinematic so it never reacts to physics/gravity, but still raises
            // trigger events against the enemies' (non-rigidbody) colliders.
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        /// <summary>Assigns this projectile's travel direction, damage and speed right after Instantiate.</summary>
        public void Launch(Vector2 direction, float damage, float speed, bool isCrit = false)
        {
            this.direction = direction.normalized;
            this.damage = damage;
            this.speed = speed;
            this.isCrit = isCrit;

            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg);
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;

            if (lifeTimer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ArenaEnemy enemy = other.GetComponent<ArenaEnemy>();
            if (enemy == null) return;

            enemy.TakeDamage(damage, isCrit);
            Destroy(gameObject);
        }
    }
}
