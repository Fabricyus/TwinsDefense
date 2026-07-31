using System;
using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.Waves
{
    /// <summary>
    /// Drives the sequence of waves for a phase. This manager does not yet
    /// spawn real enemy prefabs — that depends on enemy prefabs being created
    /// in Unity, which is the next step after this scaffolding.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Waves")]
        [Tooltip("Waves for this phase, in the order they should be played.")]
        [SerializeField] private List<WaveData> waves;

        [Header("Spawning")]
        [SerializeField] private Transform enemySpawnPoint;

        /// <summary>Raised once the last wave in `waves` has finished.</summary>
        public event Action OnAllWavesCompleted;

        /// <summary>
        /// TODO: instantiate the enemies of the current wave respecting each
        /// WaveEntry's spawnInterval (via a Coroutine), advance to the next
        /// wave once all its enemies are spawned/defeated, and invoke
        /// OnAllWavesCompleted after the last wave finishes.
        /// </summary>
        public void StartNextWave()
        {
        }
    }
}
