using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Enemies;

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
        [Tooltip("Final placeholder waypoint enemies walk toward. Real Tilemap path comes in a later pass.")]
        [SerializeField] private Transform pathEnd;

        private int currentWaveIndex;


        /// <summary>Raised once the last wave in `waves` has finished.</summary>
        public event Action OnAllWavesCompleted;

        /// <summary>Raised each time a single wave's enemies have all been defeated or reached the goal.</summary>
        public event Action<WaveData> OnWaveCompleted;


        /// <summary>
        /// TODO: instantiate the enemies of the current wave respecting each
        /// WaveEntry's spawnInterval (via a Coroutine), advance to the next
        /// wave once all its enemies are spawned/defeated, and invoke
        /// OnAllWavesCompleted after the last wave finishes.
        /// </summary>
public void StartNextWave()
        {
            if (currentWaveIndex >= waves.Count) return;

            StartCoroutine(RunWaveSequenced(waves[currentWaveIndex]));
        }

/// <summary>Forces a specific wave to play immediately, out of the normal sequence (used by the tutorial).</summary>
        public void TriggerWave(WaveData wave)
        {
            StartCoroutine(RunWave(wave));
        }

        private IEnumerator RunWaveSequenced(WaveData wave)
        {
            yield return RunWave(wave);

            currentWaveIndex++;

            if (currentWaveIndex >= waves.Count)
            {
                OnAllWavesCompleted?.Invoke();
            }
        }

        private IEnumerator RunWave(WaveData wave)
        {
            yield return new WaitForSeconds(wave.delayBeforeStart);

            int spawnedCount = 0;
            int resolvedCount = 0;

            foreach (WaveEntry entry in wave.entries)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyData, () => resolvedCount++);
                    spawnedCount++;
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            yield return new WaitUntil(() => resolvedCount >= spawnedCount);

            OnWaveCompleted?.Invoke(wave);
        }

        private void SpawnEnemy(EnemyData enemyData, Action onResolved)
        {
            if (enemyData == null || enemyData.enemyPrefab == null)
            {
                Debug.LogWarning($"WaveManager: EnemyData '{enemyData?.enemyName}' has no enemyPrefab assigned.", this);
                onResolved?.Invoke();
                return;
            }

            GameObject instance = Instantiate(enemyData.enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
            Enemy enemy = instance.GetComponent<Enemy>();

            if (enemy == null)
            {
                onResolved?.Invoke();
                return;
            }

            enemy.waypoints = pathEnd != null ? new[] { pathEnd } : Array.Empty<Transform>();

            void HandleResolved()
            {
                enemy.OnEnemyDefeated -= HandleResolved;
                enemy.OnReachedGoal -= HandleResolved;
                onResolved?.Invoke();
            }

            enemy.OnEnemyDefeated += HandleResolved;
            enemy.OnReachedGoal += HandleResolved;
        }

    }
}
