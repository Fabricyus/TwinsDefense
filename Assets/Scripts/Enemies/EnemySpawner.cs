using System.Collections;
using UnityEngine;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Continuously spawns enemies at random positions on a ring around the
    /// player, always outside camera view. No difficulty scaling yet — spawn
    /// rate is fixed (that arrives with the XP/level system).
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawning")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float spawnRadiusMin = 12f;
        [SerializeField] private float spawnRadiusMax = 16f;
        [SerializeField] private GameObject enemyPrefab;

        private Transform player;

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }

            StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefab == null || player == null) return;

            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
            Vector2 spawnPosition = (Vector2)player.position + randomDirection * distance;

            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
