using UnityEngine;

namespace TwinsDefense.Data
{
    [System.Serializable]
    public struct StarLevel
    {
        [Tooltip("1 to 5.")]
        public int starIndex;
        [Tooltip("Gem cost to reach THIS star from the previous one.")]
        public int gemCost;
        public float damageMultiplier;
        public float rangeMultiplier;
        public float fireRateMultiplier;
    }

    /// <summary>
    /// Data-driven definition of a single tower variant.
    /// One asset = one of the 12 towers (e.g. "Izzy_FireWitch.asset").
    /// Combat/placement code should only ever read from this asset,
    /// never hardcode tower numbers.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTowerData", menuName = "TwinsDefense/Tower Data", order = 0)]
    public class TowerData : ScriptableObject
    {
        [Header("Identity")]
        public string towerDisplayName;
        public TowerCharacter character;
        public TowerVariant variant;
        [TextArea] public string description;

        [Header("Economy")]
        [Tooltip("Cost in Gems to summon this tower onto the field.")]
        public int gemCost;
        [Tooltip("Cost in Gems for the next upgrade level, if applicable.")]
        public int upgradeCost;

        [Header("Core Combat Stats")]
        [Tooltip("Base damage per hit. 0 for pure-support Ralph variants.")]
        public float damage;
        [Tooltip("Attacks per second. 0 for pure-support Ralph variants.")]
        public float fireRate;
        [Tooltip("Attack range in grid/world units, or aura radius for Ralph.")]
        public float range;

        [Header("Visuals & Prefabs")]
        public GameObject towerPrefab;
        public GameObject projectilePrefab;
        public Sprite icon;

        [Header("Special Effect")]
        public TowerEffectType effectType;
        public TowerEffectStats effectStats;

        [Header("Star Upgrade Table")]
        [Tooltip("Index 0 = stats/cost for reaching ★2, index 3 = stats/cost for reaching ★5, etc. Star 1 is the base stats above (no entry needed).")]
        public StarLevel[] starLevels = new StarLevel[]
        {
            new StarLevel { starIndex = 2, gemCost = 75,  damageMultiplier = 1.20f, rangeMultiplier = 1.10f, fireRateMultiplier = 1.0f },
            new StarLevel { starIndex = 3, gemCost = 150, damageMultiplier = 1.45f, rangeMultiplier = 1.10f, fireRateMultiplier = 1.15f },
            new StarLevel { starIndex = 4, gemCost = 300, damageMultiplier = 1.85f, rangeMultiplier = 1.25f, fireRateMultiplier = 1.25f },
            new StarLevel { starIndex = 5, gemCost = 700, damageMultiplier = 3.30f, rangeMultiplier = 1.55f, fireRateMultiplier = 1.55f },
        };

        [Header("Talent Tree Subclasses")]
        [Tooltip("Display names of this character's 3 unlockable evolutions, shown locked in the Upgrade Panel.")]
        public string[] subclassNames = new string[3];


        /// <summary>
        /// Quick guard used by combat/UI code to know if this tower
        /// actually deals direct damage (as opposed to Ralph's aura-only towers).
        /// </summary>
        public bool IsDamageDealer => effectType != TowerEffectType.AuraSupport;
    }
}
