using System.Collections.Generic;
using UnityEngine;

namespace TwinsDefense.Data
{
    /// <summary>
    /// Data-driven definition of a single complete wave within a phase:
    /// which enemies spawn, in what groups, after how much delay, and
    /// whether this wave represents the phase's mid-boss or final boss.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWaveData", menuName = "TwinsDefense/Wave Data", order = 0)]
    public class WaveData : ScriptableObject
    {
        /// <summary>Sequential number of this wave within the phase (1-based).</summary>
        public int waveNumber;

        /// <summary>Ordered list of spawn groups that make up this wave.</summary>
        public List<WaveEntry> entries;

        /// <summary>Seconds to wait after the previous wave ends before this wave starts.</summary>
        public float delayBeforeStart;

        /// <summary>Marks this wave as the phase's mid-boss encounter.</summary>
        public bool isMidBossWave;

        /// <summary>Marks this wave as the phase's final boss encounter.</summary>
        public bool isFinalBossWave;
    }
}
