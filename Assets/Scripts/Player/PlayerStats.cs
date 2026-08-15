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

        /// <summary>Set by PlayerCharacterData.ApplyPurchasedStars (3+/5+ stars) — number of Star Projectile instances StarProjectileLauncher fires every cooldown. Independent of extraProjectileCount: this is a separate, low-damage boomerang shot, not another AutoAttack projectile.</summary>
        public float starProjectileCount = 0f;
        public float pierceCount = 0f;
        public float attackRange = 5f;

        /// <summary>Scale multiplier applied to fired projectiles (1 = normal size). Starts at 1, not 0, so the Bigger Impact card's percentage bonus (stat *= 1 + value/100) has something to multiply.</summary>
        public float areaOfEffect = 1f;
        public float maxHP = 100f;

        /// <summary>Damage mitigation stat, consumed by PlayerHealth.TakeDamage via a diminishing-returns curve (100 / (100 + defense)). Starts at 0 — no mitigation until the player picks up a Defense card.</summary>
        public float defense = 0f;
        public float hpRegen = 0f;
        public float iFrameDuration = 0.5f;
        public float moveSpeed = 5f;
        public float pickupRadius = 3f;
        public float xpGainMultiplier = 1f;
        public float coinGainMultiplier = 1f;

        /// <summary>Flat percentage chance (0-100) added on top of the character's own ExplodeOnKill passive, if any — see AutoAttack.ResolveOnHitPassives.</summary>
        public float explodeOnKillChance = 0f;

        /// <summary>Flat percentage chance (0-100) to fully negate an incoming hit — rolled in PlayerHealth.TakeDamage before Defense mitigation or i-frames are touched.</summary>
        public float blockChance = 0f;

        /// <summary>Set by PlayerCharacterData.ApplyPurchasedStars — flat percentage points added to the character's own on-hit passive's proc chance (Stun/Slow/Thunder/Chain/ExplodeOnKill). Never affects card-granted procs (e.g. explodeOnKillChance above), only the character's native passive.</summary>
        public float passiveProcChanceBonus = 0f;

        /// <summary>Set by PlayerCharacterData.ApplyPurchasedStars — percentage increase applied to the character's own on-hit passive's magnitude (slow%, stun/slow duration, Thunder/ExplodeOnKill damage multiplier).</summary>
        public float passiveMagnitudeBonusPercent = 0f;

        /// <summary>Set by PlayerCharacterData.ApplyPurchasedStars once enough Star Upgrades are purchased for this character/tier — a cosmetic-only cast trail on fired projectiles.</summary>
        public bool hasStarCosmeticTrail = false;
        public Color starCosmeticTrailColor = new Color(1f, 0.92f, 0.55f, 0.9f);

        /// <summary>Set by PlayerCharacterData.ApplyPurchasedStars once all 5 stars are purchased for this character/tier — cosmetic-only sunburst aura at the player's feet (see PlayerStarAuraVFX), reusing CardRarityVFX's Epic card aura look.</summary>
        public bool hasFiveStarAura = false;
    }
}
