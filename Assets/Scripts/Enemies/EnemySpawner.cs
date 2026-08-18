using System.Collections;
using System.Linq;
using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Progression;
using TwinsDefense.Systems;
using TwinsDefense.UI;
using TwinsDefense.VFX;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Continuously spawns enemies at random positions on a ring around the
    /// player, always outside camera view. Spawn interval shrinks and the
    /// Sprinter starts appearing as the player's level rises, and a specific
    /// boss spawns once the player picks their level-up card for its assigned
    /// level (see HandleCardPicked / bossSpawns). Spawning pauses (not stops —
    /// the loop keeps ticking, just skips SpawnEnemy) once ArenaEnemy.Active
    /// reaches maxActiveEnemies, so a run that outpaces the player's clear
    /// speed plateaus instead of piling up enemies without bound.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        private struct BossSpawnEntry
        {
            public int level;
            public GameObject bossPrefab;
        }

        [Header("Spawning")]
        [Tooltip("Seconds between spawns at level 0, before per-level decay.")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float minSpawnInterval = 0.2f;
        [Tooltip("Fraction spawnInterval shrinks by for each level gained (e.g. 0.05 = 5% faster per level).")]
        [Range(0f, 1f)]
        [SerializeField] private float spawnIntervalDecayPerLevel = 0.05f;
        [SerializeField] private float spawnRadiusMin = 12f;
        [SerializeField] private float spawnRadiusMax = 16f;
        [Tooltip("Spawning pauses once ArenaEnemy.Active.Count reaches this — prevents unbounded pile-up (and the frame-time/GC collapse that comes with it) on long runs or weak builds that can't clear fast enough, e.g. the AlwaysFirstOption challenges.")]
        [SerializeField] private int maxActiveEnemies = 40;
        [SerializeField] private GameObject enemyPrefab;

        [Header("Sprinter")]
        [SerializeField] private GameObject sprinterPrefab;
        [Tooltip("Player level at which the Sprinter starts appearing in the spawn pool.")]
        [SerializeField] private int sprinterUnlockLevel = 5;
        [Tooltip("Chance to spawn a Sprinter instead of the normal enemy, once unlocked.")]
        [Range(0f, 1f)]
        [SerializeField] private float sprinterSpawnChance = 0.3f;

        [Header("Bomb Pack")]
        [Tooltip("The fast bomb pack (bombPackFast) — gated separately by bombPackFastUnlockLevel below, not this section's slow-pack level.")]
        [SerializeField] private GameObject bombPackPrefab;
        [SerializeField] private GameObject bombPackSlowPrefab;
        [Tooltip("Player level at which the slow bomb pack starts appearing in the spawn pool.")]
        [SerializeField] private int bombPackUnlockLevel = 8;
        [Tooltip("Player level at which the fast bomb pack (bombPackPrefab) starts appearing in the spawn pool.")]
        [SerializeField] private int bombPackFastUnlockLevel = 21;
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
        [Tooltip("Each entry spawns exactly one instance of its bossPrefab the moment the player's level reaches that exact value.")]
        [SerializeField] private BossSpawnEntry[] bossSpawns;
        [Tooltip("The bossSpawns level whose kill unlocks level 20 in CampaignProgress (see HookBossDefeatReport) — today that's the Reaper at level 10.")]
        [SerializeField] private int reaperMilestoneLevel = 10;
        [Tooltip("The bossSpawns level whose kill counts toward CampaignProgress's level-30 unlock (see HookBossDefeatReport) — today that's the skull at level 20.")]
        [SerializeField] private int skullMilestoneLevel = 20;
        [Tooltip("Plays the boss-arrival portrait banner once per boss spawn, using that boss's own sprite.")]
        [SerializeField] private BossIntroBanner bossIntroBanner;
        [Tooltip("Boss spawn (and its intro banner) fires off this panel's OnCardPicked instead of LevelManager.OnLevelChanged, so the boss only appears once the player has actually picked their level-up card — not the instant the level is reached, while the cards panel is still up.")]
        [SerializeField] private LevelUpCardsUI levelUpCardsUI;
        [Tooltip("World-space diameter the arrival ripple grows to before fading out.")]
        [SerializeField] private float rippleMaxDiameter = 14f;
        [Tooltip("Ends the run with a win screen instead of the usual boss-kill reward once the highest-level boss in bossSpawns is defeated.")]
        [SerializeField] private GameOverController gameOverController;

        private Transform player;
        private bool isBossFightActive;
        private int finalBossLevel;

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }

            finalBossLevel = bossSpawns.Length > 0 ? bossSpawns.Max(entry => entry.level) : int.MaxValue;

            if (levelUpCardsUI != null)
            {
                levelUpCardsUI.OnCardPicked += HandleCardPicked;
            }

            StartCoroutine(SpawnLoop());
        }

        private void OnDestroy()
        {
            if (levelUpCardsUI != null)
            {
                levelUpCardsUI.OnCardPicked -= HandleCardPicked;
            }
        }

        /// <summary>Spawns the specific boss assigned to this level, if any (see bossSpawns) — clearing every other enemy off screen first so it's a clean boss-vs-player fight, and pausing regular spawns until the boss dies. Fires off LevelUpCardsUI.OnCardPicked (panel already closed, game already resumed), not the raw level change, so the boss/banner never appears while the level-up cards are still on screen.</summary>
        private void HandleCardPicked(int level)
        {
            foreach (BossSpawnEntry entry in bossSpawns)
            {
                if (entry.level != level || entry.bossPrefab == null) continue;

                GameObject boss = SpawnAtRing(entry.bossPrefab);
                if (boss == null) continue;

                RippleVFX.Spawn(boss.transform.position, rippleMaxDiameter);
                ClearNonBossEnemies(boss);
                isBossFightActive = true;

                if (bossIntroBanner != null && entry.bossPrefab.TryGetComponent(out SpriteRenderer bossSpriteRenderer))
                {
                    bossIntroBanner.Show(bossSpriteRenderer.sprite);
                }

                HookBossDefeatReport(boss, level);
            }
        }

        /// <summary>Instantly (and silently — no coin/exp drops) kills every active arena enemy except the boss that just spawned, so the fight starts as boss-vs-player only.</summary>
        private void ClearNonBossEnemies(GameObject boss)
        {
            ArenaEnemy[] snapshot = new ArenaEnemy[ArenaEnemy.Active.Count];
            ArenaEnemy.Active.CopyTo(snapshot);

            foreach (ArenaEnemy enemy in snapshot)
            {
                if (enemy == null || enemy.gameObject == boss) continue;
                enemy.HitKill();
            }
        }

        /// <summary>
        /// Reports the boss kill for the player's selected character/tier once this specific boss
        /// instance dies, grants the player a full level's worth of XP as the reward, and resumes
        /// normal spawning. ArenaEnemy has no boss-specific identity, so this hooks its generic
        /// death event right at the spawn site instead.
        ///
        /// Three ways this ends the run instead of resuming:
        /// - The Reaper (reaperMilestoneLevel, today 10): the first kill ends the run as a win and
        ///   unlocks level 20 in CampaignProgress. Only fires once, ever — after that, killing it
        ///   again (it doesn't respawn, but future bossSpawns entries could reuse the level) is a
        ///   no-op here.
        /// - The skull (skullMilestoneLevel, today 20): every kill ends the run as a win AND counts
        ///   toward CampaignProgress's level-30 unlock, until that unlock actually lands (3 kills) —
        ///   after which killing it no longer ends the run, so the player can push on to level 30.
        /// - The highest-level boss in bossSpawns (today 30, magBoss): always ends the run as a win
        ///   and marks the campaign complete — this is the real ending, no further gating.
        /// </summary>
        private void HookBossDefeatReport(GameObject boss, int bossLevel)
        {
            if (boss == null || !boss.TryGetComponent(out ArenaEnemy arenaEnemy)) return;

            arenaEnemy.OnEnemyDefeated += () =>
            {
                CharacterProgressTracker.Instance.ReportBossKilled(
                    SelectedRunContext.Instance.SelectedCharacter,
                    bossLevel,
                    SelectedRunContext.Instance.SelectedTier);

                LevelManager.Instance?.CompleteCurrentLevelExp();
                isBossFightActive = false;

                if (bossLevel == reaperMilestoneLevel && !CampaignProgress.Level20Unlocked)
                {
                    CampaignProgress.UnlockLevel20();
                    gameOverController?.TriggerMissionComplete();
                }
                else if (bossLevel == skullMilestoneLevel && !CampaignProgress.Level30Unlocked)
                {
                    CampaignProgress.ReportSkullKilled();
                    gameOverController?.TriggerMissionComplete();
                }
                else if (bossLevel >= finalBossLevel)
                {
                    CampaignProgress.ReportGameCompleted();
                    ReportChallengeIfCompleted();
                    gameOverController?.TriggerMissionComplete();
                }
            };
        }

