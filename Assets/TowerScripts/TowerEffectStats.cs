using System;
using UnityEngine;

namespace TwinsDefense.Data
{
    /// <summary>
    /// Holds every possible "special effect" parameter a tower could use.
    /// Only the fields relevant to a tower's TowerEffectType are meant to be
    /// filled in the inspector; the rest stay at 0/default and are ignored
    /// by the combat code. Keeping this flat (instead of many small SOs)
    /// makes it trivial to tune numbers directly in the TowerData asset.
    /// </summary>
    [Serializable]
    public class TowerEffectStats
    {
        [Header("Piercing (Izzy Ranger)")]
        public int pierceCount;

        [Header("Area / Damage over Time (Izzy Fire Witch)")]
        public float aoeRadius;
        public float dotDamagePerSecond;
        public float dotDuration;

        [Header("Crowd Control (Izzy Pop Star / Court Ice Witch)")]
        public float stunDuration;
        [Range(0f, 1f)] public float slowPercent;
        public float slowDuration;
        public float shatterBonusDamage; // extra damage dealt to a slowed/frozen target

        [Header("Chain (Izzy Pop Star / Court Megabrain)")]
        public int chainTargets;
        [Range(0f, 1f)] public float chainDamagePercentOfMaxHP;

        [Header("Drain / Vulnerability (Court Evil)")]
        [Range(0f, 1f)] public float lifeDrainPercent;
        [Range(0f, 1f)] public float vulnerabilityAmpPercent;
        public float vulnerabilityDuration;

        [Header("Aura (Ralph variants)")]
        public float auraRadius;
        [Range(0f, 1f)] public float auraDamageBonusPercent;
        [Range(0f, 1f)] public float auraRangeBonusPercent;
        [Range(0f, 1f)] public float auraAttackSpeedBonusPercent;
        [Range(0f, 1f)] public float auraSlowOnEnemiesPercent; // Ralph the Cute
    }
}
