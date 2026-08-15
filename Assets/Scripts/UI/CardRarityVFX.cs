using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.Data;
using TwinsDefense.VFX;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Rarity flourish for a level-up card: a soft pulsing aura glow behind
    /// the card (blue for Rare, yellow for Epic) plus a handful of small
    /// embers that drift upward from the card's bottom edge and fade out,
    /// clipped to a footer strip by a RectMask2D so they never spill past
    /// the bottom portion of the card. Hidden entirely for Common cards.
    /// Everything animates on unscaled time since the card draft runs with
    /// the game paused (Time.timeScale = 0), matching CardSlotUI's own
    /// hover/punch animations. No custom shader or real ParticleSystem —
    /// embers are plain UI Images so RectMask2D clips them natively.
    /// </summary>
    public class CardRarityVFX : MonoBehaviour
    {
        [Header("Aura")]
        [SerializeField] private Image auraGlow;
        [SerializeField] private float auraPulseSpeed = 1.5f;
        [SerializeField] private float auraMinScale = 0.95f;
        [SerializeField] private float auraMaxScale = 1.08f;
        [SerializeField] private float auraMinAlpha = 0.35f;
        [SerializeField] private float auraMaxAlpha = 0.65f;
        [Tooltip("Degrees per second the aura spins around Z — slow, continuous, gives the glow a bit of life beyond the pulse.")]
        [SerializeField] private float auraRotationSpeed = 20f;

        [Header("Embers")]
        [Tooltip("Masked RectTransform (footer strip) the embers rise inside of and get clipped by.")]
        [SerializeField] private RectTransform emberContainer;
        [SerializeField] private int emberCount = 6;
        [SerializeField] private float emberRiseSpeed = 40f;
        [SerializeField] private float emberMinSize = 8f;
        [SerializeField] private float emberMaxSize = 16f;
        [SerializeField] private float emberLifetime = 1.4f;

        [Header("Colors")]
        [SerializeField] private Color rareColor = new Color(0.3f, 0.55f, 1f, 1f);
        [SerializeField] private Color epicColor = new Color(1f, 0.82f, 0.2f, 1f);

        private readonly List<Ember> embers = new List<Ember>();
        private Coroutine runRoutine;

        private class Ember
        {
            public RectTransform rect;
            public Image image;
            public float age;
            public float lifetime;
            public float speed;
        }

        private void Awake()
        {
            if (auraGlow != null)
            {
                // A plain radially-symmetric circle looks identical whether it's rotating or not —
                // this sprite has visible rays so the Z-rotation in Run() actually reads.
                auraGlow.sprite = SunburstSprite.Get();
            }

            for (int i = 0; i < emberCount; i++)
            {
                GameObject obj = new GameObject("Ember", typeof(RectTransform), typeof(Image));
                obj.transform.SetParent(emberContainer, false);

                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                Image img = obj.GetComponent<Image>();
                img.sprite = AttackCircleVFX.GetCircleSprite();
                img.raycastTarget = false;

                embers.Add(new Ember { rect = rt, image = img });
            }
        }

        /// <summary>Shows (and re-colors/restarts) the effect for Rare/Epic, or hides this GameObject entirely for Common.</summary>
        public void SetRarity(CardRarity rarity)
        {
            StopRun();

            if (rarity == CardRarity.Common)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            Color color = rarity == CardRarity.Epic ? epicColor : rareColor;

            if (auraGlow != null)
            {
                auraGlow.color = color;
            }

            foreach (Ember ember in embers)
            {
                ember.image.color = color;
                ResetEmber(ember, randomizeAge: true);
            }

            runRoutine = StartCoroutine(Run());
        }

        private void OnDisable()
        {
            StopRun();
        }

        private void StopRun()
        {
            if (runRoutine != null)
            {
                StopCoroutine(runRoutine);
                runRoutine = null;
            }
        }

        private IEnumerator Run()
        {
            float pulseT = 0f;

            while (true)
            {
                float dt = Time.unscaledDeltaTime;
                pulseT += dt * auraPulseSpeed;

                if (auraGlow != null)
                {
                    float pulse = (Mathf.Sin(pulseT) + 1f) * 0.5f;
                    auraGlow.transform.localScale = Vector3.one * Mathf.Lerp(auraMinScale, auraMaxScale, pulse);
                    auraGlow.transform.Rotate(0f, 0f, auraRotationSpeed * dt);
                    Color c = auraGlow.color;
                    c.a = Mathf.Lerp(auraMinAlpha, auraMaxAlpha, pulse);
                    auraGlow.color = c;
                }

                foreach (Ember ember in embers)
                {
                    ember.age += dt;
                    if (ember.age >= ember.lifetime)
                    {
                        ResetEmber(ember, randomizeAge: false);
                        continue;
                    }

                    float t = ember.age / ember.lifetime;
                    Vector2 pos = ember.rect.anchoredPosition;
                    pos.y += ember.speed * dt;
                    ember.rect.anchoredPosition = pos;

                    Color c = ember.image.color;
                    c.a = 1f - t;
                    ember.image.color = c;
                }

                yield return null;
            }
        }

        /// <summary>Sends an ember back to a random spot along the container's bottom edge with fresh size/speed/lifetime. randomizeAge staggers the very first batch so they don't all pop in sync.</summary>
        private void ResetEmber(Ember ember, bool randomizeAge)
        {
            float width = emberContainer != null ? emberContainer.rect.width : 100f;
            float x = Random.Range(-width * 0.5f, width * 0.5f);
            ember.rect.anchoredPosition = new Vector2(x, 0f);

            float size = Random.Range(emberMinSize, emberMaxSize);
            ember.rect.sizeDelta = new Vector2(size, size);

            ember.lifetime = emberLifetime * Random.Range(0.8f, 1.2f);
            ember.speed = emberRiseSpeed * Random.Range(0.85f, 1.15f);
            ember.age = randomizeAge ? Random.Range(0f, ember.lifetime) : 0f;
        }

    }
}