/// <summary>Checked only on the final boss (Magpie) kill — see HookBossDefeatReport. Looks up the "Flawless Form" challenge for whichever character/tier is currently selected (see ChallengeDefinitions) and, if this run's RunChallengeTracker never broke that tier's specific rule, records it as completed.</summary>
        private void ReportChallengeIfCompleted()
        {
            CharacterId playedCharacter = SelectedRunContext.Instance.SelectedCharacter;
            int playedTier = SelectedRunContext.Instance.SelectedTier;

            if (!ChallengeDefinitions.TryFind(playedCharacter, playedTier, out ChallengeDefinition challenge)) return;
            if (RunChallengeTracker.Instance == null || !RunChallengeTracker.Instance.SatisfiesRule(challenge)) return;

            CharacterProgressTracker.Instance.ReportChallengeCompleted(playedCharacter, playedTier);
        }


        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(CurrentSpawnInterval());
                if (isBossFightActive || ArenaEnemy.Active.Count >= maxActiveEnemies) continue;
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

            if (level >= bombPackFastUnlockLevel && bombPackPrefab != null && Random.value < bombPackSpawnChance)
            {
                prefabToSpawn = bombPackPrefab;
            }
            else if (level >= bombPackUnlockLevel && bombPackSlowPrefab != null && Random.value < bombPackSpawnChance)
            {
                prefabToSpawn = bombPackSlowPrefab;
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
