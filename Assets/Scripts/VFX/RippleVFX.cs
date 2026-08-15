using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// White circular shockwave — like a water-drop ripple — that expands
    /// outward from the spawn point while fading to nothing. Used to
    /// punctuate a boss's arrival. Fully self-contained (procedural ring
    /// sprite, no art asset needed), reusing the same generate-and-cache
    /// approach as AttackCircleVFX's filled circle.
    /// </summary>
    public class RippleVFX : MonoBehaviour
    {
        private static Sprite cachedRingSprite;

        private SpriteRenderer spriteRenderer;
        private float duration;
        private float startDiameter;
        private float maxDiameter;
        private Color color;
        private float elapsed;

        /// <param name="maxDiameter">World-space diameter the ring grows to before it's fully faded out.</param>
        public static RippleVFX Spawn(Vector2 position, float maxDiameter = 14f, float duration = 0.8f)
        {
            GameObject obj = new GameObject("RippleVFX");
            obj.transform.position = position;

            RippleVFX ripple = obj.AddComponent<RippleVFX>();
            ripple.duration = duration;
            ripple.startDiameter = 0.5f;
            ripple.maxDiameter = maxDiameter;
            ripple.color = new Color(1f, 1f, 1f, 0.9f);

            ripple.spriteRenderer = obj.AddComponent<SpriteRenderer>();
            ripple.spriteRenderer.sprite = GetRingSprite();
            ripple.spriteRenderer.color = ripple.color;
            ripple.spriteRenderer.sortingOrder = 20;
            obj.transform.localScale = Vector3.one * ripple.startDiameter;

            Destroy(obj, duration);
            return ripple;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - t) * (1f - t); // ease-out: fast expansion up front, settles near the end

            float diameter = Mathf.Lerp(startDiameter, maxDiameter, eased);
            transform.localScale = Vector3.one * diameter;

            Color c = color;
            c.a = color.a * (1f - t);
            spriteRenderer.color = c;
        }

        /// <summary>Generates (and caches) a soft-edged white ring sprite — a hollow circle, unlike AttackCircleVFX's filled one.</summary>
        private static Sprite GetRingSprite()
        {
            if (cachedRingSprite != null) return cachedRingSprite;

            const int size = 128;
            const float pixelsPerUnit = size;
            const float outerRadius = size / 2f;
            const float ringThickness = 10f;
            const float innerRadius = outerRadius - ringThickness;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size / 2f, size / 2f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float outerAlpha = Mathf.Clamp01((outerRadius - distance) / 1.5f);
                    float innerAlpha = Mathf.Clamp01((distance - innerRadius) / 1.5f);
                    float alpha = Mathf.Min(outerAlpha, innerAlpha);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();

            cachedRingSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            return cachedRingSprite;
        }
    }
}
