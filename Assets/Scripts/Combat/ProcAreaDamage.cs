using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Enemies;
using TwinsDefense.Systems;

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
        private Vector3 baseScale;
        private SpriteRenderer spriteRenderer;
        private float baseAlpha = 1f;

        private void Awake()
        {
            hitbox = GetComponent<CircleCollider2D>();
            hitbox.isTrigger = true;
            baseScale = transform.localScale;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                baseAlpha = spriteRenderer.color.a;
                ApplyOpacity(ProjectileOpacitySettings.Value);
            }
        }

        private void OnEnable()
        {
            ProjectileOpacitySettings.OnChanged += ApplyOpacity;
        }

        private void OnDisable()
        {
            ProjectileOpacitySettings.OnChanged -= ApplyOpacity;
        }

        /// <summary>Scales the sprite's own alpha by the player's projectile-opacity preference — same slider/setting Projectile/StarProjectile/ProjectileTrailVFX already use.</summary>
        private void ApplyOpacity(float opacity)
        {
            if (spriteRenderer == null) return;

            Color color = spriteRenderer.color;
            color.a = baseAlpha * opacity;
            spriteRenderer.color = color;
        }

        /// <summary>Scales the FX GameObject (visual + hitbox) by areaOfEffectScale, then damages every ArenaEnemy caught inside it (once each). popupColor tints the damage popup per character (e.g. thunderFx/holyFx/heartFx) instead of the default crit gold.</summary>
        public void Detonate(float damage, bool isCrit, float areaOfEffectScale, Color? popupColor = null)
        {
            float scaleMultiplier = Mathf.Max(0.01f, areaOfEffectScale);
            transform.localScale = baseScale * scaleMultiplier;

            float worldRadius = hitbox.radius * transform.lossyScale.x;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, worldRadius);
            var damagedEnemies = new HashSet<ArenaEnemy>();

            foreach (Collider2D hit in hits)
            {
                ArenaEnemy enemy = hit.GetComponent<ArenaEnemy>();
                if (enemy == null || !damagedEnemies.Add(enemy)) continue;

                enemy.TakeDamage(damage, isCrit, popupColor: popupColor);
            }
        }
    }
}
