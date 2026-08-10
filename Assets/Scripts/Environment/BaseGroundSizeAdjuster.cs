using UnityEngine;

namespace TwinsDefense.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BaseGroundSizeAdjuster : MonoBehaviour
    {
        [SerializeField] private float heightIncrease = 0.01f;
        [SerializeField] private SpriteRenderer spriteRenderer;
         
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer.drawMode != SpriteDrawMode.Tiled)
                return;

            Vector2 size = spriteRenderer.size;
            size.y += heightIncrease;
            spriteRenderer.size = size;
        }

        private void FixedUpdate()
        {
            if (spriteRenderer.drawMode != SpriteDrawMode.Tiled)
                return;

            Vector2 size = spriteRenderer.size;
            size.y += heightIncrease;
            size.x += heightIncrease;
            spriteRenderer.size = size;
        }
    }
}
