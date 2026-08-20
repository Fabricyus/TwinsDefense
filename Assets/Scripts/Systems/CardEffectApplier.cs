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

            // Not gated by isSpecial — see CardSlotUI.BuildDescription. secondValue == 0 means no
            // second effect was set, so this is a safe no-op for every card that doesn't use one.
            if (card.secondValue != 0f)
            {
                ApplyEffect(card.secondEffectType, card.secondValue, card.secondIsPercentage, target);
            }

            if (card.additionalEffects == null) return;

            foreach (CardEffect effect in card.additionalEffects)
            {
                ApplyEffect(effect.effectType, effect.value, effect.isPercentage, target);
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
                    // Caps CurrentHP down when a card shrinks the max (e.g. Glass Cannon) — otherwise
                    // it would sit stuck above the new cap until the next hit or heal touches it.
                    target.GetComponent<PlayerHealth>()?.ClampToMaxHP();
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
                    // isPercentage here means "% of current Max HP" (e.g. Second Wind's 100% = full
                    // heal), not the usual "% of the stat being modified" since InstantHeal isn't a stat.
                    float healAmount = isPercentage ? target.maxHP * (value / 100f) : value;
                    target.GetComponent<PlayerHealth>()?.Heal(healAmount);
                    break;
                case CardEffectType.ExplodeOnKillChance:
                    target.explodeOnKillChance = Apply(target.explodeOnKillChance, value, isPercentage);
                    break;
                case CardEffectType.BlockChance:
                    target.blockChance = Apply(target.blockChance, value, isPercentage);
                    break;
                case CardEffectType.StarProjectileCount:
                    target.starProjectileCount = Apply(target.starProjectileCount, value, isPercentage);
                    break;
                case CardEffectType.PassiveProcChanceBonus:
                    target.passiveProcChanceBonus = Apply(target.passiveProcChanceBonus, value, isPercentage);
                    break;
                case CardEffectType.StarDamageBonus:
                    target.starDamageBonusPercent = Apply(target.starDamageBonusPercent, value, isPercentage);
                    break;
                case CardEffectType.StarRangeBonus:
                    target.starRangeBonusPercent = Apply(target.starRangeBonusPercent, value, isPercentage);
                    break;
                case CardEffectType.StarCooldownReduction:
                    target.starCooldownReductionSeconds = Apply(target.starCooldownReductionSeconds, value, isPercentage);
                    break;
                case CardEffectType.HolyStrikeChance:
                    target.holyStrikeChance = Apply(target.holyStrikeChance, value, isPercentage);
                    break;
                case CardEffectType.StaticStrikeChance:
                    target.staticStrikeChance = Apply(target.staticStrikeChance, value, isPercentage);
                    break;
                case CardEffectType.ProjectileSplitOnHit:
                    target.hasProjectileSplitOnHit = true;
                    break;
                case CardEffectType.RainbowNova:
                    target.hasRainbowNova = true;
                    break;
            }
        }

        private static float Apply(float stat, float value, bool isPercentage)
        {
            return isPercentage ? stat * (1f + value / 100f) : stat + value;
        }
    }
}
