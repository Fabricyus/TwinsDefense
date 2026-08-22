using System.Collections;
using UnityEngine;
using TwinsDefense.Environment;
using TwinsDefense.Player;
using TwinsDefense.VFX;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// SkullBoss's Phase 2 attack: a red laser line that tracks the player's
    /// live position for trackDuration seconds, then locks in place for
    /// lockDuration seconds (pulsing as a final warning) before exploding —
    /// damaging the player anywhere along the locked line, not just at its
    /// tip. The beam's tip isn't the player itself — it's the point where the
    /// origin->player ray exits the arena (see ArenaBounds), so it always
    /// spans the full map instead of stopping short at wherever the player
    /// happens to be standing. Fully self-contained (no prefab/art asset
    /// needed), drawn with a LineRenderer; reuses ExplosionVFX, spaced along
    /// the whole segment, for the detonation.
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

            // Locked direction (through wherever the player was standing when tracking ended),
            // extended to the arena edge — not just the player's raw position.
            Vector2 lockedEnd = ComputeEdgeEndpoint(target.position);

            float pulseTimer = 0f;
            while (pulseTimer < lockDuration)
            {
                UpdateLineToPoint(lockedEnd);
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

        /// <summary>Aims the beam through aimPoint (the player's position), then extends it to the arena edge before drawing.</summary>
        private void UpdateLine(Vector2 aimPoint)
        {
            UpdateLineToPoint(ComputeEdgeEndpoint(aimPoint));
        }

        private void UpdateLineToPoint(Vector2 endPoint)
        {
            line.SetPosition(0, origin.position);
            line.SetPosition(1, endPoint);
        }

        /// <summary>Extends the origin->aimPoint ray out to where it exits ArenaBounds.WorldBounds. Falls back to aimPoint itself (the old stops-at-the-player behavior) if ArenaBounds isn't in the scene or the ray is degenerate.</summary>
        private Vector2 ComputeEdgeEndpoint(Vector2 aimPoint)
        {
            Vector2 originPos = origin.position;
            Vector2 direction = aimPoint - originPos;

            if (ArenaBounds.Instance == null || direction.sqrMagnitude < 0.0001f)
            {
                return aimPoint;
            }

            return ExtendToBoundsEdge(originPos, direction, ArenaBounds.Instance.WorldBounds);
        }

        /// <summary>Standard ray/AABB slab exit-point calculation, assuming origin sits inside bounds — returns where the origin->direction ray crosses the box edge.</summary>
        private static Vector2 ExtendToBoundsEdge(Vector2 origin, Vector2 direction, Bounds bounds)
        {
            direction = direction.normalized;
            float exitT = float.MaxValue;

            if (Mathf.Abs(direction.x) > 0.0001f)
            {
                float exitXt = Mathf.Max((bounds.min.x - origin.x) / direction.x, (bounds.max.x - origin.x) / direction.x);
                exitT = Mathf.Min(exitT, exitXt);
            }

            if (Mathf.Abs(direction.y) > 0.0001f)
            {
                float exitYt = Mathf.Max((bounds.min.y - origin.y) / direction.y, (bounds.max.y - origin.y) / direction.y);
                exitT = Mathf.Min(exitT, exitYt);
            }

            return exitT > 0f && exitT < float.MaxValue ? origin + direction * exitT : origin + direction * 100f;
        }

        /// <summary>Damages the player anywhere along the beam (not just at the tip) and flashes an explosion burst spaced along the whole segment, so the whole line visibly detonates instead of just its endpoint.</summary>
        private void Explode(Vector2 endPoint)
        {
            Vector2 start = origin.position;
            SpawnLineExplosion(start, endPoint);

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            // PlayerHurtbox lives on a small child GameObject, not the tagged root — TryGetComponent
            // only checks the object itself, so this must search children (see PlayerHurtbox's doc).
            PlayerHurtbox hurtbox = playerObject != null ? playerObject.GetComponentInChildren<PlayerHurtbox>() : null;
            if (hurtbox == null) return;

            Vector2 closestPoint = ClosestPointOnSegment(playerObject.transform.position, start, endPoint);
            if (Vector2.Distance(playerObject.transform.position, closestPoint) <= width * 0.5f)
            {
                hurtbox.Health.TakeDamage(damage, closestPoint);
            }
        }

        private void SpawnLineExplosion(Vector2 start, Vector2 end)
        {
            float length = Vector2.Distance(start, end);
            // Capped — the beam now reaches the arena edge instead of stopping at the player, so
            // length is no longer naturally bounded by how close the boss and player are.
            int burstCount = Mathf.Clamp(Mathf.CeilToInt(length / (width * 3f)) + 1, 2, 30);

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
