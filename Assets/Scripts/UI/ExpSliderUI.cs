using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.Progression;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Keeps the exp Slider's fill in sync with LevelManager.CurrentExp (0..1).
    /// Intended to sit on the Slider GameObject itself.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class ExpSliderUI : MonoBehaviour
    {
        private Slider slider;

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
                HandleExpChanged(LevelManager.Instance.CurrentExp);
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
            slider.value = currentExp;
        }
    }
}
