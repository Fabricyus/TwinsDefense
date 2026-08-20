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
        ChainOnHit,
        DefensePerLevel,     // appended at the end — existing assets serialize this enum as an int, inserting earlier would remap them
        ExplodeOnKill        // chance to AoE-damage nearby enemies when a kill lands
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

        [Header("Generic value (percent/level, flat HP or Defense/level, etc.)")]
        public float value; // e.g. 2 for "+2% gold per level", 10 for "+10 HP per level" — placeholder-friendly, designer-editable

        [Header("Proc-based effects (Stun/Slow/Thunder/Chain/ExplodeOnKill)")]
        [Range(0f, 100f)] public float procChancePercent;
        [Tooltip("ThunderStrikeOnHit: multiplier on player damage (e.g. 3.0 = 300%). ExplodeOnKill: fraction of player damage dealt as AoE (e.g. 0.5 = half).")]
        public float damageMultiplier;
        [Tooltip("SlowOnHit only: how much to slow the target's move speed by, 0-100.")]
        public float procMagnitudePercent;
        [Tooltip("StunOnHit/SlowOnHit only: how long the effect lasts, in seconds.")]
        public float procDurationSeconds;
        [Tooltip("ExplodeOnKill only: tint of the explosion particle burst (e.g. blue for an icy character).")]
        public Color explosionColor = new Color(1f, 0.55f, 0.1f, 1f);
        [Tooltip("ThunderStrikeOnHit only: tint of this proc's damage popup text (e.g. light blue for a thunder bolt, light yellow for a holy bolt, pink for a heart).")]
        public Color strikeColor = Color.white;

        [Header("Run-start flat bonus")]
        public RunStartStatType runStartStat;
        public float runStartStatValue;
    }
}
