using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Enemies;

namespace TwinsDefense.Combat
{
    /// <summary>
    /// One-shot AoE hit dealt by a character's proc VFX (thunderFx_0, holyFx_0,
    /// heartFx_0): damages every ArenaEnemy inside a circle hitbox centered on
    /// the FX, scaled by the player's current Area of Effect stat. Call
    /// Detonate right after Instantiate.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class ProcAreaDamage : MonoBehaviour
    {
        private CircleCollider2D hitbox;
        private float baseRadius;

        private void Awake()
        {
            hitbox = GetComponent<CircleCollider2D>();
            hitbox.isTrigger = true;
            baseRadius = hitbox.radius;
        }

        /// <summary>Scales the hitbox by areaOfEffectScale, then damages every ArenaEnemy caught inside it (once each).</summary>
        public void Detonate(float damage, bool isCrit, float areaOfEffectScale)
        {
            hitbox.radius = baseRadius * Mathf.Max(0.01f, areaOfEffectScale);

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitbox.radius);
            var damagedEnemies = new HashSet<ArenaEnemy>();

            foreach (Collider2D hit in hits)
            {
                ArenaEnemy enemy = hit.GetComponent<ArenaEnemy>();
                if (enemy == null || !damagedEnemies.Add(enemy)) continue;

                enemy.TakeDamage(damage, isCrit);
            }
        }
    }
}
