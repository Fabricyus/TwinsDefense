using UnityEngine;

namespace TwinsDefense.Data
{
    /// <summary>
    /// Data-driven definition of a single enemy/monster variant.
    /// One asset = one enemy type (e.g. normal ghost, fast monster, mid-boss).
    /// Spawning/combat code should only ever read from this asset,
    /// never hardcode enemy numbers.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "TwinsDefense/Enemy Data", order = 0)]
    public class EnemyData : ScriptableObject
    {
        /// <summary>Display name of the enemy (e.g. "Wandering Ghost").</summary>
        public string enemyName;

        /// <summary>Role of this enemy within a wave (Normal, Fast, Tanky, MidBoss, FinalBoss).</summary>
        public EnemyType type;

        /// <summary>Total hit points before the enemy is defeated.</summary>
        public float maxHealth;

        /// <summary>Movement speed along the fixed path, in world units per second.</summary>
        public float moveSpeed;

        /// <summary>Damage dealt to the player's base if this enemy reaches the end of the path.</summary>
        public float damageToBase;

        /// <summary>Amount of Gems awarded to the player when this enemy is defeated.</summary>
        public int gemReward;

        /// <summary>Flavor text / lore description shown in UI or bestiary.</summary>
        [TextArea] public string description;

        /// <summary>Prefab instantiated by the WaveManager when spawning this enemy type.</summary>
        public GameObject enemyPrefab;

    }
}
