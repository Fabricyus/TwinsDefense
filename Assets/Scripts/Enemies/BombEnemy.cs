using UnityEngine;
using TwinsDefense.Player;
using TwinsDefense.VFX;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Suicide-bomber contact behavior: the first time this enemy touches the
    /// player, it deals a fixed hit to the player, damages itself, and spawns
    /// ArenaEnemy's shared contact-hit circle (same one Brute uses) at a larger
    /// scale as an explosion. Self-damage is routed through ArenaEnemy.TakeDamage
    /// so the normal death path (coin/exp drop, OnEnemyDefeated, Destroy) still runs.
    /// Detonates once — set contactDamage to 0 on this prefab's ArenaEnemy so its
    /// own repeating contact-damage tick doesn't also hit the player.
    /// </summary>
    [RequireComponent(typeof(ArenaEnemy))]
    public class BombEnemy : MonoBehaviour
    {
        [SerializeField] private float selfDamageOnContact = 100f;
        [SerializeField] private float playerDamageOnContact = 25f;
        [SerializeField] private float explosionScale = 2f;

        private ArenaEnemy arenaEnemy;
        private bool hasDetonated;

        private void Awake()
        {
            arenaEnemy = GetComponent<ArenaEnemy>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasDetonated || !other.TryGetComponent(out PlayerHurtbox hurtbox)) return;

            hasDetonated = true;

            hurtbox.Health.TakeDamage(playerDamageOnContact, transform.position);
            SpawnExplosion();
            arenaEnemy.TakeDamage(selfDamageOnContact);
        }

        private void SpawnExplosion()
        {
            GameObject prefab = arenaEnemy.AttackCirclePrefab;
            if (prefab != null)
            {
                GameObject fx = Instantiate(prefab, transform.position + arenaEnemy.AttackCircleOffset, Quaternion.identity);
                fx.transform.localScale *= explosionScale;
            }

            ExplosionVFX.Spawn(transform.position, explosionScale);
        }
    }
}
