using System.Collections;
using UnityEngine;
using TwinsDefense.Combat;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Optional ranged attack for arena enemies (e.g. Sprinter): once in range,
    /// stops moving for a brief windup (reusing ArenaEnemy's stun so movement
    /// pauses without duplicating that logic), plays a punch-scale telegraph,
    /// then fires a straight-line EnemyProjectile at the player with some aim
    /// error. Movement and contact damage stay on ArenaEnemy — this only adds
    /// the shooting. Fire() is virtual so variants (e.g. DiamondRangedAttacker's
    /// multi-shot cross pattern) can reuse the windup/telegraph machinery as-is.
    /// </summary>
    public class EnemyRangedAttacker : MonoBehaviour
    {
        [Header("Firing")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireInterval = 2f;
        [SerializeField] private float projectileSpeed = 6f;
        [SerializeField] private float damage = 5f;
        [SerializeField] private float attackRange = 10f;
        [Tooltip("Origin the projectile spawns from. Defaults to this transform if left unassigned.")]
        [SerializeField] private Transform firePoint;

        [Header("Windup")]
        [Tooltip("How long the enemy stops moving and plays the punch-scale telegraph before actually firing.")]
        [SerializeField] private float windupDuration = 0.6f;
        [SerializeField] private Vector3 punchScaleAmount = new Vector3(0.3f, 0.3f, 0f);
        [Tooltip("Random +/- angle (degrees) added to the shot's aim, so it's not perfectly precise.")]
        [SerializeField] private float aimErrorDegrees = 30f;

        private ArenaEnemy arenaEnemy;
        private float fireTimer;
        private bool isWindingUp;

        protected Transform Player { get; private set; }
        protected Transform FirePoint => firePoint;
        protected float Damage => damage;
        protected float ProjectileSpeed => projectileSpeed;

        private void Start()
        {
            arenaEnemy = GetComponent<ArenaEnemy>();

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Player = playerObject.transform;
            }

            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            if (Player == null || projectilePrefab == null || isWindingUp) return;

            fireTimer += Time.deltaTime;
            if (fireTimer < fireInterval) return;

            float sqrDistance = ((Vector2)Player.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance > attackRange * attackRange) return;

            fireTimer = 0f;
            StartCoroutine(AttackSequence());
        }

        private IEnumerator AttackSequence()
        {
            isWindingUp = true;

            arenaEnemy?.ApplyStun(windupDuration);
            iTween.PunchScale(gameObject, iTween.Hash(
                "amount", punchScaleAmount,
                "time", windupDuration
            ));

            yield return new WaitForSeconds(windupDuration);

            Fire();
            isWindingUp = false;
        }

        protected virtual void Fire()
        {
            Vector2 aimDirection = ((Vector2)Player.position - (Vector2)firePoint.position).normalized;
            Vector2 direction = RotateDegrees(aimDirection, Random.Range(-aimErrorDegrees, aimErrorDegrees));
            SpawnProjectile(direction);
        }

        protected void SpawnProjectile(Vector2 direction)
        {
            GameObject instance = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            if (instance.TryGetComponent(out EnemyProjectile projectile))
            {
                projectile.Launch(direction, damage, projectileSpeed);
            }
        }

        protected static Vector2 RotateDegrees(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }
    }
}
