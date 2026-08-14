using System.Collections;
using UnityEngine;
using TwinsDefense.Progression;
using TwinsDefense.Systems;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Continuously spawns enemies at random positions on a ring around the
    /// player, always outside camera view. Spawn interval shrinks and the
    /// Sprinter starts appearing as the player's level rises, and a scaling
    /// wave of bosses is spawned every few levels (see HandleLevelChanged).
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

        [Header("Bomb Pack")]
        [SerializeField] private GameObject bombPackPrefab;
        [SerializeField] private GameObject bombPackSlowPrefab;
        [Tooltip("Player level at which bomb packs start appearing in the spawn pool.")]
        [SerializeField] private int bombPackUnlockLevel = 8;
        [Tooltip("Chance to spawn a bomb pack instead of the normal enemy, once unlocked (rolled independently for each of the two pack prefabs).")]
        [Range(0f, 1f)]
        [SerializeField] private float bombPackSpawnChance = 0.05f;

        [Header("Diamond")]
        [SerializeField] private GameObject diamondPrefab;
        [Tooltip("Player level at which Diamond starts appearing in the spawn pool.")]
        [SerializeField] private int diamondUnlockLevel = 15;
        [Tooltip("Chance to spawn a Diamond instead of the normal enemy, once unlocked.")]
        [Range(0f, 1f)]
        [SerializeField] private float diamondSpawnChance = 0.1f;

        [Header("Boss")]
        [SerializeField] private GameObject bossPrefab;
        [Tooltip("A wave of bosses spawns every time the player's level is a multiple of this value, with the wave size scaling as level / this value (e.g. level 10 -> 1 boss, level 20 -> 2, level 30 -> 3).")]
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

        /// <summary>Drops a scaling wave of bosses into the arena whenever the player reaches a level that's a multiple of bossLevelInterval — e.g. with the default interval of 10, level 10 spawns 1, level 20 spawns 2, level 30 spawns 3.</summary>
        private void HandleLevelChanged(int level)
        {
            if (level <= 0 || bossPrefab == null || level % bossLevelInterval != 0) return;

            int bossCount = level / bossLevelInterval;
            for (int i = 0; i < bossCount; i++)
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

            if (level >= bombPackUnlockLevel)
            {
                if (bombPackPrefab != null && Random.value < bombPackSpawnChance)
                {
                    prefabToSpawn = bombPackPrefab;
                }
                else if (bombPackSlowPrefab != null && Random.value < bombPackSpawnChance)
                {
                    prefabToSpawn = bombPackSlowPrefab;
                }
            }

            if (level >= diamondUnlockLevel && diamondPrefab != null && Random.value < diamondSpawnChance)
            {
                prefabToSpawn = diamondPrefab;
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
