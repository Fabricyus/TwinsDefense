using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Brief circle flash spawned under an enemy as visual feedback for a
    /// contact-damage hit. Purely visual — no collider, no damage logic;
    /// self-destroys after its lifetime. The circle sprite is generated in
    /// code (and cached) so this needs no art asset.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class AttackCircleVFX : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.1f;
        [SerializeField] private Color color = new Color(1f, 0.15f, 0.15f, 0.6f);

        private static Sprite cachedCircleSprite;

        private void Awake()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetCircleSprite();
            spriteRenderer.color = color;
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        /// <summary>Generates (and caches) a soft-edged white circle sprite — reused by other VFX (e.g. ReaperHazardCircle) that need a circle with no art asset.</summary>
        public static Sprite GetCircleSprite()
        {
            if (cachedCircleSprite != null) return cachedCircleSprite;

            const int size = 64;
            const float pixelsPerUnit = size;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = Mathf.Clamp01((radius - distance) / 1.5f); // soft ~1.5px edge
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();

            cachedCircleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            return cachedCircleSprite;
        }
    }
}
