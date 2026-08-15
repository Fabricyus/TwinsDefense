using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Systems;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Resolves the CharacterMetaData for whichever character/tier was chosen in
    /// Character Selection (see SelectedRunContext) and exposes it to the rest of
    /// the player's components. Swaps in that tier's Animator controller, if one
    /// is assigned, so evolved forms play their own animations.
    /// </summary>
    public class PlayerCharacterData : MonoBehaviour
    {
        [SerializeField] private CharacterMetaDataRegistry metaDataRegistry;
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerStats stats;

        public CharacterMetaData Current { get; private set; }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (stats == null)
            {
                stats = GetComponent<PlayerStats>();
            }

            if (metaDataRegistry == null)
            {
                Debug.LogWarning("PlayerCharacterData: no CharacterMetaDataRegistry assigned — player will not be linked to its CharacterMetaData.");
                return;
            }

            Current = metaDataRegistry.GetByCharacterAndTier(SelectedRunContext.Instance.SelectedCharacter, SelectedRunContext.Instance.SelectedTier);

            if (Current == null)
            {
                Debug.LogWarning($"PlayerCharacterData: no CharacterMetaData found for {SelectedRunContext.Instance.SelectedCharacter} tier {SelectedRunContext.Instance.SelectedTier}.");
                return;
            }

            if (Current.animatorController != null && animator != null)
            {
                animator.runtimeAnimatorController = Current.animatorController;
            }

            if (stats != null)
            {
                ApplyBaseStats(Current.baseStats);
                ApplyPurchasedStars(CharacterStarUpgrades.Instance.GetStars(Current.slotId));
            }
        }

        /// <summary>
        /// Each purchased Star gives +1 flat damage, +15% Attack Fire Rate, and +1 Star Projectile
        /// (so purchasedStars == starProjectileCount 1:1 — 1 star fires 1 Star Projectile per
        /// volley, 5 stars fires 5) — see StarProjectileLauncher, a separate low-damage boomerang
        /// shot fired on its own cooldown, not another AutoAttack projectile. On top of those flat
        /// stats, every star also: strengthens the character's own on-hit passive (+2% proc
        /// chance, +8% magnitude per star — never a card-granted proc), and at 3+ stars unlocks a
        /// cosmetic cast trail on fired projectiles. See AutoAttack for where both are applied.
        /// </summary>
        private void ApplyPurchasedStars(int purchasedStars)
        {
            stats.damage += purchasedStars;
            stats.attackFireRate *= 1f + purchasedStars * 0.15f;
            stats.starProjectileCount = purchasedStars;

            stats.passiveProcChanceBonus = purchasedStars * 2f;
            stats.passiveMagnitudeBonusPercent = purchasedStars * 8f;
            stats.hasStarCosmeticTrail = purchasedStars >= 3;
            stats.hasFiveStarAura = purchasedStars >= 5;
        }

        /// <summary>Overwrites PlayerStats' inspector defaults with this character tier's starting values.</summary>
        private void ApplyBaseStats(CharacterBaseStats baseStats)
        {
            stats.damage = baseStats.damage;
            stats.attackFireRate = baseStats.attackFireRate;
            stats.projectileSpeed = baseStats.projectileSpeed;
            stats.critChance = baseStats.critChance;
            stats.critDamage = baseStats.critDamage;
            stats.extraProjectileCount = baseStats.extraProjectileCount;
            stats.pierceCount = baseStats.pierceCount;
            stats.attackRange = baseStats.attackRange;
            stats.areaOfEffect = baseStats.areaOfEffect;
            stats.maxHP = baseStats.maxHP;
            stats.defense = baseStats.defense;
            stats.hpRegen = baseStats.hpRegen;
            stats.iFrameDuration = baseStats.iFrameDuration;
            stats.moveSpeed = baseStats.moveSpeed;
            stats.pickupRadius = baseStats.pickupRadius;
            stats.xpGainMultiplier = baseStats.xpGainMultiplier;
            stats.coinGainMultiplier = baseStats.coinGainMultiplier;
        }
    }
}
