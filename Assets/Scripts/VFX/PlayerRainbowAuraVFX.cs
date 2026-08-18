using UnityEngine;
using TwinsDefense.Player;
using TwinsDefense.Systems;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Cosmetic reward for the "Reach Level 100" achievement (see the 3 new
    /// Rainbow Aura rows in AchievementsPanelController): a spinning rainbow
    /// outline traced around the player's own sprite silhouette (scaled up by
    /// silhouetteScale) via RainbowOutline.shader. Shown only when the currently selected
    /// character (SelectedRunContext.SelectedCharacter) is the one that
    /// actually earned it — Izzy's Rainbow Aura does not show up while
    /// playing Court, even though both instances use the same shader.
    /// Unlocked state is derived live from CharacterProgressTracker
    /// (GetHighestLevel >= requiredLevel) — the same PlayerPrefs-backed
    /// tracker AchievementsPanelController already reads from, so this adds
    /// no new persistence.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerRainbowAuraVFX : MonoBehaviour
    {
        [Tooltip("Level CharacterProgressTracker.GetHighestLevel must reach (with THIS character, any tier) before the aura shows. Placeholder — keep in sync with the '100' hardcoded in AchievementsPanelController's 3 Rainbow Aura rows.")]
        [SerializeField] private int requiredLevel = 100;

        [Tooltip("The player's own animated sprite — the aura mirrors its current frame and flipX every frame so the outline always matches the visible silhouette. Auto-resolves to GetComponent<SpriteRenderer>() if left empty.")]
        [SerializeField] private SpriteRenderer characterRenderer;

        [SerializeField] private Material rainbowOutlineMaterial;
        [SerializeField] private int sortingOrder = 1;
        [Tooltip("Per-axis local scale applied to the aura relative to the character's own sprite. Keep at (1,1) for the inner-glow material (Mat_RainbowAuraInner) so the aura's footprint exactly matches the player's own sprite — any scale-up here shifts the inner ring past the real silhouette edge, where the player's own (higher sortingOrder) sprite no longer fully covers it. Only push above 1 when using the outward-ring material, to sit the ring cleanly outside the body.")]
        [SerializeField] private Vector2 silhouetteScale = new Vector2(1.2f, 1f);

        [Header("Outline look (placeholders — tune in Inspector)")]
        [SerializeField] private float outlineThicknessTexels = 2f;
        [SerializeField] private int rainbowBandCount = 6;
        [SerializeField] private float spinSpeed = 1.5f;
        [SerializeField] private float brightness = 1.4f;
        [SerializeField] private float outlineAlpha = 0.9f;

        private SpriteRenderer auraRenderer;
        private Transform auraTransform;
        private Material auraMaterialInstance;
        private bool unlocked;

        private void Awake()
        {
            if (characterRenderer == null)
            {
                characterRenderer = GetComponent<SpriteRenderer>();
            }

            GameObject auraObject = new GameObject("RainbowAura");
            auraObject.transform.SetParent(transform, false);
            auraObject.transform.localScale = new Vector3(silhouetteScale.x, silhouetteScale.y, 1f);

            auraRenderer = auraObject.AddComponent<SpriteRenderer>();
            auraRenderer.sortingOrder = sortingOrder;

            if (rainbowOutlineMaterial != null)
            {
                auraMaterialInstance = new Material(rainbowOutlineMaterial);
                auraRenderer.material = auraMaterialInstance;
            }

            auraTransform = auraObject.transform;
            auraObject.SetActive(false);
        }

        private void Start()
        {
            // Same timing rationale as PlayerStarAuraVFX: read after every other
            // component's Awake has run, so SelectedRunContext and
            // CharacterProgressTracker are both settled by now. Gated per exact
            // tier (not GetHighestLevel's character-wide total) — only the
            // specific evolution that earned level 100 shows the aura, not every
            // tier of that character.
            unlocked = CharacterProgressTracker.Instance.GetHighestLevelForTier(SelectedRunContext.Instance.SelectedCharacter, SelectedRunContext.Instance.SelectedTier) >= requiredLevel;
            auraTransform.gameObject.SetActive(unlocked);

            if (unlocked && auraMaterialInstance != null)
            {
                auraMaterialInstance.SetFloat("_OutlineThickness", outlineThicknessTexels);
                auraMaterialInstance.SetFloat("_BandCount", rainbowBandCount);
                auraMaterialInstance.SetFloat("_SpinSpeed", spinSpeed);
                auraMaterialInstance.SetFloat("_Brightness", brightness);
                auraMaterialInstance.SetFloat("_Alpha", outlineAlpha);
            }
        }

        private void LateUpdate()
        {
            if (!unlocked || characterRenderer == null) return;

            // Mirror the character's current animation frame and facing every
            // frame — LateUpdate so this runs after Animator/PlayerController
            // have already updated the sprite/flip for this frame.
            auraRenderer.sprite = characterRenderer.sprite;
            auraRenderer.flipX = characterRenderer.flipX;
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
