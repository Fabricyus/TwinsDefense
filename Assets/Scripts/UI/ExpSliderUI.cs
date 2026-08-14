using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.Progression;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Keeps the exp Slider's fill in sync with LevelManager.CurrentExp (0..1),
    /// animating toward each new value instead of snapping. Intended to sit on
    /// the Slider GameObject itself.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class ExpSliderUI : MonoBehaviour
    {
        [Tooltip("Seconds the slider takes to catch up to a new exp value.")]
        [SerializeField] private float fillDuration = 0.25f;

        private Slider slider;
        private Coroutine fillRoutine;

        private void Awake()
        {
            slider = GetComponent<Slider>();
        }

        private void Start()
        {
            // Start (not OnEnable) so LevelManager.Awake has already run.
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnExpChanged += HandleExpChanged;
                slider.value = LevelManager.Instance.CurrentExp; // initial sync, no animation
            }
        }

        private void OnDisable()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnExpChanged -= HandleExpChanged;
            }
        }

        private void HandleExpChanged(float currentExp)
        {
            if (fillRoutine != null)
            {
                StopCoroutine(fillRoutine);
            }

            // A level-up resets the slider back to 0 right after it fills to 1 — snap that
            // drop instead of visibly animating backwards through the whole bar; only
            // forward progress (normal exp pickups) gets the fill animation.
            if (currentExp < slider.value)
            {
                slider.value = currentExp;
                fillRoutine = null;
                return;
            }

            fillRoutine = StartCoroutine(AnimateFill(currentExp));
        }

        /// <summary>Runs on unscaled time so the fill still plays out even the instant Time.timeScale drops to 0 on level-up.</summary>
        private IEnumerator AnimateFill(float targetValue)
        {
            float startValue = slider.value;
            float elapsed = 0f;

            while (elapsed < fillDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                slider.value = Mathf.Lerp(startValue, targetValue, fillDuration <= 0f ? 1f : elapsed / fillDuration);
                yield return null;
            }

            slider.value = targetValue;
            fillRoutine = null;
        }
    }
}
