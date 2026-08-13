using UnityEngine;
using TwinsDefense.Player;

namespace TwinsDefense.Combat
{
    /// <summary>
    /// Straight-line projectile fired by ranged enemies (e.g. Sprinter) toward
    /// the player's position at the moment of the shot — no homing. Damages
    /// the player's PlayerHurtbox on contact and self-destroys on hit or
    /// after its lifetime expires. Mirrors the player's Projectile but targets
    /// PlayerHurtbox instead of ArenaEnemy.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;

        private Vector2 direction;
        private float speed;
        private float damage;
        private float lifeTimer;

        private void Awake()
        {
            // Kinematic so it never reacts to physics/gravity, but still raises
            // trigger events against the player's hurtbox collider.
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        /// <summary>Assigns this projectile's travel direction, damage and speed right after Instantiate.</summary>
        public void Launch(Vector2 direction, float damage, float speed)
        {
            this.direction = direction.normalized;
            this.damage = damage;
            this.speed = speed;

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
            if (!other.TryGetComponent(out PlayerHurtbox hurtbox)) return;

            hurtbox.Health.TakeDamage(damage, transform.position);
            Destroy(gameObject);
        }
    }
}
