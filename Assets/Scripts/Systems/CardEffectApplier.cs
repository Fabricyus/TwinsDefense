using TwinsDefense.Data;
using TwinsDefense.Player;

namespace TwinsDefense.Systems
{
    /// <summary>Applies a drafted CardData's effect(s) onto the player's stats — two effects for special (buff+debuff) cards.</summary>
    public class CardEffectApplier
    {
        public void ApplyCard(CardData card, PlayerStats target)
        {
            ApplyEffect(card.effectType, card.value, card.isPercentage, target);

            if (card.isSpecial)
            {
                ApplyEffect(card.secondEffectType, card.secondValue, card.secondIsPercentage, target);
            }
        }

        private static void ApplyEffect(CardEffectType effectType, float value, bool isPercentage, PlayerStats target)
        {
            switch (effectType)
            {
                case CardEffectType.Damage:
                    target.damage = Apply(target.damage, value, isPercentage);
                    break;
                case CardEffectType.AttackFireRate:
                    target.attackFireRate = Apply(target.attackFireRate, value, isPercentage);
                    break;
                case CardEffectType.ProjectileSpeed:
                    target.projectileSpeed = Apply(target.projectileSpeed, value, isPercentage);
                    break;
                case CardEffectType.CritChance:
                    target.critChance = Apply(target.critChance, value, isPercentage);
                    break;
                case CardEffectType.CritDamage:
                    target.critDamage = Apply(target.critDamage, value, isPercentage);
                    break;
                case CardEffectType.ExtraProjectile:
                    target.extraProjectileCount = Apply(target.extraProjectileCount, value, isPercentage);
                    break;
                case CardEffectType.Pierce:
                    target.pierceCount = Apply(target.pierceCount, value, isPercentage);
                    break;
                case CardEffectType.AttackRange:
                    target.attackRange = Apply(target.attackRange, value, isPercentage);
                    break;
                case CardEffectType.AreaOfEffect:
                    target.areaOfEffect = Apply(target.areaOfEffect, value, isPercentage);
                    break;
                case CardEffectType.MaxHP:
                    target.maxHP = Apply(target.maxHP, value, isPercentage);
                    break;
                case CardEffectType.Defense:
                    target.defense = Apply(target.defense, value, isPercentage);
                    break;
                case CardEffectType.HPRegen:
                    target.hpRegen = Apply(target.hpRegen, value, isPercentage);
                    break;
                case CardEffectType.IFrameDuration:
                    target.iFrameDuration = Apply(target.iFrameDuration, value, isPercentage);
                    break;
                case CardEffectType.MoveSpeed:
                    target.moveSpeed = Apply(target.moveSpeed, value, isPercentage);
                    break;
                case CardEffectType.PickupRadius:
                    target.pickupRadius = Apply(target.pickupRadius, value, isPercentage);
                    break;
                case CardEffectType.XPGain:
                    target.xpGainMultiplier = Apply(target.xpGainMultiplier, value, isPercentage);
                    break;
                case CardEffectType.CoinGain:
                    target.coinGainMultiplier = Apply(target.coinGainMultiplier, value, isPercentage);
                    break;
                case CardEffectType.InstantHeal:
                    // One-shot heal of current HP, not a stat mutation — routed through the sibling
                    // PlayerHealth component (which clamps to maxHP) instead of PlayerStats.
                    target.GetComponent<PlayerHealth>()?.Heal(value);
                    break;
                case CardEffectType.ExplodeOnKillChance:
                    target.explodeOnKillChance = Apply(target.explodeOnKillChance, value, isPercentage);
                    break;
            }
        }

        private static float Apply(float stat, float value, bool isPercentage)
        {
            return isPercentage ? stat * (1f + value / 100f) : stat + value;
        }
    }
}
