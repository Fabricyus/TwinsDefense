using UnityEngine;
using UnityEngine.Tilemaps;

namespace TwinsDefense.Environment
{
    /// <summary>
    /// Exposes the world-space bounds of the arena's background tilemap
    /// (read once from its own TilemapRenderer, which already accounts for
    /// the parent Grid's position/scale) so PlayerController and
    /// CameraFollow can clamp themselves to never move past it.
    /// </summary>
    [RequireComponent(typeof(TilemapRenderer))]
    public class ArenaBounds : MonoBehaviour
    {
        public static ArenaBounds Instance { get; private set; }

        public Bounds WorldBounds { get; private set; }

        private void Awake()
        {
            Instance = this;
            WorldBounds = GetComponent<TilemapRenderer>().bounds;
        }

        /// <summary>
        /// Clamps a world position to stay within WorldBounds, inset by marginX/marginY
        /// on each axis (e.g. half the player's collider size, or the camera's half
        /// viewport extents). Falls back to centering that axis if the margin is larger
        /// than the bounds itself (viewport wider than the arena), rather than inverting
        /// the clamp range.
        /// </summary>
        public Vector2 Clamp(Vector2 position, float marginX = 0f, float marginY = 0f)
        {
            float minX = WorldBounds.min.x + marginX;
            float maxX = WorldBounds.max.x - marginX;
            float minY = WorldBounds.min.y + marginY;
            float maxY = WorldBounds.max.y - marginY;

            float x = maxX >= minX ? Mathf.Clamp(position.x, minX, maxX) : WorldBounds.center.x;
            float y = maxY >= minY ? Mathf.Clamp(position.y, minY, maxY) : WorldBounds.center.y;

            return new Vector2(x, y);
        }
    }
}
