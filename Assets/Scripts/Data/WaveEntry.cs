using System;

namespace TwinsDefense.Data
{
    /// <summary>
    /// Represents a single spawn group within a wave: one enemy type,
    /// how many of them, and the delay between each individual spawn.
    /// </summary>
    [Serializable]
    public class WaveEntry
    {
        /// <summary>The enemy type to spawn for this entry.</summary>
        public EnemyData enemyData;

        /// <summary>How many enemies of this type to spawn.</summary>
        public int count;

        /// <summary>Seconds to wait between each individual spawn of this entry.</summary>
        public float spawnInterval;
    }
}
