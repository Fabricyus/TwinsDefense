using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Shared procedural sunburst sprite (a soft glow with 8 rays radiating
    /// from a bright core) — unlike AttackCircleVFX's plain circle, this has
    /// rotational asymmetry so spinning it is actually visible. Originally
    /// authored for CardRarityVFX's Epic/Rare card aura; also used by
    /// PlayerStarAuraVFX for the 5-star world-space aura.
    /// </summary>
    public static class SunburstSprite
    {
        private static Sprite cached;

        public static Sprite Get()
        {
            if (cached != null) return cached;

            const int size = 128;
            const float pixelsPerUnit = size;
            const int spikeCount = 8;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 offset = new Vector2(x + 0.5f, y + 0.5f) - center;
                    float distance = offset.magnitude;
                    float normalizedDistance = Mathf.Clamp01(distance / radius);

                    float angle = Mathf.Atan2(offset.y, offset.x);
                    float rayShape = (Mathf.Cos(angle * spikeCount) + 1f) * 0.5f; // 0-1, spikeCount lobes around the circle
                    float rayReach = Mathf.Lerp(0.4f, 1f, rayShape); // rays extend further than the gaps between them

                    float rayEdge = Mathf.Clamp01((rayReach - normalizedDistance) * 4f);
                    float softCore = Mathf.Clamp01(1f - normalizedDistance * 1.6f); // bright center regardless of angle, so it still reads as a glow
                    float alpha = Mathf.Max(rayEdge, softCore);

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();

            cached = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            return cached;
        }
    }
}
