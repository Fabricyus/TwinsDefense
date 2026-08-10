using TMPro;
using UnityEngine;
using TwinsDefense.Progression;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Keeps expTxt (1) in sync with LevelManager.CurrentLevel. Intended to sit
    /// on the level text GameObject itself, driven by LevelManager.OnLevelChanged
    /// instead of polling every frame.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LevelCounterUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelTxt;

        private void Awake()
        {
            if (levelTxt == null)
            {
                levelTxt = GetComponent<TextMeshProUGUI>();
            }
        }

        private void Start()
        {
            // Start (not OnEnable) so LevelManager.Awake has already run.
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelChanged += HandleLevelChanged;
                HandleLevelChanged(LevelManager.Instance.CurrentLevel);
            }
        }

        private void OnDisable()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelChanged -= HandleLevelChanged;
            }
        }

        private void HandleLevelChanged(int currentLevel)
        {
            levelTxt.text = currentLevel.ToString();
        }
    }
}
