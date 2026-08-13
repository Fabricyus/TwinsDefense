using UnityEngine;

namespace TwinsDefense.Data
{
    public enum CharacterPassiveEffectType
    {
        GoldPerLevelMultiplier,
        XPPerLevelMultiplier,
        HPPerLevel,
        RunStartBonusStat,   // pairs with RunStartStatType below
        StunOnHit,
        SlowOnHit,
        ThunderStrikeOnHit,
        ChainOnHit
    }

    public enum RunStartStatType
    {
        None,
        AreaOfEffect,
        Pierce,
        Projectiles,
        Defense
    }

    /// <summary>
    /// Describes a single passive ability granted by a character tier. Separate
    /// from CardData's effect system (in-run stat cards) because these are
    /// meta-progression passives with proc chances, flat run-start bonuses, and
    /// on-hit special effects that don't fit CardData's value/isPercentage shape.
    /// </summary>
    [System.Serializable]
    public class CharacterPassiveEffect
    {
        public CharacterPassiveEffectType effectType;

        [Header("Generic value (multiplier, flat HP/level, etc.)")]
        public float value; // e.g. 2 for "x2 gold", 10 for "10 HP per level" — placeholder-friendly, designer-editable

        [Header("Proc-based effects (Stun/Slow/Thunder/Chain)")]
        [Range(0f, 100f)] public float procChancePercent;
        public float damageMultiplier; // e.g. 3.0 for "300% damage", only relevant for ThunderStrikeOnHit

        [Header("Run-start flat bonus")]
        public RunStartStatType runStartStat;
        public float runStartStatValue;
    }
}
