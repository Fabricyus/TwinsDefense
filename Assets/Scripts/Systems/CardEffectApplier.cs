using TwinsDefense.Data;
using TwinsDefense.Player;

namespace TwinsDefense.Systems
{
    /// <summary>Applies a drafted CardData's effect onto the player's stats.</summary>
    public class CardEffectApplier
    {
        public void ApplyCard(CardData card, PlayerStats target)
        {
            switch (card.effectType)
            {
                case CardEffectType.Damage:
                    target.damage = Apply(target.damage, card);
                    break;
                case CardEffectType.AttackFireRate:
                    target.attackFireRate = Apply(target.attackFireRate, card);
                    break;
                case CardEffectType.ProjectileSpeed:
                    target.projectileSpeed = Apply(target.projectileSpeed, card);
                    break;
                case CardEffectType.CritChance:
                    target.critChance = Apply(target.critChance, card);
                    break;
                case CardEffectType.CritDamage:
                    target.critDamage = Apply(target.critDamage, card);
                    break;
                case CardEffectType.ExtraProjectile:
                    target.extraProjectileCount = Apply(target.extraProjectileCount, card);
                    break;
                case CardEffectType.Pierce:
                    target.pierceCount = Apply(target.pierceCount, card);
                    break;
                case CardEffectType.AttackRange:
                    target.attackRange = Apply(target.attackRange, card);
                    break;
                case CardEffectType.AreaOfEffect:
                    target.areaOfEffect = Apply(target.areaOfEffect, card);
                    break;
                case CardEffectType.MaxHP:
                    target.maxHP = Apply(target.maxHP, card);
                    break;
                case CardEffectType.Defense:
                    target.defense = Apply(target.defense, card);
                    break;
                case CardEffectType.HPRegen:
                    target.hpRegen = Apply(target.hpRegen, card);
                    break;
                case CardEffectType.IFrameDuration:
                    target.iFrameDuration = Apply(target.iFrameDuration, card);
                    break;
                case CardEffectType.MoveSpeed:
                    target.moveSpeed = Apply(target.moveSpeed, card);
                    break;
                case CardEffectType.PickupRadius:
                    target.pickupRadius = Apply(target.pickupRadius, card);
                    break;
                case CardEffectType.XPGain:
                    target.xpGainMultiplier = Apply(target.xpGainMultiplier, card);
                    break;
                case CardEffectType.CoinGain:
                    target.coinGainMultiplier = Apply(target.coinGainMultiplier, card);
                    break;
            }
        }

        private static float Apply(float stat, CardData card)
        {
            return card.isPercentage ? stat * (1f + card.value / 100f) : stat + card.value;
        }
    }
}
