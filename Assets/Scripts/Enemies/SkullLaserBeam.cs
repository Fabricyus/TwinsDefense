using System.Collections;
using UnityEngine;
using TwinsDefense.Player;
using TwinsDefense.VFX;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// SkullBoss's Phase 2 attack: a red laser line that tracks the player's
    /// live position for trackDuration seconds, then locks in place for
    /// lockDuration seconds (pulsing as a final warning) before exploding —
    /// damaging the player anywhere along the locked line, not just at its
    /// tip. Fully self-contained (no prefab/art asset needed), drawn with a
    /// LineRenderer; reuses ExplosionVFX, spaced along the whole segment, for
    /// the detonation.
    /// </summary>
    public class SkullLaserBeam : MonoBehaviour
    {
        private Transform origin;
        private Transform target;
        private float trackDuration;
        private float lockDuration;
        private float damage;
        private float width;
        private LineRenderer line;

        private static Material cachedMaterial;

        /// <summary>Spawns and starts running the beam; it self-destroys once it explodes.</summary>
        public static SkullLaserBeam Spawn(Transform origin, Transform target, float trackDuration, float lockDuration, float damage, float width, Color color)
        {
            GameObject obj = new GameObject("SkullLaserBeam");

            LineRenderer line = obj.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.material = GetMaterial();
            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            line.numCapVertices = 4;
            line.useWorldSpace = true;
            line.sortingOrder = 12;

            SkullLaserBeam beam = obj.AddComponent<SkullLaserBeam>();
            beam.origin = origin;
            beam.target = target;
            beam.trackDuration = trackDuration;
            beam.lockDuration = lockDuration;
            beam.damage = damage;
            beam.width = width;
            beam.line = line;

            beam.StartCoroutine(beam.Run());
            return beam;
        }

        private IEnumerator Run()
        {
            float elapsed = 0f;
            while (elapsed < trackDuration)
            {
                UpdateLine(target.position);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Vector2 lockedEnd = target.position;

            float pulseTimer = 0f;
            while (pulseTimer < lockDuration)
            {
                UpdateLine(lockedEnd);
                float pulse = Mathf.PingPong(pulseTimer * 12f, 1f);
                Color pulseColor = Color.Lerp(Color.white, Color.red, 1f - pulse * 0.5f);
                line.startColor = pulseColor;
                line.endColor = pulseColor;
                pulseTimer += Time.deltaTime;
                yield return null;
            }

            Explode(lockedEnd);
            Destroy(gameObject);
        }

        private void UpdateLine(Vector2 endPoint)
        {
            line.SetPosition(0, origin.position);
            line.SetPosition(1, endPoint);
        }

        /// <summary>Damages the player anywhere along the beam (not just at the tip) and flashes an explosion burst spaced along the whole segment, so the whole line visibly detonates instead of just its endpoint.</summary>
        private void Explode(Vector2 endPoint)
        {
            Vector2 start = origin.position;
            SpawnLineExplosion(start, endPoint);

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null || !playerObject.TryGetComponent(out PlayerHurtbox hurtbox)) return;

            Vector2 closestPoint = ClosestPointOnSegment(playerObject.transform.position, start, endPoint);
            if (Vector2.Distance(playerObject.transform.position, closestPoint) <= width * 0.5f)
            {
                hurtbox.Health.TakeDamage(damage, closestPoint);
            }
        }

        private void SpawnLineExplosion(Vector2 start, Vector2 end)
        {
            float length = Vector2.Distance(start, end);
            int burstCount = Mathf.Max(2, Mathf.CeilToInt(length / (width * 3f)) + 1);

            for (int i = 0; i < burstCount; i++)
            {
                float t = burstCount == 1 ? 0f : (float)i / (burstCount - 1);
                ExplosionVFX.Spawn(Vector2.Lerp(start, end, t), width * 1.5f, Color.red);
            }
        }

        private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 segStart, Vector2 segEnd)
        {
            Vector2 segment = segEnd - segStart;
            float sqrLength = segment.sqrMagnitude;
            if (sqrLength < 0.0001f) return segStart;

            float t = Mathf.Clamp01(Vector2.Dot(point - segStart, segment) / sqrLength);
            return segStart + segment * t;
        }

        private static Material GetMaterial()
        {
            if (cachedMaterial != null) return cachedMaterial;

            cachedMaterial = new Material(Shader.Find("Sprites/Default"));
            return cachedMaterial;
        }
    }
}
