using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Always-on cosmetic rainbow rim-light for a special enemy/boss (e.g. the
    /// level-100 Mega Magpie) — a spinning rainbow silhouette outline traced
    /// just inside the sprite's own edge via RainbowOutline.shader (Mat_RainbowAuraInner),
    /// same shader/material PlayerRainbowAuraVFX uses for the level-100 player
    /// reward, just without the unlock gating (this is unconditional once the
    /// component is present). Sits on a child GameObject with a lower
    /// sortingOrder than the host's own SpriteRenderer — the inner-glow
    /// material is meant to peek out from BEHIND the sprite at its
    /// semi-transparent edge pixels, not sit visibly on top of it.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemyRainbowAuraVFX : MonoBehaviour
    {
        [Tooltip("The host's own animated sprite — the aura mirrors its current frame and flipX every frame so the outline always matches the visible silhouette. Auto-resolves to GetComponent<SpriteRenderer>() if left empty.")]
        [SerializeField] private SpriteRenderer hostRenderer;

        [SerializeField] private Material rainbowOutlineMaterial;
        [Tooltip("Kept below the host's own SpriteRenderer sortingOrder so the inner-glow ring only peeks through at the sprite's semi-transparent edge pixels instead of drawing on top of it.")]
        [SerializeField] private int sortingOrder = -1;

        [Header("Outline look (placeholders — tune in Inspector)")]
        [SerializeField] private float outlineThicknessTexels = 2f;
        [SerializeField] private int rainbowBandCount = 6;
        [SerializeField] private float spinSpeed = 1.5f;
        [SerializeField] private float brightness = 1.4f;
        [SerializeField] private float outlineAlpha = 0.9f;

        private SpriteRenderer auraRenderer;
        private Material auraMaterialInstance;

        private void Awake()
        {
            if (hostRenderer == null)
            {
                hostRenderer = GetComponent<SpriteRenderer>();
            }

            GameObject auraObject = new GameObject("RainbowAura");
            auraObject.transform.SetParent(transform, false);

            auraRenderer = auraObject.AddComponent<SpriteRenderer>();
            auraRenderer.sortingOrder = sortingOrder;

            if (rainbowOutlineMaterial != null)
            {
                auraMaterialInstance = new Material(rainbowOutlineMaterial);
                auraRenderer.material = auraMaterialInstance;

                auraMaterialInstance.SetFloat("_OutlineThickness", outlineThicknessTexels);
                auraMaterialInstance.SetFloat("_BandCount", rainbowBandCount);
                auraMaterialInstance.SetFloat("_SpinSpeed", spinSpeed);
                auraMaterialInstance.SetFloat("_Brightness", brightness);
                auraMaterialInstance.SetFloat("_Alpha", outlineAlpha);
            }
        }

        private void LateUpdate()
        {
            if (hostRenderer == null) return;

            // Mirror the host's current animation frame and facing every frame —
            // LateUpdate so this runs after any Animator has already updated the
            // sprite/flip for this frame.
            auraRenderer.sprite = hostRenderer.sprite;
            auraRenderer.flipX = hostRenderer.flipX;
        }

        private void OnDestroy()
        {
            if (auraMaterialInstance != null)
            {
                Destroy(auraMaterialInstance);
            }
        }
    }
}
