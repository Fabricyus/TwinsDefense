using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Player;
using TwinsDefense.VFX;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// A single telegraphed danger zone for ReaperEnemy's phase 2: grows from
    /// scale 0 to full diameter over growDuration, then "explodes" — damaging
    /// the player if they're standing inside it — and immediately respawns at
    /// a new position spread away from other active hazard circles, repeating
    /// until phaseEndTime. Fully self-contained (no prefab/art asset needed),
    /// reusing AttackCircleVFX's procedural circle sprite.
    /// </summary>
    public class ReaperHazardCircle : MonoBehaviour
    {
        /// <summary>All currently active hazard circles, used to keep new spawn positions spread apart.</summary>
        public static readonly HashSet<ReaperHazardCircle> Active = new HashSet<ReaperHazardCircle>();

        private float growDuration;
        private float diameter;
        private float explosionDamage;
        private float phaseEndTime;
        private float spawnAreaMultiplier;
        private float elapsed;

        public static ReaperHazardCircle Spawn(Vector2 position, float growDuration, float diameter, float explosionDamage, float phaseEndTime, float spawnAreaMultiplier = 1f)
        {
            GameObject obj = new GameObject("ReaperHazardCircle");
            obj.transform.position = position;
            obj.transform.localScale = Vector3.zero;

            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AttackCircleVFX.GetCircleSprite();
            spriteRenderer.color = new Color(1f, 0.15f, 0.15f, 0.55f);
            spriteRenderer.sortingOrder = 10;

            ReaperHazardCircle circle = obj.AddComponent<ReaperHazardCircle>();
            circle.growDuration = growDuration;
            circle.diameter = diameter;
            circle.explosionDamage = explosionDamage;
            circle.phaseEndTime = phaseEndTime;
            circle.spawnAreaMultiplier = spawnAreaMultiplier;

            return circle;
        }

        private void OnEnable()
        {
            Active.Add(this);
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / growDuration);
            transform.localScale = Vector3.one * (diameter * t);

            if (elapsed >= growDuration)
            {
                Explode();
            }
        }

        private void Explode()
        {
            ExplosionVFX.Spawn(transform.position, diameter / 2f);

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, diameter / 2f);
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out PlayerHurtbox hurtbox))
                {
                    hurtbox.Health.TakeDamage(explosionDamage, transform.position);
                    break;
                }
            }

            if (Time.time >= phaseEndTime)
            {
                Destroy(gameObject);
                return;
            }

            elapsed = 0f;
            transform.localScale = Vector3.zero;
            transform.position = FindSpreadPosition(diameter, spawnAreaMultiplier, this);
        }

        /// <summary>
        /// Picks a random point within the visible screen area — scaled by areaMultiplier
        /// (2 = twice the area, i.e. each dimension scaled by sqrt(2)) — that's at least
        /// minSpacing away from every other active hazard circle (rejection sampling),
        /// falling back to the last-tried point if it can't find a clean spot within maxAttempts.
        /// </summary>
        public static Vector2 FindSpreadPosition(float minSpacing, float areaMultiplier = 1f, ReaperHazardCircle exclude = null, int maxAttempts = 20)
        {
            Rect bounds = GetScreenWorldBounds(minSpacing * 0.5f, areaMultiplier);
            Vector2 candidate = bounds.center;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                candidate = new Vector2(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax));

                bool overlaps = false;
                foreach (ReaperHazardCircle other in Active)
                {
                    if (other == exclude) continue;
                    if (Vector2.Distance(candidate, other.transform.position) < minSpacing)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps) return candidate;
            }

            return candidate;
        }

        /// <summary>
        /// World-space rect covering the visible camera area scaled by areaMultiplier (both
        /// dimensions scaled by sqrt(areaMultiplier), so the total area itself scales exactly
        /// by areaMultiplier), inset by margin on every side so circles never spawn clipped off-screen.
        /// </summary>
        private static Rect GetScreenWorldBounds(float margin, float areaMultiplier = 1f)
        {
            float dimensionScale = Mathf.Sqrt(Mathf.Max(0f, areaMultiplier));

            Camera cam = Camera.main;
            if (cam == null)
            {
                float fallbackWidth = 16f * dimensionScale;
                float fallbackHeight = 10f * dimensionScale;
                return new Rect(-fallbackWidth / 2f + margin, -fallbackHeight / 2f + margin, fallbackWidth - margin * 2f, fallbackHeight - margin * 2f);
            }

            float height = cam.orthographicSize * 2f * dimensionScale;
            float width = height * cam.aspect;
            Vector2 center = cam.transform.position;

            return new Rect(center.x - width / 2f + margin, center.y - height / 2f + margin, width - margin * 2f, height - margin * 2f);
        }
    }
}
