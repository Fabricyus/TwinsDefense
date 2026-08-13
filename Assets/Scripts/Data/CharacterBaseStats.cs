using UnityEngine;

namespace TwinsDefense.Data
{
    /// <summary>
    /// A character tier's starting PlayerStats values, applied once at the
    /// start of a run (see PlayerCharacterData). Defaults mirror PlayerStats'
    /// own defaults, so an asset left untouched behaves exactly like today.
    /// </summary>
    [System.Serializable]
    public class CharacterBaseStats
    {
        [Header("Combat")]
        public float damage = 5f;
        public float attackFireRate = 1f;
        public float projectileSpeed = 10f;
        public float critChance = 0.1f;
        public float critDamage = 2f;
        public float extraProjectileCount = 0f;
        public float pierceCount = 0f;
        public float attackRange = 5f;
        public float areaOfEffect = 1f;

        [Header("Survival")]
        public float maxHP = 100f;
        public float defense = 1f;
        public float hpRegen = 0f;
        public float iFrameDuration = 0.5f;
        public float moveSpeed = 5f;

        [Header("Economy")]
        public float pickupRadius = 3f;
        public float xpGainMultiplier = 1f;
        public float coinGainMultiplier = 1f;
    }
}
