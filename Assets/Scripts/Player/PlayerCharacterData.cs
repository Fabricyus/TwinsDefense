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

                int purchasedStars = CharacterStarUpgrades.Instance.GetStars(Current.slotId);
                stats.damage += purchasedStars;
            }
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
