using UnityEngine;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Holder for the player's upgradeable combat/survival stats, read live by
    /// AutoAttack/PlayerController/PickupMagnet and written to by level-up
    /// cards via CardEffectApplier.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        public float damage = 5f;
        public float attackFireRate = 1f;
        public float projectileSpeed = 10f;
        public float critChance = 0.1f;
        public float critDamage = 2f;
        public float extraProjectileCount = 0f;
        public float pierceCount = 0f;
        public float attackRange = 5f;

        /// <summary>Scale multiplier applied to fired projectiles (1 = normal size). Starts at 1, not 0, so the Bigger Impact card's percentage bonus (stat *= 1 + value/100) has something to multiply.</summary>
        public float areaOfEffect = 1f;
        public float maxHP = 100f;

        /// <summary>Flat damage reduction on incoming hits (see PlayerHealth.TakeDamage). Starts at 1, not 0, so the Iron Skin card's percentage bonus (stat *= 1 + value/100) has something to multiply.</summary>
        public float defense = 1f;
        public float hpRegen = 0f;
        public float iFrameDuration = 0.5f;
        public float moveSpeed = 5f;
        public float pickupRadius = 3f;
        public float xpGainMultiplier = 1f;
        public float coinGainMultiplier = 1f;

        /// <summary>Flat percentage chance (0-100) added on top of the character's own ExplodeOnKill passive, if any — see AutoAttack.ResolveOnHitPassives.</summary>
        public float explodeOnKillChance = 0f;
    }
}
