using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Main Menu button actions: Play loads Character Selection, Exit quits
    /// the application (stops Play mode instead, when running in the Editor),
    /// and Settings/Achievements toggle their own overlay panels on top of
    /// the main buttons.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [Header("Overlay Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject achievementsPanel;

        private void Awake()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (achievementsPanel != null) achievementsPanel.SetActive(false);
        }

        public void OnPlayClicked()
        {
            SceneManager.LoadScene("CharacterSelection");
        }

        public void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OnSettingsClicked()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void OnSettingsBackClicked()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void OnAchievementsClicked()
        {
            if (achievementsPanel != null) achievementsPanel.SetActive(true);
        }

        public void OnAchievementsBackClicked()
        {
            if (achievementsPanel != null) achievementsPanel.SetActive(false);
        }
    }
}
