using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Main Menu button actions: Play loads the Save (profile list) screen,
    /// Exit quits the application (stops Play mode instead, when running in
    /// the Editor), and Settings toggles its own overlay panel on top of the
    /// main buttons. Achievements now lives in Character Selection instead —
    /// see CharacterSelectionController — since progress is scoped per save
    /// profile and no profile is active yet on this screen.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [Header("Overlay Panels")]
        [SerializeField] private GameObject settingsPanel;

        private void Awake()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void OnPlayClicked()
        {
            SceneManager.LoadScene("Save");
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
    }
}
