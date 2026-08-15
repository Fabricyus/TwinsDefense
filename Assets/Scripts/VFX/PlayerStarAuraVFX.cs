using UnityEngine;
using TwinsDefense.Player;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Cosmetic reward for maxing a character's Star Upgrades (5/5, see
    /// PlayerStats.hasFiveStarAura): the same pulsing, spinning sunburst aura
    /// CardRarityVFX shows behind an Epic level-up card, spawned world-space
    /// under the player instead — offset downward on Y so it sits at the
    /// player's feet rather than centered on the body.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerStarAuraVFX : MonoBehaviour
    {
        [Tooltip("Local offset from the player's pivot. Negative Y drops the aura down to the feet.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, -0.6f, 0f);
        [SerializeField] private float baseScale = 1.2f;
        [SerializeField] private Color color = new Color(1f, 0.82f, 0.2f, 0.55f);
        [SerializeField] private int sortingOrder = 0;

        [Header("Pulse & Spin (matches CardRarityVFX's aura)")]
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float minScale = 0.95f;
        [SerializeField] private float maxScale = 1.08f;
        [SerializeField] private float minAlpha = 0.35f;
        [SerializeField] private float maxAlpha = 0.65f;
        [SerializeField] private float rotationSpeed = 20f;

        private PlayerStats stats;
        private SpriteRenderer auraRenderer;
        private Transform auraTransform;
        private float pulseT;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();

            GameObject auraObject = new GameObject("FiveStarAura");
            auraObject.transform.SetParent(transform, false);
            auraObject.transform.localPosition = offset;
            auraObject.transform.localScale = Vector3.one * baseScale;

            auraRenderer = auraObject.AddComponent<SpriteRenderer>();
            auraRenderer.sprite = SunburstSprite.Get();
            auraRenderer.color = color;
            auraRenderer.sortingOrder = sortingOrder;

            auraTransform = auraObject.transform;
            auraObject.SetActive(false);
        }

        private void Start()
        {
            // Read after PlayerCharacterData.Awake has applied purchased stars — Start runs
            // after every component's Awake, so hasFiveStarAura is settled by now.
            auraTransform.gameObject.SetActive(stats.hasFiveStarAura);
        }

        private void Update()
        {
            if (!auraTransform.gameObject.activeSelf) return;

            pulseT += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseT) + 1f) * 0.5f;

            auraTransform.localScale = Vector3.one * baseScale * Mathf.Lerp(minScale, maxScale, pulse);
            auraTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            Color c = auraRenderer.color;
            c.a = Mathf.Lerp(minAlpha, maxAlpha, pulse);
            auraRenderer.color = c;
        }
    }
}
