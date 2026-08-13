using System.Collections;
using UnityEngine;
using TwinsDefense.Progression;
using TwinsDefense.Systems;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Continuously spawns enemies at random positions on a ring around the
    /// player, always outside camera view. Spawn interval shrinks and the
    /// Sprinter starts appearing as the player's level rises, and a boss
    /// enemy is spawned every few levels.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawning")]
        [Tooltip("Seconds between spawns at level 0, before per-level decay.")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float minSpawnInterval = 0.2f;
        [Tooltip("Fraction spawnInterval shrinks by for each level gained (e.g. 0.05 = 5% faster per level).")]
        [Range(0f, 1f)]
        [SerializeField] private float spawnIntervalDecayPerLevel = 0.05f;
        [SerializeField] private float spawnRadiusMin = 12f;
        [SerializeField] private float spawnRadiusMax = 16f;
        [SerializeField] private GameObject enemyPrefab;

        [Header("Sprinter")]
        [SerializeField] private GameObject sprinterPrefab;
        [Tooltip("Player level at which the Sprinter starts appearing in the spawn pool.")]
        [SerializeField] private int sprinterUnlockLevel = 5;
        [Tooltip("Chance to spawn a Sprinter instead of the normal enemy, once unlocked.")]
        [Range(0f, 1f)]
        [SerializeField] private float sprinterSpawnChance = 0.3f;

        [Header("Boss")]
        [SerializeField] private GameObject bossPrefab;
        [Tooltip("A boss is spawned every time the player's level is a multiple of this value.")]
        [SerializeField] private int bossLevelInterval = 10;

        private Transform player;

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelChanged += HandleLevelChanged;
            }

            StartCoroutine(SpawnLoop());
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelChanged -= HandleLevelChanged;
            }
        }

        /// <summary>Drops a boss into the arena whenever the player reaches a level that's a multiple of bossLevelInterval.</summary>
        private void HandleLevelChanged(int level)
        {
            if (level > 0 && bossPrefab != null && level % bossLevelInterval == 0)
            {
                GameObject boss = SpawnAtRing(bossPrefab);
                HookBossDefeatReport(boss, level);
            }
        }

        /// <summary>Reports the boss kill for the player's selected character/tier once this specific boss instance dies. ArenaEnemy has no boss-specific identity, so this hooks its generic death event right at the spawn site instead.</summary>
        private void HookBossDefeatReport(GameObject boss, int bossLevel)
        {
            if (boss == null || !boss.TryGetComponent(out ArenaEnemy arenaEnemy)) return;

            arenaEnemy.OnEnemyDefeated += () => CharacterProgressTracker.Instance.ReportBossKilled(
                SelectedRunContext.Instance.SelectedCharacter,
                bossLevel,
                SelectedRunContext.Instance.SelectedTier);
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(CurrentSpawnInterval());
                SpawnEnemy();
            }
        }

        /// <summary>spawnInterval decayed by spawnIntervalDecayPerLevel for each level gained, floored at minSpawnInterval.</summary>
        private float CurrentSpawnInterval()
        {
            int level = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : 0;
            float interval = spawnInterval * Mathf.Pow(1f - spawnIntervalDecayPerLevel, level);
            return Mathf.Max(minSpawnInterval, interval);
        }

        private void SpawnEnemy()
        {
            int level = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : 0;

            GameObject prefabToSpawn = enemyPrefab;
            if (level >= sprinterUnlockLevel && sprinterPrefab != null && Random.value < sprinterSpawnChance)
            {
                prefabToSpawn = sprinterPrefab;
            }

            SpawnAtRing(prefabToSpawn);
        }

        private GameObject SpawnAtRing(GameObject prefab)
        {
            if (prefab == null || player == null) return null;

            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
            Vector2 spawnPosition = (Vector2)player.position + randomDirection * distance;

            return Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
    }
}
