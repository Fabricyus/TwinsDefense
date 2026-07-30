using UnityEngine;

namespace TwinsDefense.Data
{
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

        /// <summary>
        /// Quick guard used by combat/UI code to know if this tower
        /// actually deals direct damage (as opposed to Ralph's aura-only towers).
        /// </summary>
        public bool IsDamageDealer => effectType != TowerEffectType.AuraSupport;
    }
}
