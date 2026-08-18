using System.Collections;
using UnityEngine;
using TwinsDefense.Environment;
using TwinsDefense.Player;
using TwinsDefense.VFX;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// MagBoss's Phase 3 attack: a horizontal beam spanning the full screen width,
    /// starting above the screen and descending to the player's Y, plus a vertical
    /// beam spanning the full screen height, starting past the right edge and
    /// sliding in to the player's X — both tracking the player's live position as
    /// they close in, so together they land as a cross centered exactly on the
    /// player by the end of trackDuration. They lock, pulse as a warning for
    /// lockDuration, then both explode at once. Fully self-contained (no
    /// prefab/art asset needed); mirrors SkullLaserBeam's track/lock/explode
    /// shape but spans screen edges instead of boss-to-target.
    /// </summary>
    public class MagpieCrossLaser : MonoBehaviour
    {
        private Transform target;
        private float trackDuration;
        private float lockDuration;
        private float damage;
        private float width;
        private LineRenderer horizontalLine;
        private LineRenderer verticalLine;

        private static Material cachedMaterial;

        /// <summary>Spawns and starts running the cross; it self-destroys once it explodes.</summary>
        public static MagpieCrossLaser Spawn(Transform target, float trackDuration, float lockDuration, float damage, float width, Color color)
        {
            GameObject obj = new GameObject("MagpieCrossLaser");

            MagpieCrossLaser cross = obj.AddComponent<MagpieCrossLaser>();
            cross.target = target;
            cross.trackDuration = trackDuration;
            cross.lockDuration = lockDuration;
            cross.damage = damage;
            cross.width = width;
            cross.horizontalLine = CreateLine(obj.transform, "Horizontal", color, width);
            cross.verticalLine = CreateLine(obj.transform, "Vertical", color, width);

            cross.StartCoroutine(cross.Run());
            return cross;
        }

        private static LineRenderer CreateLine(Transform parent, string name, Color color, float width)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(parent, false);

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.material = GetMaterial();
            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            line.numCapVertices = 4;
            line.useWorldSpace = true;
            line.sortingOrder = 12;
            return line;
        }

        private IEnumerator Run()
        {
            Bounds bounds = GetScreenWorldBounds();
            float startY = bounds.max.y;
            float startX = bounds.max.x;

            float elapsed = 0f;
            while (elapsed < trackDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / trackDuration);

                float y = Mathf.Lerp(startY, target.position.y, t);
                float x = Mathf.Lerp(startX, target.position.x, t);
                UpdateLines(bounds, y, x, horizontalLine.startColor);

                yield return null;
            }

            float lockedY = target.position.y;
            float lockedX = target.position.x;

            float pulseTimer = 0f;
            while (pulseTimer < lockDuration)
            {
                pulseTimer += Time.deltaTime;
                float pulse = Mathf.PingPong(pulseTimer * 12f, 1f);
                Color pulseColor = Color.Lerp(Color.white, Color.red, 1f - pulse * 0.5f);
                UpdateLines(bounds, lockedY, lockedX, pulseColor);
                yield return null;
            }

            Explode(bounds, lockedY, lockedX);
            Destroy(gameObject);
        }

        private void UpdateLines(Bounds bounds, float y, float x, Color color)
        {
            horizontalLine.SetPosition(0, new Vector3(bounds.min.x, y, 0f));
            horizontalLine.SetPosition(1, new Vector3(bounds.max.x, y, 0f));
            horizontalLine.startColor = color;
            horizontalLine.endColor = color;

            verticalLine.SetPosition(0, new Vector3(x, bounds.min.y, 0f));
            verticalLine.SetPosition(1, new Vector3(x, bounds.max.y, 0f));
            verticalLine.startColor = color;
            verticalLine.endColor = color;
        }

        /// <summary>Bursts an explosion strip along both full beams and damages the player once if they're standing on either line — not gated on both, so a dodge onto just one arm still gets caught.</summary>
        private void Explode(Bounds bounds, float lockedY, float lockedX)
        {
            SpawnLineExplosion(new Vector2(bounds.min.x, lockedY), new Vector2(bounds.max.x, lockedY));
            SpawnLineExplosion(new Vector2(lockedX, bounds.min.y), new Vector2(lockedX, bounds.max.y));

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            // PlayerHurtbox lives on a small child GameObject, not the tagged root — TryGetComponent
            // only checks the object itself, so this must search children (see PlayerHurtbox's doc).
            PlayerHurtbox hurtbox = playerObject != null ? playerObject.GetComponentInChildren<PlayerHurtbox>() : null;
            if (hurtbox == null) return;

            Vector2 playerPos = playerObject.transform.position;
            bool onHorizontal = Mathf.Abs(playerPos.y - lockedY) <= width * 0.5f;
            bool onVertical = Mathf.Abs(playerPos.x - lockedX) <= width * 0.5f;

            if (onHorizontal || onVertical)
            {
                hurtbox.Health.TakeDamage(damage, playerPos);
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

        /// <summary>Spans the arena's actual background (Grid/bg's ArenaBounds) so both beams stretch edge-to-edge across the playable area, not just whatever the camera currently frames. Falls back to the camera viewport if ArenaBounds hasn't spawned (e.g. a test scene without the arena tilemap).</summary>
        private static Bounds GetScreenWorldBounds()
        {
            if (ArenaBounds.Instance != null)
            {
                return ArenaBounds.Instance.WorldBounds;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                return new Bounds(Vector3.zero, new Vector3(16f, 10f, 0f));
            }

            float screenHeight = cam.orthographicSize * 2f;
            float screenWidth = screenHeight * cam.aspect;
            Vector3 center = cam.transform.position;
            center.z = 0f;

            return new Bounds(center, new Vector3(screenWidth, screenHeight, 0f));
        }

        private static Material GetMaterial()
        {
            if (cachedMaterial != null) return cachedMaterial;

            cachedMaterial = new Material(Shader.Find("Sprites/Default"));
            return cachedMaterial;
        }
    }
}
