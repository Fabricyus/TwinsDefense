using UnityEngine;
using TwinsDefense.Enemies;
using TwinsDefense.Combat;
using TwinsDefense.Data;
using TwinsDefense.VFX;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Automatically targets and fires at the nearest enemy in range on a
    /// fixed interval. No manual aiming — this is the player's baseline attack.
    /// Reads its numbers live from PlayerStats so level-up cards take effect
    /// immediately.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class AutoAttack : MonoBehaviour
    {
        [Header("Firing")]
        [Tooltip("Used only if no PlayerCharacterData is found, or its CharacterMetaData has no projectilePrefab assigned.")]
        [SerializeField] private GameObject projectilePrefab;
        [Tooltip("Origin the projectile spawns from. Defaults to this transform if left unassigned.")]
        [SerializeField] private Transform firePoint;
        [Tooltip("Angle in degrees between adjacent projectiles when Extra Projectile cards add more than one shot.")]
        [SerializeField] private float extraProjectileSpreadAngle = 15f;
        [Tooltip("Random +/- swing applied to each hit's damage before crit, so consecutive hits aren't perfectly uniform.")]
        [SerializeField] private float damageVariance = 2f;

        [Header("Extra Projectile Falloff")]
        [Tooltip("Damage multiplier reduction applied to each projectile beyond the first two (base shot + first Extra Projectile stack) in a multi-shot volley.")]
        [SerializeField] private float extraProjectileDamageFalloff = 0.2f;
        [Tooltip("Floor for the per-projectile damage multiplier, so heavily stacked multi-shot builds don't drop to near-zero per shot.")]
        [SerializeField] private float minExtraProjectileDamageMultiplier = 0.5f;

        private PlayerStats stats;
        private PlayerCharacterData characterData;
        private float attackTimer;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            characterData = GetComponent<PlayerCharacterData>();
        }

        private void Start()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            if (stats.attackFireRate <= 0f) return;

            attackTimer += Time.deltaTime;

            if (attackTimer >= 1f / stats.attackFireRate)
            {
                attackTimer = 0f;
                Attack();
            }
        }

        private void Attack()
        {
            ArenaEnemy target = FindNearestEnemyInRange();
            GameObject prefab = ResolveProjectilePrefab();
            if (target == null || prefab == null) return;

            Vector2 direction = ((Vector2)target.transform.position - (Vector2)firePoint.position).normalized;

            float rolledDamage = Mathf.Max(0f, stats.damage + Random.Range(-damageVariance, damageVariance));
            bool isCrit = Random.value < stats.critChance;
            float finalDamage = isCrit ? rolledDamage * stats.critDamage : rolledDamage;

            // Extra Projectile cards fan additional shots out around the aimed direction
            // instead of retargeting, so more projectiles means wider coverage.
            int totalProjectiles = 1 + Mathf.Max(0, Mathf.RoundToInt(stats.extraProjectileCount));

            for (int i = 0; i < totalProjectiles; i++)
            {
                float angleOffset = (i - (totalProjectiles - 1) / 2f) * extraProjectileSpreadAngle;
                Vector2 fireDirection = angleOffset == 0f ? direction : RotateDegrees(direction, angleOffset);
                float projectileDamage = finalDamage * ExtraProjectileDamageMultiplier(i);
                FireProjectile(prefab, fireDirection, projectileDamage, isCrit);
            }
        }

        /// <summary>Character-specific prefab (set per tier in Character Selection) takes priority over the inspector fallback.</summary>
        private GameObject ResolveProjectilePrefab()
        {
            if (characterData != null && characterData.Current != null && characterData.Current.projectilePrefab != null)
            {
                return characterData.Current.projectilePrefab;
            }

            return projectilePrefab;
        }

        private void FireProjectile(GameObject prefab, Vector2 direction, float damage, bool isCrit)
        {
            GameObject instance = Instantiate(prefab, firePoint.position, Quaternion.identity);
            Projectile projectile = instance.GetComponent<Projectile>();

            if (projectile != null)
            {
                int pierceCount = Mathf.Max(0, Mathf.RoundToInt(stats.pierceCount));
                bool isRotatingProjectile = characterData != null && characterData.Current != null && characterData.Current.isRotatingProjectile;

                OnHitPassiveEffects onHitPassives = ResolveOnHitPassives();

                projectile.Launch(direction, damage, stats.projectileSpeed, isCrit, pierceCount, stats.areaOfEffect, isRotatingProjectile, onHitPassives);
            }

            ApplyStarCosmeticTrail(instance);
        }

        /// <summary>Star Upgrade cosmetic (3+ stars): dyes the projectile's existing trail (if its prefab already has one baked in, e.g. an evolved form) or adds one on the fly — never changes gameplay, just the color/presence of a cast trail.</summary>
        private void ApplyStarCosmeticTrail(GameObject projectileInstance)
        {
            if (!stats.hasStarCosmeticTrail) return;

            ProjectileTrailVFX trail = projectileInstance.GetComponent<ProjectileTrailVFX>();
            if (trail == null)
            {
                trail = projectileInstance.AddComponent<ProjectileTrailVFX>();
            }

            trail.Configure(stats.starCosmeticTrailColor);
        }

        /// <summary>Reads the selected character's on-hit passives (ThunderStrike/Stun/Slow/Chain/ExplodeOnKill) from its passiveEffects list, plus any ExplodeOnKillChance stacked on from cards. Star Upgrades strengthen the character's own passive — never a card-granted proc — via stats.passiveProcChanceBonus/passiveMagnitudeBonusPercent, applied below wherever a value comes from the character's own CharacterPassiveEffect.</summary>
        private OnHitPassiveEffects ResolveOnHitPassives()
        {
            var passives = new OnHitPassiveEffects();
            float magnitudeMultiplier = 1f + stats.passiveMagnitudeBonusPercent / 100f;

            if (characterData == null || characterData.Current == null)
            {
                ResolveExplodeOnKill(null, ref passives);
                return passives;
            }

            System.Collections.Generic.List<CharacterPassiveEffect> effects = characterData.Current.passiveEffects;

            CharacterPassiveEffect thunder = effects.Find(e => e.effectType == CharacterPassiveEffectType.ThunderStrikeOnHit);
            if (thunder != null)
            {
                passives.thunderChancePercent = thunder.procChancePercent + stats.passiveProcChanceBonus;
                // Treated as a guaranteed crit: the passive's own multiplier (e.g. 300%) stacks additively with
                // the player's current critDamage stat, so Crit Damage cards make this proc hit harder too.
                passives.thunderBonusDamage = stats.damage * (thunder.damageMultiplier * magnitudeMultiplier + stats.critDamage);
                passives.thunderFxPrefab = characterData.Current.procFxPrefab;
            }

            CharacterPassiveEffect stun = effects.Find(e => e.effectType == CharacterPassiveEffectType.StunOnHit);
            if (stun != null)
            {
                passives.stunChancePercent = stun.procChancePercent + stats.passiveProcChanceBonus;
                passives.stunDurationSeconds = stun.procDurationSeconds * magnitudeMultiplier;
            }

            CharacterPassiveEffect slow = effects.Find(e => e.effectType == CharacterPassiveEffectType.SlowOnHit);
            if (slow != null)
            {
                passives.slowChancePercent = slow.procChancePercent + stats.passiveProcChanceBonus;
                passives.slowMagnitudePercent = Mathf.Min(100f, slow.procMagnitudePercent * magnitudeMultiplier);
                passives.slowDurationSeconds = slow.procDurationSeconds * magnitudeMultiplier;
            }

            CharacterPassiveEffect chain = effects.Find(e => e.effectType == CharacterPassiveEffectType.ChainOnHit);
            if (chain != null)
            {
                passives.chainChancePercent = chain.procChancePercent + stats.passiveProcChanceBonus;
            }

            CharacterPassiveEffect explodeOnKill = effects.Find(e => e.effectType == CharacterPassiveEffectType.ExplodeOnKill);
            ResolveExplodeOnKill(explodeOnKill, ref passives);

            return passives;
        }

        /// <summary>Chance stacks additively between the character's own ExplodeOnKill passive (if any, boosted by Star Upgrades) and PlayerStats.explodeOnKillChance (from cards, never boosted by stars); explosion color always uses the character's own passive when present, otherwise the fixed orange cards grant on their own. Damage is no longer derived from the player's stats at all (no magnitude to boost) — Projectile.RollExplodeOnKill scales it to 100% of whichever enemy actually dies.</summary>
        private void ResolveExplodeOnKill(CharacterPassiveEffect explodeOnKill, ref OnHitPassiveEffects passives)
        {
            Color cardOnlyColor = new Color(1f, 0.55f, 0.1f, 1f);

            float nativeChance = explodeOnKill != null ? explodeOnKill.procChancePercent + stats.passiveProcChanceBonus : 0f;
            float chancePercent = nativeChance + stats.explodeOnKillChance;
            if (chancePercent <= 0f) return;

            passives.explodeOnKillChancePercent = chancePercent;
            passives.explodeOnKillColor = explodeOnKill != null ? explodeOnKill.explosionColor : cardOnlyColor;
        }

        /// <summary>Damage multiplier for the i-th projectile (0-indexed) in a multi-shot volley. The first two projectiles (base shot + first Extra Projectile stack) deal full damage; each additional projectile beyond that falls off by extraProjectileDamageFalloff per step, floored at minExtraProjectileDamageMultiplier — keeps stacking Extra Projectile sources strong without letting it be an uncapped DPS multiplier.</summary>
        private float ExtraProjectileDamageMultiplier(int projectileIndex)
        {
            if (projectileIndex <= 1) return 1f;

            float falloffSteps = projectileIndex - 1;
            return Mathf.Max(minExtraProjectileDamageMultiplier, 1f - falloffSteps * extraProjectileDamageFalloff);
        }

        private static Vector2 RotateDegrees(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }

        private ArenaEnemy FindNearestEnemyInRange()
        {
            ArenaEnemy nearest = null;
            float nearestSqrDistance = stats.attackRange * stats.attackRange;

            foreach (ArenaEnemy enemy in ArenaEnemy.Active)
            {
                float sqrDistance = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;

                if (sqrDistance <= nearestSqrDistance)
                {
                    nearest = enemy;
                    nearestSqrDistance = sqrDistance;
                }
            }

            return nearest;
        }
    }
}
