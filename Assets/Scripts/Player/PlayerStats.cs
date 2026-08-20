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
        public float pickupRadius = 3.6f;
        public float xpGainMultiplier = 1f;
        public float coinGainMultiplier = 1f;

        /// <summary>Flat percentage chance (0-100) added on top of the character's own ExplodeOnKill passive, if any — see AutoAttack.ResolveOnHitPassives.</summary>
        public float explodeOnKillChance = 0f;

        /// <summary>Flat percentage chance (0-100) to fully negate an incoming hit — rolled in PlayerHealth.TakeDamage before Defense mitigation or i-frames are touched.</summary>
        public float blockChance = 0f;

        /// <summary>Set by Star Round (see CardEffectType.StarDamageBonus) — percentage bonus applied only to StarProjectileLauncher's own damage calc, never to the main damage stat above.</summary>
        public float starDamageBonusPercent = 0f;

        /// <summary>Set by Star Round (see CardEffectType.StarRangeBonus) — percentage bonus applied only to StarProjectileLauncher's own target-search range, never to the main attackRange stat above.</summary>
        public float starRangeBonusPercent = 0f;

        /// <summary>Set by Star Round (see CardEffectType.StarCooldownReduction) — flat seconds subtracted from StarProjectileLauncher's own cooldown, never affecting the main attackFireRate stat above.</summary>
        public float starCooldownReductionSeconds = 0f;

        /// <summary>Set by PlayerCharacterData.ApplyPurchasedStars and by the Cute Stats Exclusive card (see CardEffectType.PassiveProcChanceBonus) — flat percentage points added to the character's own on-hit passive's proc chance (Stun/Slow/Thunder/Chain/ExplodeOnKill). Never affects card-granted procs (e.g. explodeOnKillChance above), only the character's native passive. Static Strike/Holy Strike/Dark Fork no longer route through this — they're their own independent-of-character procs, see holyStrikeChance/staticStrikeChance/hasProjectileSplitOnHit below.</summary>
        public float passiveProcChanceBonus = 0f;

        /// <summary>Set by PlayerCharacterData.ApplyPurchasedStars — percentage increase applied to the character's own on-hit passive's magnitude (slow%, stun/slow duration, Thunder/ExplodeOnKill damage multiplier).</summary>
        public float passiveMagnitudeBonusPercent = 0f;

        /// <summary>Set by PlayerCharacterData.ApplyPurchasedStars once enough Star Upgrades are purchased for this character/tier — a cosmetic-only cast trail on fired projectiles.</summary>
        public bool hasStarCosmeticTrail = false;
        public Color starCosmeticTrailColor = new Color(1f, 0.92f, 0.55f, 0.9f);

        /// <summary>Set by PlayerCharacterData.ApplyPurchasedStars once all 5 stars are purchased for this character/tier — cosmetic-only sunburst aura at the player's feet (see PlayerStarAuraVFX), reusing CardRarityVFX's Epic card aura look.</summary>
        public bool hasFiveStarAura = false;

        /// <summary>Set by the Holy Strike / Static Strike Exclusive cards (CardEffectType.HolyStrikeChance/StaticStrikeChance) — flat percentage chance (0-100) for any hit to proc the fixed holyFx/thunderFx strike, independent of the equipped character's own passives. See AutoAttack.ResolveOnHitPassives.</summary>
        public float holyStrikeChance = 0f;
        public float staticStrikeChance = 0f;

        /// <summary>Set by the Dark Fork Exclusive card (CardEffectType.ProjectileSplitOnHit) — on hitting an enemy, the projectile forks into two children angled +/-45 degrees off its heading. See Projectile.TrySplitOnHit.</summary>
        public bool hasProjectileSplitOnHit = false;

        /// <summary>Set by the Rainbow Nova Exclusive card (CardEffectType.RainbowNova, unlocked by defeating the secret Mega Magpie) — periodic AoE pulse that damages every active enemy in the arena. See RainbowNovaController.</summary>
        public bool hasRainbowNova = false;
    }
}
